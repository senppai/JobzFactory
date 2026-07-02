using System;
using System.Text.RegularExpressions;

namespace JF.DAL.Security
{
    /// <summary>
    /// Lightweight whitelist-ish HTML sanitizer for stored rich-text fields
    /// (job descriptions from Summernote) before rendering with @Html.Raw.
    /// It strips the common XSS vectors (script blocks, event handler
    /// attributes, javascript:/vbscript: URLs, style expressions). This is a
    /// defense-in-depth measure; for untrusted high-risk input prefer a
    /// dedicated library (e.g. HtmlSanitizer) over hand-rolled regex.
    /// </summary>
    public static class HtmlSanitizer
    {
        private static readonly Regex ScriptBlock = new Regex(
            "<script[^>]*>.*?</script>", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);
        private static readonly Regex StyleBlock = new Regex(
            "<style[^>]*>.*?</style>", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);
        private static readonly Regex EventAttribute = new Regex(
            @"\son[a-z]+\s*=\s*(""[^""]*""|'[^']*'|[^\s>]+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex ScriptUrl = new Regex(
            @"(href|src)\s*=\s*(""(javascript|vbscript):[^""]*""|'(javascript|vbscript):[^']*')",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex Expression = new Regex(
            @"expression\s*\(", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public static string Sanitize(string html)
        {
            if (string.IsNullOrEmpty(html)) return html;

            html = ScriptBlock.Replace(html, string.Empty);
            html = StyleBlock.Replace(html, string.Empty);
            html = EventAttribute.Replace(html, string.Empty);
            html = ScriptUrl.Replace(html, "$1=\"\"");
            html = Expression.Replace(html, string.Empty);
            return html;
        }
    }
}
