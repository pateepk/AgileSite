using System;
using System.Linq;
using System.Collections.Generic;
using System.Data;
using System.Xml;

using CMS.DataEngine;
using CMS.Helpers;
using CMS.PortalEngine;
using CMS.Base;

namespace CMS.OnlineMarketing
{
    using TypedDataSet = InfoDataSet<MVTVariantInfo>;

    /// <summary>
    /// Class providing MVTVariantInfo management.
    /// </summary>
    public class MVTVariantInfoProvider : AbstractInfoProvider<MVTVariantInfo, MVTVariantInfoProvider>
    {
        #region "Constructors"

        /// <summary>
        /// Constructor.
        /// </summary>
        public MVTVariantInfoProvider()
            : base(MVTVariantInfo.TYPEINFO, new HashtableSettings
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
        /// Returns a query for all the MVTVariantInfo objects.
        /// </summary>
        public static ObjectQuery<MVTVariantInfo> GetMVTVariants()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns MVT variant with specified ID.
        /// </summary>
        /// <param name="variantId">MVT variant ID</param>
        public static MVTVariantInfo GetMVTVariantInfo(int variantId)
        {
            return ProviderObject.GetInfoById(variantId);
        }


        /// <summary>
        /// Gets the MVT variant info.
        /// </summary>
        /// <param name="variantGuid">The MVT variant GUID</param>
        public static MVTVariantInfo GetMVTVariantInfo(Guid variantGuid)
        {
            return ProviderObject.GetInfoByGuid(variantGuid);
        }


        /// <summary>
        /// Returns MVT variant with specified name.
        /// </summary>
        /// <param name="variantName">MVT variant name</param>
        public static MVTVariantInfo GetMVTVariantInfo(string variantName)
        {
            return ProviderObject.GetInfoByCodeName(variantName);
        }


        /// <summary>
        /// Sets (updates or inserts) specified MVT variant.
        /// </summary>
        /// <param name="variantObj">MVT variant to be set</param>
        public static void SetMVTVariantInfo(MVTVariantInfo variantObj)
        {
            ProviderObject.SetInfo(variantObj);
        }


        /// <summary>
        /// Deletes specified MVT variant.
        /// </summary>
        /// <param name="variantObj">MVT variant to be deleted</param>
        public static void DeleteMVTVariantInfo(MVTVariantInfo variantObj)
        {
            ProviderObject.DeleteInfo(variantObj);
        }


        /// <summary>
        /// Deletes MVT variant with specified ID.
        /// </summary>
        /// <param name="variantId">MVT variant ID</param>
        public static void DeleteMVTVariantInfo(int variantId)
        {
            MVTVariantInfo mvtVariantObj = GetMVTVariantInfo(variantId);
            DeleteMVTVariantInfo(mvtVariantObj);
        }


        /// <summary>
        /// Deletes MVT variant with specified ID.
        /// </summary>
        /// <param name="variantGuid">The MVT variant GUID</param>
        public static void DeleteMVTVariantInfo(Guid variantGuid)
        {
            MVTVariantInfo mvtVariantObj = GetMVTVariantInfo(variantGuid);
            DeleteMVTVariantInfo(mvtVariantObj);
        }

        #endregion


        #region "Public methods - Advanced"

        /// <summary>
        /// Gets pairs "MVTVariantInstanceGUID, MVTVariantGUID" which represent web part instances and their variant GUID for the specified combination.
        /// </summary>
        /// <param name="pageTemplateId">The page template id</param>
        /// <param name="combinationId">The combination id</param>
        public static TypedDataSet GetMVTVariants(int pageTemplateId, int combinationId)
        {
            return ProviderObject.GetMVTVariantsInternal(pageTemplateId, combinationId);
        }


        /// <summary>
        /// Saves the variant and generates new combinations if the variant is new.
        /// </summary>
        /// <param name="variantId">The variant id</param>
        /// <param name="variantName">Name of the variant</param>
        /// <param name="variantDisplayName">Display name of the variant</param>
        /// <param name="variantDescription">The variant description</param>
        /// <param name="variantEnabled">Indicates whether the variant is enabled</param>
        /// <param name="zoneId">The zone id</param>
        /// <param name="instanceGuid">The instance GUID</param>
        /// <param name="pageTemplateId">The page template id</param>
        /// <param name="documentId">The document id</param>
        /// <param name="xmlWebParts">Web parts XML</param>
        public static int SaveVariant(int variantId, string variantName, string variantDisplayName, string variantDescription, bool variantEnabled, string zoneId, Guid instanceGuid, int pageTemplateId, int documentId, XmlNode xmlWebParts)
        {
            return ProviderObject.SaveVariantInternal(variantId, variantName, variantDisplayName, variantDescription, variantEnabled, zoneId, instanceGuid, pageTemplateId, documentId, xmlWebParts);
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
        /// Enables/Disables the web part variants.
        /// </summary>
        /// <param name="pageTemplateId">The page template id</param>
        /// <param name="combinationId">The combination id</param>
        /// <param name="enable">Indicates whether enable or disable the combination variants</param>
        public static void EnableWebPartVariants(int pageTemplateId, int combinationId, bool enable)
        {
            ProviderObject.EnableWebPartVariantsInternal(pageTemplateId, combinationId, enable);
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
        /// Gets all the MVT variants for the specified template.
        /// </summary>
        /// <param name="pageTemplateId">The page template id</param>
        public static TypedDataSet GetMVTVariants(int pageTemplateId)
        {
            return ProviderObject.GetMVTVariantsInternal(pageTemplateId);
        }


        /// <summary>
        /// Gets the MVT variants.
        /// </summary>
        /// <param name="pageTemplateId">The page template id</param>
        /// <param name="zoneId">The zone id</param>
        /// <param name="instanceGuid">The instance GUID</param>
        /// <param name="documentId">The document id</param>
        public static TypedDataSet GetMVTVariants(int pageTemplateId, string zoneId, Guid instanceGuid, int documentId)
        {
            return ProviderObject.GetMVTVariantsInternal(pageTemplateId, zoneId, instanceGuid, documentId);
        }


        /// <summary>
        /// Loads the MVT variants for the specified instance (web part/zone/widget).
        /// Return true, if the variants were successfully loaded.
        /// </summary>
        /// <param name="instance">The zone instance of web part instance</param>
        /// <param name="isZone">Indicates if the parameter instance object is a zone</param>
        /// <param name="documentId">Document ID if the instance holds a widget</param>
        /// <returns>
        /// True, if the variants were successfully loaded. False, if it was not possible to load variants, for example due to missing information in the instance.
        /// </returns>
        public static bool LoadMVTVariants(object instance, bool isZone, int documentId = 0)
        {
            return ProviderObject.LoadMVTVariantsInternal(instance, isZone, documentId);
        }


        /// <summary>
        /// Returns the MVT variant id for the specified template and code name.
        /// </summary>
        /// <param name="pageTemplateId">The page template id</param>
        /// <param name="variantName">The variant code name</param>
        public static int GetMVTVariantId(int pageTemplateId, string variantName)
        {
            return ProviderObject.GetMVTVariantIdInternal(pageTemplateId, variantName);
        }


        /// <summary>
        /// Clones all the MVT variants and their combinations of the specific page template.
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
        protected override void SetInfo(MVTVariantInfo info)
        {
            base.SetInfo(info);

            CacheHelper.TouchKey("om.mvtvariant|bytemplateid|" + info.MVTVariantPageTemplateID);

            // Clear the cached instances
            info.WebPartInstance = null;
            info.WebPartZoneInstance = null;

            if ((!info.MVTVariantEnabled) && (info.MVTVariantEnabledOriginal != info.MVTVariantEnabled))
            {
                // Disable related combinations
                MVTCombinationInfoProvider.EnableCombinations(info.MVTVariantPageTemplateID, info.MVTVariantID, false);
            }
        }


        /// <summary>
        /// Deletes the object to the database.
        /// </summary>
        /// <param name="info">Object to delete</param>
        protected override void DeleteInfo(MVTVariantInfo info)
        {
            // Get all combinations generated using this variant
            var combinationVariants = MVTCombinationVariationInfoProvider.GetCombinationVariations().WhereEquals("MVTVariantID", info.MVTVariantID).ToList();

            base.DeleteInfo(info);

            if (combinationVariants.Any())
            {
                // Get condition containing all combinations generated using this variant
                var condition = new WhereCondition().WhereIn("MVTCombinationID", combinationVariants.Select(t => t.MVTCombinationID).ToList());

                // Delete them from binding table and from combination table
                MVTCombinationVariationInfoProvider.DeleteMVTCombinationVariationInfo(condition.WhereCondition);
                MVTCombinationInfoProvider.DeleteMVTCombinationInfo(condition.WhereCondition);
            }

            // Clear the combinations hash table
            ProviderHelper.ClearHashtables(MVTCombinationInfo.OBJECT_TYPE, true);

            if (info != null)
            {
                // Clear variant cache
                CacheHelper.TouchKey("om.mvtvariant|bytemplateid|" + info.MVTVariantPageTemplateID);
            }
        }

        #endregion


        #region "Internal methods - Advanced"

        /// <summary>
        /// Gets pairs "MVTVariantInstanceGUID, MVTVariantGUID" which represent web part instances and their variant GUID for the specified combination.
        /// </summary>
        /// <param name="pageTemplateId">The page template id</param>
        /// <param name="combinationId">The combination id</param>
        protected virtual TypedDataSet GetMVTVariantsInternal(int pageTemplateId, int combinationId)
        {
            TypedDataSet ds = null;

            // Cache list of combination variants
            using (var cs = new CachedSection<TypedDataSet>(ref ds, 1440, true, null, "onlinemarketingcombinationvariants", pageTemplateId, combinationId))
            {
                // Check whether data should be loaded
                if (cs.LoadData)
                {
                    // Get variants data

                    ds = GetMVTVariants().WhereIn("MVTVariantID", new IDQuery(MVTCombinationVariationInfo.OBJECT_TYPE, "MVTVariantID").WhereEquals("MVTCombinationID", combinationId))
                                         .OrderBy("MVTVariantID")
                                         .TypedResult;

                    // Cache data if it is required
                    if (cs.Cached)
                    {
                        // Prepare cache dependency
                        cs.CacheDependency = CacheHelper.GetCacheDependency("om.mvtvariant|bytemplateid|" + pageTemplateId);
                    }

                    cs.Data = ds;
                }
            }

            return ds;
        }


        /// <summary>
        /// Gets all the MVT variants for the specified template.
        /// </summary>
        /// <param name="pageTemplateId">The page template id</param>
        protected virtual TypedDataSet GetMVTVariantsInternal(int pageTemplateId)
        {
            TypedDataSet ds = null;

            // Cache list of MVT variants
            using (var cs = new CachedSection<TypedDataSet>(ref ds, 1440, true, null, "onlinemarketingmvtvariantsbytemplateid", pageTemplateId))
            {
                // Check whether data should be loaded
                if (cs.LoadData)
                {
                    // Get variants data
                    ds = GetMVTVariants().WhereEquals("MVTVariantPageTemplateID", pageTemplateId)
                                         .TypedResult;

                    // Cache data if it is required
                    if (cs.Cached)
                    {
                        // Prepare cache dependency
                        cs.CacheDependency = CacheHelper.GetCacheDependency("om.mvtvariant|bytemplateid|" + pageTemplateId);
                    }

                    cs.Data = ds;
                }
            }

            return ds;
        }


        /// <summary>
        /// Gets the MVT variants.
        /// </summary>
        /// <param name="pageTemplateId">The page template id</param>
        /// <param name="zoneId">The zone id</param>
        /// <param name="instanceGuid">The instance GUID</param>
        /// <param name="documentId">The document id</param>
        protected virtual TypedDataSet GetMVTVariantsInternal(int pageTemplateId, string zoneId, Guid instanceGuid, int documentId)
        {
            TypedDataSet ds = GetMVTVariantsInternal(pageTemplateId);

            // Limit dataset for required zone/instance
            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                InfoDataSet<MVTVariantInfo> variants = new InfoDataSet<MVTVariantInfo>();
                variants.Tables.Add(ds.Tables[0].Clone());

                // Loop through all variants
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    // Zone id
                    if (ValidationHelper.GetString(dr["MVTVariantZoneID"], String.Empty).EqualsCSafe(zoneId, true))
                    {
                        // Instance GUID
                        if (ValidationHelper.GetGuid(dr["MVTVariantInstanceGUID"], Guid.Empty) == instanceGuid)
                        {
                            // Document id
                            if (ValidationHelper.GetInteger(dr["MVTVariantDocumentID"], 0) == documentId)
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
        /// Deletes all variants of all widgets in the document.
        /// </summary>
        /// <param name="zoneId">The zone id</param>
        /// <param name="pageTemplateId">The page template id</param>
        /// <param name="documentId">The document id</param>
        protected virtual void DeleteWidgetVariantsInternal(string zoneId, int pageTemplateId, int documentId)
        {
            if (!string.IsNullOrEmpty(zoneId) && (pageTemplateId > 0) && (documentId > 0))
            {
                var variants = GetMVTVariants().BinaryData(false)
                                               .WhereEquals("MVTVariantPageTemplateID", pageTemplateId)
                                               .WhereEquals("MVTVariantZoneID", zoneId)
                                               .WhereEquals("MVTVariantDocumentID", documentId);

                // Delete the variants using the standard DeleteInfo() with the RemoveDependencies procedure
                foreach (var variant in variants)
                {
                    // Delete the variant and its combinations
                    DeleteInfo(variant);
                }

                // Clear variant cache
                CacheHelper.TouchKey("om.mvtvariant|bytemplateid|" + pageTemplateId);
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
            parameters.Add("@MVTVariantZoneID", zoneId);
            parameters.Add("@NewZoneID", newZoneId);
            parameters.Add("@MVTVariantPageTemplateID", pageTemplateId);

            string where = "(MVTVariantPageTemplateID = @MVTVariantPageTemplateID) AND (MVTVariantZoneID = @MVTVariantZoneID)";

            if (instanceGuid != Guid.Empty)
            {
                where = SqlHelper.AddWhereCondition(where, "(MVTVariantInstanceGUID = @MVTVariantInstanceGUID)");
                parameters.Add("@MVTVariantInstanceGUID", instanceGuid);
            }
            else
            {
                where = SqlHelper.AddWhereCondition(where, "(MVTVariantInstanceGUID IS NOT NULL)");
            }

            // Update
            UpdateData("MVTVariantZoneID = @NewZoneID", parameters, where);

            // Clear cache
            ClearHashtables(true);
            CacheHelper.TouchKey("om.mvtvariant|bytemplateid|" + pageTemplateId);
        }


        /// <summary>
        /// Enables/Disables the web part variants.
        /// </summary>
        /// <param name="pageTemplateId">The page template id</param>
        /// <param name="combinationId">The combination id</param>
        /// <param name="enabled">Indicates whether enable or disable the combination variants</param>
        protected virtual void EnableWebPartVariantsInternal(int pageTemplateId, int combinationId, bool enabled)
        {
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@MVTVariantEnabled", enabled);
            parameters.Add("@MVTCombinationID", combinationId);

            // Update
            UpdateData("MVTVariantEnabled = @MVTVariantEnabled", parameters, "MVTVariantID IN (SELECT MVTVariantID FROM OM_MVTCombinationVariation WHERE MVTCombinationID = @MVTCombinationID)");

            // Clear cache
            ClearHashtables(true);
            CacheHelper.TouchKey("om.mvtvariant|bytemplateid|" + pageTemplateId);
        }


        /// <summary>
        /// Saves the variant and generates new combinations if the variant is new.
        /// </summary>
        /// <param name="variantId">The variant id</param>
        /// <param name="variantName">Name of the variant</param>
        /// <param name="variantDisplayName">Display name of the variant</param>
        /// <param name="variantDescription">The variant description</param>
        /// <param name="variantEnabled">Indicates whether the variant is enabled</param>
        /// <param name="zoneId">The zone id</param>
        /// <param name="instanceGuid">The instance GUID</param>
        /// <param name="pageTemplateId">The page template id</param>
        /// <param name="documentId">The document id</param>
        /// <param name="xmlWebParts">Web parts XML</param>
        protected virtual int SaveVariantInternal(int variantId, string variantName, string variantDisplayName, string variantDescription, bool variantEnabled, string zoneId, Guid instanceGuid, int pageTemplateId, int documentId, XmlNode xmlWebParts)
        {
            string webPartsXml = xmlWebParts.OuterXml;
            return SaveVariantInternal(variantId, variantName, variantDisplayName, variantDescription, variantEnabled, zoneId, instanceGuid, pageTemplateId, documentId, webPartsXml);
        }


        /// <summary>
        /// Saves the variant and generates new combinations if the variant is new.
        /// </summary>
        /// <param name="variantId">The variant id</param>
        /// <param name="variantName">Name of the variant</param>
        /// <param name="variantDisplayName">Display name of the variant</param>
        /// <param name="variantDescription">The variant description</param>
        /// <param name="variantEnabled">Indicates whether the variant is enabled</param>
        /// <param name="zoneId">The zone id</param>
        /// <param name="instanceGuid">The instance GUID</param>
        /// <param name="pageTemplateId">The page template id</param>
        /// <param name="documentId">The document id</param>
        /// <param name="webPartsXml">Web parts XML string</param>
        protected virtual int SaveVariantInternal(int variantId, string variantName, string variantDisplayName, string variantDescription, bool variantEnabled, string zoneId, Guid instanceGuid, int pageTemplateId, int documentId, string webPartsXml)
        {
            bool isNewVariant = (variantId == 0);

            MVTVariantInfo variantObj = null;

            if (isNewVariant)
            {
                // Create a new variant
                variantObj = new MVTVariantInfo();

                variantObj.MVTVariantName = variantName;
                variantObj.MVTVariantDisplayName = variantDisplayName;
                variantObj.MVTVariantDescription = variantDescription;
                variantObj.MVTVariantEnabled = variantEnabled;
                variantObj.MVTVariantInstanceGUID = instanceGuid;
                variantObj.MVTVariantZoneID = zoneId;
                variantObj.MVTVariantPageTemplateID = pageTemplateId;
                if (documentId > 0)
                {
                    variantObj.MVTVariantDocumentID = documentId;
                }
            }
            else
            {
                // Get existing variant
                variantObj = GetMVTVariantInfo(variantId);
            }

            if (variantObj != null)
            {
                pageTemplateId = variantObj.MVTVariantPageTemplateID;

                // Save the variant properties
                variantObj.MVTVariantWebParts = webPartsXml;

                // Create transaction - because of the combination number -> string: "COMBINATION_NAME_PREFIX + combinationNumber" has to be unique in the web part variants scope.
                using (var tr = BeginTransaction())
                {
                    SetMVTVariantInfo(variantObj);

                    // Create new combinations
                    if (isNewVariant)
                    {
                        int combinationNumber = MVTCombinationInfoProvider.GetNewCombinationNumber(pageTemplateId, documentId);

                        MVTCombinationInfo combinationInfo = new MVTCombinationInfo();
                        combinationInfo.MVTCombinationName = MVTCombinationInfoProvider.GetCombinationName(combinationNumber);
                        combinationInfo.MVTCombinationPageTemplateID = pageTemplateId;
                        combinationInfo.MVTCombinationEnabled = true;
                        combinationInfo.MVTCombinationCustomName = MVTCombinationInfoProvider.GetNewCombinationCustomName(null, ((!string.IsNullOrEmpty(variantDisplayName)) ? variantDisplayName : variantName), combinationNumber);

                        // Set the documentId for combinations containing widgets
                        combinationInfo.MVTCombinationDocumentID = documentId;

                        MVTCombinationInfoProvider.SetMVTCombinationInfo(combinationInfo);
                        combinationNumber++;

                        // Get all combinations which do not contain this web part/zone and clone them + add this new variant.
                        DataSet combinations = null;
                        if (instanceGuid == Guid.Empty)
                        {
                            // Zone variant
                            combinations = MVTCombinationInfoProvider.GetCombinationsWithoutZone(pageTemplateId, zoneId, documentId);
                        }
                        else
                        {
                            // Web part variant
                            combinations = MVTCombinationInfoProvider.GetCombinationsWithoutWebpart(pageTemplateId, instanceGuid, documentId);
                        }

                        foreach (DataRow row in combinations.Tables[0].Rows)
                        {
                            int currentCombinationId = ValidationHelper.GetInteger(row["MVTCombinationID"], 0);
                            bool currentCombinationIsDefault = ValidationHelper.GetBoolean(row["MVTCombinationIsDefault"], false);
                            string currentCombinationCustomName = ValidationHelper.GetString(row["MVTCombinationCustomName"], string.Empty);

                            if ((!currentCombinationIsDefault)
                                && (currentCombinationId != combinationInfo.MVTCombinationID))
                            {
                                MVTCombinationInfo cInfo = new MVTCombinationInfo(row);
                                cInfo.MVTCombinationID = 0;
                                cInfo.MVTCombinationCustomName = MVTCombinationInfoProvider.GetNewCombinationCustomName(currentCombinationCustomName, ((!string.IsNullOrEmpty(variantDisplayName)) ? variantDisplayName : variantName), combinationNumber);
                                cInfo.MVTCombinationName = MVTCombinationInfoProvider.GetCombinationName(combinationNumber);
                                cInfo.MVTCombinationGUID = new Guid();

                                // Set the documentId for combinations containing widgets
                                if (documentId > 0)
                                {
                                    cInfo.MVTCombinationDocumentID = documentId;
                                }

                                MVTCombinationInfoProvider.SetMVTCombinationInfo(cInfo);
                                combinationNumber++;

                                MVTCombinationVariationInfoProvider.CombineCombinationVariants(currentCombinationId, cInfo.MVTCombinationID, variantObj.MVTVariantID);
                            }
                        }

                        MVTCombinationVariationInfoProvider.AddRelationship(combinationInfo.MVTCombinationID, variantObj.MVTVariantID);

                        // If inserting disabled variant => disable all created combinations
                        if (!variantObj.MVTVariantEnabled)
                        {
                            MVTCombinationInfoProvider.EnableCombinations(variantObj.MVTVariantPageTemplateID, variantObj.MVTVariantID, false);
                        }
                    }

                    // Commit transaction
                    tr.Commit();
                }

                return variantObj.MVTVariantID;
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
            MVTVariantInfo variantObj = GetMVTVariantInfo(variantId);

            if (variantObj != null)
            {
                variantObj.MVTVariantWebParts = xmlWebParts.OuterXml;

                // Save the changes
                SetMVTVariantInfo(variantObj);
            }
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
                    // Delete only MVT variants
                    if (wpi.VariantMode == VariantModeEnum.MVT)
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
                                DeleteMVTVariantInfo(variant.VariantID);
                            }
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Returns the MVT variant id for the specified template and code name.
        /// </summary>
        /// <param name="pageTemplateId">The page template id</param>
        /// <param name="variantName">The variant code name</param>
        protected virtual int GetMVTVariantIdInternal(int pageTemplateId, string variantName)
        {
            var variant = GetMVTVariants().TopN(1).Column("MVTVariantID").BinaryData(false)
                                          .WhereEquals("MVTVariantPageTemplateID", pageTemplateId)
                                          .WhereEquals("MVTVariantName", variantName)
                                          .FirstOrDefault();

            return variant?.MVTVariantID ?? 0;
        }


        /// <summary>
        /// Loads the MVT variants for the specified instance (web part/zone/widget).
        /// Return true, if the variants were successfully loaded.
        /// </summary>
        /// <param name="instance">The zone instance of web part instance</param>
        /// <param name="isZone">Indicates if the parameter instance object is a zone</param>
        /// <param name="documentId">Document ID if the instance holds a widget</param>
        /// <returns>
        /// True, if the variants were successfully loaded. False, if it was not possible to load variants, for example due to missing information in the instance.
        /// </returns>
        protected virtual bool LoadMVTVariantsInternal(object instance, bool isZone, int documentId)
        {
            if (instance == null)
            {
                return false;
            }

            WebPartZoneInstance zoneInstance = null;
            WebPartInstance webPartInstance = null;

            int pageTemplateId = 0;
            string zoneId = string.Empty;
            Guid instanceGuid = Guid.Empty;
            VariantModeEnum currentVariantMode = VariantModeEnum.None;

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
            DataSet ds = GetMVTVariants(pageTemplateId, zoneId, instanceGuid, documentId);

            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                // If there are Content personalization variants already in the instance, set the Conflicted mode and do not continue
                if (currentVariantMode == VariantModeEnum.ContentPersonalization)
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
                // There are no Content personalization variants in the instance => set the variant mode to MVT and proceed to loading the MVT variants
                else
                {
                    if (isZone)
                    {
                        zoneInstance.VariantMode = VariantModeEnum.MVT;
                    }
                    else
                    {
                        webPartInstance.VariantMode = VariantModeEnum.MVT;
                    }
                }

                // Load all the variants for the current instance
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    string variantXml = ValidationHelper.GetString(row["MVTVariantWebParts"], string.Empty);
                    int variantId = ValidationHelper.GetInteger(row["MVTVariantID"], 0);
                    if (!string.IsNullOrEmpty(variantXml))
                    {
                        // Try to get the variant instance from the info object. If not found, then create the variant instance and store it in the variant info object
                        MVTVariantInfo variantObj = GetMVTVariantInfo(variantId);
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
                                    variantInstance.VariantMode = VariantModeEnum.MVT;

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
                                    variantInstance.VariantMode = VariantModeEnum.MVT;

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
                if (zoneInstance.VariantMode != VariantModeEnum.ContentPersonalization)
                {
                    zoneInstance.ZoneInstanceVariants = zoneInstanceVariants;
                }
            }
            else
            {
                if (webPartInstance.VariantMode != VariantModeEnum.ContentPersonalization)
                {
                    webPartInstance.PartInstanceVariants = webpartInstanceVariants;
                }
            }

            return true;
        }


        /// <summary>
        /// Clones all the MVT variants and their combinations of the specific page template.
        /// </summary>
        /// <param name="originalTemplateId">The original template id</param>
        /// <param name="newTemplateId">The new template id</param>
        protected virtual void CloneTemplateVariantsInternal(int originalTemplateId, int newTemplateId)
        {
            // Get all variants for the original template
            DataSet ds = GetMVTVariants(originalTemplateId);
            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                // Create the default combination for the new template
                MVTCombinationInfoProvider.EnsureTestCombination(newTemplateId);

                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    // Skip widget variants since the cloning does not work.
                    if (ValidationHelper.GetInteger(row["MVTVariantDocumentID"], 0) != 0)
                    {
                        continue;
                    }

                    // Save a copy of the original variant. This method will also create all relevant combinations.
                    SaveVariantInternal(0,
                                        Guid.NewGuid().ToString(),
                                        row["MVTVariantDisplayName"] as string,
                                        row["MVTVariantDescription"] as string,
                                        ValidationHelper.GetBoolean(row["MVTVariantEnabled"], false),
                                        row["MVTVariantZoneID"] as string,
                                        ValidationHelper.GetGuid(row["MVTVariantInstanceGUID"], Guid.Empty),
                                        newTemplateId,
                                        0,
                                        row["MVTVariantWebParts"] as string);
                }

                DataSet originalCombinations = MVTCombinationInfoProvider.GetCombinationsForTemplate(originalTemplateId, false);
                DataSet newCombinations = MVTCombinationInfoProvider.GetCombinationsForTemplate(newTemplateId, false);

                // Copy the properties of the original combinations (custom name, enabled, isDefault) into the newly created combinations
                if (!DataHelper.DataSourceIsEmpty(originalCombinations)
                    && !DataHelper.DataSourceIsEmpty(newCombinations))
                {
                    DataRow originalCombinationRow = null;
                    DataRow newCombinationRow = null;

                    // Loop through all the new and old combinations and update the values of the new combinations (custom name, enabled, isDefault)
                    for (int i = 0; i < originalCombinations.Tables[0].Rows.Count; i++)
                    {
                        originalCombinationRow = originalCombinations.Tables[0].Rows[i];
                        if (newCombinations.Tables[0].Rows.Count > i)
                        {
                            newCombinationRow = newCombinations.Tables[0].Rows[i];
                            int newCombinationId = ValidationHelper.GetInteger(newCombinationRow["MVTCombinationID"], 0);
                            string oldCustomName = ValidationHelper.GetString(originalCombinationRow["MVTCombinationCustomName"], string.Empty);
                            bool oldEnabled = ValidationHelper.GetBoolean(originalCombinationRow["MVTCombinationEnabled"], false);
                            bool oldIsDefault = ValidationHelper.GetBoolean(originalCombinationRow["MVTCombinationIsDefault"], false);
                            MVTCombinationInfo ci = MVTCombinationInfoProvider.GetMVTCombinationInfo(newCombinationId);
                            if (ci != null)
                            {
                                ci.MVTCombinationCustomName = oldCustomName;
                                ci.MVTCombinationEnabled = oldEnabled;
                                ci.MVTCombinationIsDefault = oldIsDefault;

                                // Update the newly created combination
                                ci.Update();
                            }
                        }
                    }
                }
            }
        }

        #endregion
    }
}