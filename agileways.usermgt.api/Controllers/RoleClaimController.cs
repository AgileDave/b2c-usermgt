using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using agileways.usermgt.shared.server.Services;
using agileways.usermgt.shared.Models.DirectoryObjects;
using agileways.usermgt.api.Shared.RestModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;

namespace agileways.usermgt.api.Controllers
{

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

        [HttpGet]
        public async Task<IEnumerable<AppRegistration>> GetAppRegistrations()
        {
            return await _graph.GetAppRegistrations();
        }

        [HttpPost]
        public async Task<IActionResult> UserRoles()
        {
            string input = null;

            // If not data came in, then return
            if (this.Request.Body == null)
            {
                return StatusCode((int)HttpStatusCode.Conflict, new RoleResponse("Request content is null", HttpStatusCode.Conflict));
            }

            // Read the input claims from the request body
            using (StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8))
            {
                input = await reader.ReadToEndAsync();
            }

            // Check input content value
            if (string.IsNullOrEmpty(input))
            {
                return StatusCode((int)HttpStatusCode.Conflict, new RoleResponse("Request content is empty", HttpStatusCode.Conflict));
            }

            // Check input content value
            if (string.IsNullOrEmpty(input))
            {
                return StatusCode((int)HttpStatusCode.Conflict, new RoleResponse("Request content is empty", HttpStatusCode.Conflict));
            }

            // Convert the input string into RoleRequest object
            var inputClaims = RoleRequest.Parse(input);

            if (inputClaims == null)
            {
                return StatusCode((int)HttpStatusCode.Conflict, new RoleResponse("Can not deserialize input claims", HttpStatusCode.Conflict));
            }

            //Check if the language parameter is presented
            if (string.IsNullOrEmpty(inputClaims.clientId))
            {
                return StatusCode((int)HttpStatusCode.Conflict, new RoleResponse("Client/App ID is null or empty", HttpStatusCode.Conflict));
            }

            //Check if the objectId parameter is presented
            if (string.IsNullOrEmpty(inputClaims.objectId))
            {
                return StatusCode((int)HttpStatusCode.Conflict, new RoleResponse("User object Id is null or empty", HttpStatusCode.Conflict));
            }

            try
            {
                var userRoleObjs = await _graph.GetUserAssignedRolesForApplicationByClientIdAsync(inputClaims.clientId, inputClaims.objectId);
                var userRoles = userRoleObjs.Select(ur => ur.Value);

                return StatusCode((int)HttpStatusCode.OK, new RoleResponse(string.Empty, HttpStatusCode.OK)
                {
                    roles = userRoles
                });
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.Conflict, new RoleResponse($"General error (REST API): {ex.Message}", HttpStatusCode.Conflict));
            }
        }
    }
}