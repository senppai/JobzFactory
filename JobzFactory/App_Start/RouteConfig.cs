using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace JobzFactory
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "AccountReset",
                url: "account/reset/{token}",
                defaults: new { controller = "Account", action = "Reset" },
                constraints: new { token = @"[A-Za-z0-9\-]+" }
            );

            routes.MapRoute(
                name: "Account",
                url: "account/{action}",
                defaults: new { controller = "Account", action = "Login" }
            );

            routes.MapRoute(
                name: "Login",
                url: "login",
                defaults: new { controller = "Account", action = "Login" }
            );

            routes.MapRoute(
                name: "Logout",
                url: "logout",
                defaults: new { controller = "Account", action = "LogOut" }
            );

            routes.MapRoute(
                name: "Profile",
                url: "profile/{action}",
                defaults: new { controller = "Profile", action = "Index" }
            );

            routes.MapRoute(
                name: "SignUpActivate",
                url: "signup/activate/{token}",
                defaults: new { controller = "SignUp", action = "Activate" },
                constraints: new { token = @"[A-Za-z0-9\-]+" }
            );

            routes.MapRoute(
                name: "SignUp",
                url: "signup/{action}",
                defaults: new { controller = "SignUp", action = "Register" }
            );

            routes.MapRoute(
                name: "ApplicationCv",
                url: "job/cv/{id}",
                defaults: new { controller = "Ressource", action = "ApplicationCv" },
                constraints: new { id = @"\d+" }
            );

            routes.MapRoute(
                name: "ApplicationCvLegacy",
                url: "Ressource/CV/OffrePostule/{id}",
                defaults: new { controller = "Ressource", action = "ApplicationCv" },
                constraints: new { id = @"\d+" }
            );

            routes.MapRoute(
              name: "Recruteur",
              url: "Recruteur/{url}",
              defaults: new { controller = "Recruteur", action = "detail", url = UrlParameter.Optional }
          );


            routes.MapRoute(
              name: "SendApply",
              url: "job/SendApply",
              defaults: new { controller = "Job", action = "SendApply", url = UrlParameter.Optional }
          );

            routes.MapRoute(
               name: "DetailJob",
               url: "job/{url}",
               defaults: new { controller = "Job", action = "detail", url = UrlParameter.Optional }
           );

            routes.MapRoute(
              name: "Apply",
              url: "Apply/{url}",
              defaults: new { controller = "Job", action = "Apply", url = UrlParameter.Optional }
          );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Default", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
