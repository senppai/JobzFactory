using System.Web.Mvc;

namespace JF.DAL.Security
{
    /// <summary>
    /// AuthorizeAttribute that distinguishes "not logged in" (redirect to forms
    /// loginUrl) from "logged in but missing role" (redirect to AccessDenied).
    /// Role checks use CustomPrincipal.IsInRole, populated from the auth ticket.
    /// </summary>
    public class RoleAuthorizeAttribute : AuthorizeAttribute
    {
        public string AccessDeniedController { get; set; } = "Default";
        public string AccessDeniedAction { get; set; } = "AccessDenied";

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            if (filterContext.HttpContext.User != null && filterContext.HttpContext.User.Identity.IsAuthenticated)
            {
                filterContext.Result = new RedirectToRouteResult(
                    new System.Web.Routing.RouteValueDictionary(
                        new { controller = AccessDeniedController, action = AccessDeniedAction }));
            }
            else
            {
                // Not authenticated -> let forms auth redirect to the configured loginUrl.
                base.HandleUnauthorizedRequest(filterContext);
            }
        }
    }
}
