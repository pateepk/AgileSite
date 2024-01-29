using System;
using System.Web;
using System.Web.UI;

using CMS.Base;
using CMS.Base.Web.UI;
using CMS.Helpers;

namespace CMS.UIControls
{
    /// <summary>
    /// Cache log base class.
    /// </summary>
    public class CacheLog : LogControl
    {
        #region "Properties"

        /// <summary>
        /// Debug settings for this particular log
        /// </summary>
        public override DebugSettings Settings
        {
            get
            {
                return CacheDebug.Settings;
            }
        }
        
        #endregion


        #region "Methods"

        /// <summary>
        /// OnPreRender event handler
        /// </summary>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            ScriptHelper.RegisterDialogScript(Page);
        }


        /// <summary>
        /// Returns formatted information for given parameters.
        /// </summary>
        /// <param name="page">Page object</param>
        /// <param name="cacheKey">Cache key</param>
        /// <param name="dependencies">Cache dependencies</param>
        /// <param name="cacheValue">Cache value</param>
        /// <param name="cacheOperation">Cache operation</param>
        public static string GetInformation(Page page, object cacheKey, object dependencies, object cacheValue, object cacheOperation)
        {
            string info = HTMLHelper.HTMLEncodeLineBreaks(ValidationHelper.GetString(cacheKey, ""));

            if ((ValidationHelper.GetString(cacheValue, "") != "") && (ValidationHelper.GetString(cacheOperation, "") != CacheOperation.TOUCH))
            {
                info += "<span class=\"debug-log-cachekey\">" +
                        GetValue(page, cacheKey, cacheValue) +
                        "</span>";
            }

            if (ValidationHelper.GetString(dependencies, "") != "")
            {
                info += "<div class=\"debug-log-cachedep\" >" +
                        HTMLHelper.EnsureHtmlLineEndings(ValidationHelper.GetString(dependencies, "")) +
                        "</div>";
            }
            return info;
        }


        /// <summary>
        /// Gets the item value.
        /// </summary>
        /// <param name="page">Page object</param>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        public static string GetValue(Page page, object key, object value)
        {
            string result = HTMLHelper.HTMLEncodeLineBreaks((string)value);

            if (!String.IsNullOrEmpty(result))
            {
                var button = new CMSGridActionButton
                {
                    CssClass = "debug-log-view",
                    IconCssClass = "icon-eye",
                    IconStyle = GridIconStyle.Allow,
                    ToolTip = ResHelper.GetString("General.View"),
                    OnClientClick = "modalDialog('" + UrlResolver.ResolveUrl("~/CMSModules/System/Debug/System_ViewObject.aspx?source=cache&key=") + HttpUtility.UrlEncode((string)key) + "', 'CacheLogItem', '1000', '700'); return false;"
                };

                result = button.GetRenderedHTML();
            }

            return result;
        }

        #endregion
    }
}
