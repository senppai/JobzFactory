using System.Web.Mvc;

namespace JF.DAL.Security
{
    public static class HtmlHelperSecurityExtensions
    {
        /// <summary>Sanitizes stored rich-text HTML so it is safe to render with @Html.Raw.</summary>
        public static MvcHtmlString Sanitize(this HtmlHelper html, string rawHtml)
        {
            return MvcHtmlString.Create(HtmlSanitizer.Sanitize(rawHtml));
        }
    }
}
