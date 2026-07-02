using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace JobzFactory.Controllers
{
    public class RecruteurController : Controller
    {

        JF.DAL.Context.jobzFactoryEntities db = new JF.DAL.Context.jobzFactoryEntities();

        // GET: Recruteur
        public ActionResult Index()
        {
            return View();
        }

        // GET: Recruteur/OUMDIN
        public ActionResult detail(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return HttpNotFound();
            }

            var obj = db.Recruteur.FirstOrDefault(a => a.url == url);
            if (obj == null)
            {
                return HttpNotFound();
            }
            return View(obj);
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