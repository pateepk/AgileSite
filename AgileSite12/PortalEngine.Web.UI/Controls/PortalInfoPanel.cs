using System;
using System.Web.UI;
using System.Web.UI.WebControls;

using CMS.Base.Web.UI;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Membership;
using CMS.SiteProvider;

namespace CMS.PortalEngine.Web.UI
{
    /// <summary>
    /// Portal engine info panel displaying information about:
    ///   - Page in the preview mode outside the administration interface
    ///   - Page in the Safe mode
    ///   - Virtual context user 
    ///   - Missing page placeholder
    /// </summary>
    internal class PortalInfoPanel : Panel, INamingContainer
    {
        #region "Variables"

        private Label lblInfo;
        private string mUserCulture;

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets the preferred UI culture (HTML encoded due to XSS) for current user
        /// </summary>
        private string UserCulture
        {
            get
            {
                return mUserCulture ?? (mUserCulture = HTMLHelper.HTMLEncode(PortalManager.UserCulture));
            }
        }


        /// <summary>
        /// Preview text
        /// </summary>
        private string Message
        {
            get
            {
                return lblInfo.Text;
            }
            set
            {
                lblInfo.Text = value;
            }
        }


        /// <summary>
        /// Parent portal manager
        /// </summary>
        private CMSPortalManager PortalManager
        {
            get;
            set;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Initializes a new instance of the <see cref="PortalInfoPanel"/>.
        /// </summary>
        /// <param name="manager">Parent portal manager</param>
        public PortalInfoPanel(CMSPortalManager manager)
        {
            PortalManager = manager;
        }

        #region "Page events"

        /// <summary>
        /// OnPreRender handler
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPreRender(EventArgs e)
        {
            // Sets information messages
            CreateInformationLabels(PortalManager.ViewMode);

            // Hide panel for empty text
            Visible = !String.IsNullOrEmpty(Message);

            base.OnPreRender(e);
        }


        /// <summary>
        /// Ensures the controls collection for info panel
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            // Preview text
            lblInfo = new Label
            {
                CssClass = "preview-info-text",
                EnableViewState = false,
            };
            Controls.Add(lblInfo);
        }


        /// <summary>
        /// OnLoad event handler
        /// </summary>
        protected override void OnLoad(EventArgs e)
        {
            // InfoPanel css class
            CssClass = "preview-info";

            if (PortalManager.ViewMode.IsLiveSite() ||
                PortalContext.ViewMode.IsEditLive() ||
                !String.IsNullOrEmpty(QueryHelper.GetString("previewobjectname", String.Empty)))
            {
                Visible = false;
            }

            base.OnLoad(e);
        }

        #endregion


        #region "Text methods"

        /// <summary>
        /// Adds the information labels
        /// </summary>
        /// <param name="viewMode">View mode</param>
        private void CreateInformationLabels(ViewModeEnum viewMode)
        {
            bool missingPagePlaceholder = (PortalManager.CMSPagePlaceholders == null) || (PortalManager.CMSPagePlaceholders.Count == 0);

            if (PortalHelper.SafeMode)
            {
                // Display information about the safe mode
                string redirectUrl = URLHelper.RemoveParameterFromUrl(RequestContext.CurrentURL, "safemode");

                Message = String.Format(ResHelper.GetString("ContentEdit.SafeMode", UserCulture), HTMLHelper.EncodeForHtmlAttribute(redirectUrl));
            }
            else if (VirtualContext.IsPreviewLinkInitialized)
            {
                // Display the virtual context user (used in a preview link)
                Message = String.Format(ResHelper.GetString("ContentEdit.UserContext", UserCulture), HTMLHelper.HTMLEncode(MembershipContext.AuthenticatedUser.GetFormattedUserName(true)));
            }
            // Ensure no placeholders message or additional design message
            else if ((viewMode == ViewModeEnum.Design)
                && (missingPagePlaceholder || !String.IsNullOrEmpty(PortalManager.AdditionalDesignMessage)))
            {
                string designModeMessage = PortalManager.AdditionalDesignMessage;

                if (missingPagePlaceholder)
                {
                    designModeMessage = ResHelper.GetString("PortalManager.NoPlaceholders");
                }

                // Append text to the edit object panel
                if (PortalManager.CurrentObjectEditMenu != null)
                {
                    PortalManager.CurrentObjectEditMenu.InformationText = designModeMessage;
                    RegisterShowPanelScript();
                }
                // Or use standard panel
                else
                {
                    Message = designModeMessage;
                }
            }
            else
            {
                // Render view mode message. This message is shown only when page is displayed outside of CMS administration interface.
                Message = GetViewModeText(viewMode);
                RegisterShowPanelScript();
            }
        }


        /// <summary>
        /// Gets the default text with dependence on current view mode
        /// </summary>
        private string GetViewModeText(ViewModeEnum viewMode)
        {
            string previewInfo = String.Empty;

            switch (viewMode)
            {
                // Edit mode 
                case ViewModeEnum.Edit:
                case ViewModeEnum.EditDisabled:
                case ViewModeEnum.EditNotCurrent:
                    {
                        previewInfo = ResHelper.GetString("ContentEdit.EditMode", UserCulture);
                    }
                    break;

                // Preview mode
                case ViewModeEnum.Preview:
                    {
                        previewInfo = ResHelper.GetString("ContentEdit.PreviewMode", UserCulture);
                    }
                    break;

                default:
                    // Preview mode info
                    if (PortalContext.IsDesignMode(viewMode))
                    {
                        previewInfo = ResHelper.GetString("ContentEdit.DesignMode", UserCulture);
                    }
                    break;
            }

            // Complete the preview text
            string documentName = PortalManager.CurrentPageInfo.GetDocumentName();

            return String.Format(previewInfo, HTMLHelper.HTMLEncode(documentName), HTMLHelper.EncodeForHtmlAttribute(GetLiveSiteUrl()));
        }


        /// <summary>
        /// Gets the close link URL
        /// </summary>
        private string GetLiveSiteUrl()
        {
            // Update VieMode parameter from QueryString to Live site view mode
            string url = RequestContext.CurrentURL;
            return URLHelper.UpdateParameterInUrl(url, "viewmode", ((int)ViewModeEnum.LiveSite).ToString());
        }

        #endregion


        /// <summary>
        /// Registers the script which shows the info panel if the page is not displayed within the administration interface and if the preview mode on live-site is enabled.
        /// </summary>
        /// <remarks>Also sets initial state of info panel to hidden.</remarks>
        private void RegisterShowPanelScript()
        {
            // Avoid registering script for dashboard widgets. There is no need to redirect page to livesite or change the panel height. It was causing infinite redirects.
            if (PortalManager.ViewMode == ViewModeEnum.DashboardWidgets)
            {
                return;
            }

            Style.Add(HtmlTextWriterStyle.Display, "none");

            var allowPreviewModePanel = SettingsKeyInfoProvider.GetBoolValue("CMSAllowPreviewMode", SiteContext.CurrentSiteName);

            ScriptHelper.RegisterModule(
                Page,
                "CMS.PortalEngine/PortalInfoPanel",
                new
                {
                    clientID = ClientID,
                    previewModePanelAllowed = allowPreviewModePanel,
                    liveSiteUrl = GetLiveSiteUrl()
                });
            
        }

        #endregion
    }
}
