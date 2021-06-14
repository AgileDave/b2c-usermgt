using System;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;

namespace clientApp1.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");

            builder.Services.AddHttpClient("clientApp1.ServerAPI", client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress))
                .AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>();

            // Supply HttpClient instances that include access tokens when making requests to the server project
            builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("clientApp1.ServerAPI"));

            builder.Services.AddMsalAuthentication(options =>
            {
                builder.Configuration.Bind("AzureAdB2C", options.ProviderOptions.Authentication);
                options.ProviderOptions.DefaultAccessTokenScopes.Add("https://kubedave.onmicrosoft.com/2868c1f3-feb7-4a11-8c11-051bb972c4ac/API.Access");
            });

            builder.Services.AddAuthorizationCore(opt =>
            {
                opt.AddPolicy("AppAdminRole", pol =>
                    pol.Requirements.Add(new RoleRequirement("App.Admin")));
                opt.AddPolicy("CompanyAdminRole", pol =>
                    pol.Requirements.Add(new RoleRequirement("App.Admin", "Company.Admin")));
                opt.AddPolicy("CompanyUserRole", pol =>
                    pol.Requirements.Add(new RoleRequirement("App.Admin", "Company.Admin", "Company.User")));
            });

            builder.Services.AddSingleton<IAuthorizationHandler, RoleHandler>();

            await builder.Build().RunAsync();
        }
    }

    public class RoleRequirement : IAuthorizationRequirement
    {
        public string[] RoleNames { get; }
        public RoleRequirement(params string[] roleNames)
        {
            RoleNames = roleNames;
        }
    }


    public class RoleHandler : AuthorizationHandler<RoleRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, RoleRequirement requirement)
        {
            if (!context.User.HasClaim(c => c.Type == "roles"))
            {
                return Task.CompletedTask;
            }

            var roles = context.User.FindFirst(c => c.Type == "roles").Value;
            foreach (var roleName in requirement.RoleNames)
            {
                if (roles.Contains(roleName))
                {
                    context.Succeed(requirement);
                    break;
                }
            }
            return Task.CompletedTask;
        }
    }


}
