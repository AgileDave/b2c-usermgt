using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using agileways.usermgt.shared.server.Services;
using agileways.usermgt.shared.Models.DirectoryObjects;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace agileways.usermgt.api.Controllers
{

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
        public async Task<IEnumerable<User>> Get()
        {
            _logger.LogInformation($"searching for all users");
            return await _graph.GetUsers();
        }

        [HttpGet("{companyId}")]
        public async Task<IEnumerable<User>> GetUsersByCompany(string companyId)
        {
            _logger.LogInformation($"searching for {companyId}");
            return await _graph.GetUsersForCompanyAsync(companyId);
        }
    }
}