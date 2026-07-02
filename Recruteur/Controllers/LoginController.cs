using JF.DAL.Security;
using Newtonsoft.Json;
using Recruteur.Models;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace Recruteur.Controllers
{
    public class LoginController : Controller
    {
        private readonly JF.DAL.Context.jobzFactoryEntities db = new JF.DAL.Context.jobzFactoryEntities();

        // GET: Login
        public ActionResult Index()
        {
            return View(new LoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(LoginViewModel model, string returnUrl = "")
        {
            if (ModelState.IsValid)
            {
                var user = db.Recruteur_Contact
                             .Where(u => u.compte == model.compte)
                             .Include(a => a.Recruteur)
                             .FirstOrDefault();

                if (user != null && PasswordHasher.Verify(user.motPasse, model.motPasse, out bool needsUpgrade))
                {
                    if (needsUpgrade)
                    {
                        user.motPasse = PasswordHasher.Hash(model.motPasse);
                        db.SaveChanges();
                    }

                    IssueAuthTicket(user, model.RememberMe);
                    StoreSession(user);

                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                        return Redirect(returnUrl);

                    return RedirectToAction("TableauBord", "Default");
                }

                ModelState.AddModelError("", "Incorrect username and/or password");
            }

            return View(model);
        }

        [CustomAuthorize(Roles = "RecruteurAdmin")]
        public ActionResult LogOut()
        {
            FormsAuthentication.SignOut();
            Session.Remove("compteRecruteur");
            return RedirectToAction("Index", "Login");
        }

        // GET: Login/SignUp
        public ActionResult SignUp()
        {
            return View(new RecruteurViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SignUp(RecruteurViewModel model)
        {
            if (ModelState.IsValid)
            {
                var contactEmail = (model.adresseMailContact ?? "").Trim().ToLowerInvariant();

                if (db.Recruteur_Contact.Any(u => u.compte == contactEmail))
                {
                    ModelState.AddModelError("adresseMailContact", "An account already exists with this email.");
                    return View(model);
                }

                var recruteur = new JF.DAL.Context.Recruteur
                {
                    nomRecruteur = model.recruteur,
                    adresseMail = model.adresseMail,
                    dateCreation = DateTime.Now,
                    url = (model.recruteur ?? "").Trim().ToLower().Replace(" ", "_")
                };
                db.Recruteur.Add(recruteur);
                db.SaveChanges();

                var recruteurContact = new JF.DAL.Context.Recruteur_Contact
                {
                    compte = contactEmail,
                    motPasse = PasswordHasher.Hash(model.motPasse),
                    nom = model.nom,
                    prenom = model.prenom,
                    profil = 1,
                    adresseMail = model.adresseMailContact,
                    C_idRecruteur = recruteur.id,
                    dateCreation = DateTime.Now
                };
                db.Recruteur_Contact.Add(recruteurContact);
                db.SaveChanges();

                TempData["FinRegister"] = true;
                return RedirectToAction("FinRegister");
            }
            return View(model);
        }

        // GET: Login/FinRegister
        public ActionResult FinRegister()
        {
            if (TempData["FinRegister"] as bool? ?? false)
                return View();
            return RedirectToAction("Index");
        }

        private void IssueAuthTicket(JF.DAL.Context.Recruteur_Contact user, bool persistent)
        {
            var serializeModel = new CustomRecruteurModel
            {
                id = user.id,
                nom = user.nom,
                prenom = user.prenom,
                roles = new[] { "RecruteurAdmin" },
                idRecruteur = user.C_idRecruteur,
                recruteur = user.Recruteur?.nomRecruteur
            };

            string userData = JsonConvert.SerializeObject(serializeModel);
            var authTicket = new FormsAuthenticationTicket(
                1, user.compte, DateTime.Now, DateTime.Now.AddDays(30),
                persistent, userData);

            var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, FormsAuthentication.Encrypt(authTicket))
            {
                HttpOnly = true
            };
            if (HttpContext.Request.IsSecureConnection)
                cookie.Secure = true;

            Response.Cookies.Add(cookie);
        }

        private void StoreSession(JF.DAL.Context.Recruteur_Contact user)
        {
            // Never keep the password hash in session.
            user.motPasse = null;
            VarSessionRecruteur.compteRecruteur = user;
        }
    }
}
