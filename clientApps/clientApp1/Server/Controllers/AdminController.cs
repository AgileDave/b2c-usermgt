

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using clientApp1.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace clientApp1.Server.Controllers
{

    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly ILogger<AdminController> _logger;
        public AdminController(ILogger<AdminController> logger)
        {
            _logger = logger;
        }

        [HttpGet("users")]
        public async Task<IEnumerable<User>> GetUsers()
        {
            var httpClient = new HttpClient();
            IEnumerable<User> users = new List<User>();

            var claims = User.Claims;
            foreach (var claim in claims)
            {
                _logger.LogInformation($"{claim.Type}: {claim.Value}");
            }

            var role = claims.FirstOrDefault(c => c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role")?.Value;
            var companyId = claims.FirstOrDefault(c => c.Type == "company")?.Value;

            if (role == "Company.Admin")
            {
                if (!String.IsNullOrWhiteSpace(companyId))
                {
                    var resp = await httpClient.GetAsync($"https://gy-usermgt-api.azurewebsites.net/user/{companyId}");
                    var body = await resp.Content.ReadAsStringAsync();

                    _logger.LogInformation(body);

                    users = JsonSerializer.Deserialize<IEnumerable<User>>(body);

                }
            }
            else if (role == "App.Admin")
            {
                var resp = await httpClient.GetAsync($"https://gy-usermgt-api.azurewebsites.net/user");
                var body = await resp.Content.ReadAsStringAsync();

                _logger.LogInformation(body);

                users = JsonSerializer.Deserialize<IEnumerable<User>>(body);

            }

            return users;
        }
    }
}