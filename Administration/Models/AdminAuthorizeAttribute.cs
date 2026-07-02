using JF.DAL.Security;

namespace Administration.Models
{
    /// <summary>
    /// Administration portal role gate. Unauthenticated users go to the forms
    /// loginUrl (~/Account/Login); authenticated users without the Admin role
    /// go to Account/AccessDenied.
    /// </summary>
    public class AdminAuthorizeAttribute : RoleAuthorizeAttribute
    {
        public AdminAuthorizeAttribute()
        {
            AccessDeniedController = "Account";
            AccessDeniedAction = "AccessDenied";
        }
    }
}
