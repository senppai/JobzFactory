using JF.DAL.Context;
using JF.DAL.Security;
using JobzFactory.Models;
using System.Configuration;
using System.IO;
using System.Web.Mvc;

namespace JobzFactory.Controllers
{
    public class RessourceController : Controller
    {
        private readonly jobzFactoryEntities db = new jobzFactoryEntities();

        // GET: /job/cv/2?t=<signed token>  or  /Ressource/CV/OffrePostule/2?t=<token>
        // CVs are only served when the request carries a valid HMAC token issued by an
        // authorized portal (Recruteur/Admin). This blocks sequential-id enumeration.
        public ActionResult ApplicationCv(int id, string t)
        {
            var secret = ConfigurationManager.AppSettings["CvTokenSecret"];
            if (string.IsNullOrEmpty(secret) || !CvToken.Validate(t, secret, id))
            {
                return HttpNotFound();
            }

            var application = db.Offre_Postuler.Find(id);
            if (application == null || string.IsNullOrWhiteSpace(application.nomCV))
            {
                return HttpNotFound();
            }

            var filePath = CvStorage.ResolveApplicationCvPath(Server, id, application.nomCV);
            if (filePath == null)
            {
                return HttpNotFound();
            }

            var mimeType = GetMimeType(application.extantion ?? Path.GetExtension(application.nomCV));
            return File(filePath, mimeType);
        }

        private static string GetMimeType(string extension)
        {
            switch ((extension ?? string.Empty).ToLowerInvariant())
            {
                case ".pdf":
                    return "application/pdf";
                case ".doc":
                    return "application/msword";
                case ".docx":
                    return "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                default:
                    return "application/octet-stream";
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
