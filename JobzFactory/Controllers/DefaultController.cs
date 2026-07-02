using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;
using PagedList.Mvc;

namespace JobzFactory.Controllers
{
    public class DefaultController : Controller
    {
        JF.DAL.Context.jobzFactoryEntities db = new JF.DAL.Context.jobzFactoryEntities();

        // GET: Default
        public ActionResult Index(int? page, string search, int? city, int? sector)
        {
            int pageNumber = page ?? 1;
            const int pageSize = 6;

            var query = db.Offre
                .Where(o => o.isPublie)
                .Include(o => o.Ville)
                .Include(o => o.Ville.Pays)
                .Include(o => o.Recruteur)
                .Include(o => o.SecteurActivite);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(o => o.titreOffre.Contains(term));
            }

            if (city.HasValue && city.Value > 0)
            {
                var cityId = city.Value;
                query = query.Where(o => o.C_idVille.HasValue && o.C_idVille.Value == cityId);
            }

            if (sector.HasValue && sector.Value > 0)
            {
                var sectorId = sector.Value;
                query = query.Where(o => o.C_idSecteurActivite.HasValue && o.C_idSecteurActivite.Value == sectorId);
            }

            var list = query
                .OrderByDescending(o => o.id)
                .ToPagedList(pageNumber, pageSize);

            ViewBag.Search = search;
            ViewBag.City = city;
            ViewBag.Sector = sector;
            ViewBag.CityFilter = new SelectList(
                db.Ville.OrderBy(v => v.ville1),
                "id",
                "ville1",
                city);
            ViewBag.SectorFilter = new SelectList(
                db.SecteurActivite.OrderBy(s => s.SecteurActivite1),
                "id",
                "SecteurActivite1",
                sector);

            if (Request.IsAjaxRequest()
                || string.Equals(Request.Headers["X-Requested-With"], "XMLHttpRequest", StringComparison.OrdinalIgnoreCase))
            {
                return PartialView("_JobList", list);
            }

            return View(list);
        }

        // Generic error landing page for customErrors.
        [AllowAnonymous]
        public ActionResult Error()
        {
            return View();
        }

        // 404 landing page for customErrors.
        [AllowAnonymous]
        public ActionResult NotFound()
        {
            Response.StatusCode = 404;
            return View();
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