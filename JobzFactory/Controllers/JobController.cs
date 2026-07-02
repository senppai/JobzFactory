using JF.DAL.Context;
using JF.DAL.Security;
using JobzFactory.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Data.Entity;
using System.Web.Mvc;


namespace JobzFactory.Controllers
{
    public class JobController : Controller
    {

        JF.DAL.Context.jobzFactoryEntities db = new JF.DAL.Context.jobzFactoryEntities();

        private static readonly string[] AllowedCvExtensions = { ".pdf", ".doc", ".docx" };
        private const int MaxCvBytes = 10 * 1024 * 1024; // 10 MB

        // GET: Job/Detail
        public ActionResult Detail(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return RedirectToAction("Index", "Default");
            }

            var obj = db.Offre
                .Include(a => a.Recruteur)
                .Include(a => a.Ville)
                .Include(a => a.Ville.Pays)
                .Include(a => a.SecteurActivite)
                .FirstOrDefault(a => a.url == url);

            if (obj == null)
            {
                return HttpNotFound();
            }

            return View(obj);
        }

        // GET: Job/Apply
        [CandidateAuthorize]
        public ActionResult Apply(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return RedirectToAction("Index", "Default");
            }

            var obj = db.Offre.FirstOrDefault(a => a.url == url);

            if (obj == null)
            {
                return HttpNotFound();
            }

            var profil = GetCurrentProfil();

            // Require a verified email before applying (titre is cleared on activation).
            if (profil != null && !string.IsNullOrEmpty(profil.titre))
            {
                return RedirectToAction("Index", "Profile", new { needVerify = 1 });
            }

            PostulerModel model = new PostulerModel();
            model.titreOffre = obj.titreOffre;
            model.idOffre = obj.id;
            model.offre = obj.message;
            model.url = obj.url;
            model.adresseMailAPostuler = obj.adresseMailAPostuler;

            // Prefill from the authenticated candidate's profile.
            if (profil != null)
            {
                model.adresseMail = profil.adresseMail;
                model.nom = profil.nom;
                model.prenom = profil.prenom;
                model.gsm = profil.gsm;
                model.tel = profil.Tel;
                ViewBag.SavedCvs = profil.Profil_CV.OrderByDescending(c => c.DateCreation).ToList();
                ViewBag.ProfilId = profil.id;
            }

            TempData["AMP"] = model.adresseMailAPostuler;
            return View(model);
        }

        // POST: Job/Apply
        // To protect from overposting attacks, please enable the specific properties you want to bind to for
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [CandidateAuthorize]
        public ActionResult Apply(PostulerModel model)
        {
            var offre = db.Offre.FirstOrDefault(o => o.id == model.idOffre);
            if (offre == null)
            {
                return HttpNotFound();
            }

            // Only allow applications to offers that are currently published.
            if (!offre.isPublie)
            {
                ModelState.AddModelError("", "This offer is no longer accepting applications.");
            }

            var profil = GetCurrentProfil();
            if (profil == null)
            {
                // Not authenticated -> CandidateAuthorize should have redirected already.
                return RedirectToAction("Login", "Account");
            }

            if (!string.IsNullOrEmpty(profil.titre))
            {
                return RedirectToAction("Index", "Profile", new { needVerify = 1 });
            }

            model.titreOffre = offre.titreOffre;
            model.url = offre.url;
            model.offre = offre.message;
            model.adresseMailAPostuler = offre.adresseMailAPostuler;
            ViewBag.SavedCvs = profil.Profil_CV.OrderByDescending(c => c.DateCreation).ToList();
            ViewBag.ProfilId = profil.id;

            // Accept either a fresh upload or a previously saved CV from the library.
            Profil_CV savedCv = null;
            if (model.savedCvId.HasValue)
            {
                savedCv = db.Profil_CV.FirstOrDefault(c => c.id == model.savedCvId.Value && c.C_idProfil == profil.id);
            }

            string uploadedExt = null;
            if (model.fichieCV != null && model.fichieCV.ContentLength > 0)
            {
                if (model.fichieCV.ContentLength > MaxCvBytes)
                {
                    ModelState.AddModelError("", "Your CV must be 10 MB or smaller.");
                }
                else
                {
                    uploadedExt = Path.GetExtension(SanitizeFileName(model.fichieCV.FileName)).ToLowerInvariant();
                    if (!AllowedCvExtensions.Contains(uploadedExt))
                    {
                        ModelState.AddModelError("", "Only PDF, DOC and DOCX files are accepted.");
                    }
                }
            }
            else if (savedCv == null)
            {
                ModelState.AddModelError("", "Please upload your CV or choose one from your library.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // Always operate on the authenticated candidate's profile (no find-or-create by email).
                profil.nom = TrimToMax(model.nom, 50);
                profil.prenom = TrimToMax(model.prenom, 50);
                profil.gsm = SanitizePhone(model.gsm) ?? profil.gsm;
                db.SaveChanges();

                // Prevent duplicate applications for the same offer by the same profile.
                var alreadyApplied = db.Offre_Postuler.Any(a => a.C_idOffre == offre.id && a.C_idProfil == profil.id);
                if (alreadyApplied)
                {
                    ModelState.AddModelError("", "You have already applied to this offer.");
                    return View(model);
                }

                string safeFileName;
                var offrePostuler = new Offre_Postuler
                {
                    message = TrimToMax(model.message, 300),
                    DatePostuler = DateTime.Now,
                    C_idProfil = profil.id,
                    C_idOffre = model.idOffre
                };

                if (savedCv != null && uploadedExt == null)
                {
                    // Reuse a saved CV: copy the stored file into the application folder.
                    var storedName = profil.id + "_" + savedCv.id + savedCv.extantion;
                    var srcPath = CvStorage.GetProfilCvPath(Server, profil.id, storedName);
                    if (srcPath == null)
                    {
                        ModelState.AddModelError("", "The selected CV could not be found. Please upload it again.");
                        return View(model);
                    }
                    safeFileName = SanitizeFileName(savedCv.nomCV);
                    offrePostuler.nomCV = TrimToMax(safeFileName, 300);
                    offrePostuler.extantion = TrimToMax(savedCv.extantion, 5);
                    db.Offre_Postuler.Add(offrePostuler);
                    db.SaveChanges();

                    var dossier = CvStorage.GetApplicationFolder(Server, offrePostuler.id, create: true);
                    System.IO.File.Copy(srcPath, Path.Combine(dossier, safeFileName), overwrite: true);
                }
                else
                {
                    safeFileName = SanitizeFileName(model.fichieCV.FileName);
                    var fileExtension = Path.GetExtension(safeFileName);
                    offrePostuler.nomCV = TrimToMax(safeFileName, 300);
                    offrePostuler.extantion = TrimToMax(fileExtension, 5);
                    db.Offre_Postuler.Add(offrePostuler);
                    db.SaveChanges();

                    var dossier = CvStorage.GetApplicationFolder(Server, offrePostuler.id, create: true);
                    model.fichieCV.SaveAs(Path.Combine(dossier, safeFileName));
                }
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex)
            {
                var details = string.Join(" ", ex.EntityValidationErrors
                    .SelectMany(e => e.ValidationErrors)
                    .Select(e => e.PropertyName + ": " + e.ErrorMessage));
                ModelState.AddModelError("", "Could not save your application. " + details);
                return View(model);
            }

            TempData["SendMessage"] = true;
            return RedirectToAction("SendApply");
        }

        private Profil GetCurrentProfil()
        {
            var id = (HttpContext.User as CustomPrincipal)?.Id ?? 0;
            if (id <= 0) return null;
            return db.Profil.Include(p => p.Profil_CV).FirstOrDefault(p => p.id == id);
        }

        private static string TrimToMax(string value, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return value;
            }

            value = value.Trim();
            return value.Length > maxLength ? value.Substring(0, maxLength) : value;
        }

        private static string SanitizeFileName(string fileName)
        {
            var name = Path.GetFileName(fileName);
            if (string.IsNullOrWhiteSpace(name))
            {
                return "cv.pdf";
            }

            foreach (var invalidChar in Path.GetInvalidFileNameChars())
            {
                name = name.Replace(invalidChar, '_');
            }

            return name.Replace(' ', '_');
        }

        private static string SanitizePhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
            {
                return null;
            }

            var digits = new string(phone.Where(char.IsDigit).ToArray());
            if (digits.Length == 0)
            {
                return null;
            }

            if (digits.Length > 12)
            {
                digits = digits.Substring(digits.Length - 12);
            }

            return digits;
        }

        // GET: Default/Edit/5
        public ActionResult SendApply()
        {
            var message = TempData["SendMessage"] as bool? ?? false;

            if (message)
            {
                return View();
            }
            return RedirectToAction("Index", "Default");
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
