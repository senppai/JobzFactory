using JF.DAL.Context;
using JF.DAL.Security;
using JobzFactory.Models;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace JobzFactory.Controllers
{
    public class SignUpController : Controller
    {
        private readonly jobzFactoryEntities db = new jobzFactoryEntities();

        // GET: /signup
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(SignUpModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var email = (model.adresseMail ?? string.Empty).Trim().ToLowerInvariant();
            if (db.Profil.Any(p => p.adresseMail == email))
            {
                ModelState.AddModelError("adresseMail", "An account already exists with this email.");
                return View(model);
            }

            var verificationCode = Guid.NewGuid().ToString();

            var profil = new Profil
            {
                adresseMail = email,
                gsm = null,
                motPasse = PasswordHasher.Hash(model.motPasse),
                nom = model.nom,
                prenom = model.prenom,
                dateCreation = DateTime.Now,
                titre = verificationCode,
                Tel = null
            };
            db.Profil.Add(profil);
            db.SaveChanges();

            try
            {
                db.Profil_Link.Add(new Profil_Link
                {
                    C_idProfil = profil.id,
                    token = verificationCode,
                    dateGeneration = DateTime.Now,
                    dateActivation = null,
                    dateExpiration = DateTime.Now.AddMinutes(10),
                    etat = false
                });
                db.SaveChanges();

                var activateLink = Url.Action("Activate", "SignUp", new { token = verificationCode }, Request.Url.Scheme);
                Email.SendVerificationLink(email, activateLink);
            }
            catch (Exception ex)
            {
                // Profile is created; verification email failed. Log and continue so the user is not blocked.
                System.Diagnostics.Trace.TraceError("Verification email failed: {0}", ex);
                TempData["EmailWarning"] = "Your profile was created, but we could not send the verification email. You can still browse and apply to jobs.";
            }

            return RedirectToAction("EndSignUp");
        }

        // GET: /signup/confirm
        public ActionResult EndSignUp()
        {
            return View();
        }

        // GET: /signup/activate/{token}
        public ActionResult Activate(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return HttpNotFound();
            }

            var link = db.Profil_Link.Include(l => l.Profil).FirstOrDefault(x => x.token == token);
            if (link == null || link.Profil == null)
            {
                return HttpNotFound();
            }

            if (link.etat)
            {
                ViewBag.adresseMail = link.Profil.adresseMail;
                ViewBag.firstname = link.Profil.prenom;
                ViewBag.lastname = link.Profil.nom;
                return View();
            }

            if (link.dateExpiration < DateTime.Now)
            {
                ViewBag.Expired = true;
                return View();
            }

            link.dateActivation = DateTime.Now;
            link.etat = true;
            link.Profil.titre = null;
            db.SaveChanges();

            ViewBag.adresseMail = link.Profil.adresseMail;
            ViewBag.firstname = link.Profil.prenom;
            ViewBag.lastname = link.Profil.nom;
            return View();
        }

        private static string GenerateRandomToken(int length)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            var bytes = new byte[length];
            using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }
            var result = new char[length];
            for (int i = 0; i < length; i++) result[i] = chars[bytes[i] % chars.Length];
            return new string(result);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
