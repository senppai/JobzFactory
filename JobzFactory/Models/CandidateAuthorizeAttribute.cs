using JF.DAL.Security;

namespace JobzFactory.Models
{
    /// <summary>
    /// Candidate (job seeker) role gate for the public portal. Derives from the shared
    /// RoleAuthorizeAttribute so unauthenticated users are sent to the forms loginUrl
    /// (/login) and authenticated users without the Candidate role are sent to
    /// Account/Login. Role checks use the CustomPrincipal populated in Global.asax.
    /// </summary>
    public class CandidateAuthorizeAttribute : RoleAuthorizeAttribute
    {
        public CandidateAuthorizeAttribute()
        {
            Roles = "Candidate";
            AccessDeniedController = "Account";
            AccessDeniedAction = "Login";
        }
    }
}
