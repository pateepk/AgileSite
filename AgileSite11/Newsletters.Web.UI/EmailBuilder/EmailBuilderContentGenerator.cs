using System;
using System.Collections.Generic;

using CMS.Helpers;
using CMS.Newsletters.Filters;
using CMS.Newsletters.Internal;

namespace CMS.Newsletters.Web.UI
{
    /// <summary>
    /// Class that processes the email template code to be used in the email builder content iframe.
    /// It decorates the HTML with additional scripts, style sheets and replaces the zone placeholders with empty drop zones.
    /// </summary>
    internal static class EmailBuilderContentGenerator
    {
        internal const string ZONE_CLASS_NAME = "cms-zone";
        internal const string EMPTY_ZONE_RESOURCE_STRING = "emailbuilder.emptyzonetext";


        /// <summary>
        /// Prepares the HTML for the email builder.
        /// </summary>
        /// <param name="templateCode">The template code.</param>
        /// <param name="customFilter">Filter applied to the widget content.</param>
        public static string PrepareHtml(string templateCode, IEmailContentFilter customFilter)
        {
            if (String.IsNullOrEmpty(templateCode))
            {
                return null;
            }

            // Apply filter first because we don't want the filter to be applied on extra components which are added for sake of Email builder.
            templateCode = ApplyFilter(templateCode, customFilter);

            // Don't add external links when template code is empty because EmailHtmlModifier requires valid non-empty HTML document.
            if (!String.IsNullOrEmpty(templateCode))
            {
                templateCode = AddExternalLinks(templateCode);
            }

            return WidgetZonePlaceholderHelper.ReplacePlaceholders(templateCode, GetEmptyZoneMarkup);
        }


        /// <summary>
        /// Add style sheets and javascript to the <paramref name="templateCode"/>. Relative links are used, it's not necessary to make them absolute.
        /// </summary>
        /// <param name="templateCode">The template code.</param>
        private static string AddExternalLinks(string templateCode)
        {
            var htmlModifier = new EmailHtmlModifier(templateCode, false);

            // Add email builder scripts and styles into the template HTML
            AddJavaScripts(htmlModifier);
            AddStyleSheets(htmlModifier);

            return htmlModifier.GetHtml();
        }


        private static string ApplyFilter(string templateCode, IEmailContentFilter customFilter)
        {
            if (customFilter != null)
            {
                templateCode = customFilter.Apply(templateCode);
            }

            return templateCode;
        }


        /// <summary>
        /// Returns the empty widget zone markup to be used in the email builder.
        /// </summary>
        /// <param name="zoneId">The zone identifier.</param>
        internal static string GetEmptyZoneMarkup(string zoneId)
        {
            return $@"<div class=""{ZONE_CLASS_NAME}"" data-zone-id=""{zoneId}"" data-empty-zone-text=""{HTMLHelper.EncodeForHtmlAttribute(ResHelper.GetString(EMPTY_ZONE_RESOURCE_STRING))}""></div>";
        }


        /// <summary>
        /// Adds necessary JavaScripts for email builder. 
        /// </summary>
        private static void AddJavaScripts(EmailHtmlModifier htmlModifier)
        {
            htmlModifier.AppendElementToBody("script", new Dictionary<string, string>
            {
                { "type", "text/javascript"},
                { "src", URLHelper.ResolveUrl("~/CMSScripts/jquery/jquery-core.js") }
            });

            htmlModifier.AppendElementToBody("script", new Dictionary<string, string>
            {
                { "type", "text/javascript"},
                { "src", URLHelper.ResolveUrl("~/CMSScripts/Vendor/SortableJS/scroll.js") }
            });

            var activationScript = "$cmsj(document).ready(function() { $cmsj().dndPageScroll({activate: false}); });";
            htmlModifier.AppendElementToBody("script", new Dictionary<string, string>
            {
                { "type", "text/javascript"}
            }, activationScript);
        }


        /// <summary>
        /// Adds necessary style sheets for email builder. 
        /// </summary>
        private static void AddStyleSheets(EmailHtmlModifier htmlModifier)
        {
            htmlModifier.AppendElementToHead("link", new Dictionary<string, string>
            {
                { "rel", "stylesheet"},
                { "href", URLHelper.ResolveUrl("~/CMSModules/Newsletters/EmailBuilder/emailbuilder.iframe.css") }
            });
        }
    }
}
