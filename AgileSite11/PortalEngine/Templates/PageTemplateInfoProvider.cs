using System;
using System.Collections.Generic;
using System.Data;
using System.Web;

using CMS.Base;
using CMS.DataEngine;
using CMS.DataEngine.Query;
using CMS.Helpers;
using CMS.IO;
using CMS.SiteProvider;

namespace CMS.PortalEngine
{
    using TypedDataSet = InfoDataSet<PageTemplateInfo>;

    /// <summary>
    /// Provides access to information about page templates.
    /// </summary>
    public class PageTemplateInfoProvider : AbstractInfoProvider<PageTemplateInfo, PageTemplateInfoProvider>
    {
        #region "Variables"

        /// <summary>
        /// ID from which start the virtual page templates for web parts
        /// </summary>
        public const int WEBPART_TEMPLATE_STARTID = 1000000000;


        private static bool? mClonePageTemplateInfo;


        /// <summary>
        /// Template layouts directory.
        /// </summary>
        private const string TEMPLATE_LAYOUTS_DIRECTORY = "~/CMSVirtualFiles/Templates/Shared";


        /// <summary>
        /// Ad-hoc template layouts directory.
        /// </summary>
        private const string ADHOC_TEMPLATE_LAYOUTS_DIRECTORY = "~/CMSVirtualFiles/Templates/AdHoc";


        /// <summary>
        /// Template layouts directory.
        /// </summary>
        private const string TEMPLATES_DIRECTORY = "~/CMSTemplates";


        /// <summary>
        /// Indicates whether search task should be created if template was changed.
        /// </summary>
        private static bool? mCreateSearchTasks;

        #endregion


        #region "Public properties"

        /// <summary>
        /// Gets or sets the value that indicates whether page template/device layouts should be stored externally
        /// </summary>
        public static bool StorePageTemplatesInExternalStorage
        {
            get
            {
                return SettingsKeyInfoProvider.GetBoolValue("CMSStorePageTemplatesInFS");
            }
            set
            {
                SettingsKeyInfoProvider.SetGlobalValue("CMSStorePageTemplatesInFS", value);
            }
        }


        /// <summary>
        /// Gets or sets the value that indicates whether search tasks should be created if template was changed.
        /// </summary>
        public static bool CreateSearchTasks
        {
            get
            {
                if (mCreateSearchTasks == null)
                {
                    mCreateSearchTasks = ValidationHelper.GetBoolean(SettingsHelper.AppSettings["CMSCreateTemplateSearchTasks"], false);
                }
                return mCreateSearchTasks.Value;
            }
            set
            {
                mCreateSearchTasks = value;
            }
        }


        /// <summary>
        /// If true, page info which is retrieved by the methods is a clone of the original page info object.
        /// </summary>
        public static bool ClonePageTemplateInfo
        {
            get
            {
                if (mClonePageTemplateInfo == null)
                {
                    mClonePageTemplateInfo = ValidationHelper.GetBoolean(SettingsHelper.AppSettings["CMSClonePageTemplateInfo"], false);
                }

                return mClonePageTemplateInfo.Value;
            }
            set
            {
                mClonePageTemplateInfo = value;
            }
        }


        /// <summary>
        /// Gets the template layouts directory.
        /// </summary>
        public static string TemplateLayoutsDirectory
        {
            get
            {
                return TEMPLATE_LAYOUTS_DIRECTORY;
            }
        }


        /// <summary>
        /// Gets the ad-hoc template layouts directory.
        /// </summary>
        public static string AdhocTemplateLayoutsDirectory
        {
            get
            {
                return ADHOC_TEMPLATE_LAYOUTS_DIRECTORY;
            }
        }


        /// <summary>
        /// Gets the template layouts directory.
        /// </summary>
        public static string TemplatesDirectory
        {
            get
            {
                return TEMPLATES_DIRECTORY;
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor.
        /// </summary>
        public PageTemplateInfoProvider()
            : base(PageTemplateInfo.TYPEINFO, new HashtableSettings
				{
					ID = true,
					Name = true
				})
        {
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Sets the specified page template.
        /// </summary>
        /// <param name="template">Page template object</param>
        public static void SetPageTemplateInfo(PageTemplateInfo template)
        {
            ProviderObject.SetInfo(template);
        }


        /// <summary>
        /// Gets all templates in specified category.
        /// </summary>
        /// <param name="categoryId">Parent category id. Use categoryId = 0 for templates under root</param>
        public static TypedDataSet GetTemplatesInCategory(int categoryId)
        {
            string where;
            if (categoryId > 0)
            {
                where = "(PageTemplateCategoryID=" + categoryId + ")";
            }
            else
            {
                where = "(PageTemplateCategoryID IS NULL)";
            }
            return GetTemplates().Where(where).OrderBy("PageTemplateDisplayName").Columns("PageTemplateID, PageTemplateDisplayName, PageTemplateCodeName, PageTemplateIsReusable").BinaryData(true).TypedResult;
        }


        /// <summary>
        /// Gets the page template object.
        /// </summary>
        /// <param name="templateName">Page template name</param>
        public static PageTemplateInfo GetPageTemplateInfo(string templateName)
        {
            return GetPageTemplateInfo(templateName, 0);
        }


        /// <summary>
        /// Gets the page template object.
        /// </summary>
        /// <param name="templateName">Page template name</param>
        /// <param name="siteId">Site ID</param>
        public static PageTemplateInfo GetPageTemplateInfo(string templateName, int siteId)
        {
            PageTemplateInfo result = ProviderObject.GetInfoByCodeName(templateName, siteId);

            // Return the result
            if (result == null)
            {
                return null;
            }
            else if (ClonePageTemplateInfo)
            {
                return result.Clone();
            }
            else
            {
                return result;
            }
        }


        /// <summary>
        /// Deletes all ad hoc templates for given node (UI element) based on it's GUID.
        /// </summary>
        /// <param name="guid">Item's guid</param>
        /// <param name="siteId">Site ID</param>
        public static void DeleteAdHocTemplates(Guid guid, int siteId = ProviderHelper.ALL_SITES)
        {
            ProviderObject.DeleteAdHocTemplatesInternal(guid, siteId);
        }


        /// <summary>
        /// Gets the page template object.
        /// </summary>
        /// <param name="templateId">Page template ID</param>
        public static PageTemplateInfo GetPageTemplateInfo(int templateId)
        {
            // Handle the virtual page template from web part
            if (templateId > WEBPART_TEMPLATE_STARTID)
            {
                var wpi = WebPartInfoProvider.GetWebPartInfo(templateId - WEBPART_TEMPLATE_STARTID);
                if (wpi == null)
                {
                    return null;
                }

                return wpi.GetVirtualPageTemplate();
            }

            PageTemplateInfo result = ProviderObject.GetInfoById(templateId);

            // Return the result
            if (result == null)
            {
                return null;
            }
            else if (ClonePageTemplateInfo)
            {
                return result.Clone();
            }
            else
            {
                return result;
            }
        }


        /// <summary>
        /// Adds page template to specific site.
        /// </summary>
        /// <param name="templateId">Page template ID</param>
        /// <param name="siteId">Site ID</param>
        public static void AddPageTemplateToSite(int templateId, int siteId)
        {
            PageTemplateSiteInfoProvider.AddPageTemplateToSite(templateId, siteId);
        }


        /// <summary>
        /// Removes page template from site.
        /// </summary>
        /// <param name="templateId">Page template ID</param>
        /// <param name="siteId">Site ID</param>
        public static void RemovePageTemplateFromSite(int templateId, int siteId)
        {
            PageTemplateSiteInfoProvider.RemovePageTemplateFromSite(templateId, siteId);
        }


        /// <summary>
        /// Deletes page template.
        /// </summary>
        /// <param name="template">Page template object</param>
        public static void DeletePageTemplate(PageTemplateInfo template)
        {
            ProviderObject.DeleteInfo(template);
        }


        /// <summary>
        /// Deletes page template with specific ID.
        /// </summary>
        /// <param name="templateId">Page template ID</param>
        public static void DeletePageTemplate(int templateId)
        {
            // Get the template
            PageTemplateInfo template = ProviderObject.GetInfoById(templateId);
            if (template != null)
            {
                DeletePageTemplate(template);
            }
            else
            {
                throw new Exception("[PageTemplateInfoProvider.DeletePageTemplate]: Page template ID " + templateId + " not found.");
            }
        }


        /// <summary>
        /// Checks if exists relation between page template and site.
        /// </summary>
        /// <param name="templateId">Page template ID</param>
        /// <param name="siteId">Site ID</param>
        public static bool IsPageTemplateOnSite(int templateId, int siteId)
        {
            return PageTemplateSiteInfoProvider.IsPageTemplateOnSite(templateId, siteId);
        }


        /// <summary>
        /// Checks whether the page template with given name already exists.
        /// </summary>
        /// <param name="templateName">Page template name</param>
        public static bool PageTemplateNameExists(string templateName)
        {
            return PageTemplateNameExists(templateName, 0);
        }


        /// <summary>
        /// Checks whether the page template with given name already exists.
        /// </summary>
        /// <param name="templateName">Page template name</param>
        /// <param name="siteId">Site ID</param>
        public static bool PageTemplateNameExists(string templateName, int siteId)
        {
            return (GetPageTemplateInfo(templateName, siteId) != null);
        }


        /// <summary>
        /// Get all page templates using ObjectQuery.
        /// </summary>
        public static ObjectQuery<PageTemplateInfo> GetTemplates()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Gets page templates allowed for specified site.
        /// </summary>
        /// <param name="siteId">Site ID</param>
        public static TypedDataSet GetAllowedTemplates(int siteId)
        {
            return GetAllowedTemplatesInCategory(siteId, 0);
        }


        /// <summary>
        /// Gets page templates allowed for specified site.
        /// </summary>
        /// <param name="siteId">Site ID</param>
        /// <param name="categoryId">Page template category</param>
        public static TypedDataSet GetAllowedTemplatesInCategory(int siteId, int categoryId)
        {
            return ProviderObject.GetAllowedTemplatesInCategoryInternal(siteId, categoryId);
        }


        /// <summary>
        /// Gets new page template object cloned as an ad-hoc template from the given source page template info.
        /// </summary>
        /// <param name="sourceInfo">Source page template info</param>
        /// <param name="displayName">Cloned page template display name</param>
        /// <param name="siteId">Site ID</param>
        /// <param name="nodeGuid">GUID of the document owner node for reference</param>
        public static PageTemplateInfo CloneTemplateAsAdHoc(PageTemplateInfo sourceInfo, string displayName, int siteId, Guid nodeGuid)
        {
            // Fire event
            using (var h = PageTemplateEvents.PageTemplateCloneAsAdHoc.StartEvent(sourceInfo))
            {
                PageTemplateInfo newInfo = h.EventArguments.NewPageTemplate;

                if (h.CanContinue())
                {
                    // Do not perform cloning if it was already handled by the listener of the Before event. In that case NewPageTemplate would be set to the new object.
                    if (newInfo == null)
                    {
                        // Create new info by cloning with clear
                        newInfo = sourceInfo.Clone(true);

                        // Prepare the display name
                        if (displayName == null)
                        {
                            displayName = "ad-hoc " + newInfo.PageTemplateGUID;
                        }

                        // Setup the fields for new info
                        newInfo.DisplayName = displayName;
                        newInfo.CodeName = newInfo.PageTemplateGUID.ToString();

                        newInfo.IsReusable = false;

                        newInfo.PageTemplateNodeGUID = nodeGuid;

                        newInfo.PageTemplateCloneAsAdHoc = false;
         
                        // For UI page template don't assign site ID to cloned template
                        if (newInfo.PageTemplateType != PageTemplateTypeEnum.UI)
                        {
                            newInfo.PageTemplateSiteID = siteId;
                        }
                    }

                    // Setup the category
                    PageTemplateCategoryInfo ci = (newInfo.PageTemplateType == PageTemplateTypeEnum.UI) ? PageTemplateCategoryInfoProvider.GetAdHocUICategory() : PageTemplateCategoryInfoProvider.GetAdHocCategory();
                    if (ci == null)
                    {
                        throw new Exception("[PageTemplateInfoProvider.CloneTemplateAsAdHoc] Ad-hoc category is no available.");
                    }

                    newInfo.CategoryID = ci.CategoryId;

                    SetPageTemplateInfo(newInfo);

                    h.EventArguments.NewPageTemplate = newInfo;
                }
                h.FinishEvent();

                return newInfo;
            }
        }


        /// <summary>
        /// Gets full path of the page template.
        /// </summary>
        /// <param name="template">Page template object</param>
        public static string GetFullPhysicalPath(PageTemplateInfo template)
        {
            return GetFullPhysicalPath(template, null);
        }


        /// <summary>
        /// Gets full path of the page template.
        /// </summary>
        /// <param name="template">Page template object</param>
        /// <param name="webFullPath">Full path to the root of the web project (e.g. c:\WebProject\)</param>
        public static string GetFullPhysicalPath(PageTemplateInfo template, string webFullPath)
        {
            if ((template != null) && (!template.IsPortal))
            {
                return FileHelper.GetFullPhysicalPath(template.FileName, TemplatesDirectory, webFullPath);
            }
            return "";
        }


        /// <summary>
        /// Gets the page template type enumeration for the given string value.
        /// </summary>
        /// <param name="type">String type</param>
        public static PageTemplateTypeEnum GetPageTemplateTypeEnum(string type)
        {
            PageTemplateTypeEnum result = PageTemplateTypeEnum.Unknown;

            switch (type.ToLowerCSafe())
            {
                case "portal":
                    result = PageTemplateTypeEnum.Portal;
                    break;

                case "aspx":
                    result = PageTemplateTypeEnum.Aspx;
                    break;

                case "aspxportal":
                    result = PageTemplateTypeEnum.AspxPortal;
                    break;

                case "dashboard":
                    result = PageTemplateTypeEnum.Dashboard;
                    break;

                case "mvc":
                    result = PageTemplateTypeEnum.MVC;
                    break;

                case "ui":
                    result = PageTemplateTypeEnum.UI;
                    break;
            }

            return result;
        }


        /// <summary>
        /// Gets the page template type code for the given enum value.
        /// </summary>
        /// <param name="type">Enumeration type</param>
        public static string GetPageTemplateTypeCode(PageTemplateTypeEnum type)
        {
            string result = "";

            switch (type)
            {
                case PageTemplateTypeEnum.Portal:
                    result = "portal";
                    break;

                case PageTemplateTypeEnum.Aspx:
                    result = "aspx";
                    break;

                case PageTemplateTypeEnum.AspxPortal:
                    result = "aspxportal";
                    break;

                case PageTemplateTypeEnum.Dashboard:
                    result = "dashboard";
                    break;

                case PageTemplateTypeEnum.MVC:
                    result = "mvc";
                    break;

                case PageTemplateTypeEnum.UI:
                    result = "ui";
                    break;
            }

            return result;
        }

        #endregion


        #region "Page layout methods"

        /// <summary>
        /// Returns web part layout info for specified path
        /// </summary>
        /// <param name="path">Path</param>
        /// <param name="isAdHoc">Indicates whether current object has binding on ad-hoc template</param>
        public static PageTemplateInfo GetVirtualObject(string path, bool isAdHoc)
        {
            List<string> prefixes = new List<string>();
            // Get layout code name and web part code name
            string templateName = VirtualPathHelper.GetVirtualObjectName(path, isAdHoc ? AdhocTemplateLayoutsDirectory : TemplateLayoutsDirectory, ref prefixes);
            string siteName = isAdHoc ? prefixes[0] : null;

            return GetPageTemplateInfo(templateName, SiteInfoProvider.GetSiteID(siteName));
        }


        /// <summary>
        /// Gets full layout name from the given virtual layout path.
        /// </summary>
        /// <param name="url">Virtual layout path</param>
        /// <returns>Full layout name</returns>
        public static string GetPageLayoutName(string url)
        {
            return GetPageLayoutNameInternal(url, TemplateLayoutsDirectory);
        }


        /// <summary>
        /// Gets full ad-hoc layout name from the given virtual layout path.
        /// </summary>
        /// <param name="url">Virtual layout path</param>
        /// <returns>Full layout name</returns>
        public static string GetAdhocPageLayoutName(string url)
        {
            return GetPageLayoutNameInternal(url, AdhocTemplateLayoutsDirectory);
        }

        #endregion


        #region "Internal methods"

        /// <summary>
        /// Inserts or Updates the object to the database.
        /// </summary>
        /// <param name="info">Object to insert / update</param>
        protected override void SetInfo(PageTemplateInfo info)
        {
            if (info != null)
            {
                // Only call update data for virtual template
                if (info is WebPartVirtualPageTemplateInfo)
                {
                    info.Generalized.UpdateData();
                    return;
                }

                EnsureConsistency(info);

                int oldCategoryId = 0;

                bool isUpdate = (info.PageTemplateId > 0);

                // Change version GUID for VPP only if the layout changed
                bool layoutChanged = info.ItemChanged("PageTemplateLayout") || info.ItemChanged("PageTemplateCSS");
                if (layoutChanged || String.IsNullOrEmpty(info.PageTemplateVersionGUID))
                {
                    info.PageTemplateVersionGUID = Guid.NewGuid().ToString();
                }

                // Properties changed, empty the cached properties form info
                if (info.ItemChanged("PageTemplateProperties"))
                {
                    info.PageTemplatePropertiesForm = null;
                }

                // Ensure correct site assignment
                if (info.IsReusable)
                {
                    info.PageTemplateSiteID = 0;
                }

                // Ensure existing category for AdHoc templates
                if (!info.IsReusable)
                {
                    // Try get assigned category
                    PageTemplateCategoryInfo ci = PageTemplateCategoryInfoProvider.GetPageTemplateCategoryInfo(info.CategoryID);
                    if (ci == null)
                    {
                        // If category doesn't exist, get adhoc
                        ci = PageTemplateCategoryInfoProvider.GetAdHocCategory();
                        if (ci == null)
                        {
                            throw new Exception("Ad-hoc category is no available.");
                        }

                        // Assign adHoc category to the current template
                        info.CategoryID = ci.CategoryId;
                    }
                }

                // Save the web parts
                info.WebParts = info.TemplateInstance.GetZonesXML();

                if (isUpdate)
                {
                    // Get old category ID
                    PageTemplateInfo oldTemplate = GetInfoByCodeName(info.CodeName, info.PageTemplateSiteID, false);
                    if (oldTemplate != null)
                    {
                        if (oldTemplate.PageTemplateId != info.PageTemplateId)
                        {
                            throw new Exception("Page template '" + info.CodeName + "' already exists.");
                        }
                        // Get old category ID
                        oldCategoryId = oldTemplate.CategoryID;
                    }
                }

                // Set the page template
                base.SetInfo(info);

                if (isUpdate)
                {
                    // Clear the dependent cache items
                    if (info.Generalized.TouchCacheDependencies)
                    {
                        CacheHelper.TouchKey("template|" + info.PageTemplateId);
                    }
                }

                // Update webpart category children count
                PageTemplateCategoryInfoProvider.UpdateCategoryTemplateChildCount(oldCategoryId, info.CategoryID);

                if (HttpContext.Current != null)
                {
                    // Remove web part settings from the cache
                    CacheHelper.Remove("CMSVirtualWebParts");
                }
            }
        }


        /// <summary>
        /// Deletes all ad hoc templates for given node (UI element) based on it's GUID.
        /// </summary>
        /// <param name="guid">Item's GUID</param>
        /// <param name="siteId">Site ID</param>
        protected virtual void DeleteAdHocTemplatesInternal(Guid guid, int siteId)
        {
            if (guid != Guid.Empty)
            {
                DataSet ds = GetTemplates().WhereEquals("PageTemplateNodeGUID", guid).OnSite(siteId, true).Column("PageTemplateID");
                if (!DataHelper.DataSourceIsEmpty(ds))
                {
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        int id = ValidationHelper.GetInteger(dr["PageTemplateID"], 0);
                        DeletePageTemplate(id);
                    }
                }
            }
        }


        /// <summary>
        /// Ensures consistency of the page template info data
        /// </summary>
        /// <param name="template">Page template</param>
        private static void EnsureConsistency(PageTemplateInfo template)
        {
            if (template.PageTemplateType == PageTemplateTypeEnum.MVC)
            {
                // MVC template
                template.FileName = String.Empty;

                template.ShowAsMasterTemplate = false;
                template.PageTemplateCloneAsAdHoc = false;
            }
            else if (template.PageTemplateType == PageTemplateTypeEnum.Portal)
            {
                // Portal template of various types
                template.FileName = String.Empty;

                // Save inherit levels
                if (template.ShowAsMasterTemplate)
                {
                    template.InheritPageLevels = "/";
                }
            }
            else
            {
                // ASPX page templates
                //template.FileName = FileSystemSelector.Value.ToString();

                template.ShowAsMasterTemplate = false;
                template.PageTemplateCloneAsAdHoc = false;

                template.InheritPageLevels = "";
            }

            if (template.IsReusable)
            {
                template.PageTemplateNodeGUID = Guid.Empty;
            }
        }


        /// <summary>
        /// Deletes the object to the database.
        /// </summary>
        /// <param name="info">Object to delete</param>
        protected override void DeleteInfo(PageTemplateInfo info)
        {
            if (info != null)
            {
                // Delete page template
                base.DeleteInfo(info);

                // Update category counts
                PageTemplateCategoryInfoProvider.UpdateCategoryTemplateChildCount(0, info.CategoryID);

                // Clear the dependent cache items
                if (info.Generalized.TouchCacheDependencies)
                {
                    CacheHelper.TouchKey("template|" + info.PageTemplateId);
                }
            }
        }


        /// <summary>
        /// Gets page templates allowed for specified site.
        /// </summary>
        /// <param name="siteId">Site ID</param>
        /// <param name="categoryId">Page template category ID</param>
        protected virtual TypedDataSet GetAllowedTemplatesInCategoryInternal(int siteId, int categoryId)
        {
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@SiteID", siteId);
            parameters.Add("@CategoryID", categoryId);
            parameters.EnsureDataSet<PageTemplateInfo>();

            return ConnectionHelper.ExecuteQuery("cms.pagetemplate.selectAllowedPageTemplates", parameters, null, "PageTemplateDisplayName").As<PageTemplateInfo>();
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Gets full layout name from the given virtual layout path.
        /// </summary>
        /// <param name="url">Virtual layout path</param>
        /// <param name="directory">Base directory</param>
        /// <returns>Full layout name</returns>
        private static string GetPageLayoutNameInternal(string url, string directory)
        {
            if (HttpContext.Current != null)
            {
                string physicalPath = HttpContext.Current.Request.MapPath(url);
                string physicalPathVirtDir = HttpContext.Current.Request.MapPath(directory);

                // gets the path behind layout directory
                string newPath = physicalPath.Remove(0, physicalPathVirtDir.Length + 1);
                newPath = newPath.Replace("\\", ".");
                newPath = URLHelper.RemoveFirstPart(newPath, ".");

                // gets file name from the specified path without extension
                newPath = Path.GetFileNameWithoutExtension(newPath);

                return newPath;
            }
            return "";
        }

        #endregion
    }
}