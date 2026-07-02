using System;
using System.Linq;
using System.Security.Principal;

namespace JF.DAL.Security
{
    /// <summary>
    /// Custom IPrincipal populated from the FormsAuthentication ticket userData
    /// (set in Global.asax Application_PostAuthenticateRequest). Carries the
    /// user's roles so [Authorize(Roles=...)] / RoleAuthorizeAttribute work.
    /// </summary>
    [Serializable]
    public class CustomPrincipal : IPrincipal
    {
        public IIdentity Identity { get; private set; }

        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int? RecruiterId { get; set; }
        public string[] Roles { get; set; } = new string[0];

        public CustomPrincipal(string username)
        {
            this.Identity = new GenericIdentity(username);
        }

        public bool IsInRole(string role)
        {
            if (Roles == null) return false;
            // role may be a comma-separated list from [Authorize(Roles="A,B")].
            var wanted = role.Split(',').Select(r => r.Trim()).Where(r => r.Length > 0);
            return wanted.Any(w => Roles.Contains(w, StringComparer.OrdinalIgnoreCase));
        }
    }
}
