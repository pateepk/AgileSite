using System;
using System.ComponentModel;
using System.Web;
using System.Web.Http.Description;
using System.Web.Mvc;

using CMS.Helpers;

namespace Kentico.Content.Web.Mvc
{
    /// <summary>
    /// This system controller is a part detection mechanism whether a browser prevents cookies from being set for 3rd party domains.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [ApiExplorerSettings(IgnoreApi = true)]
    public sealed class CookiePolicyDetectionController : Controller
    {
        /// <summary>
        /// Default action.
        /// </summary>
        /// <param name="origin">Expected origin of parent frame</param>
        /// <param name="cookieName">Cookie name to set.</param>
        /// <param name="cookieValue">Cookie value.</param>
        [HttpGet]
        public ActionResult Index(string origin, string cookieName, string cookieValue)
        {
            if (!QueryHelper.ValidateHash("hash")) {
                return HttpNotFound();
            }

            Response.AddHeader("X-Frames-Options", $"allow-from {origin}");
            Response.SetCookie(new HttpCookie(cookieName, cookieValue) { Expires = DateTime.Now.AddMinutes(1) });

            string html = $@"<!DOCTYPE html>

<html>
<head>
    <meta name=""viewport"" content=""width=device-width"" />
    <title>Index</title>
    <script type=""text/javascript"">
        window.addEventListener('message', function (event) {{
            if (event && event.origin === '{origin}') {{
                var cookieValue = GetCookieValue(event.data);

                event.source.postMessage(cookieValue, event.origin);
            }}
        }});

        function GetCookieValue(name) {{
            var cookies = document.cookie.split(';');

            if (!cookies) return;

            for (var i = 0; i < cookies.length; i++) {{
                var cookie = cookies[i].trim();
                if (cookie.indexOf(name + '=') == 0) {{
                    return cookie.substring(name.length + 1, cookie.length);
                }}
            }}
        }}
    </script>
</head>
<body>
</body>
</html>";

            return Content(html, "text/html");
        }
    }
}
