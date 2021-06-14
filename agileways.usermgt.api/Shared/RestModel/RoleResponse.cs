
using System.Collections.Generic;
using System.Net;
using System.Reflection;

namespace agileways.usermgt.api.Shared.RestModel
{
    public class RoleResponse
    {
        public string version { get; set; }
        public int status { get; set; }
        public string userMessage { get; set; }

        // Optional claims
        public IEnumerable<string> roles { get; set; }

        public RoleResponse(string message, HttpStatusCode status)
        {
            this.userMessage = message;
            this.status = (int)status;
            this.version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }
    }
}