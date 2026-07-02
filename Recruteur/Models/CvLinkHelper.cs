using System.Configuration;
using System.Web;
using JF.DAL.Security;

namespace Recruteur.Models
{
    /// <summary>
    /// Builds signed, time-limited CV download URLs that point at the public
    /// JobzFactory site. JobzFactory validates the token before serving the file,
    /// so CVs cannot be fetched by guessing sequential application ids.
    /// </summary>
    public static class CvLinkHelper
    {
        public static string Build(int applicationId, int ttlMinutes = 60)
        {
            var baseUrl = ConfigurationManager.AppSettings["UrlMain"] ?? "/";
            var secret = ConfigurationManager.AppSettings["CvTokenSecret"];
            var token = CvToken.Generate(applicationId, secret, ttlMinutes);
            return $"{baseUrl.TrimEnd('/')}/job/cv/{applicationId}?t={HttpUtility.UrlEncode(token)}";
        }
    }
}
