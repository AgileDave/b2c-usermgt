

using System.Collections.Generic;

namespace agileways.usermgt.shared.Models.DirectoryObjects
{
    public class ServicePrincipal
    {
        public string Id { get; set; }
        public string AppId { get; set; }
        public string ServicePrincipalType { get; set; }
        public int AppRoleCount { get; set; }
        public IEnumerable<AppRoleAssignments> AppRoleAssignments { get; set; }
        public IEnumerable<AppRoleAssignedTo> AppRoleAssignedTo { get; set; }
    }
}