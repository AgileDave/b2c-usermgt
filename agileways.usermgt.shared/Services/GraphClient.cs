using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using agileways.usermgt.shared.Models.DirectoryObjects;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using Microsoft.Graph.Auth;
using Microsoft.Identity.Client;

namespace agileways.usermgt.shared.Services
{
    public class GraphClient : IGraphClient
    {
        private IConfiguration _config;
        private readonly AuthenticationConfig _authConfig;
        private readonly IConfidentialClientApplication _app;
        private GraphServiceClient _graphClient;

        public GraphClient(IConfiguration config)
        {
            _config = config;
            _authConfig = ReadFromJsonFile();

            _app = ConfidentialClientApplicationBuilder.Create(_authConfig.ClientId)
                .WithClientSecret(_authConfig.ClientSecret)
                .WithAuthority(new Uri(_authConfig.Authority))
                .Build();

            // With client credentials flows the scopes is ALWAYS of the shape "resource/.default", as the 
            // application permissions need to be set statically (in the portal or by PowerShell), and then granted by
            // a tenant administrator. 
            string[] scopes = new string[] { $"{_authConfig.ApiUrl}.default" };

            InitAsync(_app, scopes).GetAwaiter().GetResult();
        }


        private async Task InitAsync(IConfidentialClientApplication app, string[] scopes)
        {
            AuthenticationResult result = null;
            try
            {
                result = await app.AcquireTokenForClient(scopes)
                    .ExecuteAsync();
            }
            catch (MsalServiceException ex) when (ex.Message.Contains("AADSTS70011"))
            {
                // Invalid scope. The scope has to be of the form "https://resourceurl/.default"
                // Mitigation: change the scope to be as expected
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Scope provided is not supported");
                Console.ResetColor();
            }

            _graphClient = new GraphServiceClient(new ClientCredentialProvider(app, String.Join(" ", scopes)));
        }

        public async Task<IEnumerable<AppRegistration>> GetAppRegistrations()
        {
            var appPages = await _graphClient.Applications
                                        .Request()
                                        .GetAsync();

            var apps = appPages.Select(a => new AppRegistration
            {
                Id = a.Id,
                DisplayName = a.DisplayName,
                RoleCount = a.AppRoles.Count()
            });
            return apps;
        }


        private AuthenticationConfig ReadFromJsonFile()
        {
            var section = _config.GetSection("AzureAdB2CGraph");

            return new AuthenticationConfig
            {
                Instance = section.GetValue<string>("Instance"),
                ApiUrl = section.GetValue<string>("ApiUrl"),
                Tenant = section.GetValue<string>("Tenant"),
                ClientId = section.GetValue<string>("ClientId"),
                ClientSecret = section.GetValue<string>("ClientSecret")
            };
        }

        public async Task<IEnumerable<Role>> GetAppRolesForAppRegistration(string appId)
        {
            var app = await _graphClient.Applications[appId]
                                        .Request()
                                        .GetAsync();

            var appRoles = app?.AppRoles.Select(r => new Role
            {
                Id = r.Id.ToString(),
                Name = r.DisplayName,
                Description = r.Description,
                Value = r.Value
            });

            return appRoles;
        }

        public async Task<bool> AddRoleToAppRegistration(string appId, Role role)
        {
            var app = await _graphClient.Applications[appId]
                                .Request()
                                .GetAsync();

            var newRoles = app.AppRoles.Append(new AppRole
            {
                Id = Guid.NewGuid(),
                DisplayName = role.Name,
                Value = role.Value,
                Description = role.Description,
                AllowedMemberTypes = new string[] { "User" },
                IsEnabled = true
            });

            var newApp = new Application
            {
                AppRoles = newRoles
            };

            await _graphClient.Applications[appId]
                            .Request()
                            .UpdateAsync(newApp);

            return true;
        }

        public async Task<bool> UpdateAppRegistrationRole(string appId, Role role)
        {
            var app = await _graphClient.Applications[appId]
                    .Request()
                    .GetAsync();
            var appRoles = app.AppRoles;
            var appRole = appRoles.FirstOrDefault(a => a.Id.ToString() == role.Id);

            if (appRole != null)
            {
                appRole.DisplayName = role.Name;
                appRole.Description = role.Description;
                appRole.Value = role.Value;

                var newApp = new Application
                {
                    AppRoles = appRoles
                };
                await _graphClient.Applications[appId]
                                    .Request()
                                    .UpdateAsync(newApp);

                return true;
            }

            return false;
        }

        public async Task<Role> GetAppRoleForAppRegistration(string appId, string roleId)
        {
            var app = await _graphClient.Applications[appId]
                    .Request()
                    .GetAsync();

            var appRole = app.AppRoles.FirstOrDefault(a => a.Id.ToString() == roleId);

            if (appRole != null)
            {
                return new Role
                {
                    Description = appRole.Description,
                    Id = roleId,
                    Name = appRole.DisplayName,
                    Value = appRole.Value
                };
            }
            else
            {
                return new Role();
            }
        }

        public async Task<IEnumerable<Models.DirectoryObjects.User>> GetUsers()
        {
            var users = await _graphClient.Users
                                        .Request()
                                        .Select(u => new
                                        {
                                            u.Id,
                                            u.DisplayName,
                                            u.GivenName,
                                            u.Surname,
                                            u.Identities,
                                            u.UserPrincipalName
                                        })
                                        .GetAsync();

            var userPages = users.Select(u => new Models.DirectoryObjects.User
            {
                Id = u.Id,
                DisplayName = u.DisplayName,
                GivenName = u.GivenName,
                Surname = u.Surname,
                Identities = u.Identities.Select(i => new Models.DirectoryObjects.UserIdentity
                {
                    SignInType = i.SignInType,
                    Issuer = i.Issuer,
                    IssuerAssignedId = i.IssuerAssignedId
                }),
                UserPrincipalName = u.UserPrincipalName
            });
            return userPages;
        }

        public async Task<Models.DirectoryObjects.ServicePrincipal> GetServicePrincipalFromApplicationName(string appName)
        {
            var spns = await _graphClient.ServicePrincipals
                                        .Request()
                                        .Filter($"displayName eq '{appName}'")
                                        .GetAsync();

            var spn = spns.FirstOrDefault();

            return new Models.DirectoryObjects.ServicePrincipal
            {
                Id = spn.Id,
                AppId = spn.AppId,
                ServicePrincipalType = spn.ServicePrincipalType,
                AppRoleCount = spn.AppRoles.Count()
            };
        }

        public async Task<IEnumerable<AppRoleAssignedTo>> GetAppRoleAssignedTosAsync(string spnId)
        {
            var appRoleAssignTos = await _graphClient.ServicePrincipals[spnId]
                                                        .AppRoleAssignedTo
                                                        .Request()
                                                        .GetAsync();

            return appRoleAssignTos.Select(arat => new AppRoleAssignedTo
            {
                AppRoleId = arat.AppRoleId?.ToString(),
                PrincipalDisplayName = arat.PrincipalDisplayName,
                ResourceDisplayName = arat.ResourceDisplayName,
                ResourceId = arat.ResourceId?.ToString(),
                PrincipalId = arat.PrincipalId?.ToString(),
                PrincipalType = arat.PrincipalType,
                Id = arat.Id
            });
        }

        public async Task<AppRegistration> GetAppRegistration(string appId)
        {
            var app = await _graphClient.Applications[appId]
                                        .Request()
                                        .GetAsync();

            return new AppRegistration
            {
                Id = app.Id,
                DisplayName = app.DisplayName,
                RoleCount = app.AppRoles.Count()
            };
        }

        public async Task<AppRegistration> GetAppRegistrationByClientId(string clientId)
        {
            var appPages = await _graphClient.Applications
                                        .Request()
                                        .Filter($"appId eq '{clientId}'")
                                        .GetAsync();

            var app = appPages.First();

            return new AppRegistration
            {
                Id = app.Id,
                DisplayName = app.DisplayName,
                RoleCount = app.AppRoles.Count()
            };
        }

        public async Task<AppRoleAssignedTo> AddAppRoleAssignmentToSpn(string spnId, AppRoleAssignedTo arat)
        {
            var newArat = new AppRoleAssignment
            {
                AppRoleId = Guid.Parse(arat.AppRoleId),
                PrincipalId = Guid.Parse(arat.PrincipalId),
                ResourceId = Guid.Parse(arat.ResourceId)
            };

            var newAssignment = await _graphClient.ServicePrincipals[spnId]
                                .AppRoleAssignedTo
                                .Request()
                                .AddAsync(newArat);

            return new AppRoleAssignedTo
            {
                AppRoleId = newAssignment.AppRoleId.ToString(),
                PrincipalDisplayName = newAssignment.PrincipalDisplayName,
                PrincipalId = newAssignment.PrincipalId.ToString(),
                PrincipalType = newAssignment.PrincipalType,
                ResourceDisplayName = newAssignment.ResourceDisplayName,
                ResourceId = newAssignment.ResourceId.ToString(),
                Id = newAssignment.Id
            };
        }

        public async Task<bool> DeleteAppRoleAssignmentFromSpn(string spnId, string principalId)
        {
            await _graphClient.ServicePrincipals[spnId]
                                .AppRoleAssignedTo[principalId]
                                .Request()
                                .DeleteAsync();

            return true;
        }

        public async Task<IEnumerable<AppRoleAssignedTo>> GetAppRoleAssignmentsForUserIdAsync(string userId)
        {

            var arats = await _graphClient.Users[userId]
                                    .AppRoleAssignments
                                    .Request()
                                    .GetAsync();

            return arats.Select(a => new AppRoleAssignedTo
            {
                AppRoleId = a.AppRoleId?.ToString(),
                Id = a.Id,
                PrincipalDisplayName = a.PrincipalDisplayName,
                PrincipalId = a.PrincipalId?.ToString(),
                PrincipalType = a.PrincipalType,
                ResourceDisplayName = a.ResourceDisplayName,
                ResourceId = a.ResourceId?.ToString(),
                AppRoleName = this.GetRoleForSpnIdAndRoleIdAsync(a.ResourceId?.ToString(), a.AppRoleId?.ToString()).Result.Name
            });
        }

        public async Task<Role> GetRoleForSpnIdAndRoleIdAsync(string spnId, string roleId)
        {

            var spn = await _graphClient.ServicePrincipals[spnId]
                                .Request()
                                .GetAsync();

            var apps = await _graphClient.Applications
                                .Request()
                                .Filter($"appId eq '{spn.AppId}'")
                                .GetAsync();
            var app = apps.First();

            return await this.GetAppRoleForAppRegistration(app.Id, roleId);
        }

        public async Task<IEnumerable<Role>> GetUserAssignedRolesForApplication(string appId, string userId)
        {

            var app = await this.GetAppRegistration(appId);
            var spn = await this.GetServicePrincipalFromApplicationName(app.DisplayName);
            var appRoles = await this.GetAppRolesForAppRegistration(appId);
            var appRoleDict = appRoles.ToDictionary(
                k => k.Id,
                v => v.Value
            );
            var userRoles = await _graphClient.Users[userId]
                        .AppRoleAssignments
                        .Request()
                        .Filter($"resourceId eq {spn.Id}")
                        .GetAsync();

            return userRoles.Select(ur => new Role
            {
                Description = string.Empty,
                Id = ur.AppRoleId?.ToString(),
                Name = string.Empty,
                Value = appRoleDict[ur.AppRoleId?.ToString()]
            });
        }
    }
}