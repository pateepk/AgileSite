using System;
using System.Threading;

using CMS.Base;
using CMS.Base.Web.UI;
using CMS.DocumentEngine;
using CMS.Helpers;
using CMS.MacroEngine;
using CMS.Membership;
using CMS.PortalEngine.Web.UI;
using CMS.PortalEngine;
using CMS.SiteProvider;

namespace CMS.UIControls
{
    /// <summary>
    /// Base page for editable items
    /// </summary>
    public class CMSAbstractEditablePage : CMSModalPage
    {
        #region "Variables"

        PageInfo mCurrentPageInfo = null;
        WebPartInstance mCurrentWebPartInstance = null;

        #endregion


        #region "Properties"

        /// <summary>
        /// Node ID
        /// </summary>
        public int CurrentNodeID
        {
            get
            {
                return QueryHelper.GetInteger("nodeid", 0);
            }
        }

        /// <summary>
        /// Alias path
        /// </summary>
        public string AliasPath
        {
            get
            {
                return QueryHelper.GetString("aliaspath", String.Empty);
            }
        }


        /// <summary>
        /// Web part ID
        /// </summary>
        public string WebPartId
        {
            get
            {
                return QueryHelper.GetString("webpartid", String.Empty);
            }
        }


        /// <summary>
        /// Instance GUID
        /// </summary>
        public Guid InstanceGUID
        {
            get
            {
                return QueryHelper.GetGuid("instanceguid", Guid.Empty);
            }
        }


        /// <summary>
        /// Culture
        /// </summary>
        public override string  CultureCode
        {
            get
            {
                return QueryHelper.GetString("culture", CultureHelper.GetPreferredCulture());
            }
        }


        /// <summary>
        /// Template ID
        /// </summary>
        public int PageTemplateId
        {
            get
            {
                return QueryHelper.GetInteger("templateid", 0);
            }
        }


        /// <summary>
        /// Zone ID
        /// </summary>
        public string ZoneId
        {
            get
            {
                return QueryHelper.GetString("zoneid", String.Empty);
            }
        }


        /// <summary>
        /// Zone variant ID
        /// </summary>
        public int ZoneVariantId
        {
            get
            {
                return QueryHelper.GetInteger("zonevariantid", 0);
            }
        }


        /// <summary>
        /// Variant ID
        /// </summary>
        public int VariantId
        {
            get
            {
                return QueryHelper.GetInteger("variantid", 0);
            }
        }


        /// <summary>
        /// Variant mode
        /// </summary>
        public VariantModeEnum VariantMode
        {
            get
            {
                return VariantModeFunctions.GetVariantModeEnum(QueryHelper.GetString("variantmode", String.Empty).ToLowerCSafe());
            }
        }


        /// <summary>
        /// Gets the control ID from url
        /// </summary>
        public string ControlID
        {
            get
            {
                return QueryHelper.GetString("controlid", String.Empty);
            }
        }


        /// <summary>
        /// Gets the page title suffix
        /// </summary>
        public string PageTitleSuffix
        {
            get
            {
                // Set page title
                string title = String.Empty;
                if (CurrentWebPartInstance != null)
                {
                    // Set title
                    title = ValidationHelper.GetString(CurrentWebPartInstance.GetValue("RegionTitle"), String.Empty);
                    if (String.IsNullOrEmpty(title))
                    {
                        title = ValidationHelper.GetString(CurrentWebPartInstance.GetValue("WebPartTitle"), String.Empty);
                        if (String.IsNullOrEmpty(title))
                        {
                            title = ValidationHelper.GetString(CurrentWebPartInstance.GetValue("ControlID"), String.Empty);
                        }
                    }
                }

                return title;
            }
        }

        #endregion


        #region "HTML area properties"

        /// <summary>
        /// Gets the current page info
        /// </summary>
        public PageInfo CurrentPageInfo
        {
            get
            {
                if (mCurrentPageInfo == null)
                {
                    // Get the page info
                    PageInfo pi = PageInfoProvider.GetPageInfo(SiteContext.CurrentSiteName, AliasPath, CultureCode, String.Empty, CurrentNodeID, false);
                    // Load version
                    pi.LoadVersion(DocumentManager.Node);
                    mCurrentPageInfo = pi;
                }
                return mCurrentPageInfo;
            }
        }


        /// <summary>
        /// Gets the current web part instance
        /// </summary>
        public WebPartInstance CurrentWebPartInstance
        {
            get
            {
                if ((mCurrentWebPartInstance == null) && (CurrentPageInfo != null))
                {
                    // Get template
                    PageTemplateInfo pti = CurrentPageInfo.UsedPageTemplateInfo;
                    if (pti != null)
                    {
                        WebPartInstance webPartInstance = null;

                        if (ZoneVariantId > 0)
                        {
                            // Zone variant
                            WebPartZoneInstance wpzi = pti.TemplateInstance.GetZone(ZoneId, ZoneVariantId);
                            webPartInstance = wpzi.GetWebPart(InstanceGUID);
                        }
                        else
                        {
                            // Standard zone
                            webPartInstance = pti.TemplateInstance.GetWebPart(InstanceGUID, WebPartId);
                            
                            // If instance not found try to find in document (widget)
                            if ((webPartInstance == null) && (CurrentPageInfo.DocumentTemplateInstance != null))
                            {
                                webPartInstance = CurrentPageInfo.DocumentTemplateInstance.GetWebPart(InstanceGUID, WebPartId);
                            }
                        }

                        if ((VariantId > 0) && (webPartInstance != null) && (webPartInstance.PartInstanceVariants != null))
                        {
                            // Check OnlineMarketing permissions.
                            if (CheckMarketingPermissions("Read"))
                            {
                                webPartInstance = webPartInstance.FindVariant(VariantId);
                            }
                            else
                            {
                                // Not authorized for OnlineMarketing - Manage.
                                RedirectToInformation(String.Format(GetString("general.permissionresource"), "Read", (VariantMode == VariantModeEnum.ContentPersonalization) ? "CMS.ContentPersonalization" : "CMS.MVTest"));
                            }
                        }

                        mCurrentWebPartInstance = webPartInstance;
                    }
                }

                return mCurrentWebPartInstance;
            }
        }

        #endregion


        #region "Private properties"

        /// <summary>
        /// Indicates whether the control is used in the ASPX or Portal engine mode
        /// </summary>
        private bool IsASPXMode
        {
            get
            {
                return string.IsNullOrEmpty(WebPartId);
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// PreInit event handler
        /// </summary>
        protected override void OnPreInit(EventArgs e)
        {
            EnsureDocumentManager = true;
            base.OnPreInit(e);
        }


        /// <summary>
        /// Init event handler
        /// </summary>
        protected override void OnInit(EventArgs e)
        {
            // Handle document manager actions
            DocumentManager.OnSaveData += new EventHandler<DocumentManagerEventArgs>(DocumentManager_OnSaveData);
            
            // Set Document manager properties
            DocumentManager.NodeID = CurrentNodeID;
            DocumentManager.CultureCode = CultureCode;

            DocumentManager.SetRefreshFlag = true;

            // Load editable content
            LoadContent();

            base.OnInit(e);

            CssRegistration.RegisterCssLink(Page, "Design", "OnSiteEdit.css");
        }


        /// <summary>
        /// Saves the control content.
        /// </summary>
        public void SaveContent(PageInfo pageInfo)
        {
            if (pageInfo == null)
            {
                return;
            }

            // Get the content
            string content = GetContent();

            // If content == null, not allowed to change
            if (content != null)
            {
                // Prepare the ID
                string id = this.ID.ToLowerCSafe();
                if (CurrentWebPartInstance != null)
                {
                    id = CurrentWebPartInstance.ControlID.ToLowerCSafe();
                }
                else if (IsASPXMode)
                {
                    id = ControlID;
                }

                if ((CurrentWebPartInstance != null) && (CurrentWebPartInstance.CurrentVariantInstance != null))
                {
                    if (CurrentWebPartInstance.CurrentVariantInstance.ControlID != CurrentWebPartInstance.ControlID)
                    {
                        return;
                    }
                }

                if (InstanceGUID != Guid.Empty)
                {
                    id += ";" + InstanceGUID.ToString().ToLowerCSafe();
                    if (CurrentWebPartInstance != null)
                    {
                        if (CurrentWebPartInstance.CurrentVariantInstance != null)
                        {
                            id += "(" + CurrentWebPartInstance.CurrentVariantInstance.ControlID + ")";
                        }
                        else if (CurrentWebPartInstance.IsWidget && CurrentWebPartInstance.IsVariant)
                        {
                            id += "(" + CurrentWebPartInstance.ControlID + ")";
                        }
                    }
                }

                // Save the value
                if (!IsASPXMode)
                {
                    // Save into the editable web parts in the Portal engine mode
                    pageInfo.EditableWebParts[id] = content;
                }
                else
                {
                    // Save into the editable regions in the ASPX mode
                    pageInfo.EditableRegions[id] = content;
                }
            }
        }


        /// <summary>
        /// Loads region content
        /// </summary>
        protected void LoadContent()
        {
            // Get the page info
            PageInfo pi = CurrentPageInfo;
            if (pi != null)
            {
                string id = string.Empty;

                if (!IsASPXMode)
                {
                    // Get web part instance
                    WebPartInstance webPartInstance = CurrentWebPartInstance;
                    // Check whether instance is defined
                    if (webPartInstance == null)
                    {
                        ShowInformation(GetString("WebPartProperties.WebPartNotFound"));
                        return;
                    }

                    id = WebPartId.ToLowerCSafe() + ";" + InstanceGUID.ToString().ToLowerCSafe();
                }
                else
                {
                    id = ControlID;
                }

                string content = pi.EditableItems[id];

                LoadContent(content, true);
            }
        }


        /// <summary>
        /// Gets editable content
        /// </summary>
        public virtual string GetContent()
        {
            throw new Exception("[CMSAbstractEditablePage] GetContent method must be implemented on child page");
        }


        /// <summary>
        /// Loads editable content
        /// </summary>
        /// <param name="content">Content text</param>
        /// <param name="forceReload">Indicates whether content should be loaded always</param>
        public virtual void LoadContent(string content, bool forceReload)
        {
            throw new Exception("[CMSAbstractEditablePage] LoadContent method must be implemented on child page");
        }


        /// <summary>
        /// Check view mode and document permissions
        /// </summary>
        /// <returns>Returns new viewmode if changed</returns>
        protected ViewModeEnum CheckPermissions()
        {
            ViewModeEnum viewMode = ViewModeEnum.Edit;
            // Check permissions for current document
            if (!CMSPortalManager.IsAuthorizedPerDocument(DocumentManager.Node, viewMode))
            {
                RedirectToAccessDenied("onsite.notallowedtoedit");
            }
            // Check workflow step
            else
            {
                viewMode = PortalHelper.GetWorkflowViewMode(DocumentManager.Node, DocumentManager, viewMode);
            }

            return viewMode;
        }


        /// <summary>
        /// Checks permissions (depends on variant mode) 
        /// </summary>
        /// <param name="permissionName">Permission name</param>
        protected bool CheckMarketingPermissions(string permissionName)
        {
            var cui = MembershipContext.AuthenticatedUser;

            switch (VariantMode)
            {
                case VariantModeEnum.MVT:
                    return cui.IsAuthorizedPerResource("cms.mvtest", permissionName);

                case VariantModeEnum.ContentPersonalization:
                    return cui.IsAuthorizedPerResource("cms.contentpersonalization", permissionName);

                case VariantModeEnum.Conflicted:
                case VariantModeEnum.None:
                    return cui.IsAuthorizedPerResource("cms.mvtest", permissionName) || cui.IsAuthorizedPerResource("cms.contentpersonalization", permissionName);
            }

            return true;
        }


        /// <summary>
        /// Save changes
        /// </summary>
        private void DocumentManager_OnSaveData(object sender, DocumentManagerEventArgs e)
        {
            SaveContent(CurrentPageInfo);
            DocumentManager.Node.SetValue("DocumentContent", CurrentPageInfo.DocumentContent);
        }


        /// <summary>
        /// Resolves the given macro when macro resolving is enabled in the web part properties. 
        /// </summary>
        /// <param name="value">Macro to be resolved</param>
        protected object ResolveMacros(object value)
        {
            if (value is string)
            {
                bool disableMacros = (CurrentWebPartInstance == null) || ValidationHelper.GetBoolean(CurrentWebPartInstance.GetValue("DisableMacros"), false);
                if (!disableMacros)
                {
                    // Resolve properties macros
                    var resolver = MacroContext.CurrentResolver.CreateChild();
                    resolver.Culture = Thread.CurrentThread.CurrentCulture.ToString();

                    return resolver.ResolveMacros(value.ToString());
                }
            }

            return value;
        }

        #endregion
    }
}
