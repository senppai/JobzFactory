using System;
using System.Configuration;
using System.Net;
using System.Net.Mail;

namespace JobzFactory.Models
{
    /// <summary>
    /// Sends candidate email-verification messages using SMTP settings from Web.config.
    /// Migrated from the former Profil portal; TLS certificate validation is no longer disabled.
    /// </summary>
    public class Email
    {
        private static string SenderEmail =>
            ConfigurationManager.AppSettings["Email"] ?? "no-reply@jobzfactory.com";

        private static string SenderPassword =>
            ConfigurationManager.AppSettings["Password"] ?? "";

        private static string SenderName =>
            ConfigurationManager.AppSettings["SenderName"] ?? "JobzFactory";

        private static string SmtpHost =>
            ConfigurationManager.AppSettings["SmtpHost"] ?? "smtp.gmail.com";

        private static int SmtpPort =>
            int.TryParse(ConfigurationManager.AppSettings["SmtpPort"], out var port) ? port : 587;

        public static void SendVerificationLink(string userEmail, string verificationLink)
        {
            if (string.IsNullOrWhiteSpace(SenderPassword))
            {
                throw new InvalidOperationException(
                    "Email password is not configured. Add your SMTP password to Web.config (key: Password).");
            }

            var fromMail = new MailAddress(SenderEmail, SenderName);
            var toEmail = new MailAddress(userEmail);
            var subject = "Activate your Jobz Factory profile";
            var body =
                "<div style='font-family:Arial,sans-serif;max-width:560px;margin:0 auto;padding:24px;'>" +
                "<h2 style='color:#4f46e5;margin:0 0 16px;'>Welcome to Jobz Factory</h2>" +
                "<p style='color:#334155;line-height:1.6;'>Thanks for creating your profile. Please confirm your email address to start receiving job offers.</p>" +
                "<p style='margin:24px 0;'><a href='" + verificationLink + "' style='display:inline-block;background:#4f46e5;color:#fff;padding:12px 24px;border-radius:8px;text-decoration:none;font-weight:bold;'>Activate my account</a></p>" +
                "<p style='color:#64748b;font-size:13px;'>This link expires in 10 minutes. If you did not sign up, you can ignore this email.</p>" +
                "<p style='color:#94a3b8;font-size:12px;margin-top:24px;'>— " + SenderName + ", Jobz Factory</p>" +
                "</div>";

            using (var smtp = new SmtpClient(SmtpHost, SmtpPort))
            {
                smtp.EnableSsl = true;
                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new NetworkCredential(SenderEmail, SenderPassword);
                smtp.Timeout = 30000;

                using (var message = new MailMessage(fromMail, toEmail)
                {
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                })
                {
                    smtp.Send(message);
                }
            }
        }

        public static void SendPasswordResetLink(string userEmail, string resetLink)
        {
            if (string.IsNullOrWhiteSpace(SenderPassword))
            {
                throw new InvalidOperationException(
                    "Email password is not configured. Add your SMTP password to Web.config (key: Password).");
            }

            var fromMail = new MailAddress(SenderEmail, SenderName);
            var toEmail = new MailAddress(userEmail);
            var subject = "Reset your Jobz Factory password";
            var body =
                "<div style='font-family:Arial,sans-serif;max-width:560px;margin:0 auto;padding:24px;'>" +
                "<h2 style='color:#4f46e5;margin:0 0 16px;'>Reset your password</h2>" +
                "<p style='color:#334155;line-height:1.6;'>We received a request to reset the password for your Jobz Factory account. Click the button below to choose a new password.</p>" +
                "<p style='margin:24px 0;'><a href='" + resetLink + "' style='display:inline-block;background:#4f46e5;color:#fff;padding:12px 24px;border-radius:8px;text-decoration:none;font-weight:bold;'>Reset password</a></p>" +
                "<p style='color:#64748b;font-size:13px;'>This link expires in 30 minutes. If you did not request a password reset, you can safely ignore this email.</p>" +
                "<p style='color:#94a3b8;font-size:12px;margin-top:24px;'>— " + SenderName + ", Jobz Factory</p>" +
                "</div>";

            using (var smtp = new SmtpClient(SmtpHost, SmtpPort))
            {
                smtp.EnableSsl = true;
                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new NetworkCredential(SenderEmail, SenderPassword);
                smtp.Timeout = 30000;

                using (var message = new MailMessage(fromMail, toEmail)
                {
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                })
                {
                    smtp.Send(message);
                }
            }
        }
    }
}
