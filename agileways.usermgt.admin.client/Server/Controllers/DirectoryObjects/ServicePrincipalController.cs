

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
    public class ServicePrincipalController : ControllerBase
    {
        private readonly ILogger<ServicePrincipalController> _logger;
        private IGraphClient _graph;
        public ServicePrincipalController(IGraphClient graph, ILogger<ServicePrincipalController> logger)
        {
            _logger = logger;
            _graph = graph;
        }

        [HttpGet("/app/{appId}/spn")]
        public async Task<ServicePrincipal> GetSpnForAppRegistrationId(string appId)
        {
            var app = await _graph.GetAppRegistration(appId);
            return await _graph.GetServicePrincipalFromApplicationName(app.DisplayName);
        }

    }
}