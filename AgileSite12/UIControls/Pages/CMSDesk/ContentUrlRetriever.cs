using System;
using System.Web;
using System.Web.UI;

using CMS.Base;
using CMS.Base.Web.UI;
using CMS.DocumentEngine;
using CMS.Helpers;
using CMS.PortalEngine;

namespace CMS.UIControls
{
    using GetUrlFunc = Func<UIPageURLSettings, string>;

    /// <summary>
    /// Class for generation content URLs
    /// </summary>
    public class ContentUrlRetriever
    {
        #region "Constants"

        /// <summary>
        /// Separator for callback result
        /// </summary>
        public const string CALLBACK_SEPARATOR = "##SEP##";

        private const string URL_PREFIX = "##URL##";
        private const string CALLBACK_PARAMETER = "urlparams";

        #endregion


        #region "Properties"

        /// <summary>
        /// Function to retrieve the document page URL
        /// </summary>
        protected GetUrlFunc GetDocumentPageUrl
        {
            get;
            set;
        }

        #endregion


        #region "Page events"

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="page">Page</param>
        /// <param name="getUrl">Function to retrieve the document URL</param>
        public ContentUrlRetriever(Page page, GetUrlFunc getUrl)
        {
            page.PreInit += OnPreInit;

            GetDocumentPageUrl = getUrl;
        }


        /// <summary>
        /// PreInit event handler
        /// </summary>
        protected void OnPreInit(object sender, EventArgs e)
        {
            var context = HttpContext.Current;

            var parameter = context.Request.Params[CALLBACK_PARAMETER];
            if (parameter != null)
            {
                context.Response.Clear();
                context.Response.Write(URL_PREFIX + GetRequestedUrl(parameter, true));
                context.Response.ContentType = "text/plain";
                context.Response.End();
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Gets requested URL for UI page
        /// </summary>
        /// <param name="eventArgument">Event arguments</param>
        /// <param name="ajaxRequest">Indicates if the URL is requested from an AJAX request</param>
        protected string GetRequestedUrl(string eventArgument, bool ajaxRequest = false)
        {
            string result = null;
            if ((eventArgument != null) && eventArgument.StartsWithCSafe(URL_PREFIX))
            {
                // Trim start identifier
                eventArgument = eventArgument.Substring(URL_PREFIX.Length);

                string[] args = eventArgument.Split(new[] { CALLBACK_SEPARATOR }, StringSplitOptions.None);

                var settings = new UIPageURLSettings
                {
                    Mode = ValidationHelper.GetString(args[0], string.Empty).ToLowerCSafe(),
                    Action = ValidationHelper.GetString(args[1], string.Empty).ToLowerCSafe(),
                    NodeID = ValidationHelper.GetInteger(args[2], 0),
                    Culture = ValidationHelper.GetString(args[3], null),
                    DeviceProfile = ValidationHelper.GetString(args[4], null),
                    AdditionalQuery = ValidationHelper.GetString(args[5], null),
                    SplitViewSourceURL = ValidationHelper.GetString(args[6], null),
                    TransformToCompareUrl = ValidationHelper.GetBoolean(args[7], false),
                    IncludeLiveSiteURL = ValidationHelper.GetBoolean(args[8], false)
                };
                
                // Get URL
                result = GetRequestedUrlInternal(settings, ajaxRequest);
            }

            return result;
        }


        /// <summary>
        /// Gets URL based on settings. If <see cref="UIPageURLSettings.IncludeLiveSiteURL" /> is set, also absolute URL of selected node will be included in result.
        /// </summary>
        /// <param name="settings">URL configuration object</param>
        /// <returns>Content URL</returns>
        public string GetRequestedUrl(UIPageURLSettings settings)
        {
            return GetRequestedUrlInternal(settings, false);
        }


        /// <summary>
        /// Gets URL based on settings. If <see cref="UIPageURLSettings.IncludeLiveSiteURL" /> is set, also absolute URL of selected node will be included in result.
        /// </summary>
        /// <param name="settings">URL configuration object</param>
        /// <returns>Content URL</returns>
        /// <param name="ajaxRequest">Indicates if the URL is requested from an AJAX request</param>
        protected string GetRequestedUrlInternal(UIPageURLSettings settings, bool ajaxRequest)
        {
            ResolveModeAndAction(settings);

            string requestedUrl = GetDocumentPageUrl(settings);

            // Resolve URL for AJAX request
            if (ajaxRequest)
            {
                requestedUrl = ScriptHelper.ResolveUrl(requestedUrl);
            }

            if (settings.IncludeLiveSiteURL)
            {
                var node = settings.Node;
                if (node == null)
                {
                    // Get node from any culture to provide URL on non translated node (may cause 404 page not found)
                    var tree = new TreeProvider();
                    node = tree.SelectSingleNode(settings.NodeID, TreeProvider.ALL_CULTURES);
                }

                var liveUrl = DocumentURLProvider.GetPresentationUrl(node, RequestContext.FullDomain);
                liveUrl = URLHelper.AddParameterToUrl(liveUrl, "viewmode", ((int)ViewModeEnum.LiveSite).ToString());

                requestedUrl += String.Format("{0}{1}", CALLBACK_SEPARATOR, liveUrl);
            }

            return requestedUrl;
        }


        /// <summary>
        /// Gets correct action based on given display mode.
        /// </summary>
        /// <param name="settings">URL configuration object</param>
        /// <returns>Returns 'content' action when action is not set and given node corresponds with 'content' action</returns>
        protected void ResolveModeAndAction(UIPageURLSettings settings)
        {
            // Keep empty mode
            if (String.IsNullOrEmpty(settings.Mode))
            {
                return;
            }

            // Do not check virtual page view mode otherwise get current view mode from string value
            ViewModeEnum viewMode = ViewModeEnum.Unknown;
            if (!settings.Mode.EqualsCSafe("page", true))
            {
                viewMode = ViewModeCode.FromString(settings.Mode);
            }

            if (string.IsNullOrEmpty(settings.Action) && ((settings.Mode == "page") || ViewModeCode.IsSubsetOfEditMode(viewMode) || (viewMode == ViewModeEnum.Unknown)))
            {
                settings.Action = "content";

                if (viewMode == ViewModeEnum.Unknown)
                {
                    // Fallback for unknown mode
                    settings.Mode = "edit";
                }
            }
        }

        #endregion
    }
}
