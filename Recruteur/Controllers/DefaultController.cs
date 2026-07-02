using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using JF.DAL.Context;
using Recruteur.Models;

namespace Recruteur.Controllers
{
    [CustomAuthorize(Roles = "RecruteurAdmin")]
    public class DefaultController : Controller
    {
        jobzFactoryEntities db = new jobzFactoryEntities();
        // GET: Default — redirect legacy URL to dashboard
        public ActionResult Index()
        {
            return RedirectToAction("TableauBord");
        }

        // GET: Default/TableauBord
        public ActionResult TableauBord()
        {
            var recruiterId = VarSessionRecruteur.compteRecruteur.C_idRecruteur.Value;
            var contact = VarSessionRecruteur.compteRecruteur;

            var offers = db.Offre
                .Include(o => o.Ville)
                .Include(o => o.Offre_Postuler)
                .Where(o => o.C_idRecruteur == recruiterId)
                .ToList();

            var applications = db.Offre_Postuler
                .Include(a => a.Profil)
                .Include(a => a.Offre)
                .Where(a => a.Offre.C_idRecruteur == recruiterId)
                .OrderByDescending(a => a.DatePostuler)
                .Take(6)
                .ToList();

            var model = new DashboardViewModel
            {
                RecruiterName = contact.prenom + " " + contact.nom,
                TotalOffers = offers.Count,
                PublishedOffers = offers.Count(o => o.isPublie),
                DraftOffers = offers.Count(o => !o.isPublie),
                TotalApplications = offers.Sum(o => o.Offre_Postuler.Count),
                RecentOffers = offers
                    .OrderByDescending(o => o.id)
                    .Take(5)
                    .Select(o => new DashboardOfferItem
                    {
                        Id = o.id,
                        Title = o.titreOffre,
                        City = o.Ville != null ? o.Ville.ville1 : "—",
                        IsPublished = o.isPublie,
                        ApplicationCount = o.Offre_Postuler.Count
                    })
                    .ToList(),
                RecentApplications = applications
                    .Select(a => new DashboardApplicationItem
                    {
                        Id = a.id,
                        CandidateName = a.Profil.prenom + " " + a.Profil.nom,
                        OfferTitle = a.Offre.titreOffre,
                        OfferId = a.C_idOffre,
                        AppliedAt = a.DatePostuler
                    })
                    .ToList()
            };

            return View(model);
        }

        // GET: Default/introuvable
        [AllowAnonymous]
        public ActionResult introuvable()
        {
            return View();
        }


        // GET: Default/AccessDenied
        [AllowAnonymous]
        public ActionResult AccessDenied()
        {
            return View();
        }

    }
}