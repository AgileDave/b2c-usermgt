
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using agileways.usermgt.admin.client.Server.Services;
using agileways.usermgt.admin.client.Shared.DirectoryObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace agileways.usermgt.admin.client.Server.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class AssignmentsController : ControllerBase
    {
        private readonly ILogger<AssignmentsController> _logger;
        private IGraphClient _graph;
        public AssignmentsController(IGraphClient graph, ILogger<AssignmentsController> logger)
        {
            _logger = logger;
            _graph = graph;
        }


        [HttpGet("{appId}")]
        public async Task<IEnumerable<AppRoleAssignedTo>> GetAppRoleAssignedTos(string appId)
        {
            var app = await _graph.GetAppRegistration(appId);

            _logger.LogInformation($"Got app {app.DisplayName}");

            var spn = await _graph.GetServicePrincipalFromApplicationName(app.DisplayName);

            _logger.LogInformation($"Got SPN {spn.Id}");

            return await _graph.GetAppRoleAssignedTosAsync(spn.Id);
        }

        [HttpPost("/apps/{spnId}/assignrole")]
        public async Task<IActionResult> AddAppRoleAssignment(string spnId, [FromBody] AppRoleAssignedTo arat)
        {
            var result = await _graph.AddAppRoleAssignmentToSpn(spnId, arat);
            return Ok();
        }

        [HttpDelete("/apps/{spnId}/assignment/{principalId}")]
        public async Task<IActionResult> DeleteAppRoleAssignment(string spnId, string principalId)
        {
            var result = await _graph.DeleteAppRoleAssignmentFromSpn(spnId, principalId);
            return Ok();
        }
    }

}
