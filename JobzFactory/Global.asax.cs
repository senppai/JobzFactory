using JF.DAL.Security;
using JobzFactory.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;

namespace JobzFactory
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

            var path = Request.Url.AbsolutePath;
            if (path.Length > 1 && path.EndsWith("/") &&
                (path.IndexOf("/Ressource/CV/OffrePostule/", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                 path.IndexOf("/job/cv/", System.StringComparison.OrdinalIgnoreCase) >= 0))
            {
                var trimmed = path.TrimEnd('/');
                var query = Request.Url.Query;
                Response.Redirect(trimmed + query, endResponse: true);
            }
        }

        // Rebuild the CustomPrincipal from the forms ticket on every request so that
        // [CandidateAuthorize] / User.IsInRole("Candidate") work without session state.
        protected void Application_PostAuthenticateRequest()
        {
            var cookieName = FormsAuthentication.FormsCookieName;
            var authCookie = Request.Cookies[cookieName];
            if (authCookie == null || string.IsNullOrEmpty(authCookie.Value))
            {
                return;
            }

            FormsAuthenticationTicket ticket;
            try
            {
                ticket = FormsAuthentication.Decrypt(authCookie.Value);
            }
            catch
            {
                return;
            }

            if (ticket == null || ticket.Expired || string.IsNullOrEmpty(ticket.UserData))
            {
                return;
            }

            CustomCandidateModel serializeModel;
            try
            {
                serializeModel = JsonConvert.DeserializeObject<CustomCandidateModel>(ticket.UserData);
            }
            catch
            {
                return;
            }

            if (serializeModel == null) return;

            var principal = new CustomPrincipal(ticket.Name)
            {
                Id = serializeModel.id,
                FirstName = serializeModel.prenom,
                LastName = serializeModel.nom,
                Roles = (serializeModel.roles ?? new List<string>()).ToArray()
            };

            HttpContext.Current.User = principal;
            System.Threading.Thread.CurrentPrincipal = principal;
        }
    }
}
