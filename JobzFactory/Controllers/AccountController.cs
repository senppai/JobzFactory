using JF.DAL.Context;
using JF.DAL.Security;
using JobzFactory.Models;
using Newtonsoft.Json;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace JobzFactory.Controllers
{
    public class AccountController : Controller
    {
        private readonly jobzFactoryEntities db = new jobzFactoryEntities();

        // GET: /login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl = "")
        {
            if (HttpContext.User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Profile");
            }
            ViewBag.ReturnUrl = returnUrl;
            return View(new LoginViewModel());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model, string returnUrl = "")
        {
            if (!ModelState.IsValid)
            {
                ViewBag.ReturnUrl = returnUrl;
                return View(model);
            }

            var email = (model.adresseMail ?? string.Empty).Trim().ToLowerInvariant();
            var profil = db.Profil.FirstOrDefault(p => p.adresseMail == email);

            if (profil != null && !string.IsNullOrEmpty(profil.motPasse)
                && PasswordHasher.Verify(profil.motPasse, model.motPasse, out bool needsUpgrade))
            {
                if (needsUpgrade)
                {
                    profil.motPasse = PasswordHasher.Hash(model.motPasse);
                    db.SaveChanges();
                }

                IssueAuthTicket(profil, model.RememberMe);

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }
                return RedirectToAction("Index", "Profile");
            }

            ModelState.AddModelError("", "Invalid email or password.");
            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        // GET: /logout
        public ActionResult LogOut()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Default");
        }

        // GET: /account/forgot
        [AllowAnonymous]
        public ActionResult Forgot()
        {
            return View(new ForgotViewModel());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Forgot(ForgotViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var email = (model.adresseMail ?? string.Empty).Trim().ToLowerInvariant();
            var profil = db.Profil.FirstOrDefault(p => p.adresseMail == email);

            // Always show the confirmation page (do not reveal whether the email exists).
            if (profil != null)
            {
                var token = Guid.NewGuid().ToString();
                db.Profil_Link.Add(new Profil_Link
                {
                    C_idProfil = profil.id,
                    token = token,
                    dateGeneration = DateTime.Now,
                    dateExpiration = DateTime.Now.AddMinutes(30),
                    dateActivation = null,
                    etat = false
                });
                db.SaveChanges();

                try
                {
                    var resetLink = Url.Action("Reset", "Account", new { token = token }, Request.Url.Scheme);
                    Email.SendPasswordResetLink(email, resetLink);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.TraceError("Password reset email failed: {0}", ex);
                }
            }

            return RedirectToAction("ForgotConfirmation");
        }

        [AllowAnonymous]
        public ActionResult ForgotConfirmation()
        {
            return View();
        }

        // GET: /account/reset/{token}
        [AllowAnonymous]
        public ActionResult Reset(string token)
        {
            var link = db.Profil_Link.FirstOrDefault(l => l.token == token);
            if (link == null || link.etat || link.dateExpiration < DateTime.Now)
            {
                ViewBag.Invalid = true;
            }
            return View(new ResetViewModel { token = token });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Reset(ResetViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var link = db.Profil_Link.FirstOrDefault(l => l.token == model.token);
            if (link == null || link.etat || link.dateExpiration < DateTime.Now)
            {
                ModelState.AddModelError("", "This password reset link is invalid or has expired. Please request a new one.");
                return View(model);
            }

            var profil = db.Profil.FirstOrDefault(p => p.id == link.C_idProfil);
            if (profil == null)
            {
                ModelState.AddModelError("", "We could not find an account for this reset link.");
                return View(model);
            }

            profil.motPasse = PasswordHasher.Hash(model.motPasse);
            link.etat = true;
            link.dateActivation = DateTime.Now;
            db.SaveChanges();

            TempData["PasswordReset"] = true;
            return RedirectToAction("Login");
        }

        private void IssueAuthTicket(Profil profil, bool persistent)
        {
            var serializeModel = new CustomCandidateModel
            {
                id = profil.id,
                nom = profil.nom,
                prenom = profil.prenom,
                adresseMail = profil.adresseMail,
                roles = new[] { "Candidate" }
            };

            string userData = JsonConvert.SerializeObject(serializeModel);
            var ticket = new FormsAuthenticationTicket(
                1, profil.adresseMail, DateTime.Now, DateTime.Now.AddDays(30),
                persistent, userData);

            var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, FormsAuthentication.Encrypt(ticket))
            {
                HttpOnly = true
            };
            if (HttpContext.Request.IsSecureConnection)
            {
                cookie.Secure = true;
            }

            Response.Cookies.Add(cookie);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
