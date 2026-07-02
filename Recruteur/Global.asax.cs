using JF.DAL.Context;
using JF.DAL.Security;
using Newtonsoft.Json;
using Recruteur.Models;
using System;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;

namespace Recruteur
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
            if (ticket == null || string.IsNullOrEmpty(ticket.UserData)) return;

            CustomRecruteurModel model;
            try { model = JsonConvert.DeserializeObject<CustomRecruteurModel>(ticket.UserData); }
            catch { return; }
            if (model == null) return;

            var principal = new CustomPrincipal(ticket.Name)
            {
                Id = model.id,
                FirstName = model.prenom,
                LastName = model.nom,
                RecruiterId = model.idRecruteur,
                Roles = (model.roles != null ? model.roles.ToArray() : new string[0])
            };
            context.User = principal;
        }

        // Session state is not available until AcquireRequestState, so the session
        // cache rebuild (for an expired session while the auth cookie is still valid)
        // must happen here rather than in PostAuthenticateRequest.
        protected void Application_AcquireRequestState(object sender, EventArgs e)
        {
            var context = HttpContext.Current;
            if (context == null)
                return;

            if (!(context.User is CustomPrincipal principal) || principal.Id <= 0)
                return;

            if (VarSessionRecruteur.compteRecruteur != null)
                return; // already cached this session

            using (var db = new jobzFactoryEntities())
            {
                var contact = db.Recruteur_Contact.Include(c => c.Recruteur)
                                                   .FirstOrDefault(c => c.id == principal.Id);
                if (contact != null)
                {
                    contact.motPasse = null;
                    VarSessionRecruteur.compteRecruteur = contact;
                }
            }
        }
    }
}
