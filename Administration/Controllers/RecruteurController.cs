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
    public class RecruteurController : Controller
    {
        private jobzFactoryEntities db = new jobzFactoryEntities();

        // GET: Recruteur
        public async Task<ActionResult> Index()
        {
            var recruteurs = db.Recruteur.OrderByDescending(r => r.dateCreation);
            return View(await recruteurs.ToListAsync());
        }

        // GET: Recruteur/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Recruteur recruteur = await db.Recruteur.FindAsync(id);
            if (recruteur == null)
            {
                return HttpNotFound();
            }
            return View(recruteur);
        }

        // GET: Recruteur/Create
        public ActionResult Create()
        {
            var m = new Recruteur { dateCreation = DateTime.Now };
            return View(m);
        }

        // POST: Recruteur/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "nomRecruteur,siteweb,adresseMail,url,description,adresseSocial")] Recruteur recruteur)
        {
            if (ModelState.IsValid)
            {
                recruteur.dateCreation = DateTime.Now;
                db.Recruteur.Add(recruteur);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(recruteur);
        }

        // GET: Recruteur/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Recruteur recruteur = await db.Recruteur.FindAsync(id);
            if (recruteur == null)
            {
                return HttpNotFound();
            }
            return View(recruteur);
        }

        // POST: Recruteur/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "id,nomRecruteur,siteweb,adresseMail,url,description,adresseSocial")] Recruteur recruteur)
        {
            if (ModelState.IsValid)
            {
                var existing = await db.Recruteur.FindAsync(recruteur.id);
                if (existing == null)
                {
                    return HttpNotFound();
                }

                existing.nomRecruteur = recruteur.nomRecruteur;
                existing.siteweb = recruteur.siteweb;
                existing.adresseMail = recruteur.adresseMail;
                existing.url = recruteur.url;
                existing.description = recruteur.description;
                existing.adresseSocial = recruteur.adresseSocial;
                // dateCreation is preserved (not editable).
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(recruteur);
        }

        // GET: Recruteur/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Recruteur recruteur = await db.Recruteur.FindAsync(id);
            if (recruteur == null)
            {
                return HttpNotFound();
            }
            return View(recruteur);
        }

        // POST: Recruteur/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            Recruteur recruteur = await db.Recruteur.FindAsync(id);
            if (recruteur == null)
            {
                return HttpNotFound();
            }
            db.Recruteur.Remove(recruteur);
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
