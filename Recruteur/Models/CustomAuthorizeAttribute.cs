using JF.DAL.Security;

namespace Recruteur.Models
{
    /// <summary>
    /// Recruteur portal role gate. Derives from the shared RoleAuthorizeAttribute
    /// so unauthenticated users go to the forms loginUrl and authenticated users
    /// without the role go to Default/AccessDenied. Role checks use the
    /// CustomPrincipal populated in Global.asax Application_PostAuthenticateRequest.
    /// </summary>
    public class CustomAuthorizeAttribute : RoleAuthorizeAttribute
    {
        public CustomAuthorizeAttribute()
        {
            AccessDeniedController = "Default";
            AccessDeniedAction = "AccessDenied";
        }
    }
}
