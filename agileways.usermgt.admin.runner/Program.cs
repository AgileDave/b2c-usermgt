using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Graph;
using Microsoft.Graph.Auth;
using Microsoft.Identity.Client;

namespace agileways.usermgt.admin.runner
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                RunAsync().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);
                Console.ResetColor();
            }

            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
        }

        private static async Task RunAsync()
        {
            AuthenticationConfig config = AuthenticationConfig.ReadFromJsonFile("appsettings.json");

            // Even if this is a console application here, a daemon application is a confidential client application
            IConfidentialClientApplication app;

            app = ConfidentialClientApplicationBuilder.Create(config.ClientId)
                .WithClientSecret(config.ClientSecret)
                .WithAuthority(new Uri(config.Authority))
                .Build();

            // With client credentials flows the scopes is ALWAYS of the shape "resource/.default", as the 
            // application permissions need to be set statically (in the portal or by PowerShell), and then granted by
            // a tenant administrator. 
            string[] scopes = new string[] { $"{config.ApiUrl}.default" };

            AuthenticationResult result = null;
            try
            {
                result = await app.AcquireTokenForClient(scopes)
                    .ExecuteAsync();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Token acquired");
                Console.WriteLine($"{result.AccessToken}");
                Console.ResetColor();
            }
            catch (MsalServiceException ex) when (ex.Message.Contains("AADSTS70011"))
            {
                // Invalid scope. The scope has to be of the form "https://resourceurl/.default"
                // Mitigation: change the scope to be as expected
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Scope provided is not supported");
                Console.ResetColor();
            }

            var graphClient = new GraphServiceClient(new ClientCredentialProvider(app, String.Join(" ", scopes)));

            var userRequest = graphClient.Users.Request();
            var userPages = await userRequest.GetAsync();
            foreach (var user in userPages)
            {
                Console.WriteLine($"Retrieved {user.DisplayName}");
            }

            var appRequest = graphClient.Applications.Request();
            var appPages = await appRequest.GetAsync();
            foreach (var page in appPages)
            {
                Console.WriteLine($"Retrieved {page.DisplayName} with ID {page.Id}");
                var spPages = await graphClient.ServicePrincipals
                                                .Request()
                                                .Filter($"displayName eq '{page.DisplayName}'")
                                                .GetAsync();
                foreach (var spPage in spPages)
                {
                    Console.WriteLine($"\tHas SPN ID of {spPage.Id}");
                    var appRolesCount = spPage.AppRoles.Count();
                    Console.WriteLine($"\tHas {appRolesCount} App Roles Defined");
                    if (appRolesCount > 0)
                    {
                        var appRoleAssignments = await graphClient.ServicePrincipals[$"{spPage.Id}"]
                                                                    .AppRoleAssignments
                                                                    .Request()
                                                                    .GetAsync();
                        foreach (var ara in appRoleAssignments)
                        {
                            Console.WriteLine($"\t{ara.PrincipalDisplayName} is assigned to {ara.ResourceDisplayName}");
                        }

                        var appRoleAssignTos = await graphClient.ServicePrincipals[$"{spPage.Id}"]
                                                                    .AppRoleAssignedTo
                                                                    .Request()
                                                                    .GetAsync();

                        foreach (var arat in appRoleAssignTos)
                        {
                            Console.WriteLine($"\t[ASSIGN-TO] {arat.PrincipalDisplayName} is assigned to {arat.ResourceDisplayName} with role id {arat.AppRoleId}");
                        }
                    }
                }
            }

        }
    }
}
