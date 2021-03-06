

using System.Collections.Generic;
using System.Threading.Tasks;
using agileways.usermgt.shared.server.Services;
using agileways.usermgt.shared.Models.DirectoryObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace agileways.usermgt.admin.client.Server.Controllers
{

    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class RoleClaimController : ControllerBase
    {
        private readonly ILogger<RoleClaimController> _logger;
        private IGraphClient _graph;
        public RoleClaimController(IGraphClient graph, ILogger<RoleClaimController> logger)
        {
            _logger = logger;
            _graph = graph;
        }

        [HttpGet("user/{userId}/app/{appId}")]
        public async Task<IEnumerable<Role>> GetRolesForUserAndApp(string userId, string appId)
        {

            return await _graph.GetUserAssignedRolesForApplication(appId, userId);
        }
    }
}