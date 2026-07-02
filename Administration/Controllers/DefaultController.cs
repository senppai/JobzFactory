using System.Linq;
using System.Web.Mvc;
using Administration.Models;
using JF.DAL.Context;

namespace Administration.Controllers
{
    [AdminAuthorize(Roles = "Admin")]
    public class DefaultController : Controller
    {
        public ActionResult Index()
        {
            using (var db = new jobzFactoryEntities())
            {
                ViewBag.OfferCount = db.Offre.Count();
                ViewBag.PublishedCount = db.Offre.Count(o => o.isPublie);
                ViewBag.RecruiterCount = db.Recruteur.Count();
            }

            return View();
        }
    }
}