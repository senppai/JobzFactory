using JF.DAL.Context;
using JF.DAL.Security;
using JobzFactory.Models;
using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace JobzFactory.Controllers
{
    // Every action in this controller requires an authenticated Candidate.
    [CandidateAuthorize]
    public class ProfileController : Controller
    {
        private readonly jobzFactoryEntities db = new jobzFactoryEntities();

        private static readonly string[] AllowedCvExtensions = { ".pdf", ".doc", ".docx" };
        private const int MaxCvBytes = 10 * 1024 * 1024; // 10 MB

        private int CurrentProfilId => (HttpContext.User as CustomPrincipal)?.Id ?? 0;

        private Profil LoadProfil()
        {
            return db.Profil
                .Include(p => p.Ville)
                .Include(p => p.SecteurActivite)
                .FirstOrDefault(p => p.id == CurrentProfilId);
        }

        // GET: /profile
        public ActionResult Index()
        {
            var profil = LoadProfil();
            if (profil == null)
            {
                FormsAuthenticationWrapper.SignOut();
                return RedirectToAction("Login", "Account");
            }

            var appliedOffreIds = db.Offre_Postuler
                .Where(a => a.C_idProfil == profil.id)
                .Select(a => a.C_idOffre)
                .ToList();

            ViewBag.AppliedCount = appliedOffreIds.Count;
            ViewBag.CvCount = profil.Profil_CV.Count;
            ViewBag.IsVerified = string.IsNullOrEmpty(profil.titre);
            ViewBag.NeedVerify = Request.QueryString["needVerify"] == "1";

            ViewBag.RecentApplications = db.Offre_Postuler
                .Include(a => a.Offre.Recruteur)
                .Include(a => a.Offre.Ville.Pays)
                .Where(a => a.C_idProfil == profil.id)
                .OrderByDescending(a => a.DatePostuler)
                .Take(5)
                .ToList();

            // Matched offers: published offers matching the candidate's sector or city,
            // excluding offers already applied to. Falls back to recent published offers.
            var matchedQuery = db.Offre
                .Include(o => o.Recruteur)
                .Include(o => o.Ville.Pays)
                .Include(o => o.SecteurActivite)
                .Where(o => o.isPublie && !appliedOffreIds.Contains(o.id));

            var hasSector = profil.C_idSecteur.HasValue;
            var hasVille = profil.C_idVille.HasValue;

            if (hasSector || hasVille)
            {
                var sectorId = profil.C_idSecteur ?? 0;
                var villeId = profil.C_idVille ?? 0;
                matchedQuery = matchedQuery.Where(o =>
                    (hasSector && o.C_idSecteurActivite == sectorId) ||
                    (hasVille && o.C_idVille == villeId));
            }

            ViewBag.MatchedOffers = matchedQuery
                .OrderByDescending(o => o.id)
                .Take(6)
                .ToList();

            return View(profil);
        }

        // GET: /profile/applications
        public ActionResult Applications()
        {
            var profil = LoadProfil();
            if (profil == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var applications = db.Offre_Postuler
                .Include(a => a.Offre.Recruteur)
                .Include(a => a.Offre.Ville.Pays)
                .Include(a => a.Offre.SecteurActivite)
                .Where(a => a.C_idProfil == profil.id)
                .OrderByDescending(a => a.DatePostuler)
                .ToList();

            return View(applications);
        }

        // GET: /profile/edit
        public ActionResult Edit()
        {
            var profil = LoadProfil();
            if (profil == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var model = new EditProfileViewModel
            {
                prenom = profil.prenom,
                nom = profil.nom,
                dateNaissance = profil.dateNaissance,
                gsm = profil.gsm,
                Tel = profil.Tel,
                adresse = profil.adresse,
                C_idVille = profil.C_idVille,
                C_idSecteur = profil.C_idSecteur
            };

            PopulateEditSelectLists(model);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(EditProfileViewModel model)
        {
            var profil = LoadProfil();
            if (profil == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (!ModelState.IsValid)
            {
                PopulateEditSelectLists(model);
                return View(model);
            }

            profil.prenom = model.prenom;
            profil.nom = model.nom;
            profil.dateNaissance = model.dateNaissance;
            profil.gsm = model.gsm;
            profil.Tel = model.Tel;
            profil.adresse = model.adresse;
            profil.C_idVille = model.C_idVille;
            profil.C_idSecteur = model.C_idSecteur;

            db.SaveChanges();
            TempData["ProfileSaved"] = true;
            return RedirectToAction("Index");
        }

        // POST: /profile/changepassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(ChangePasswordViewModel model)
        {
            var profil = LoadProfil();
            if (profil == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (!ModelState.IsValid)
            {
                TempData["PasswordError"] = "Please correct the errors and try again.";
                return RedirectToAction("Edit");
            }

            if (!PasswordHasher.Verify(profil.motPasse, model.ancienMotPasse, out _))
            {
                TempData["PasswordError"] = "Your current password is incorrect.";
                return RedirectToAction("Edit");
            }

            profil.motPasse = PasswordHasher.Hash(model.nouveauMotPasse);
            db.SaveChanges();
            TempData["PasswordSaved"] = true;
            return RedirectToAction("Index");
        }

        // POST: /profile/cv/upload
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UploadCv(HttpPostedFileBase file)
        {
            var profil = LoadProfil();
            if (profil == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (file == null || file.ContentLength == 0)
            {
                TempData["CvError"] = "Please choose a CV file to upload.";
                return RedirectToAction("Index");
            }

            if (file.ContentLength > MaxCvBytes)
            {
                TempData["CvError"] = "Your CV must be 10 MB or smaller.";
                return RedirectToAction("Index");
            }

            var ext = Path.GetExtension(SanitizeFileName(file.FileName)).ToLowerInvariant();
            if (!AllowedCvExtensions.Contains(ext))
            {
                TempData["CvError"] = "Only PDF, DOC and DOCX files are accepted.";
                return RedirectToAction("Index");
            }

            var cvRow = new Profil_CV
            {
                C_idProfil = profil.id,
                DateCreation = DateTime.Now,
                nomCV = TrimToMax(SanitizeFileName(file.FileName), 300),
                extantion = TrimToMax(ext, 5)
            };
            db.Profil_CV.Add(cvRow);
            db.SaveChanges();

            // Save the file using a deterministic name {profilId}_{cvId}{ext} so the
            // Profil_CV row maps 1:1 to a file on disk (used by DownloadCv / DeleteCv / apply reuse).
            var folder = CvStorage.GetProfilCvFolder(Server, profil.id, create: true);
            var storedName = profil.id + "_" + cvRow.id + ext;
            file.SaveAs(Path.Combine(folder, storedName));

            TempData["CvSaved"] = true;
            return RedirectToAction("Index");
        }

        // POST: /profile/cv/delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteCv(int id)
        {
            var profil = LoadProfil();
            if (profil == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var cv = db.Profil_CV.FirstOrDefault(c => c.id == id && c.C_idProfil == profil.id);
            if (cv == null)
            {
                return HttpNotFound();
            }

            // The stored file on disk is named {profilId}_{cvId}{ext} to keep names unique.
            var storedName = profil.id + "_" + cv.id + cv.extantion;
            var path = CvStorage.GetProfilCvPath(Server, profil.id, storedName);
            if (path != null && System.IO.File.Exists(path))
            {
                System.IO.File.Delete(path);
            }

            db.Profil_CV.Remove(cv);
            db.SaveChanges();

            TempData["CvDeleted"] = true;
            return RedirectToAction("Index");
        }

        // GET: /profile/cv/download/{id}
        public ActionResult DownloadCv(int id)
        {
            var profil = LoadProfil();
            if (profil == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var cv = db.Profil_CV.FirstOrDefault(c => c.id == id && c.C_idProfil == profil.id);
            if (cv == null)
            {
                return HttpNotFound();
            }

            var storedName = profil.id + "_" + cv.id + cv.extantion;
            var path = CvStorage.GetProfilCvPath(Server, profil.id, storedName);
            if (path == null)
            {
                return HttpNotFound();
            }

            return File(path, MimeMappingForCv(cv.extantion), cv.nomCV);
        }

        private void PopulateEditSelectLists(EditProfileViewModel model)
        {
            ViewBag.Villes = new SelectList(db.Ville.OrderBy(v => v.ville1).ToList(), "id", "ville1", model.C_idVille);
            ViewBag.Secteurs = new SelectList(db.SecteurActivite.OrderBy(s => s.SecteurActivite1).ToList(), "id", "SecteurActivite1", model.C_idSecteur);
        }

        private static string SanitizeFileName(string fileName)
        {
            var name = Path.GetFileName(fileName);
            if (string.IsNullOrWhiteSpace(name)) return "cv.pdf";
            foreach (var invalidChar in Path.GetInvalidFileNameChars())
            {
                name = name.Replace(invalidChar, '_');
            }
            return name.Replace(' ', '_');
        }

        private static string TrimToMax(string value, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(value)) return value;
            value = value.Trim();
            return value.Length > maxLength ? value.Substring(0, maxLength) : value;
        }

        private static string MimeMappingForCv(string ext)
        {
            if (string.IsNullOrEmpty(ext)) return "application/octet-stream";
            switch (ext.ToLowerInvariant())
            {
                case ".pdf": return "application/pdf";
                case ".doc": return "application/msword";
                case ".docx": return "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                default: return "application/octet-stream";
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }

    // Thin wrapper so controllers can sign out without referencing System.Web.Security
    // in every helper. Keeps the auth flow in one place.
    internal static class FormsAuthenticationWrapper
    {
        public static void SignOut() => System.Web.Security.FormsAuthentication.SignOut();
    }
}
