using Administration.Models;
using JF.DAL.Security;
using System;
using System.Configuration;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace Administration.Controllers
{
    public class AccountController : Controller
    {
        // GET: Account/Login
        public ActionResult Login(string returnUrl = "")
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string username, string password, string returnUrl = "")
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError("", "Username and password are required.");
                ViewBag.ReturnUrl = returnUrl;
                return View();
            }

            var expectedUser = ConfigurationManager.AppSettings["AdminUsername"];
            var storedHash = ConfigurationManager.AppSettings["AdminPasswordHash"];

            if (string.IsNullOrEmpty(expectedUser) || string.IsNullOrEmpty(storedHash))
            {
                ModelState.AddModelError("", "Administrator account is not configured.");
                ViewBag.ReturnUrl = returnUrl;
                return View();
            }

            bool userOk = string.Equals(username.Trim(), expectedUser, StringComparison.OrdinalIgnoreCase);
            bool passOk = PasswordHasher.Verify(storedHash, password, out bool needsUpgrade);

            if (!(userOk && passOk))
            {
                ModelState.AddModelError("", "Invalid credentials.");
                ViewBag.ReturnUrl = returnUrl;
                return View();
            }

            if (needsUpgrade)
            {
                // Hash is in a legacy format; re-issue a modern hash in config on next deploy.
                // Not writable from here at runtime; flagged for the operator.
                System.Diagnostics.Trace.TraceWarning("Admin password hash needs upgrade (legacy format).");
            }

            IssueAuthTicket(username.Trim());
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            return RedirectToAction("Index", "Default");
        }

        [AdminAuthorize(Roles = "Admin")]
        public ActionResult LogOut()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login");
        }

        [AllowAnonymous]
        public ActionResult AccessDenied()
        {
            Response.StatusCode = 403;
            return View();
        }

        [AllowAnonymous]
        public ActionResult NotFound()
        {
            Response.StatusCode = 404;
            return View();
        }

        private void IssueAuthTicket(string username)
        {
            // userData carries the comma-separated roles (no external JSON dependency).
            var userData = "Admin";
            var ticket = new FormsAuthenticationTicket(
                1, username, DateTime.Now, DateTime.Now.AddDays(1),
                false, userData);

            var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, FormsAuthentication.Encrypt(ticket))
            {
                HttpOnly = true
            };
            if (HttpContext.Request.IsSecureConnection)
                cookie.Secure = true;
            Response.Cookies.Add(cookie);
        }
    }
}
