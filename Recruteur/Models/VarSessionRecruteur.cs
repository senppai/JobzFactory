using JF.DAL.Context;
using System.Web;

namespace Recruteur.Models
{
    public class VarSessionRecruteur
    {

        static string userSessionVar = "compteRecruteur";
        public static Recruteur_Contact compteRecruteur
        {

            get
            {
                // Session is null during early pipeline events (e.g. PostAuthenticateRequest);
                // guard so callers in those events don't throw.
                var session = HttpContext.Current?.Session;
                if (session == null)
                    return null;

                return session[userSessionVar] as Recruteur_Contact;
            }
            set
            {
                var session = HttpContext.Current?.Session;
                if (session == null)
                    return;

                session[userSessionVar] = value;
            }

        }

    }
}