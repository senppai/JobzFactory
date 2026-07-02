using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web.Mvc;
using Administration.Models;
using JF.DAL.Context;

namespace Administration.Controllers
{
    [AdminAuthorize(Roles = "Admin")]
    public class OffreController : Controller
    {
        private jobzFactoryEntities db = new jobzFactoryEntities();

        // GET: Offre
        public async Task<ActionResult> Index()
        {
            var offre = db.Offre.Include(o => o.Recruteur)
                                .Include(o => o.Ville)
                                .Include(o => o.SecteurActivite)
                                .OrderByDescending(o => o.id);
            return View(await offre.ToListAsync());
        }

        // GET: Offre/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Offre offre = await db.Offre
                .Include(o => o.Recruteur)
                .Include(o => o.Ville)
                .Include(o => o.SecteurActivite)
                .FirstOrDefaultAsync(o => o.id == id);
            if (offre == null)
            {
                return HttpNotFound();
            }
            return View(offre);
        }

        // GET: Offre/Create
        public ActionResult Create()
        {
            var m = new Offre();
            ViewBag.C_idRecruteur = new SelectList(db.Recruteur, "id", "nomRecruteur");
            ViewBag.C_idVille = new SelectList(db.Ville, "id", "ville1");
            ViewBag.C_idSecteurActivite = new SelectList(db.SecteurActivite, "id", "SecteurActivite1");
            return View(m);
        }

        // POST: Offre/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "message,C_idRecruteur,C_idVille,isPublie,titreOffre,C_idSecteurActivite,url,isConfidentiel,adresseMailAPostuler,nomImage,numberOffre")] Offre offre)
        {
            if (ModelState.IsValid)
            {
                db.Offre.Add(offre);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.C_idRecruteur = new SelectList(db.Recruteur, "id", "nomRecruteur", offre.C_idRecruteur);
            ViewBag.C_idVille = new SelectList(db.Ville, "id", "ville1", offre.C_idVille);
            ViewBag.C_idSecteurActivite = new SelectList(db.SecteurActivite, "id", "SecteurActivite1", offre.C_idSecteurActivite);
            return View(offre);
        }

        // GET: Offre/Edit/5
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
            ViewBag.C_idRecruteur = new SelectList(db.Recruteur, "id", "nomRecruteur", offre.C_idRecruteur);
            ViewBag.C_idVille = new SelectList(db.Ville, "id", "ville1", offre.C_idVille);
            ViewBag.C_idSecteurActivite = new SelectList(db.SecteurActivite, "id", "SecteurActivite1", offre.C_idSecteurActivite);
            return View(offre);
        }

        // POST: Offre/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "id,message,C_idRecruteur,C_idVille,isPublie,titreOffre,C_idSecteurActivite,url,isConfidentiel,adresseMailAPostuler,nomImage,numberOffre")] Offre offre)
        {
            if (ModelState.IsValid)
            {
                db.Entry(offre).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.C_idRecruteur = new SelectList(db.Recruteur, "id", "nomRecruteur", offre.C_idRecruteur);
            ViewBag.C_idVille = new SelectList(db.Ville, "id", "ville1", offre.C_idVille);
            ViewBag.C_idSecteurActivite = new SelectList(db.SecteurActivite, "id", "SecteurActivite1", offre.C_idSecteurActivite);
            return View(offre);
        }

        // GET: Offre/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Offre offre = await db.Offre
                .Include(o => o.Recruteur)
                .Include(o => o.Ville)
                .Include(o => o.SecteurActivite)
                .FirstOrDefaultAsync(o => o.id == id);
            if (offre == null)
            {
                return HttpNotFound();
            }
            return View(offre);
        }

        // POST: Offre/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            Offre offre = await db.Offre.FindAsync(id);
            if (offre == null)
            {
                return HttpNotFound();
            }
            db.Offre.Remove(offre);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
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
