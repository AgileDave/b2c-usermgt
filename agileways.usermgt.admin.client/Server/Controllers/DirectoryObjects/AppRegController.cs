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
    [Route("[controller]")]
    public class AppRegController : ControllerBase
    {
        private readonly ILogger<AppRegController> _logger;
        private IGraphClient _graph;
        public AppRegController(IGraphClient graph, ILogger<AppRegController> logger)
        {
            _logger = logger;
            _graph = graph;
        }


        [HttpGet]
        public async Task<IEnumerable<AppRegistration>> GetAppRegistrations()
        {
            return await _graph.GetAppRegistrations();
        }
    }
}