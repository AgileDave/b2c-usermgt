
using System.Collections.Generic;

namespace agileways.usermgt.admin.client.Shared.DirectoryObjects
{
    public class User
    {
        public string DisplayName { get; set; }
        public string GivenName { get; set; }
        public string Surname { get; set; }
        public string Id { get; set; }
        public string UserPrincipalName { get; set; }
        public IEnumerable<UserIdentity> Identities { get; set; }
    }
}