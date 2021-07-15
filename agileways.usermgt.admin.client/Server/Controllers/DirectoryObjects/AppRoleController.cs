using System.Collections.Generic;
using System.Threading.Tasks;
using agileways.usermgt.shared.Services;
using agileways.usermgt.shared.Models.DirectoryObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace agileways.usermgt.admin.client.Server.Controllers
{
    [Authorize]
    [ApiController]
    public class AppRoleController : ControllerBase
    {
        private readonly ILogger<AppRoleController> _logger;
        private IGraphClient _graph;
        public AppRoleController(IGraphClient graph, ILogger<AppRoleController> logger)
        {
            _logger = logger;
            _graph = graph;
        }

        [HttpGet("app/{appId}/roles")]
        public async Task<IEnumerable<Role>> GetRolesForAppRegistrations(string appId)
        {
            return await _graph.GetAppRolesForAppRegistration(appId);
        }

        [HttpPost("apps/{id}/roles")]
        public async Task<IActionResult> AddRoleToAppRegistration(string id, [FromBody] Role role)
        {
            var result = await _graph.AddRoleToAppRegistration(id, role);

            if (result)
            {
                return Ok();
            }
            return BadRequest("something bad happened");
        }

        [HttpPut("apps/{id}/roles")]
        public async Task<IActionResult> UpdateAppRegistrationRole(string id, [FromBody] Role role)
        {
            var result = await _graph.UpdateAppRegistrationRole(id, role);

            if (result)
            {
                return Ok();
            }
            return BadRequest("something bad happened");
        }

        [HttpGet("apps/{id}/roles/{roleId}")]
        public async Task<Role> GetRoleForAppReg(string id, string roleId) =>
             await _graph.GetAppRoleForAppRegistration(id, roleId);
    }
}
