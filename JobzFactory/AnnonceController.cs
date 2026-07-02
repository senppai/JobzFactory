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
        public async Task<ActionResult> Index(int? id)
        {
            int? idRecruetur = VarSessionRecruteur.compteRecruteur.C_idRecruteur;
            var offre = db.Offre.Where(a => a.C_idRecruteur == idRecruetur).Include(o => o.Recruteur).Include(o => o.Ville).Include(o => o.SecteurActivite);
            
            
            return View(await offre.ToListAsync());
        }

        
        public async Task<ActionResult> History(int? page)
        {
            int PageNumber = page ?? 1;

            int PageCount = 6;

            int? idRecruetur = VarSessionRecruteur.compteRecruteur.C_idRecruteur;

            var offre_history = db.Offre_Historique.Where(a => a.C_idUtilisateur == idRecruetur).Include(o => o.ActionOffre).OrderByDescending(o => o.dateAction).ToPagedList(PageNumber, PageCount);
            return View(offre_history);
            //return View(await offre_history.ToListAsync());
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
                    string RandomURL = Guid.NewGuid().ToString();

                    //offre.dateCreation = DateTime.Now;
                    offre.isPublie = false;
                    offre.C_idRecruteur = VarSessionRecruteur.compteRecruteur.C_idRecruteur.Value;
                    offre.isConfidentiel = false;
                    string titre=offre.titreOffre.Replace(" ", "_");
                    offre.url = titre+"_" + RandomURL;
                    db.Offre.Add(offre);
                    await db.SaveChangesAsync();
                    JF.DAL.Context.Offre_Historique History = new JF.DAL.Context.Offre_Historique();
                    History.C_idOffre = offre.id;
                    History.dateAction = DateTime.Now;
                    History.C_idTypeAction = 1;
                    History.C_idUtilisateur = VarSessionRecruteur.compteRecruteur.C_idRecruteur.Value;
                    History.commentaire = null;
                    db.Offre_Historique.Add(History);
                    db.SaveChanges();
                    //await db.SaveChangesAsync();

                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    var a = ex;
                    var me = a.Message;

                }
       
            }
            ViewBag.C_idRecruteur = new SelectList(db.Recruteur, "id", "nomRecruteur", offre.C_idRecruteur);
            ViewBag.C_idVille = new SelectList(db.Ville, "id", "ville1", offre.C_idVille);
            ViewBag.C_idSecteurActivite = new SelectList(db.SecteurActivite, "id", "SecteurActivite1", offre.C_idSecteurActivite);
            return View(offre);
        }

        public ActionResult validate(int id)
        {
            JF.DAL.Context.Offre offre = db.Offre.Single(x => x.id == id);

            offre.isPublie = true;
            db.SaveChanges() ;

            JF.DAL.Context.Offre_Historique History = new JF.DAL.Context.Offre_Historique();
            History.C_idOffre = offre.id;
            History.dateAction = DateTime.Now;
            History.C_idTypeAction = 2;
            History.C_idUtilisateur = VarSessionRecruteur.compteRecruteur.C_idRecruteur.Value;
            History.commentaire = null;
            db.Offre_Historique.Add(History);
            db.SaveChanges();
            return RedirectToAction("index");
        }

         public ActionResult Approve(int id)
         {
         JF.DAL.Context.Offre offre = db.Offre.Single(x => x.id == id);

         //offre.DateValidation = DateTime.Now;
         //offre.C_idUtilisateurValide = VarSessionRecruteur.compteRecruteur.C_idRecruteur.Value;
         offre.isPublie = true;
         db.SaveChanges();

         JF.DAL.Context.Offre_Historique History = new JF.DAL.Context.Offre_Historique();
         History.C_idOffre = offre.id;
         History.dateAction = DateTime.Now;
         History.C_idTypeAction = 4;
         History.C_idUtilisateur = VarSessionRecruteur.compteRecruteur.C_idRecruteur.Value;
         History.commentaire = null;
         db.Offre_Historique.Add(History);
         db.SaveChanges();

         return RedirectToAction("index");
         }
        public ActionResult disable(int id)
        {
            JF.DAL.Context.Offre offre = db.Offre.Single(x => x.id == id);

            //offre.C_idUtilisateurValide = null;
            offre.isPublie = false;
            offre.url = null;
            db.SaveChanges();

            JF.DAL.Context.Offre_Historique History = new JF.DAL.Context.Offre_Historique();
            History.C_idOffre = offre.id;
            History.dateAction = DateTime.Now;
            History.C_idTypeAction = 3;
            History.C_idUtilisateur = VarSessionRecruteur.compteRecruteur.C_idRecruteur.Value;
            History.commentaire = null;
            db.Offre_Historique.Add(History);
            db.SaveChanges();
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
                .Include(a => a.Offre_Postuler)
                .Include(b=>b.Offre_Historique)
                .Where(a => a.id == id)
                .FirstOrDefaultAsync()
                ;

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
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "id,message,dateCreation,C_idRecruteur,C_idVille,Etat,MotifBlocage,MessageBlocage,C_idUtilisateurValide,DateValidation,titreOffre,C_idSecteurActivite,url,isConfidentiel,adresseMailAPostuler,nomImage")] Offre offre)
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

        // GET: EspaceRecruteur/Annonce/Delete/5
        public async Task<ActionResult> Delete(int? id)
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