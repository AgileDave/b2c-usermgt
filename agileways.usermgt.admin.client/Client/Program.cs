using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authorization;

namespace agileways.usermgt.admin.client.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");

            builder.Services.AddHttpClient("agileways.usermgt.admin.client.ServerAPI", client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress))
                .AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>();

            // Supply HttpClient instances that include access tokens when making requests to the server project
            builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("agileways.usermgt.admin.client.ServerAPI"));

            builder.Services.AddMsalAuthentication(options =>
            {
                builder.Configuration.Bind("AzureAdB2C", options.ProviderOptions.Authentication);
                options.ProviderOptions.DefaultAccessTokenScopes.Add("https://kubedave.onmicrosoft.com/df3b9765-f8a0-41ef-91d5-782dd536c084/API.Access");

                //options.UserOptions.RoleClaim = "roles";
            });

            // builder.Services.AddAuthorizationCore(opt => opt.AddPolicy("GlobalAdmin",
            //                                                 pol => pol.RequireClaim("roles", "Global.RoleAdmin")));

            builder.Services.AddAuthorizationCore(opt =>
            {
                opt.AddPolicy("GlobalAdmin", pol =>
                    pol.Requirements.Add(new RoleRequirement("Global.RoleAdmin")));
                opt.AddPolicy("CompanyUserAdmin", pol =>
                    pol.Requirements.Add(new RoleRequirement("Global.RoleAdmin, Company.UserAdmin")));
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
