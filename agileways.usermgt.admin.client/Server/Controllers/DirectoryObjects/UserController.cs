
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
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> _logger;
        private IGraphClient _graph;
        public UserController(IGraphClient graph, ILogger<UserController> logger)
        {
            _logger = logger;
            _graph = graph;
        }

        [HttpGet]
        public async Task<IEnumerable<User>> GetUsers()
        {
            return await _graph.GetUsers();
        }

        [HttpGet("{userId}/assignments")]
        public async Task<IEnumerable<AppRoleAssignedTo>> GetAppRoleAssignmentsForUser(string userId)
        {
            return await _graph.GetAppRoleAssignmentsForUserId(userId);
        }
    }
}