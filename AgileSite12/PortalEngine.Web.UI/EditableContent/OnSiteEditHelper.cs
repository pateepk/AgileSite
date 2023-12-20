using System.Text;

using CMS.Base;
using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.Helpers;
using CMS.SiteProvider;
using CMS.Synchronization;

namespace CMS.PortalEngine.Web.UI
{
    /// <summary>
    /// On-site editing helper methods
    /// </summary>
    public class OnSiteEditHelper
    {
        #region "Public properties"

        /// <summary>
        /// Gets the full url to the web part properties page (WebPartProperties.aspx).
        /// </summary>
        /// <value>The web part properties page.</value>
        public static string WebPartPropertiesPageUrl
        {
            get
            {
                return URLHelper.ResolveUrl("~/CMSModules/PortalEngine/UI/Webparts/WebPartProperties.aspx");
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Returns a string that contains URL parameters needed to open Edit dialog for editable controls.
        /// </summary>
        /// <param name="pageInfo">Current page info</param>
        /// <param name="webPartInstance">Web part instance</param>
        internal static string GetEditDialogURLParameters(PageInfo pageInfo, WebPartInstance webPartInstance)
        {
            var urlParams = new StringBuilder();

            if (pageInfo == null)
            {
                return string.Empty;
            }

            urlParams.Append(
                "nodeid=", pageInfo.NodeID,
                "&aliaspath=", pageInfo.NodeAliasPath,
                "&culture=", pageInfo.DocumentCulture
            );

            if (webPartInstance == null)
            {
                return urlParams.ToString();
            }

            urlParams.Append(
                "&webpartid=", webPartInstance.ControlID,
                "&instanceguid=", webPartInstance.InstanceGUID,
                "&zoneid=", webPartInstance.ParentZone.ZoneID,
                "&templateid=", pageInfo.UsedPageTemplateInfo.PageTemplateId,
                "&zonevariantid=", webPartInstance.ParentZone.VariantID,
                "&variantid=", webPartInstance.VariantID
            );

            if ((webPartInstance.ParentZone.VariantID > 0) || (webPartInstance.VariantID > 0))
            {
                VariantModeEnum variantMode = ((webPartInstance.VariantID > 0) ? webPartInstance.VariantMode : webPartInstance.ParentZone.VariantMode);
                if (webPartInstance.VariantMode != VariantModeEnum.None)
                {
                    urlParams.Append("&variantmode=", VariantModeFunctions.GetVariantModeString(variantMode));
                }
            }

            return urlParams.ToString();
        }


        /// <summary>
        /// Creates URL for edit dialog
        /// </summary>
        /// <param name="editPageUrl">Basic edit URL</param>
        /// <param name="pageInfo">Current page info</param>
        /// <param name="webPartInstance">Web part instance</param>
        public static string GetEditDialogURL(string editPageUrl, PageInfo pageInfo, WebPartInstance webPartInstance)
        {
            string urlParams = GetEditDialogURLParameters(pageInfo, webPartInstance);
            return URLHelper.AppendQuery(editPageUrl, urlParams);
        }


        /// <summary>
        /// Gets the start tag for the on-site editing envelope. It's rendered if the user specified in property <see cref="OnSiteEditStartTagConfiguration.CurrentUser" /> of the <paramref name="tagConfig" /> parameter is either authorized to edit the specified page or is a designer.
        /// </summary>
        /// <param name="tagConfig">The object contains configuration data of how the On-site Edit start tag will be rendered.</param>
        public static string GetOnSiteEditStartTag(OnSiteEditStartTagConfiguration tagConfig)
        {
            var currentUser = tagConfig.CurrentUser;
            var pageInfo = tagConfig.Page;

            if ((pageInfo == null) || (currentUser == null))
            {
                return string.Empty;
            }

            // Check permissions whether the current user is an editor and can edit this document
            bool documentModify = (currentUser.IsAuthorizedPerTreeNode(pageInfo.NodeID, NodePermissionsEnum.Modify) != AuthorizationResultEnum.Denied);
            bool userIsDocumentEditor = documentModify && currentUser.CheckPrivilegeLevel(UserPrivilegeLevelEnum.Editor, SiteContext.CurrentSiteName);

            StringBuilder tagBuilder = new StringBuilder();

            // Generate the span only when there is something to display (either properties url or edit page url or both). If On-Site tag is rendered for an invisible web part then the web part will be displayed in the Hidden menu.
            // User must be either global admin, content editor permitted to edit current page, or designer.
            if (PortalContext.CurrentUserIsDesigner || (userIsDocumentEditor && !string.IsNullOrEmpty(tagConfig.EditUrl) && tagConfig.WebPartIsEditable))
            {
                var webPartInstance = tagConfig.WebPartInstance;
                var urlParams = new StringBuilder();
                bool isASPXMode = (webPartInstance == null);

                // Setup the opening span tag
                tagBuilder.Append("<span class=\"OnSiteWebPartBegin\"");

                // Add common URL parameter to display a dialog
                urlParams.Append(GetEditDialogURLParameters(pageInfo, webPartInstance));

                if (!isASPXMode)
                {
                    // Portal engine mode
                    tagBuilder.Append(" id=\"OE_", pageInfo.DocumentID, "_", webPartInstance.ControlID, "\" data-title=\"", GetWebPartTitle(tagConfig, webPartInstance.ControlID), "\"");

                    bool showProperties = !SynchronizationHelper.UseCheckinCheckout || pageInfo.UsedPageTemplateInfo.Generalized.IsCheckedOutByUser(currentUser);

                    // Render properties if the user is designer
                    if (showProperties && PortalContext.CurrentUserIsDesigner)
                    {
                        string webPartPropertiesPageUrl = WebPartPropertiesPageUrl;
                        webPartPropertiesPageUrl = URLHelper.AppendQuery(webPartPropertiesPageUrl, urlParams.ToString());
                        webPartPropertiesPageUrl = HTMLHelper.HTMLEncode(webPartPropertiesPageUrl);
                        tagBuilder.Append(" data-propertiespageurl=\"", webPartPropertiesPageUrl, "\"");
                    }
                }
                else
                {
                    // ASPX mode
                    if (tagConfig.ControlObject is CMSAbstractEditableControl)
                    {
                        var editableControl = (CMSAbstractEditableControl)tagConfig.ControlObject;
                        string controlId = editableControl.ID;

                        urlParams.Append("&controlid=", controlId);
                        tagBuilder.AppendFormat(@" id=""OE_{0}_{1}"" data-title=""{2}""", pageInfo.DocumentID, controlId, GetWebPartTitle(tagConfig, controlId));

                        // Append dialog url parameters if defined
                        string[] editableControlParams = editableControl.GetEditDialogParameters();
                        if (editableControlParams != null)
                        {
                            // Append dialog url parameters
                            foreach (string urlParam in editableControlParams)
                            {
                                urlParams.Append("&", urlParam);
                            }
                        }
                    }
                }

                // Render the URL of the page which ensures editing of the editable content
                if (userIsDocumentEditor && tagConfig.WebPartIsEditable)
                {
                    var editPageUrl = tagConfig.EditUrl;
                    editPageUrl = URLHelper.AppendQuery(editPageUrl, urlParams.ToString());
                    editPageUrl = HTMLHelper.HTMLEncode(editPageUrl);
                    tagBuilder.Append(" data-editpageurl=\"", editPageUrl, "\"");

                    if (!tagConfig.DialogWidth.IsEmpty)
                    {
                        tagBuilder.Append(" data-dialogwidth=\"", tagConfig.DialogWidth.ToString(), "\"");
                    }
                }

                // Close the span tag
                tagBuilder.Append("></span>");
            }

            return tagBuilder.ToString();
        }


        private static string GetWebPartTitle(OnSiteEditStartTagConfiguration tagConfig, string controlID)
        {
            return HTMLHelper.HTMLEncode(!string.IsNullOrEmpty(tagConfig.WebPartTitle) ? tagConfig.WebPartTitle : controlID);
        }


        /// <summary>
        /// Gets the end tag for the on-site editing envelope.
        /// </summary>
        public static string GetOnSiteEditEndTag()
        {
            return "<span class=\"OnSiteWebPartEnd\"></span>";
        }


        /// <summary>
        /// Gets the empty editable web part span tag.
        /// </summary>
        /// <param name="webPartTitle">The web part title</param>
        public static string GetEmptyEditableWebPartTag(string webPartTitle)
        {
            return "<span class=\"OEEmptyEditableWebPart\">" + HTMLHelper.HTMLEncode(ResHelper.GetStringFormat("onsiteedit.emptyeditablewebpart", webPartTitle)) + "</span>";
        }

        #endregion
    }
}