using System;

using CMS.DocumentEngine;
using CMS.Helpers;
using CMS.Localization;
using CMS.Membership;
using CMS.PortalEngine.Web.UI;
using CMS.PortalEngine;
using CMS.SiteProvider;

namespace CMS.UIControls
{
    /// <summary>
    /// Base page for the properties pages to apply global settings to the pages.
    /// </summary>
    public abstract class CMSWebPartPropertiesPage : CMSDesignPage
    {
        #region "Variables"

        /// <summary>
        /// Alias path
        /// </summary>
        protected string aliasPath = QueryHelper.GetString("aliaspath", "");
        
        /// <summary>
        /// web part id
        /// </summary>
        protected string webpartId = QueryHelper.GetString("webpartid", "");
        
        /// <summary>
        /// Zone id
        /// </summary>
        protected string zoneId = QueryHelper.GetString("zoneid", "");
        
        /// <summary>
        /// Instance GUID
        /// </summary>
        protected Guid instanceGuid = QueryHelper.GetGuid("instanceguid", Guid.Empty);
        
        /// <summary>
        /// Template id
        /// </summary>
        protected int templateId = QueryHelper.GetInteger("templateid", 0);
        
        /// <summary>
        /// Indicates whether is creating new web part
        /// </summary>
        protected bool isNew = QueryHelper.GetBoolean("isnew", false);
        
        /// <summary>
        /// Culture code
        /// </summary>
        protected string cultureCode = QueryHelper.GetString("culture", LocalizationContext.PreferredCultureCode);

        /// <summary>
        /// Position
        /// </summary>
        protected int position = -1;

        /// <summary>
        /// Position left
        /// </summary>
        protected int positionLeft = 0;
        
        /// <summary>
        /// Position top
        /// </summary>
        protected int positionTop = 0;

        /// <summary>
        /// Indicates whether is new variant
        /// </summary>
        protected bool isNewVariant = QueryHelper.GetBoolean("isnewvariant", false);
        
        /// <summary>
        /// Variant id
        /// </summary>
        protected int variantId = QueryHelper.GetInteger("variantid", 0);
        
        /// <summary>
        /// Zone variant id
        /// </summary>
        protected int zoneVariantId = QueryHelper.GetInteger("zonevariantid", 0);
        
        /// <summary>
        /// Variant mode
        /// </summary>
        protected VariantModeEnum variantMode = VariantModeFunctions.GetVariantModeEnum(QueryHelper.GetString("variantmode", string.Empty));
        
        /// <summary>
        /// Frames manager
        /// </summary>
        protected WebPartFramesManager mFramesManager = null;

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

        #endregion


        #region "Methods"

        /// <summary>
        /// PreInit event handler.
        /// </summary>
        protected override void OnPreInit(EventArgs e)
        {
            IsDialog = true;

            base.OnPreInit(e);

            // Check standard design permissions
            var currentUser = MembershipContext.AuthenticatedUser;
            if (!currentUser.IsAuthorizedPerUIElement("CMS.Design", new string[] { "Design", "Design.WebPartProperties" }, SiteContext.CurrentSiteName))
            {
                RedirectToUIElementAccessDenied("CMS.Design", "Design;Design.WebPartProperties");
            }

            // Check content design
            if (!currentUser.IsAuthorizedPerResource("CMS.Design", "Design"))
            {
                RedirectToAccessDenied("CMS.Design", "Design");
            }

            // Load the position arguments
            string pos = QueryHelper.GetString("position", "");
            string[] posItems = pos.Split('|');

            position = ValidationHelper.GetInteger(posItems[0], -1);
            if (posItems.Length >= 3)
            {
                positionLeft = ValidationHelper.GetInteger(posItems[1], 0);
                positionTop = ValidationHelper.GetInteger(posItems[2], 0);
            }

            RegisterEscScript();

            // When displaying an existing variant of a web part, get the variant mode for its original web part
            if (variantId > 0)
            {
                // Get page info
                PageInfo pi = GetPageInfo(aliasPath, templateId, cultureCode);
                if ((pi != null) && (pi.UsedPageTemplateInfo != null) && ((pi.UsedPageTemplateInfo.TemplateInstance != null)))
                {
                    // Ensure that all the MVT/Content personalization variants are loaded
                    pi.UsedPageTemplateInfo.TemplateInstance.LoadVariants(false, VariantModeEnum.None);

                    // Get the original webpart and retrieve its variant mode
                    WebPartInstance webpartInstance = pi.UsedPageTemplateInfo.TemplateInstance.GetWebPart(instanceGuid, zoneVariantId, 0);
                    if (webpartInstance != null)
                    {
                        variantMode = webpartInstance.VariantMode;
                    }
                }
            }

            // When displaying an existing variant of a zone, get the variant mode for its original zone
            if ((zoneVariantId > 0) && (variantMode == VariantModeEnum.None))
            {
                // Get page info
                PageInfo pi = GetPageInfo(aliasPath, templateId, cultureCode);
                if ((pi != null) && (pi.UsedPageTemplateInfo != null) && ((pi.UsedPageTemplateInfo.TemplateInstance != null)))
                {
                    // Get the original zone and retrieve its variant mode
                    WebPartZoneInstance zoneInstance = pi.UsedPageTemplateInfo.TemplateInstance.GetZone(zoneId);
                    if ((zoneInstance != null) && (zoneInstance.VariantMode != VariantModeEnum.None))
                    {
                        variantMode = zoneInstance.VariantMode;
                    }
                }
            }
        }


        /// <summary>
        /// OnInit event.
        /// </summary>
        protected override void OnInit(EventArgs e)
        {
 	        base.OnInit(e);

            // Add the FramesManager object to the controls collection
            mFramesManager = new WebPartFramesManager();
            if ((CurrentMaster != null) && (CurrentMaster.PanelContent != null))
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


        /// <summary>
        /// Gets the page info for the properties page.
        /// </summary>
        /// <param name="aliasPath">Alias path</param>
        /// <param name="templateId">Template ID</param>
        public static PageInfo GetPageInfo(string aliasPath, int templateId)
        {
            return GetPageInfo(aliasPath, templateId, LocalizationContext.PreferredCultureCode);
        }


        /// <summary>
        /// Gets the page info for the properties page.
        /// </summary>
        /// <param name="aliasPath">Alias path</param>
        /// <param name="templateId">Template ID</param>
        /// <param name="culture">Preferred culture to use along with alias path</param>
        public static PageInfo GetPageInfo(string aliasPath, int templateId, string culture)
        {
            PageInfo pi = null;
            if (String.IsNullOrEmpty(aliasPath))
            {
                // Virtual page info for the design mode out of context
                pi = PageInfoProvider.GetVirtualPageInfo(templateId);
            }
            else
            {
                // Get page info for the given document
                pi = PageInfoProvider.GetPageInfo(SiteContext.CurrentSiteName, aliasPath, culture, null, SiteContext.CurrentSite.CombineWithDefaultCulture);
            }

            return pi;
        }

        #endregion
    }
}