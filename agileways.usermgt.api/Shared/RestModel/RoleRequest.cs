
using System.Text.Json;

namespace agileways.usermgt.api.Shared.RestModel
{
    public class RoleRequest
    {
        public string objectId { get; set; }
        public string clientId { get; set; }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }

        public static RoleRequest Parse(string JSON)
        {
            return JsonSerializer.Deserialize<RoleRequest>(JSON);
        }
    }
}