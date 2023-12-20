using System;
using System.Linq;
using System.Web;

using CMS.DocumentEngine;
using CMS.Helpers;
using CMS.Localization;
using CMS.PortalEngine.Web.UI;
using CMS.PortalEngine;

namespace CMS.UIControls
{
    /// <summary>
    /// Base page for the properties pages to apply global settings to the pages.
    /// </summary>
    public abstract class CMSWidgetPropertiesPage : CMSDeskPage
    {
        #region "Variables"

        private PageInfo mPageInfo;
        private bool? pageInfoFound;
        private TreeNode mNode;

        /// <summary>
        /// Widget id
        /// </summary>
        protected string widgetId = QueryHelper.GetString("widgetid", String.Empty);

        /// <summary>
        /// Widget name
        /// </summary>
        protected string widgetName = QueryHelper.GetString("widgetname", "");

        /// <summary>
        /// Alias path
        /// </summary>
        protected string aliasPath = HttpUtility.UrlDecode(QueryHelper.GetString("aliasPath", String.Empty));

        /// <summary>
        /// Template id
        /// </summary>
        protected int templateId = QueryHelper.GetInteger("templateid", 0);

        /// <summary>
        /// Zone Id
        /// </summary>
        protected string zoneId = QueryHelper.GetString("zoneid", String.Empty);

        /// <summary>
        /// Zone type
        /// </summary>
        protected WidgetZoneTypeEnum zoneType = QueryHelper.GetString("zonetype", "").ToEnum<WidgetZoneTypeEnum>();

        /// <summary>
        /// Instance GUID
        /// </summary>
        protected Guid instanceGuid = QueryHelper.GetGuid("instanceguid", Guid.Empty);

        /// <summary>
        /// Indicates whether is new widget
        /// </summary>
        protected bool isNewWidget = QueryHelper.GetBoolean("isnew", false);

        /// <summary>
        /// Indicates whether is inline widget
        /// </summary>
        protected bool inline = QueryHelper.GetBoolean("inline", false);

        /// <summary>
        /// Indicates whether is new variant
        /// </summary>
        protected bool isNewVariant = QueryHelper.GetBoolean("isnewvariant", false);

        /// <summary>
        /// Variant id
        /// </summary>
        protected int variantId = QueryHelper.GetInteger("variantid", 0);

        /// <summary>
        /// Variant mode
        /// </summary>
        protected VariantModeEnum variantMode = VariantModeFunctions.GetVariantModeEnum(QueryHelper.GetString("variantmode", string.Empty));

        /// <summary>
        /// Culture
        /// </summary>
        protected string culture = QueryHelper.GetString("culture", LocalizationContext.PreferredCultureCode);

        /// <summary>
        /// Frames manager
        /// </summary>
        protected WebPartFramesManager mFramesManager;

        #endregion


        #region "Protected properties"

        /// <summary>
        /// Gets the frames manager which ensures communication between frames in a modal dialog.
        /// </summary>
        protected WebPartFramesManager FramesManager
        {
            get
            {
                return mFramesManager;
            }
        }


        /// <summary>
        /// Gets the current page info.
        /// </summary>
        protected PageInfo PageInfo
        {
            get
            {
                if ((mPageInfo == null) && (pageInfoFound == null))
                {
                    mPageInfo = CMSWebPartPropertiesPage.GetPageInfo(aliasPath, templateId, culture);

                    pageInfoFound = (mPageInfo != null);

                    // For page info with initialized document load versioned data (not for UI page info)
                    if ((mPageInfo != null) && mPageInfo.IsDocument)
                    {
                        mPageInfo.LoadVersion(Node);
                        PortalContext.ViewMode = PortalHelper.GetWorkflowViewMode(Node, DocumentManager, PortalContext.ViewMode);
                    }
                }

                return mPageInfo;
            }
        }


        /// <summary>
        /// Gets the current document.
        /// </summary>
        protected new TreeNode Node
        {
            get
            {
                return mNode ?? (mNode = GetDocument());
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// PreInit event handler.
        /// </summary>
        protected override void OnPreInit(EventArgs e)
        {
            base.OnPreInit(e);

            RequireSite = false;

            RegisterEscScript();
        }


        /// <summary>
        /// OnInit event.
        /// </summary>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            // Add the FramesManager object to the controls collection
            mFramesManager = new WebPartFramesManager();
            if (CurrentMaster?.PanelContent != null)
            {
                // Page can contain <%...%> tags
                CurrentMaster.PanelContent.Controls.Add(mFramesManager);
            }
            else
            {
                // Page cannot contain <%...%> tags
                Controls.Add(mFramesManager);
            }
        }


        private TreeNode GetDocument()
        {
            return DocumentHelper.GetDocuments()
                                 .Path(aliasPath)
                                 .Culture(culture)
                                 .CombineWithDefaultCulture()
                                 .OnCurrentSite()
                                 .WithCoupledColumns(false)
                                 .TopN(1)
                                 .FirstOrDefault();
        }

        #endregion
    }
}