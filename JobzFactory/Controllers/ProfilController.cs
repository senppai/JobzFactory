using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using JF.DAL.Context;

namespace JobzFactory.Controllers
{
    public class ProfilController : Controller
    {
        private jobzFactoryEntities db = new jobzFactoryEntities();

        // GET: Profil
        //public async Task<ActionResult> Index()
        //{
        //    var profil = db.Profil.Include(p => p.SecteurActivite).Include(p => p.Ville);
        //    return View(await profil.ToListAsync());
        //}

        // GET: Profil/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Profil profil = await db.Profil.FindAsync(id);
            if (profil == null)
            {
                return HttpNotFound();
            }
            return View(profil);
        }

        // GET: Profil/Create
        public ActionResult Create()
        {
            var m = new Profil();
            ViewBag.C_idSecteur = new SelectList(db.SecteurActivite, "id", "SecteurActivite1");
            ViewBag.C_idVille = new SelectList(db.Ville, "id", "ville1");
            return View(m);
        }

        // POST: Profil/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "nom,prenom,dateNaissance,adresseMail,gsm,Tel,titre,C_idVille,adresse,C_idSecteur")] Profil profil)
        {
            if (ModelState.IsValid)
            {
                profil.dateCreation = DateTime.Now;
                db.Profil.Add(profil);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.C_idSecteur = new SelectList(db.SecteurActivite, "id", "SecteurActivite1", profil.C_idSecteur);
            ViewBag.C_idVille = new SelectList(db.Ville, "id", "ville1", profil.C_idVille);
            return View(profil);
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
