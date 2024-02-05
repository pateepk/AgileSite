using System;
using System.Collections.Generic;
using System.Data;
using System.Web;
using System.Collections;
using System.Xml;

using CMS.DataEngine;
using CMS.Helpers;
using CMS.MacroEngine;
using CMS.PortalEngine;
using CMS.Base;
using CMS.DocumentEngine;

namespace CMS.OnlineMarketing
{
    using TypedDataSet = InfoDataSet<ContentPersonalizationVariantInfo>;

    /// <summary>
    /// Class providing ContentPersonalizationVariant management.
    /// </summary>
    public class ContentPersonalizationVariantInfoProvider : AbstractInfoProvider<ContentPersonalizationVariantInfo, ContentPersonalizationVariantInfoProvider>
    {
        #region "Constructors"

        /// <summary>
        /// Constructor
        /// </summary>
        public ContentPersonalizationVariantInfoProvider()
            : base(ContentPersonalizationVariantInfo.TYPEINFO, new HashtableSettings
                {
                    ID = true,
                    Name = true,
                    Load = LoadHashtableEnum.All
                })
        {
        }

        #endregion


        #region "Public methods - Basic"

        /// <summary>
        /// Returns a query for all the ContentPersonalizationVariantInfo objects.
        /// </summary>
        public static ObjectQuery<ContentPersonalizationVariantInfo> GetContentPersonalizationVariants()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Gets all the Content personalization variants for the specified template.
        /// </summary>
        /// <param name="pageTemplateId">The page template id</param>
        public static TypedDataSet GetContentPersonalizationVariants(int pageTemplateId)
        {
            return ProviderObject.GetContentPersonalizationVariantsInternal(pageTemplateId);
        }


        /// <summary>
        /// Returns content personalization variant with specified ID.
        /// </summary>
        /// <param name="variantId">Content personalization variant ID.</param>        
        public static ContentPersonalizationVariantInfo GetContentPersonalizationVariant(int variantId)
        {
            return ProviderObject.GetInfoById(variantId);
        }


        /// <summary>
        /// Returns content personalization variant with specified name.
        /// </summary>
        /// <param name="variantName">Content personalization variant name.</param>                
        public static ContentPersonalizationVariantInfo GetContentPersonalizationVariant(string variantName)
        {
            return ProviderObject.GetInfoByCodeName(variantName);
        }


        /// <summary>
        /// Sets (updates or inserts) specified content personalization variant.
        /// </summary>
        /// <param name="variantObj">Content personalization variant to be set.</param>
        public static void SetContentPersonalizationVariant(ContentPersonalizationVariantInfo variantObj)
        {
            ProviderObject.SetInfo(variantObj);
        }


        /// <summary>
        /// Deletes specified content personalization variant.
        /// </summary>
        /// <param name="variantObj">Content personalization variant to be deleted.</param>
        public static void DeleteContentPersonalizationVariant(ContentPersonalizationVariantInfo variantObj)
        {
            ProviderObject.DeleteInfo(variantObj);
        }


        /// <summary>
        /// Deletes content personalization variant with specified ID.
        /// </summary>
        /// <param name="variantId">Content personalization variant ID.</param>
        public static void DeleteContentPersonalizationVariant(int variantId)
        {
            ContentPersonalizationVariantInfo variantObj = GetContentPersonalizationVariant(variantId);
            DeleteContentPersonalizationVariant(variantObj);
        }

        #endregion


        #region "Public methods - Advanced"

        /// <summary>
        /// Combines the specified page template instance with Content personalization variants.
        /// </summary>
        /// <param name="pi">The page info</param>
        /// <param name="instance">The page template instance</param>
        /// <param name="viewMode">The current view mode</param>
        public static PageTemplateInstance CombineWithPersonalization(PageInfo pi, PageTemplateInstance instance, ViewModeEnum viewMode)
        {
            return ProviderObject.CombineWithPersonalizationInternal(pi, instance, viewMode);
        }


        /// <summary>
        /// Indicates whether the content personalization is enabled.
        /// </summary>
        /// <param name="siteName">Name of the site</param>
        public static bool ContentPersonalizationEnabled(string siteName)
        {
            return SettingsKeyInfoProvider.GetBoolValue(siteName + ".CMSContentPersonalizationEnabled");
        }


        /// <summary>
        /// Gets the Content personalization variants for a web part/zone/widget.
        /// </summary>
        /// <param name="pageTemplateId">The page template id</param>
        /// <param name="zoneId">The zone id</param>
        /// <param name="instanceGuid">The instance GUID</param>
        /// <param name="documentId">The document id</param>
        public static TypedDataSet GetContentPersonalizationVariants(int pageTemplateId, string zoneId, Guid instanceGuid, int documentId)
        {
            return ProviderObject.GetContentPersonalizationVariantsInternal(pageTemplateId, zoneId, instanceGuid, documentId);
        }


        /// <summary>
        /// Saves the variant.
        /// </summary>
        /// <param name="variantId">The variant id</param>
        /// <param name="variantName">Name of the variant</param>
        /// <param name="variantDisplayName">Display name of the variant</param>
        /// <param name="variantDescription">The variant description</param>
        /// <param name="variantEnabled">Indicates whether the variant is enabled</param>
        /// <param name="variantDisplayCondition">The variant display condition</param>
        /// <param name="zoneId">The zone id</param>
        /// <param name="instanceGuid">The instance GUID</param>
        /// <param name="pageTemplateId">The page template id</param>
        /// <param name="documentId">The document id</param>
        /// <param name="xmlWebParts">Web parts XML</param>
        public static int SaveVariant(int variantId, string variantName, string variantDisplayName, string variantDescription, bool variantEnabled, string variantDisplayCondition, string zoneId, Guid instanceGuid, int pageTemplateId, int documentId, XmlNode xmlWebParts)
        {
            return ProviderObject.SaveVariantInternal(variantId, variantName, variantDisplayName, variantDescription, variantEnabled, variantDisplayCondition, zoneId, instanceGuid, pageTemplateId, documentId, xmlWebParts);
        }


        /// <summary>
        /// Saves the variant properties.
        /// </summary>
        /// <param name="variantId">The variant id</param>
        /// <param name="xmlWebParts">Web parts XML</param>
        public static void SaveVariantWebparts(int variantId, XmlNode xmlWebParts)
        {
            ProviderObject.SaveVariantWebpartsInternal(variantId, xmlWebParts);
        }


        /// <summary>
        /// Updates the web part variants with a new zone id.
        /// </summary>
        /// <param name="pageTemplateId">The page template id</param>
        /// <param name="zoneId">The zone id.</param>
        /// <param name="instanceGuid">The instance GUID</param>
        /// <param name="newZoneId">The new zone id</param>
        public static void UpdateWebPartVariants(int pageTemplateId, string zoneId, Guid instanceGuid, string newZoneId)
        {
            ProviderObject.UpdateWebPartVariantsInternal(pageTemplateId, zoneId, instanceGuid, newZoneId);
        }


        /// <summary>
        /// Removes the variants of the selected web parts.
        /// </summary>
        /// <param name="webParts">The web parts</param>
        public static void RemoveWebPartsVariants(IEnumerable<WebPartInstance> webParts)
        {
            ProviderObject.RemoveWebPartsVariantsInternal(webParts);
        }


        /// <summary>
        /// Deletes all variants of all widgets in the document.
        /// </summary>
        /// <param name="zoneId">The zone id</param>
        /// <param name="templateId">The template id</param>
        /// <param name="documentId">The document id</param>
        public static void DeleteWidgetVariants(string zoneId, int templateId, int documentId)
        {
            ProviderObject.DeleteWidgetVariantsInternal(zoneId, templateId, documentId);
        }


        /// <summary>
        /// Moves the variant up in the position sequence (up = smaller VariantPosition = sooner in the navigation)
        /// </summary>
        /// <param name="variantId">The variant id</param>
        public static void MoveVariantUp(int variantId)
        {
            ProviderObject.MoveVariantUpInternal(variantId);
        }


        /// <summary>
        /// Moves the variant down in the position sequence (down = larger VariantPosition = later in the navigation)
        /// </summary>
        /// <param name="variantId">The variant id</param>
        public static void MoveVariantDown(int variantId)
        {
            ProviderObject.MoveVariantDownInternal(variantId);
        }


        /// <summary>
        /// Loads the Content personalization variants for the specified instance (web part/zone/widget).
        /// Return true, if the variants were successfully loaded.
        /// </summary>
        /// <param name="instance">The zone instance of web part instance</param>
        /// <param name="isZone">Indicates if the parameter instance object is a zone</param>
        /// <param name="documentId">Document ID if the instance holds a widget</param>
        /// <returns>True, if the variants were successfully loaded. False, if it was not possible to load variants, for example due to missing information in the instance.</returns>
        public static bool LoadContentPersonalizationVariants(object instance, bool isZone, int documentId = 0)
        {
            return ProviderObject.LoadContentPersonalizationVariantsInternal(instance, isZone, documentId);
        }


        /// <summary>
        /// Clones all the Content personalization variants of the specific page template.
        /// </summary>
        /// <param name="originalTemplateId">The original template id</param>
        /// <param name="newTemplateId">The new template id</param>
        public static void CloneTemplateVariants(int originalTemplateId, int newTemplateId)
        {
            ProviderObject.CloneTemplateVariantsInternal(originalTemplateId, newTemplateId);
        }

        #endregion


        #region "Internal methods - Basic"

        /// <summary>
        /// Inserts or Updates the object to the database.
        /// </summary>
        /// <param name="info">Object to insert / update</param>
        protected override void SetInfo(ContentPersonalizationVariantInfo info)
        {
            base.SetInfo(info);

            // Clear the cached instances
            info.WebPartInstance = null;
            info.WebPartZoneInstance = null;

            // Clear variant cache
            CacheHelper.TouchKey("om.personalizationvariant|bytemplateid|" + info.VariantPageTemplateID);
        }


        /// <summary>
        /// Deletes the object to the database.
        /// </summary>
        /// <param name="info">Object to delete</param>
        protected override void DeleteInfo(ContentPersonalizationVariantInfo info)
        {
            base.DeleteInfo(info);

            if (info != null)
            {
                // Clear variant cache
                CacheHelper.TouchKey("om.personalizationvariant|bytemplateid|" + info.VariantPageTemplateID);
            }
        }

        #endregion


        #region "Internal methods - Advanced"

        /// <summary>
        /// Combines the specified page template instance with content personalization variants.
        /// </summary>
        /// <param name="pi">The page info</param>
        /// <param name="instance">The page template instance</param>
        /// <param name="viewMode">The view mode</param>
        protected virtual PageTemplateInstance CombineWithPersonalizationInternal(PageInfo pi, PageTemplateInstance instance, ViewModeEnum viewMode)
        {
            // Check whether page info and template instance are defined
            if ((pi != null) && (instance != null) && (instance.ParentPageTemplate != null))
            {
                int templateId = instance.ParentPageTemplate.PageTemplateId;
                Hashtable chosenVariants = null;

                // When to in the live site, try to display the variants which are stored in a cookie
                if (viewMode != ViewModeEnum.LiveSite)
                {
                    // variantCodes format: wp_1:variantId1#wp_5:variantId4#...
                    string cookieName = CookieName.GetVariantSliderPositionsCookieName(templateId);

                    string variantCodes = HttpUtility.UrlDecode(CookieHelper.GetValue(cookieName));

                    // Build the variantCodes hash table
                    if (!string.IsNullOrEmpty(variantCodes))
                    {
                        var codesArray = variantCodes.Split(new char[] { '#' });
                        chosenVariants = new Hashtable();
                        foreach (string code in codesArray)
                        {
                            string[] parts = code.Split(new char[] { ':' });
                            if (parts.Length == 2)
                            {
                                chosenVariants.Add(parts[0], parts[1]);
                            }
                        }
                    }
                }

                // Loop through all the zones and replace the zones/web parts/widgets which have a variant and comply the display condition
                foreach (WebPartZoneInstance zoneInstance in instance.WebPartZones)
                {
                    // Zone variants
                    if ((zoneInstance.VariantMode == VariantModeEnum.ContentPersonalization) && zoneInstance.HasVariants)
                    {
                        PortalContext.ContentPersonalizationVariantsEnabled = true;
                        // Combine the original zone with the variant which complies the display condition
                        WebPartZoneInstance variantInstance = GetMatchingZoneInstance(zoneInstance, templateId, viewMode, chosenVariants);
                        if (variantInstance != zoneInstance)
                        {
                            instance.CombineWith(variantInstance);
                        }
                    }
                    // Web part/widget variants
                    else
                    {
                        foreach (WebPartInstance webPartInstance in zoneInstance.WebParts)
                        {
                            if ((webPartInstance.VariantMode == VariantModeEnum.ContentPersonalization) && webPartInstance.HasVariants)
                            {
                                PortalContext.ContentPersonalizationVariantsEnabled = true;
                                // Combine the original web part/widget with the variant which complies the display condition
                                WebPartInstance variantInstance = GetMatchingWebPartInstance(webPartInstance, templateId, viewMode, chosenVariants);
                                if (variantInstance != webPartInstance)
                                {
                                    instance.CombineWith(variantInstance);
                                }
                            }
                        }
                    }
                }
            }

            return instance;
        }


        /// <summary>
        /// Gets all the Content personalization variants for the specified template.
        /// </summary>
        /// <param name="pageTemplateId">The page template id</param>
        protected virtual TypedDataSet GetContentPersonalizationVariantsInternal(int pageTemplateId)
        {
            TypedDataSet ds = null;

            // Cache list of Content personalization variants
            using (var cs = new CachedSection<TypedDataSet>(ref ds, 1440, true, null, "onlinemarketingcpvariantsbytemplateid", pageTemplateId))
            {
                // Check whether data should be loaded
                if (cs.LoadData)
                {
                    // Get variants data
                    ds = GetContentPersonalizationVariants().WhereEquals("VariantPageTemplateID", pageTemplateId)
                                                            .OrderBy("VariantPosition")
                                                            .TypedResult;

                    // Cache data if it is required
                    if (cs.Cached)
                    {
                        // Prepare cache dependency
                        cs.CacheDependency = CacheHelper.GetCacheDependency("om.personalizationvariant|bytemplateid|" + pageTemplateId);
                    }

                    cs.Data = ds;
                }
            }

            return ds;
        }


        /// <summary>
        /// Gets the Content personalization variants for a web part/zone/widget.
        /// </summary>
        /// <param name="pageTemplateId">The page template id</param>
        /// <param name="zoneId">The zone id</param>
        /// <param name="instanceGuid">The instance GUID</param>
        /// <param name="documentId">The document id</param>
        protected virtual TypedDataSet GetContentPersonalizationVariantsInternal(int pageTemplateId, string zoneId, Guid instanceGuid, int documentId)
        {
            TypedDataSet ds = GetContentPersonalizationVariantsInternal(pageTemplateId);

            // Limit dataset for required zone/instance
            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                // Create new dataset
                InfoDataSet<ContentPersonalizationVariantInfo> variants = new InfoDataSet<ContentPersonalizationVariantInfo>();
                variants.Tables.Add(ds.Tables[0].Clone());

                // Loop through all template variants
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    // Zone id
                    if (ValidationHelper.GetString(dr["VariantZoneID"], String.Empty).EqualsCSafe(zoneId, true))
                    {
                        // Instance GUID
                        if (ValidationHelper.GetGuid(dr["VariantInstanceGUID"], Guid.Empty) == instanceGuid)
                        {
                            // Document id
                            if (ValidationHelper.GetInteger(dr["VariantDocumentID"], 0) == documentId)
                            {
                                variants.Tables[0].Rows.Add(dr.ItemArray);
                            }
                        }
                    }
                }

                return variants;
            }

            return null;
        }


        /// <summary>
        /// Saves the variant and generates new combinations if the variant is new.
        /// </summary>
        /// <param name="variantId">The variant id</param>
        /// <param name="variantName">Name of the variant</param>
        /// <param name="variantDisplayName">Display name of the variant</param>
        /// <param name="variantDescription">The variant description</param>
        /// <param name="variantEnabled">Indicates whether the variant is enabled</param>
        /// <param name="variantDisplayCondition">The variant display condition</param>
        /// <param name="zoneId">The zone id</param>
        /// <param name="instanceGuid">The instance GUID</param>
        /// <param name="pageTemplateId">The page template id</param>
        /// <param name="documentId">The document id</param>
        /// <param name="xmlWebParts">Web parts XML</param>
        protected virtual int SaveVariantInternal(int variantId, string variantName, string variantDisplayName, string variantDescription, bool variantEnabled, string variantDisplayCondition, string zoneId, Guid instanceGuid, int pageTemplateId, int documentId, XmlNode xmlWebParts)
        {
            bool isNewVariant = (variantId == 0);

            ContentPersonalizationVariantInfo variantObj;

            if (isNewVariant)
            {
                // Create a new variant
                variantObj = new ContentPersonalizationVariantInfo();

                variantObj.VariantName = variantName;
                variantObj.VariantDisplayName = variantDisplayName;
                variantObj.VariantDescription = variantDescription;
                variantObj.VariantEnabled = variantEnabled;
                variantObj.VariantDisplayCondition = variantDisplayCondition;
                variantObj.VariantInstanceGUID = instanceGuid;
                variantObj.VariantZoneID = zoneId;
                variantObj.VariantPageTemplateID = pageTemplateId;
                if (documentId > 0)
                {
                    variantObj.VariantDocumentID = documentId;
                }
            }
            else
            {
                // Get existing variant
                variantObj = GetContentPersonalizationVariant(variantId);
            }

            if (variantObj != null)
            {
                // Save the variant properties
                variantObj.VariantWebParts = xmlWebParts.OuterXml;

                SetContentPersonalizationVariant(variantObj);

                return variantObj.VariantID;
            }

            return 0;
        }


        /// <summary>
        /// Saves the variant properties.
        /// </summary>
        /// <param name="variantId">The variant id.</param>
        /// <param name="xmlWebParts">Web parts XML</param>
        protected virtual void SaveVariantWebpartsInternal(int variantId, XmlNode xmlWebParts)
        {
            ContentPersonalizationVariantInfo variantObj = GetContentPersonalizationVariant(variantId);

            if (variantObj != null)
            {
                variantObj.VariantWebParts = xmlWebParts.OuterXml;

                // Save the changes
                SetContentPersonalizationVariant(variantObj);
            }
        }


        /// <summary>
        /// Updates the web part variants with a new zone id.
        /// </summary>
        /// <param name="pageTemplateId">The page template id</param>
        /// <param name="zoneId">The zone id.</param>
        /// <param name="instanceGuid">The instance GUID</param>
        /// <param name="newZoneId">The new zone id</param>
        protected virtual void UpdateWebPartVariantsInternal(int pageTemplateId, string zoneId, Guid instanceGuid, string newZoneId)
        {
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@VariantZoneID", zoneId);
            parameters.Add("@NewZoneID", newZoneId);
            parameters.Add("@VariantPageTemplateID", pageTemplateId);

            string where = "(VariantPageTemplateID = @VariantPageTemplateID) AND (VariantZoneID = @VariantZoneID)";

            if (instanceGuid != Guid.Empty)
            {
                where = SqlHelper.AddWhereCondition(where, "(VariantInstanceGUID = @VariantInstanceGUID)");
                parameters.Add("@VariantInstanceGUID", instanceGuid);
            }
            else
            {
                where = SqlHelper.AddWhereCondition(where, "(VariantInstanceGUID IS NOT NULL)");
            }

            // Update
            UpdateData("VariantZoneID = @NewZoneID", parameters, where);

            // Clear cache
            ClearHashtables(true);
            CacheHelper.TouchKey("om.personalizationvariant|bytemplateid|" + pageTemplateId);
        }


        /// <summary>
        /// Removes the variants of the selected web parts.
        /// </summary>
        /// <param name="webParts">The web parts</param>
        protected virtual void RemoveWebPartsVariantsInternal(IEnumerable<WebPartInstance> webParts)
        {
            int pageTemplateId = 0;

            if (webParts != null)
            {
                foreach (WebPartInstance wpi in webParts)
                {
                    // Delete only Content personalization variants
                    if (wpi.VariantMode == VariantModeEnum.ContentPersonalization)
                    {
                        if ((pageTemplateId == 0)
                            && (wpi.ParentZone != null)
                            && (wpi.ParentZone.ParentTemplateInstance != null)
                            && (wpi.ParentZone.ParentTemplateInstance.ParentPageTemplate != null))
                        {
                            pageTemplateId = wpi.ParentZone.ParentTemplateInstance.ParentPageTemplate.PageTemplateId;
                        }

                        if (wpi.PartInstanceVariants != null)
                        {
                            // Loop through all web part variants and delete them
                            foreach (WebPartInstance variant in wpi.PartInstanceVariants)
                            {
                                DeleteContentPersonalizationVariant(variant.VariantID);
                            }
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Deletes all variants of all widgets in the document.
        /// </summary>
        /// <param name="zoneId">The zone id</param>
        /// <param name="pageTemplateId">The page template id</param>
        /// <param name="documentId">The document id</param>
        protected virtual void DeleteWidgetVariantsInternal(string zoneId, int pageTemplateId, int documentId)
        {
            if (!string.IsNullOrEmpty(zoneId) && (pageTemplateId > 0) && (documentId > 0))
            {
                var variants = GetContentPersonalizationVariants().BinaryData(false)
                                                                  .WhereEquals("VariantPageTemplateID", pageTemplateId)
                                                                  .WhereEquals("VariantZoneID", zoneId)
                                                                  .WhereEquals("VariantDocumentID", documentId);

                // Delete the variants using the standard DeleteInfo() with the RemoveDependencies procedure
                foreach (var variant in variants)
                {
                    DeleteInfo(variant);
                }

                // Clear variant cache
                CacheHelper.TouchKey("om.personalizationvariant|bytemplateid|" + pageTemplateId);
            }
        }


        /// <summary>
        /// Moves task up in the order sequence (up = smaller VariantPosition = sooner in the navigation)
        /// </summary>
        /// <param name="variantId">The variant id</param>
        protected virtual void MoveVariantUpInternal(int variantId)
        {
            if (variantId > 0)
            {
                ContentPersonalizationVariantInfo variantObj = GetContentPersonalizationVariant(variantId);
                if (variantObj != null)
                {
                    variantObj.Generalized.MoveObjectUp();
                }
            }
        }


        /// <summary>
        /// Moves task down in the order sequence (down = larger VariantPosition = later in the navigation)
        /// </summary>
        /// <param name="variantId">The variant id</param>
        protected virtual void MoveVariantDownInternal(int variantId)
        {
            if (variantId > 0)
            {
                ContentPersonalizationVariantInfo variantObj = GetContentPersonalizationVariant(variantId);
                if (variantObj != null)
                {
                    variantObj.Generalized.MoveObjectDown();
                }
            }
        }


        /// <summary>
        /// Returns the zone instance variant which fulfill its display condition. If no variant found, return the original zone.
        /// </summary>
        /// <param name="originalZone">The original zone instance</param>
        /// <param name="templateId">The template id</param>
        /// <param name="viewMode">The view mode</param>
        /// <param name="chosenVariants">The hash table containing chosen variants</param>
        protected virtual WebPartZoneInstance GetMatchingZoneInstance(WebPartZoneInstance originalZone, int templateId, ViewModeEnum viewMode, Hashtable chosenVariants)
        {
            // If there are any zones which have a specific variant chosen, use that variant
            // Use the specified variants only in Design, Page and Preview mode. In the Live-site mode always evaluate display conditions
            if ((chosenVariants != null) && (viewMode != ViewModeEnum.LiveSite))
            {
                // Replace the original zone only when not in the Design mode
                if ((viewMode != ViewModeEnum.Design && (viewMode != ViewModeEnum.DesignDisabled)))
                {
                    // Get the chosen variant from the cookie setting
                    string itemCode = "Variant_Zone_" + originalZone.ZoneID;
                    if (chosenVariants.Contains(itemCode))
                    {
                        // Get the desired variant id from the cookie
                        int variantId = ValidationHelper.GetInteger(chosenVariants[itemCode], 0);
                        // Try to find the desired variant
                        WebPartZoneInstance zoneVariantInstance = originalZone.ZoneInstanceVariants.Find(z => z.VariantID.Equals(variantId));
                        if (zoneVariantInstance != null)
                        {
                            return zoneVariantInstance;
                        }
                    }
                }
            }
            // Otherwise replace the original zone with the first variant which complies the display condition
            else
            {
                foreach (WebPartZoneInstance zoneVariantInstance in originalZone.ZoneInstanceVariants)
                {
                    // Resolve display condition in the live site only
                    ContentPersonalizationVariantInfo variantObj = GetContentPersonalizationVariant(zoneVariantInstance.VariantID);
                    if ((variantObj != null) && (variantObj.VariantEnabled))
                    {
                        if (ValidationHelper.GetBoolean(MacroResolver.Resolve(variantObj.VariantDisplayCondition), false))
                        {
                            return zoneVariantInstance;
                        }
                    }
                }
            }

            return originalZone;
        }


        /// <summary>
        /// Returns the web part instance variant which fulfill its display condition. If no variant found, return the original web part.
        /// </summary>
        /// <param name="originalWebPart">The original web part</param>
        /// <param name="templateId">The template id</param>
        /// <param name="viewMode">The view mode</param>
        /// <param name="chosenVariants">The hash table containing chosen variants</param>
        protected virtual WebPartInstance GetMatchingWebPartInstance(WebPartInstance originalWebPart, int templateId, ViewModeEnum viewMode, Hashtable chosenVariants)
        {
            // If there are any web parts/widgets which have a specific variant chosen, use that variant.
            // Use the specified variants only in Design, Page and Preview modes. In the Live-site mode always evaluate display conditions
            if ((chosenVariants != null) && (viewMode != ViewModeEnum.LiveSite))
            {
                // Replace the original web part only when not in the Design mode
                // Replace the original widget only when not in the Edit mode
                if ((!originalWebPart.IsWidget && (viewMode != ViewModeEnum.Design && (viewMode != ViewModeEnum.DesignDisabled)))
                    || (originalWebPart.IsWidget && (viewMode != ViewModeEnum.Edit) && (viewMode != ViewModeEnum.EditDisabled)))
                {
                    // Get the chosen variant from the cookie setting
                    string itemCode = "Variant_WP_" + originalWebPart.InstanceGUID.ToString("N");
                    if (chosenVariants.Contains(itemCode))
                    {
                        // Get the desired variant id from the cookie
                        int variantId = ValidationHelper.GetInteger(chosenVariants[itemCode], 0);
                        // Try to find the desired variant
                        WebPartInstance partVariantInstance = originalWebPart.PartInstanceVariants.Find(w => w.VariantID.Equals(variantId));
                        if (partVariantInstance != null)
                        {
                            partVariantInstance.CurrentVariantInstance = partVariantInstance;
                            return partVariantInstance;
                        }
                    }
                }
            }
            // Otherwise replace the original web parts/widgets with the first variant which complies the display condition
            else
            {
                foreach (WebPartInstance partVariantInstance in originalWebPart.PartInstanceVariants)
                {
                    ContentPersonalizationVariantInfo variantObj = GetContentPersonalizationVariant(partVariantInstance.VariantID);
                    if ((variantObj != null) && (variantObj.VariantEnabled))
                    {
                        if (ValidationHelper.GetBoolean(MacroResolver.Resolve(variantObj.VariantDisplayCondition), false))
                        {
                            partVariantInstance.CurrentVariantInstance = partVariantInstance;
                            return partVariantInstance;
                        }
                    }
                }
            }

            return originalWebPart;
        }


        /// <summary>
        /// Loads the Content personalization variants for the specified instance (web part/zone/widget) running in the current <see cref="DocumentContext"/>.
        /// Return true, if the variants were successfully loaded.
        /// </summary>
        /// <param name="instance">The zone instance of web part instance</param>
        /// <param name="isZone">Indicates if the parameter instance object is a zone</param>
        /// <param name="documentId">Document ID if the instance holds a widget</param>
        /// <returns>True, if the variants were successfully loaded. False, if it was not possible to load variants, for example due to missing information in the instance.</returns>
        protected virtual bool LoadContentPersonalizationVariantsInternal(object instance, bool isZone, int documentId)
        {
            if (instance == null)
            {
                return false;
            }

            WebPartZoneInstance zoneInstance = null;
            WebPartInstance webPartInstance = null;

            int pageTemplateId;
            string zoneId;
            Guid instanceGuid = Guid.Empty;
            VariantModeEnum currentVariantMode;

            // Instance is a zone
            if (isZone)
            {
                zoneInstance = instance as WebPartZoneInstance;
                if (!MVTHelper.ContainsPageInfoObject(zoneInstance))
                {
                    // Do not continue if PageTemplateId is unknown
                    return false;
                }

                zoneId = zoneInstance.ZoneID;
                currentVariantMode = zoneInstance.VariantMode;
                pageTemplateId = zoneInstance.ParentTemplateInstance.ParentPageTemplate.PageTemplateId;
            }
            // Instance is a web part/widget
            else
            {
                webPartInstance = instance as WebPartInstance;
                if ((webPartInstance == null) || !MVTHelper.ContainsPageInfoObject(webPartInstance.ParentZone))
                {
                    // Do not continue if PageTemplateId is unknown
                    return false;
                }

                if (webPartInstance.IsWidget && (documentId == 0))
                {
                    return false;
                }

                zoneId = webPartInstance.ParentZone.ZoneID;
                instanceGuid = webPartInstance.InstanceGUID;
                pageTemplateId = webPartInstance.ParentZone.ParentTemplateInstance.ParentPageTemplate.PageTemplateId;
                currentVariantMode = webPartInstance.VariantMode;
            }

            // Create empty lists to indicate that the variants have been loaded (list==NULL: means not loaded yet, list.Count==0: means loaded but does not contain any variants)
            List<WebPartZoneInstance> zoneInstanceVariants = new List<WebPartZoneInstance>();
            List<WebPartInstance> webpartInstanceVariants = new List<WebPartInstance>();

            // Get the Content personalization variants for the instance
            DataSet ds = GetContentPersonalizationVariants(pageTemplateId, zoneId, instanceGuid, documentId);

            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                // If there are MVT variants already in the instance, set the Conflicted mode and do not continue
                if (currentVariantMode == VariantModeEnum.MVT)
                {
                    if (isZone)
                    {
                        zoneInstance.VariantMode = VariantModeEnum.Conflicted;
                    }
                    else
                    {
                        webPartInstance.VariantMode = VariantModeEnum.Conflicted;
                    }

                    return true;
                }
                // There are no MVT variants in the instance => set the variant mode to CP and proceed to loading the CP variants
                else
                {
                    if (isZone)
                    {
                        zoneInstance.VariantMode = VariantModeEnum.ContentPersonalization;
                    }
                    else
                    {
                        webPartInstance.VariantMode = VariantModeEnum.ContentPersonalization;
                    }
                }

                // Load all the variants for the current instance
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    string variantXml = ValidationHelper.GetString(row["VariantWebParts"], string.Empty);
                    int variantId = ValidationHelper.GetInteger(row["VariantID"], 0);
                    if (!string.IsNullOrEmpty(variantXml))
                    {
                        // Try to get the variant instance from the info object. If not found, then create the variant instance and store it in the variant info object
                        ContentPersonalizationVariantInfo variantObj = GetContentPersonalizationVariant(variantId);
                        if (variantObj != null)
                        {
                            if (isZone)
                            {
                                // Zone variant
                                WebPartZoneInstance variantInstance = variantObj.WebPartZoneInstance;

                                // Variant instance not found in the info object => create the variant instance
                                if (variantInstance == null)
                                {
                                    XmlDocument xmlDocument = new XmlDocument();
                                    xmlDocument.LoadXml(variantXml);
                                    XmlNode variantNode = xmlDocument.ChildNodes[0];

                                    variantInstance = new WebPartZoneInstance(variantNode);
                                    variantInstance.ParentTemplateInstance = zoneInstance.ParentTemplateInstance;
                                    variantInstance.VariantID = variantId;
                                    variantInstance.VariantMode = VariantModeEnum.ContentPersonalization;

                                    // Store the variant instance in the variant info object
                                    variantObj.WebPartZoneInstance = variantInstance;
                                }

                                // Add the variant instance into the zone variants list
                                zoneInstanceVariants.Add(variantInstance);
                            }
                            else
                            {
                                // Web part/widget variant
                                WebPartInstance variantInstance = variantObj.WebPartInstance;

                                // Variant instance not found in the info object => create the variant instance
                                if (variantInstance == null)
                                {
                                    XmlDocument xmlDocument = new XmlDocument();
                                    xmlDocument.LoadXml(variantXml);
                                    XmlNode variantNode = xmlDocument.ChildNodes[0];

                                    variantInstance = new WebPartInstance(variantNode);
                                    variantInstance.VariantID = variantId;
                                    variantInstance.ParentZone = webPartInstance.ParentZone;
                                    variantInstance.VariantMode = VariantModeEnum.ContentPersonalization;

                                    // Store the variant instance in the variant info object
                                    variantObj.WebPartInstance = variantInstance;
                                }

                                // Add the variant instance into the web part variants list
                                webpartInstanceVariants.Add(variantInstance);
                            }
                        }
                    }
                }
            }

            // Add the variant list into the base instance
            if (isZone)
            {
                if (zoneInstance.VariantMode != VariantModeEnum.MVT)
                {
                    zoneInstance.ZoneInstanceVariants = zoneInstanceVariants;
                }
            }
            else
            {
                if (webPartInstance.VariantMode != VariantModeEnum.MVT)
                {
                    webPartInstance.PartInstanceVariants = webpartInstanceVariants;
                }
            }

            return true;
        }


        /// <summary>
        /// Clones all the Content personalization variants of the specific page template.
        /// </summary>
        /// <param name="originalTemplateId">The original template id</param>
        /// <param name="newTemplateId">The new template id</param>
        protected virtual void CloneTemplateVariantsInternal(int originalTemplateId, int newTemplateId)
        {
            // Get all variants for the original template
            DataSet ds = GetContentPersonalizationVariants(originalTemplateId);
            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                // Save clone all the variants into the new template
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    ContentPersonalizationVariantInfo cpi = new ContentPersonalizationVariantInfo(row);

                    // Skip widget personalization since there is currently no support for cloning them
                    if (cpi.VariantDocumentID > 0)
                    {
                        continue;
                    }

                    cpi.VariantPageTemplateID = newTemplateId;
                    cpi.VariantID = 0;

                    // Insert the new variant for the new template
                    cpi.Insert();
                }
            }
        }

        #endregion
    }
}