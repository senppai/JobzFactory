using JF.DAL.Security;
using System;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;

namespace Administration
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);
        }

        protected void Application_BeginRequest()
        {
            Response.ContentEncoding = Encoding.UTF8;
            Response.HeaderEncoding = Encoding.UTF8;
        }

        protected void Application_PostAuthenticateRequest(object sender, EventArgs e)
        {
            var context = HttpContext.Current;
            if (context == null || !context.Request.IsAuthenticated)
                return;

            var cookie = context.Request.Cookies[FormsAuthentication.FormsCookieName];
            if (cookie == null) return;

            FormsAuthenticationTicket ticket;
            try { ticket = FormsAuthentication.Decrypt(cookie.Value); }
            catch { return; }
            if (ticket == null) return;

            var roles = (ticket.UserData ?? string.Empty)
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            context.User = new CustomPrincipal(ticket.Name)
            {
                Roles = roles
            };
        }
    }
}
