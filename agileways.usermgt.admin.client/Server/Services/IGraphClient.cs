using System.Collections.Generic;
using System.Threading.Tasks;
using agileways.usermgt.admin.client.Shared.DirectoryObjects;

namespace agileways.usermgt.admin.client.Server.Services
{
    public interface IGraphClient
    {
        Task<IEnumerable<AppRegistration>> GetAppRegistrations();
        Task<AppRegistration> GetAppRegistration(string appId);
        Task<IEnumerable<Role>> GetAppRolesForAppRegistration(string appId);
        Task<bool> AddRoleToAppRegistration(string appId, Role role);
        Task<bool> UpdateAppRegistrationRole(string appId, Role role);
        Task<Role> GetAppRoleForAppRegistration(string appId, string roleId);
        Task<IEnumerable<User>> GetUsers();
        Task<ServicePrincipal> GetServicePrincipalFromApplicationName(string appName);
        Task<IEnumerable<AppRoleAssignedTo>> GetAppRoleAssignedTosAsync(string spnId);
        Task<AppRoleAssignedTo> AddAppRoleAssignmentToSpn(string spnId, AppRoleAssignedTo arat);
        Task<bool> DeleteAppRoleAssignmentFromSpn(string spnId, string principalId);
        Task<IEnumerable<AppRoleAssignedTo>> GetAppRoleAssignmentsForUserIdAsync(string userId);
        Task<IEnumerable<Role>> GetUserAssignedRolesForApplication(string appId, string userId);
        Task<AppRegistration> GetAppRegistrationByClientId(string clientId);
        Task<Role> GetRoleForSpnIdAndRoleIdAsync(string spnId, string roleId);
    }
}