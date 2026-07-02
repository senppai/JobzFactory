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
using PagedList;
using PagedList.Mvc;

namespace Recruteur.Controllers
{
    [CustomAuthorize(Roles = "RecruteurAdmin")]
    public class AnnonceController : Controller
    {
        private jobzFactoryEntities db = new jobzFactoryEntities();

        // GET: Annonce
        public ActionResult Index(int? page, string search, string status, int? sector, int? city)
        {
            const int pageSize = 10;
            int pageNumber = page ?? 1;
            int recruiterId = VarSessionRecruteur.compteRecruteur.C_idRecruteur.Value;

            var query = db.Offre
                .Where(o => o.C_idRecruteur == recruiterId)
                .Include(o => o.Ville)
                .Include(o => o.SecteurActivite)
                .Include(o => o.Offre_Postuler);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(o => o.titreOffre.Contains(term));
            }

            if (sector.HasValue && sector.Value > 0)
            {
                var sectorId = sector.Value;
                query = query.Where(o => o.C_idSecteurActivite.HasValue && o.C_idSecteurActivite.Value == sectorId);
            }

            if (city.HasValue && city.Value > 0)
            {
                var cityId = city.Value;
                query = query.Where(o => o.C_idVille.HasValue && o.C_idVille.Value == cityId);
            }

            if (status == "live")
            {
                query = query.Where(o => o.isPublie);
            }
            else if (status == "draft")
            {
                query = query.Where(o => !o.isPublie);
            }

            var pagedOffers = query.OrderByDescending(o => o.id).ToPagedList(pageNumber, pageSize);

            ViewBag.Search = search;
            ViewBag.Status = string.IsNullOrWhiteSpace(status) ? "all" : status;
            ViewBag.Sector = sector;
            ViewBag.City = city;
            ViewBag.SectorFilter = new SelectList(
                db.SecteurActivite.OrderBy(s => s.SecteurActivite1),
                "id",
                "SecteurActivite1",
                sector);
            ViewBag.CityFilter = new SelectList(
                db.Ville.OrderBy(v => v.ville1),
                "id",
                "ville1",
                city);

            if (Request.IsAjaxRequest())
            {
                return PartialView("_OfferList", pagedOffers);
            }

            return View(pagedOffers);
        }

        
        public async Task<ActionResult> History(int? page)
        {
            int PageNumber = page ?? 1;

            int PageCount = 6;

            int? idRecruetur = VarSessionRecruteur.compteRecruteur.C_idRecruteur;

            var offre_history = db.Offre_Historique
                .Where(a => a.C_idUtilisateur == idRecruetur)
                .Include(o => o.ActionOffre)
                .Include(o => o.Offre)
                .OrderByDescending(o => o.dateAction)
                .ToPagedList(PageNumber, PageCount);
            return View(offre_history);
        }

        // GET: Annonce/Create
        public ActionResult Create()
        {
            var m = new Offre();
            int? idRecruetur = VarSessionRecruteur.compteRecruteur.C_idRecruteur;
            ViewBag.C_idVille = new SelectList(db.Ville, "id", "ville1");
            ViewBag.C_idSecteurActivite = new SelectList(db.SecteurActivite, "id", "SecteurActivite1");
            return View(m);
        }

        [HttpPost]
        [ValidateAntiForgeryToken, ValidateInput(false)]
        public async Task<ActionResult> Create(Offre offre)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    JF.DAL.Context.Codification codification = new JF.DAL.Context.Codification();
                    var dateAndTime = DateTime.Now;
                    string year = dateAndTime.Year.ToString();
                    codification.annee = year;
                    int num = db.Codification.Where(a => a.annee == year).Count();
                    codification.offre_number = num+1;
                    db.Codification.Add(codification);
                    db.SaveChanges();

                    string titre = offre.titreOffre.Replace(" ", "_");
                    //offre.dateCreation = DateTime.Now;
                    offre.isPublie = false;
                    offre.C_idRecruteur = VarSessionRecruteur.compteRecruteur.C_idRecruteur.Value;
                    offre.isConfidentiel = false;
                    offre.url = titre+"_"+ codification.offre_number + "-" + codification.annee;
                    db.Offre.Add(offre);
                    await db.SaveChangesAsync();

                    AnnonceHistoryHelper.Add(db, offre.id, AnnonceHistoryHelper.ActionCreate, "Offer created");
                    db.SaveChanges();
                    //await db.SaveChangesAsync();

                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.TraceError("AnnonceController.Create failed: {0}", ex);
                    ModelState.AddModelError("", "Could not create the offer. Please try again.");
                }
       
            }
            ViewBag.C_idRecruteur = new SelectList(db.Recruteur, "id", "nomRecruteur", offre.C_idRecruteur);
            ViewBag.C_idVille = new SelectList(db.Ville, "id", "ville1", offre.C_idVille);
            ViewBag.C_idSecteurActivite = new SelectList(db.SecteurActivite, "id", "SecteurActivite1", offre.C_idSecteurActivite);
            return View(offre);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult validate(int id)
        {
            var offre = db.Offre.FirstOrDefault(x => x.id == id);
            if (offre == null || !OwnsOffer(offre))
            {
                return HttpNotFound();
            }

            offre.isPublie = true;
            AnnonceHistoryHelper.Add(db, offre.id, AnnonceHistoryHelper.ActionPublish, "Offer published from board");
            db.SaveChanges();
            return RedirectToAction("Board", new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Approve(int id)
        {
            var offre = db.Offre.FirstOrDefault(x => x.id == id);
            if (offre == null || !OwnsOffer(offre))
            {
                return HttpNotFound();
            }

            offre.isPublie = true;
            AnnonceHistoryHelper.Add(db, offre.id, AnnonceHistoryHelper.ActionApprove, "Offer approved and published");
            db.SaveChanges();
            return RedirectToAction("index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult disable(int id, string returnTo = null)
        {
            var offre = db.Offre.FirstOrDefault(x => x.id == id);
            if (offre == null || !OwnsOffer(offre))
            {
                return HttpNotFound();
            }

            offre.isPublie = false;
            AnnonceHistoryHelper.Add(db, offre.id, AnnonceHistoryHelper.ActionUnpublish, "Offer unpublished");
            db.SaveChanges();

            if (returnTo == "board")
            {
                return RedirectToAction("Board", new { id });
            }

            return RedirectToAction("index");
        }

        // GET: EspaceRecruteur/Annonce/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Offre offre = await db.Offre.FindAsync(id);
            if (offre == null)
            {
                return HttpNotFound();
            }
            int? idRecruetur = VarSessionRecruteur.compteRecruteur.C_idRecruteur;
            if (offre.C_idRecruteur != idRecruetur.Value)
            {
                return HttpNotFound();
            }
            return View(offre);
        }
        //public ActionResult Board()
        //{
            

        //    return View();
        //}
        public async Task<ActionResult> Board(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Offre offre = await db.Offre
                .Include(o => o.Ville)
                .Include(o => o.SecteurActivite)
                .Include(o => o.Offre_Postuler.Select(p => p.Profil))
                .Include(o => o.Offre_Historique.Select(h => h.ActionOffre))
                .Where(a => a.id == id)
                .FirstOrDefaultAsync();

            if (offre == null)
            {
                return HttpNotFound();
            }
            int? idRecruetur = VarSessionRecruteur.compteRecruteur.C_idRecruteur;
            if (offre.C_idRecruteur != idRecruetur.Value)
            {
                return HttpNotFound();
            }
            

            return View(offre);
        }

        // GET: Annonce/Candidats/5
        public async Task<ActionResult> Candidats(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Offre offre = await db.Offre.Include(a => a.Offre_Postuler).Where(a => a.id == id).FirstOrDefaultAsync();
            
            if (offre == null)
            {
                return HttpNotFound();
            }
            int? idRecruetur = VarSessionRecruteur.compteRecruteur.C_idRecruteur;
            if (offre.C_idRecruteur != idRecruetur.Value)
            {
                return HttpNotFound();
            }
            return View(offre);
        }

        // GET: EspaceRecruteur/Annonce/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Offre offre = await db.Offre.FindAsync(id);
            if (offre == null)
            {
                return HttpNotFound();
            }
            int? idRecruetur = VarSessionRecruteur.compteRecruteur.C_idRecruteur;
            if (offre.C_idRecruteur != idRecruetur.Value)
            {
                return HttpNotFound();
            }
            ViewBag.C_idRecruteur = new SelectList(db.Recruteur, "id", "nomRecruteur", offre.C_idRecruteur);
            ViewBag.C_idVille = new SelectList(db.Ville, "id", "ville1", offre.C_idVille);
            ViewBag.C_idSecteurActivite = new SelectList(db.SecteurActivite, "id", "SecteurActivite1", offre.C_idSecteurActivite);
            return View(offre);
        }

        // POST: EspaceRecruteur/Annonce/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken, ValidateInput(false)]
        public async Task<ActionResult> Edit(Offre offre)
        {
            var existing = await db.Offre.FindAsync(offre.id);
            if (existing == null || !OwnsOffer(existing))
            {
                return HttpNotFound();
            }

            if (string.IsNullOrWhiteSpace(offre.titreOffre))
            {
                ModelState.AddModelError("titreOffre", "Title is required.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.C_idVille = new SelectList(db.Ville, "id", "ville1", offre.C_idVille);
                ViewBag.C_idSecteurActivite = new SelectList(db.SecteurActivite, "id", "SecteurActivite1", offre.C_idSecteurActivite);
                return View(offre);
            }

            existing.titreOffre = offre.titreOffre.Trim();
            existing.message = offre.message;
            existing.C_idVille = offre.C_idVille;
            existing.C_idSecteurActivite = offre.C_idSecteurActivite;
            existing.adresseMailAPostuler = offre.adresseMailAPostuler;

            AnnonceHistoryHelper.Add(db, existing.id, AnnonceHistoryHelper.ActionUpdate, "Offer details updated");
            await db.SaveChangesAsync();
            return RedirectToAction("Board", new { id = existing.id });
        }

        // GET: EspaceRecruteur/Annonce/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Offre offre = await db.Offre
                .Include(o => o.Ville)
                .Include(o => o.SecteurActivite)
                .Include(o => o.Offre_Postuler)
                .FirstOrDefaultAsync(o => o.id == id);

            if (offre == null)
            {
                return HttpNotFound();
            }
            if (!OwnsOffer(offre))
            {
                return HttpNotFound();
            }
            return View(offre);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            var offre = await db.Offre
                .Include(o => o.Offre_Postuler)
                .Include(o => o.Offre_Historique)
                .FirstOrDefaultAsync(o => o.id == id);

            if (offre == null || !OwnsOffer(offre))
            {
                return HttpNotFound();
            }

            // Note: Offre_Historique has a CASCADE delete to Offre, so a delete
            // history entry would be removed alongside the offer and is not written here.
            db.Offre.Remove(offre);
            await db.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        private bool OwnsOffer(Offre offre)
        {
            var sessionContact = VarSessionRecruteur.compteRecruteur;
            if (offre == null || sessionContact == null || !sessionContact.C_idRecruteur.HasValue)
                return false;

            return offre.C_idRecruteur == sessionContact.C_idRecruteur.Value;
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