using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;

namespace JobzFactory.Models
{
    public class SendEmail
    {
        //[Obsolete("Do not use this in Production code!!!", true)]
        static void NEVER_EAT_POISON_Disable_CertificateValidation()
        {
            // Disabling certificate validation can expose you to a man-in-the-middle attack
            // which may allow your encrypted message to be read by an attacker
            // https://stackoverflow.com/a/14907718/740639
            ServicePointManager.ServerCertificateValidationCallback =
                delegate (
                    object s,
                    X509Certificate certificate,
                    X509Chain chain,
                    SslPolicyErrors sslPolicyErrors
                ) {
                    return true;
                };
        }

        //[Obsolete]
        //public static void SendApplyMsg(string UserEmail)
        //{
        //    string Email = "no-reply@jobzfactory.com";
        //    string Password = ""; // configure via Web.config in production

        //    var fromMail = new MailAddress(address: Email, displayName: "Job Z Factory");
        //    var toEmail = new MailAddress(UserEmail);
        //    var fromEmailPassword = Password;
        //    string Subject = "Resume submit to recruiter";
        //    string body = "<br/> your resume has been submited to the recruiter successfully";
        //    var smtp = new SmtpClient


        //    {
        //        Host = "webmail.jobzfactory.com",
        //        Port = 587,
        //        EnableSsl = true,
        //        DeliveryMethod = SmtpDeliveryMethod.Network,
        //        UseDefaultCredentials = false,
        //        Credentials = new NetworkCredential(userName: fromMail.Address, fromEmailPassword)
        //    };
        //    smtp.Timeout = 10000000;
        //    NEVER_EAT_POISON_Disable_CertificateValidation();
        //    using (var message = new MailMessage(fromMail, toEmail)
        //    {
        //        Subject = Subject,
        //        Body = body,
        //        IsBodyHtml = true,


        //    })
        //        smtp.Send(message);
        //}

        //public static void SendResumeMsg(string UserEmail)
        //{
        //    string Email = "no-reply@jobzfactory.com";
        //    string Password = ""; // configure via Web.config in production

        //    var fromMail = new MailAddress(address: Email, displayName: "Job Z Factory");
        //    var toEmail = new MailAddress(UserEmail);
        //    var fromEmailPassword = Password;
        //    string Subject = "Someone apply to your Offre";
        //    string recuteur = "http://recruteur.jobzfactory.com/";
        //    string body = "<br/> Someone has been apply to your offre<br/>to see it click on following link<br/><a href='" + recuteur + "'>Please click here<a/>";
        //    var smtp = new SmtpClient


        //    {
        //        Host = "webmail.jobzfactory.com",
        //        Port = 587,
        //        EnableSsl = true,
        //        DeliveryMethod = SmtpDeliveryMethod.Network,
        //        UseDefaultCredentials = false,
        //        Credentials = new NetworkCredential(userName: fromMail.Address, fromEmailPassword)
        //    };
        //    smtp.Timeout = 10000000;
        //    NEVER_EAT_POISON_Disable_CertificateValidation();
        //    using (var message = new MailMessage(fromMail, toEmail)
        //    {
        //        Subject = Subject,
        //        Body = body,
        //        IsBodyHtml = true,


        //    })
        //        smtp.Send(message);
        //}
    }
}