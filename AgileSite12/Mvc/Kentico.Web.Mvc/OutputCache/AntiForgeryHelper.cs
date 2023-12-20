using System;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;

using CMS.EventLog;
using CMS.Helpers;

using Kentico.Web.Mvc.Internal;

namespace Kentico.Web.Mvc
{
    /// <summary>
    /// Helper class used for AntiForgeryToken used on pages with output cache.
    /// </summary>
    /// <remarks>
    /// <para>
    /// AntiForgery token for pages with output cache is ensured by combination of <see cref="HttpResponse.Filter"/> behavior and 
    /// custom call of <see cref="AntiForgery.GetHtml()"/> method within the response filtering. We can't use original method because the AntiForgery token
    /// is generated once and used for all responses, it means that different users have different cookie but same token in response which leads to validation error on form submit.
    /// Moreover, original method adds Set-Cookie into the headers so it's captured by output cache mechanism and output cache is not used on first request.
    /// </para>
    /// <para>
    /// In case of not-cached response AntiForgery token in view is requested by <see cref="AntiForgeryExtensions.AntiForgeryToken(ExtensionPoint{System.Web.Mvc.HtmlHelper})"/> method instead of <see cref="HtmlHelper.AntiForgeryToken()" />. 
    /// This method renders <see cref="INPUT_PLACEHOLDER"/> into the markup and registers response filter with placeholder replacement logic. The logic replaces the placeholder in response output and registers
    /// <see cref="ValidateCache"/> as validation callback with rendered token value (for cached response replacement).
    /// If AntiForgery cookie is not available the <see cref="AntiForgery.GetHtml()"/> method sets the cookie but in this request pipeline stage the output cache is not disabled.
    /// </para>
    /// <para>
    /// In case of cached response validation callback is initialized. Within the validation callback response filter is registered with AntiForgery token value replacement logic.
    /// We don't need to replace placeholder because placeholder is not in cached response the first token is cached in response. 
    /// The token value is available in validation callback data object so we just replace the token value within the response filter replacement logic.
    /// </para>
    /// </remarks>
    internal class AntiForgeryHelper
    {
        private const string INSTANCE_KEY = "KenticoAntiForgeryHelper";
        private const string FILTER_NAME = "OutputCacheAntiForgeryFilter";
        internal const string INPUT_PLACEHOLDER = @"<kenticoantiforgeryinitialplaceholder />";

        private bool? cachedRequest;
        private readonly HttpContextBase httpContext;
        private readonly ResponseFilter responseFilter;
        private readonly Func<HtmlString> getHtml;


        /// <summary>
        /// Creates new instance of <see cref="AntiForgeryHelper"/>.
        /// </summary>
        /// <param name="httpContext">Http context</param>
        /// <param name="responseFilter">Response filter instance</param>
        /// <param name="getHtml">Function for html token generator</param>
        internal AntiForgeryHelper(HttpContextBase httpContext, ResponseFilter responseFilter, Func<HtmlString> getHtml)
        {
            this.httpContext = httpContext;
            this.responseFilter = responseFilter;
            this.getHtml = getHtml;
        }


        /// <summary>
        /// Cached antiforgery token value to be replaced for cached response
        /// </summary>
        internal string TokenReplacement
        {
            get;
            set;
        }


        /// <summary>
        /// Request specific instance.
        /// </summary>
        public static AntiForgeryHelper Instance
        {
            get
            {
                var items = CMSHttpContext.Current?.Items;
                var instance = items[INSTANCE_KEY] as AntiForgeryHelper;
                if (instance == null)
                {
                    instance = new AntiForgeryHelper(CMSHttpContext.Current, ResponseFilter.Instance, () => AntiForgery.GetHtml());
                    items[INSTANCE_KEY] = instance;
                }

                return instance;
            }
        }


        /// <summary>
        /// Ensures <see cref="ResponseFilter"/> modification for initial or cached request.
        /// </summary>
        /// <param name="isCachedRequest">Indicates whether current request is initiated from cache.</param>
        internal void EnsureResponeTokens(bool isCachedRequest)
        {
            // It's not allowed to change not cached request to cached request
            if (cachedRequest.HasValue && !cachedRequest.Value)
            {
                return;
            }

            cachedRequest = isCachedRequest;

            responseFilter.Add(FILTER_NAME, (data) =>
            {
                if (!cachedRequest.GetValueOrDefault())
                {
                    return EnsureResponseTokensForInitialRequest(data);
                }

                return EnsureResponseTokensForCachedRequest(data);
            });
        }


        /// <summary>
        /// Ensures response filter modification for initial (not cached) request.
        /// </summary>
        private string EnsureResponseTokensForInitialRequest(string data)
        {
            var tokenValue = GenerateTokenSafe(out var tokenHtml);

            if (!String.IsNullOrEmpty(tokenValue))
            {
                TokenReplacement = tokenValue;
                httpContext.Response.Cache.AddValidationCallback(ValidateCache, tokenValue);
            }

            return data.Replace(INPUT_PLACEHOLDER, tokenHtml);
        }


        /// <summary>
        /// Ensures response filter modification for cached request.
        /// </summary>
        private string EnsureResponseTokensForCachedRequest(string data)
        {
            var token = TokenReplacement;
            if (token != null)
            {
                var tokenValue = GenerateTokenSafe(out var tokenHtml);
                return data.Replace(token, tokenValue);
            }

            return data;
        }


        /// <summary>
        /// Generates antiforgery token value and hidden input markup.
        /// </summary>
        /// <param name="tokenHtml">Returns HTML code for hidden input with antiforgery token.</param>
        internal string GenerateTokenSafe(out string tokenHtml)
        {
            try
            {
                tokenHtml = getHtml().ToString();
                var matches = Regex.Matches(tokenHtml, "value=\"(.+)[\"]", RegexOptions.Compiled);
                return matches?[0]?.Groups[1]?.Value;
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("GenerateToken", "AntiForgery", ex);
            }

            tokenHtml = String.Empty;
            return String.Empty;
        }


        /// <summary>
        /// Cache callback method used as response filter registrant for cached response
        /// </summary>
        private static void ValidateCache(HttpContext context, Object data, ref HttpValidationStatus status)
        {
            var antiForgeryHelper = Instance;

            antiForgeryHelper.EnsureResponeTokens(isCachedRequest: true);
            if (antiForgeryHelper.TokenReplacement == null)
            {
                antiForgeryHelper.TokenReplacement = data as string;
            }

            ResponseFilter.Instance.EnsureRepsonseFilter();
        }
    }
}
