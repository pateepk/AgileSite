using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime.Serialization;
using System.Security;

using CMS;
using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.Ecommerce;
using CMS.Helpers;
using CMS.SiteProvider;

[assembly: RegisterObjectType(typeof(SKUInfo), SKUInfo.OBJECT_TYPE_SKU)]
[assembly: RegisterObjectType(typeof(SKUInfo), SKUInfo.OBJECT_TYPE_OPTIONSKU)]
[assembly: RegisterObjectType(typeof(SKUInfo), SKUInfo.OBJECT_TYPE_VARIANT)]

namespace CMS.Ecommerce
{
    /// <summary>
    /// SKUInfo data container class.
    /// </summary>
    [Serializable]
    public class SKUInfo : AbstractInfo<SKUInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type for sku
        /// </summary>
        public const string OBJECT_TYPE_SKU = PredefinedObjectType.SKU;

        /// <summary>
        /// Object type for optionsku
        /// </summary>
        public const string OBJECT_TYPE_OPTIONSKU = "ecommerce.skuoption";

        /// <summary>
        /// Object type for variant
        /// </summary>
        public const string OBJECT_TYPE_VARIANT = "ecommerce.skuvariant";


        /// <summary>
        /// Type information for products.
        /// </summary>
        public static ObjectTypeInfo TYPEINFOSKU = new ObjectTypeInfo(typeof(SKUInfoProvider), OBJECT_TYPE_SKU, "ECommerce.SKU", "SKUID", "SKULastModified", "SKUGUID", null, "SKUName", null, "SKUSiteID", null, null)
        {
            // Child object types
            // Object dependencies
            DependsOn = new List<ObjectDependency>()
                                    {
                                        new ObjectDependency("SKUDepartmentID", DepartmentInfo.OBJECT_TYPE),
                                        new ObjectDependency("SKUManufacturerID", ManufacturerInfo.OBJECT_TYPE),
                                        new ObjectDependency("SKUInternalStatusID", InternalStatusInfo.OBJECT_TYPE),
                                        new ObjectDependency("SKUPublicStatusID", PublicStatusInfo.OBJECT_TYPE),
                                        new ObjectDependency("SKUSupplierID", SupplierInfo.OBJECT_TYPE),
                                        new ObjectDependency("SKUTaxClassID", TaxClassInfo.OBJECT_TYPE),
                                        new ObjectDependency("SKUBrandID", BrandInfo.OBJECT_TYPE),
                                        new ObjectDependency("SKUCollectionID", CollectionInfo.OBJECT_TYPE)
                                    },
            Extends = new List<ExtraColumn>()
            {
                new ExtraColumn(PredefinedObjectType.NODE, "NodeSKUID"),
            },
            // Synchronization
            SynchronizationSettings =
            {
                IncludeToSynchronizationParentDataSet = IncludeToParentEnum.Complete,
                LogSynchronization = SynchronizationTypeEnum.LogSynchronization,
                ObjectTreeLocations = new List<ObjectTreeLocation>()
                {
                    new ObjectTreeLocation(SITE, ECommerceModule.ECOMMERCE),
                    new ObjectTreeLocation(GLOBAL, ECommerceModule.ECOMMERCE),
                }
            },
            
            // Others
            LogEvents = true,
            TouchCacheDependencies = true,
            ModuleName = ModuleName.ECOMMERCE,
            Feature = FeatureEnum.Ecommerce,
            SupportsInvalidation = true,
            SupportsGlobalObjects = true,
            EnabledColumn = "SKUEnabled",
            ImportExportSettings =
            {
                LogExport = true,
                IsExportable = true,
                ObjectTreeLocations = new List<ObjectTreeLocation>()
                {
                    new ObjectTreeLocation(SITE, ECommerceModule.ECOMMERCE),
                    new ObjectTreeLocation(GLOBAL, ECommerceModule.ECOMMERCE),
                },
            },
            TypeCondition = new TypeCondition()
                .WhereIsNull("SKUParentSKUID")
                .WhereIsNull("SKUOptionCategoryID"),
            HasMetaFiles = true,
            SerializationSettings =
            {
                ExcludedFieldNames = { "SKUCreated" }
            },
            ContinuousIntegrationSettings =
            {
                Enabled = true,
                IdentificationField = "SKUGUID",
                ObjectFileNameFields = { "SKUName" }
            }
        };


        /// <summary>
        /// Type information for product options.
        /// </summary>
        public static ObjectTypeInfo TYPEINFOOPTIONSKU = new ObjectTypeInfo(typeof (SKUInfoProvider), OBJECT_TYPE_OPTIONSKU, "ECommerce.SKU", "SKUID", "SKULastModified", "SKUGUID", null, "SKUName", null, "SKUSiteID", "SKUOptionCategoryID", OptionCategoryInfo.OBJECT_TYPE)
        {
            OriginalTypeInfo = TYPEINFOSKU,
            // Child object types
            // Object dependencies
            DependsOn = new List<ObjectDependency>()
            {
                new ObjectDependency("SKUDepartmentID", DepartmentInfo.OBJECT_TYPE),
                new ObjectDependency("SKUManufacturerID", ManufacturerInfo.OBJECT_TYPE),
                new ObjectDependency("SKUInternalStatusID", InternalStatusInfo.OBJECT_TYPE),
                new ObjectDependency("SKUPublicStatusID", PublicStatusInfo.OBJECT_TYPE),
                new ObjectDependency("SKUSupplierID", SupplierInfo.OBJECT_TYPE),
                new ObjectDependency("SKUTaxClassID", TaxClassInfo.OBJECT_TYPE),
                new ObjectDependency("SKUBrandID", BrandInfo.OBJECT_TYPE),
                new ObjectDependency("SKUCollectionID", CollectionInfo.OBJECT_TYPE)
            },
            // Binding object types
            // Synchronization
            SynchronizationSettings =
            {
                IncludeToSynchronizationParentDataSet = IncludeToParentEnum.Complete,
                LogSynchronization = SynchronizationTypeEnum.TouchParent
            },
            // Others
            LogEvents = true,
            TouchCacheDependencies = true,
            ModuleName = ModuleName.ECOMMERCE,
            Feature = FeatureEnum.Ecommerce,
            SupportsGlobalObjects = true,
            SupportsInvalidation = true,
            OrderColumn = "SKUOrder",
            EnabledColumn = "SKUEnabled",
            TypeCondition = new TypeCondition()
                .WhereIsNull("SKUParentSKUID")
                .WhereIsNotNull("SKUOptionCategoryID"),
            ImportExportSettings =
            {
                LogExport = true,
                AllowSingleExport = false
            },
            HasMetaFiles = true,
            SerializationSettings =
            {
                ExcludedFieldNames = { "SKUCreated" }
            },
            ContinuousIntegrationSettings =
            {
                Enabled = true,
                IdentificationField = "SKUGUID",
                ObjectFileNameFields = { "SKUName" }
            }
        };


        /// <summary>
        /// Type information for product variants.
        /// </summary>
        public static ObjectTypeInfo TYPEINFOVARIANT = new ObjectTypeInfo(typeof(SKUInfoProvider), OBJECT_TYPE_VARIANT, "ECommerce.SKU", "SKUID", "SKULastModified", "SKUGUID", null, "SKUName", null, "SKUSiteID", "SKUParentSKUID", OBJECT_TYPE_SKU)
        {
            MacroCollectionName = "sku.SKUVariant",
            OriginalTypeInfo = TYPEINFOSKU,
            // Child object types
            // Object dependencies
            DependsOn = new List<ObjectDependency>()
                                    {
                                        new ObjectDependency("SKUDepartmentID", DepartmentInfo.OBJECT_TYPE),
                                        new ObjectDependency("SKUManufacturerID", ManufacturerInfo.OBJECT_TYPE),
                                        new ObjectDependency("SKUInternalStatusID", InternalStatusInfo.OBJECT_TYPE),
                                        new ObjectDependency("SKUPublicStatusID", PublicStatusInfo.OBJECT_TYPE),
                                        new ObjectDependency("SKUSupplierID", SupplierInfo.OBJECT_TYPE),
                                        new ObjectDependency("SKUTaxClassID", TaxClassInfo.OBJECT_TYPE),
                                        new ObjectDependency("SKUBrandID", BrandInfo.OBJECT_TYPE),
                                        new ObjectDependency("SKUCollectionID", CollectionInfo.OBJECT_TYPE)
                                    },
            // Binding object types
            // Synchronization
            SynchronizationSettings =
            {
                IncludeToSynchronizationParentDataSet = IncludeToParentEnum.Complete,
                LogSynchronization = SynchronizationTypeEnum.TouchParent
            },
            // Others
            LogEvents = true,
            TouchCacheDependencies = true,
            ModuleName = ModuleName.ECOMMERCE,
            Feature = FeatureEnum.Ecommerce,
            SupportsInvalidation = true,
            SupportsGlobalObjects = true,
            OrderColumn = "SKUOrder",
            EnabledColumn = "SKUEnabled",
            TypeCondition = new TypeCondition()
                .WhereIsNotNull("SKUParentSKUID")
                .WhereIsNull("SKUOptionCategoryID"),
            ImportExportSettings = { LogExport = true, AllowSingleExport = false },
            HasMetaFiles = true,
            SerializationSettings =
            {
                ExcludedFieldNames = { "SKUCreated" }
            },
            ContinuousIntegrationSettings =
            {
                Enabled = true,
                IdentificationField = "SKUGUID",
                ObjectFileNameFields = { "SKUName" }
            }
        };

        #endregion


        #region "Variables"

        /// <summary>
        /// SKU custom data.
        /// </summary>
        protected ContainerCustomData mSKUCustomData;

        private bool? mHasVariants;

        #endregion


        #region "GeneralizedInfo properties and methods"

        /// <summary>
        /// Type info.
        /// </summary>
        public override ObjectTypeInfo TypeInfo
        {
            get
            {
                if (SKUOptionCategoryID > 0)
                {
                    return TYPEINFOOPTIONSKU;
                }

                return (SKUParentSKUID > 0) ? TYPEINFOVARIANT : TYPEINFOSKU;
            }
        }


        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            SKUInfoProvider.DeleteSKUInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            SKUInfoProvider.SetSKUInfo(this);
        }


        /// <summary>
        /// Checks the object license. Returns true if the licensing conditions for this object were matched
        /// </summary>
        /// <param name="action">Object action</param>
        /// <param name="domainName">Domain name, if not set, uses current domain</param>
        protected sealed override bool CheckLicense(ObjectActionEnum action, string domainName)
        {
            return SKUInfoProvider.CheckLicense(action, domainName);
        }


        /// <summary>
        /// This method is called on cloned object prepared in memory by InsertAsClone method. 
        /// Override if you need to do further actions before inserting actual object to DB (insert special objects, modify foreign keys, copy files, etc.).
        /// Calls Insert() by default.
        /// </summary>
        /// <param name="settings">Cloning settings</param>
        /// <param name="result">Result of the cloning - messages in this object will be altered by processing this method</param>
        /// <param name="originalObject">Original source BaseInfo (object being cloned)</param>
        protected override void InsertAsCloneInternal(CloneSettings settings, CloneResult result, BaseInfo originalObject)
        {
            // Do not add the cloned product to existing bundles which contains original product
            settings.ExcludedOtherBindingTypes.Add(BundleInfo.OBJECT_TYPE);

            // Use special action contexts to turn off unnecessary actions 
            using (ECommerceActionContext eCommerceContext = new ECommerceActionContext())
            {
                eCommerceContext.SetLowestPriceToParent = false;

                if (IsProductOption)
                {
                    SKUOrder = GetLastObjectOrder(null);
                }

                base.InsertAsCloneInternal(settings, result, originalObject);
            }
        }


        /// <summary>
        /// This method is called once the object is completely cloned (with all children, bindings, etc.).
        /// Performs additional 
        /// </summary>
        /// <param name="settings">Cloning settings</param>
        /// <param name="result">Result of the cloning - messages in this object will be altered by processing this method</param>
        /// <param name="originalObject">Original source BaseInfo (object being cloned)</param>
        protected override void InsertAsClonePostprocessing(CloneSettings settings, CloneResult result, BaseInfo originalObject)
        {
            var metaFiles = MetaFileInfoProvider.GetMetaFiles(SKUID, OBJECT_TYPE_SKU, null, null, null, "MetaFileGroupName, MetaFileGUID, MetaFileName", 0);
            if (metaFiles != null)
            {
                foreach (var metaFile in metaFiles)
                {
                    if (metaFile.MetaFileGroupName.Equals(ObjectAttachmentsCategories.IMAGE, StringComparison.Ordinal))
                    {
                        SKUImagePath = MetaFileInfoProvider.GetMetaFileUrl(metaFile.MetaFileGUID, metaFile.MetaFileName);
                        SetObject();
                    }
                    else if (metaFile.MetaFileGroupName.Equals(ObjectAttachmentsCategories.EPRODUCT, StringComparison.Ordinal))
                    {
                        // Update MetaFileGUID for SKUFile
                        var skuFile = SKUFileInfoProvider.GetSKUFiles()
                            .WhereEquals("FileSKUID", SKUID)
                            .And()
                            .WhereEquals("FileName", metaFile.MetaFileName)
                            .TopN(1)
                            .FirstObject;

                        if (skuFile != null)
                        {
                            skuFile.FileMetaFileGUID = metaFile.MetaFileGUID;
                            skuFile.Generalized.SetObject();
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Method which is called after the order of the object was changed. Generates staging tasks and webfarm tasks by default.
        /// </summary>
        protected override void SetObjectOrderPostprocessing()
        {
            base.SetObjectOrderPostprocessing();

            // Clear the cache
            CacheHelper.TouchKey("ecommerce.sku|byid|" + SKUID);
        }


        /// <summary>
        /// Sets the field value.
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="value">New value</param>
        public override bool SetValue(string columnName, object value)
        {
            // Ensures the completeness of the object, this is needed due to inline editing with enabled CI
            if (ObjectStatus == ObjectStatusEnum.Unchanged && !IsComplete)
            {
                MakeComplete(true);
            }

            return base.SetValue(columnName, value);
        }

        #endregion


        #region "Properties"

        /// <summary>
        /// Product internal status ID. For product variant with internal status ID not set, it is returned from parent product.
        /// </summary>
        [RegisterProperty]
        [DatabaseField]
        public virtual int SKUInternalStatusID
        {
            get
            {
                return GetEffectiveIntegerValue("SKUInternalStatusID", 0);
            }
            set
            {
                SetValue("SKUInternalStatusID", value, 0);
            }
        }


        /// <summary>
        /// Product department ID. For product variant with department ID not set, it is returned from parent product.
        /// </summary>
        [RegisterProperty]
        [DatabaseField]
        public virtual int SKUDepartmentID
        {
            get
            {
                return GetEffectiveIntegerValue("SKUDepartmentID", 0);
            }
            set
            {
                SetValue("SKUDepartmentID", value, value > 0);
            }
        }


        /// <summary>
        /// Product manufacturer ID. For product variant with manufacturer ID not set, it is returned from parent product.
        /// </summary>
        [RegisterProperty]
        [DatabaseField]
        public virtual int SKUManufacturerID
        {
            get
            {
                return GetEffectiveIntegerValue("SKUManufacturerID", 0);
            }
            set
            {
                SetValue("SKUManufacturerID", value, 0);
            }
        }


        /// <summary>
        /// Product brand ID. For product variant with brand ID not set, it is returned from parent product.
        /// </summary>
        [RegisterProperty]
        [DatabaseField]
        public virtual int SKUBrandID
        {
            get
            {
                return GetEffectiveIntegerValue("SKUBrandID", 0);
            }
            set
            {
                SetValue("SKUBrandID", value, 0);
            }
        }


        /// <summary>
        /// Product collection ID. For product variant with collection ID not set, it is returned from parent product.
        /// </summary>
        [RegisterProperty]
        [DatabaseField]
        public virtual int SKUCollectionID
        {
            get
            {
                return GetEffectiveIntegerValue("SKUCollectionID", 0);
            }
            set
            {
                SetValue("SKUCollectionID", value, 0);
            }
        }


        /// <summary>
        /// SKU tax class ID
        /// </summary>
        [DatabaseField]
        public virtual int SKUTaxClassID
        {
            get
            {
                return GetEffectiveIntegerValue("SKUTaxClassID", 0);
            }
            set
            {
                SetValue("SKUTaxClassID", value, value > 0);
            }
        }


        /// <summary>
        /// Product description.
        /// </summary>
        [DatabaseField]
        public virtual string SKUDescription
        {
            get
            {
                return GetStringValue("SKUDescription", "");
            }
            set
            {
                SetValue("SKUDescription", value);
            }
        }


        /// <summary>
        /// Short product description.
        /// </summary>
        [DatabaseField]
        public virtual string SKUShortDescription
        {
            get
            {
                return GetStringValue("SKUShortDescription", "");
            }
            set
            {
                SetValue("SKUShortDescription", value);
            }
        }


        /// <summary>
        /// Product name.
        /// </summary>
        [DatabaseField]
        public virtual string SKUName
        {
            get
            {
                return GetStringValue("SKUName", "");
            }
            set
            {
                SetValue("SKUName", value);
            }
        }


        /// <summary>
        /// Product ID.
        /// </summary>
        [DatabaseField]
        public virtual int SKUID
        {
            get
            {
                return GetIntegerValue("SKUID", 0);
            }
            set
            {
                SetValue("SKUID", value);
            }
        }


        /// <summary>
        /// Product number.
        /// </summary>
        [DatabaseField]
        public virtual string SKUNumber
        {
            get
            {
                return GetStringValue("SKUNumber", "");
            }
            set
            {
                SetValue("SKUNumber", value);
            }
        }


        /// <summary>
        /// Product enabled.
        /// </summary>
        [DatabaseField]
        public virtual bool SKUEnabled
        {
            get
            {
                return GetBooleanValue("SKUEnabled", false);
            }
            set
            {
                SetValue("SKUEnabled", value);
            }
        }


        /// <summary>
        /// Product available in days.
        /// </summary>
        [DatabaseField]
        public virtual int SKUAvailableInDays
        {
            get
            {
                return GetIntegerValue("SKUAvailableInDays", 0);
            }
            set
            {
                SetValue("SKUAvailableInDays", value);
            }
        }


        /// <summary>
        /// Product public status ID. For product variant with public status ID not set, it is returned from parent product.
        /// </summary>
        [RegisterProperty]
        [DatabaseField]
        public virtual int SKUPublicStatusID
        {
            get
            {
                return GetEffectiveIntegerValue("SKUPublicStatusID", 0);
            }
            set
            {
                SetValue("SKUPublicStatusID", value, 0);
            }
        }


        /// <summary>
        /// Product supplier ID. For product variant with supplier ID not set, it is returned from parent product.
        /// </summary>
        [RegisterProperty]
        [DatabaseField]
        public virtual int SKUSupplierID
        {
            get
            {
                return GetEffectiveIntegerValue("SKUSupplierID", 0);
            }
            set
            {
                SetValue("SKUSupplierID", value, 0);
            }
        }


        /// <summary>
        /// Product price.
        /// </summary>
        [DatabaseField]
        public virtual decimal SKUPrice
        {
            get
            {
                return GetDecimalValue("SKUPrice", 0);
            }
            set
            {
                SetValue("SKUPrice", value);
            }
        }


        /// <summary>
        /// Product retail price.
        /// </summary>
        [RegisterProperty]
        [DatabaseField]
        public virtual decimal SKURetailPrice 
        {
            get
            {
                return GetEffectiveDecimalValue("SKURetailPrice", 0);
            }
            set
            {
                SetValue("SKURetailPrice", value);
            }
        }


        /// <summary>
        /// Product GUID.
        /// </summary>
        [DatabaseField]
        public virtual Guid SKUGUID
        {
            get
            {
                return GetGuidValue("SKUGUID", Guid.Empty);
            }
            set
            {
                SetValue("SKUGUID", value);
            }
        }


        /// <summary>
        /// SKU site ID. Set to 0 for global SKU.
        /// </summary>
        [DatabaseField]
        public virtual int SKUSiteID
        {
            get
            {
                return GetIntegerValue("SKUSiteID", 0);
            }
            set
            {
                SetValue("SKUSiteID", value, (value > 0));
            }
        }


        /// <summary>
        /// Product image path.
        /// </summary>
        [DatabaseField]
        public virtual string SKUImagePath
        {
            get
            {
                var imagePath = GetStringValue("SKUImagePath", "");

                // If product is variant and variant image is not defined, try to find parent image
                if (IsProductVariant && string.IsNullOrEmpty(imagePath))
                {
                    var parent = Parent as SKUInfo;
                    if (parent != null)
                    {
                        imagePath = parent.SKUImagePath;
                    }
                }

                return imagePath;
            }
            set
            {
                SetValue("SKUImagePath", value, "");
            }
        }


        /// <summary>
        /// Product weight. For product variant with weight not set, it is returned from parent product.
        /// </summary>
        [RegisterProperty]
        [DatabaseField]
        public virtual double SKUWeight
        {
            get
            {
                return GetEffectiveDoubleValue("SKUWeight", 0.0);
            }
            set
            {
                SetValue("SKUWeight", value, 0.0);
            }
        }


        /// <summary>
        /// Product height. For product variant with height not set, it is returned from parent product.
        /// </summary>
        [RegisterProperty]
        [DatabaseField]
        public virtual double SKUHeight
        {
            get
            {
                return GetEffectiveDoubleValue("SKUHeight", 0.0);
            }
            set
            {
                SetValue("SKUHeight", value, 0.0);
            }
        }


        /// <summary>
        /// Product width. For product variant with width not set, it is returned from parent product.
        /// </summary>
        [RegisterProperty]
        [DatabaseField]
        public virtual double SKUWidth
        {
            get
            {
                return GetEffectiveDoubleValue("SKUWidth", 0.0);
            }
            set
            {
                SetValue("SKUWidth", value, 0.0);
            }
        }


        /// <summary>
        /// Product depth. For product variant with depth not set, it is returned from parent product.
        /// </summary>
        [RegisterProperty]
        [DatabaseField]
        public virtual double SKUDepth
        {
            get
            {
                return GetEffectiveDoubleValue("SKUDepth", 0.0);
            }
            set
            {
                SetValue("SKUDepth", value, 0.0);
            }
        }


        /// <summary>
        /// Track inventory mode. Value is taken from parent product in case of product variant.
        /// </summary>
        [RegisterProperty]
        [DatabaseField(ValueType = typeof(string))]
        public virtual TrackInventoryTypeEnum SKUTrackInventory
        {
            get
            {
                if (IsProductVariant)
                {
                    SKUInfo parent = Parent as SKUInfo;

                    if (parent != null)
                    {
                        return parent.SKUTrackInventory;
                    }
                }

                return GetStringValue("SKUTrackInventory", "").ToEnum<TrackInventoryTypeEnum>();
            }
            set
            {
                SetValue("SKUTrackInventory", value.ToStringRepresentation());
            }
        }


        /// <summary>
        /// Product available items. In case of product variant value is taken from parent product if it tracks inventory 'ByProduct'.
        /// </summary>
        [RegisterProperty]
        [DatabaseField]
        public virtual int SKUAvailableItems
        {
            get
            {
                if (IsProductVariant)
                {
                    SKUInfo parent = Parent as SKUInfo;

                    if (parent != null && parent.SKUTrackInventory == TrackInventoryTypeEnum.ByProduct)
                    {
                        return parent.SKUAvailableItems;
                    }
                }

                return GetIntegerValue("SKUAvailableItems", 0);
            }
            set
            {
                SetValue("SKUAvailableItems", value);
            }
        }


        /// <summary>
        /// Available items quantity at which the product is to be reordered.
        /// In case of product variant value is taken from parent product if it tracks inventory 'ByProduct'.
        /// </summary>
        [RegisterProperty]
        [DatabaseField]
        public virtual int SKUReorderAt
        {
            get
            {
                if (IsProductVariant)
                {
                    SKUInfo parent = Parent as SKUInfo;

                    if (parent != null && (parent.SKUTrackInventory == TrackInventoryTypeEnum.ByProduct))
                    {
                        return parent.SKUReorderAt;
                    }
                }

                return GetIntegerValue("SKUReorderAt", 0);
            }
            set
            {
                SetValue("SKUReorderAt", value);
            }
        }


        /// <summary>
        /// Product sell only available. Value is taken from parent product in case of product variant.
        /// </summary>
        [RegisterProperty]
        [DatabaseField]
        public virtual bool SKUSellOnlyAvailable
        {
            get
            {
                if (IsProductVariant)
                {
                    SKUInfo parent = Parent as SKUInfo;

                    if (parent != null)
                    {
                        return parent.SKUSellOnlyAvailable;
                    }
                }

                return GetBooleanValue("SKUSellOnlyAvailable", false);
            }
            set
            {
                SetValue("SKUSellOnlyAvailable", value);
            }
        }


        /// <summary>
        /// Product custom data.
        /// </summary>
        [DatabaseField(ValueType = typeof(string))]
        public ContainerCustomData SKUCustomData
        {
            get
            {
                return mSKUCustomData ?? (mSKUCustomData = new ContainerCustomData(this, "SKUCustomData"));
            }
        }


        /// <summary>
        /// SKU option category ID.
        /// </summary>
        [DatabaseField]
        public virtual int SKUOptionCategoryID
        {
            get
            {
                return GetIntegerValue("SKUOptionCategoryID", 0);
            }
            set
            {
                SetValue("SKUOptionCategoryID", value, 0);
            }
        }


        /// <summary>
        /// ID of parent SKU.
        /// </summary>
        [DatabaseField]
        public virtual int SKUParentSKUID
        {
            get
            {
                return GetIntegerValue("SKUParentSKUID", 0);
            }
            set
            {
                SetValue("SKUParentSKUID", value, 0);
            }
        }


        /// <summary>
        /// SKU option order within the parent option category.
        /// </summary>
        [DatabaseField]
        public virtual int SKUOrder
        {
            get
            {
                return GetIntegerValue("SKUOrder", 0);
            }
            set
            {
                SetValue("SKUOrder", value, 0);
            }
        }


        /// <summary>
        /// Object last modified.
        /// </summary>
        [DatabaseField]
        public virtual DateTime SKULastModified
        {
            get
            {
                return GetDateTimeValue("SKULastModified", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("SKULastModified", value, DateTimeHelper.ZERO_TIME);
            }
        }


        /// <summary>
        /// Object creation date.
        /// </summary>
        [DatabaseField]
        public virtual DateTime SKUCreated
        {
            get
            {
                return GetDateTimeValue("SKUCreated", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("SKUCreated", value, DateTimeHelper.ZERO_TIME);
            }
        }


        /// <summary>
        /// Product type.
        /// </summary>
        [DatabaseField(ValueType = typeof(string))]
        public SKUProductTypeEnum SKUProductType
        {
            get
            {
                return GetStringValue("SKUProductType", "").ToEnum<SKUProductTypeEnum>();
            }
            set
            {
                SetValue("SKUProductType", value.ToStringRepresentation());
            }
        }


        /// <summary>
        /// Conversion name of the product
        /// </summary>
        [DatabaseField]
        public string SKUConversionName
        {
            get
            {
                return GetStringValue("SKUConversionName", String.Empty);
            }
            set
            {
                SetValue("SKUConversionName", value, String.Empty);
            }
        }


        /// <summary>
        /// Conversion value of the product
        /// </summary>
        [DatabaseField]
        public String SKUConversionValue
        {
            get
            {
                return GetStringValue("SKUConversionValue", String.Empty);
            }
            set
            {
                SetValue("SKUConversionValue", value, String.Empty);
            }
        }


        /// <summary>
        /// GUID of the membership associated with SKU.
        /// </summary>
        [DatabaseField]
        public Guid SKUMembershipGUID
        {
            get
            {
                return GetGuidValue("SKUMembershipGUID", Guid.Empty);
            }
            set
            {
                SetValue("SKUMembershipGUID", value, Guid.Empty);
            }
        }


        /// <summary>
        /// Validity type of SKU.
        /// </summary>
        [DatabaseField(ValueType = typeof(string))]
        public ValidityEnum SKUValidity
        {
            get
            {
                return DateTimeHelper.GetValidityEnum(GetStringValue("SKUValidity", null));
            }
            set
            {
                SetValue("SKUValidity", DateTimeHelper.GetValidityString(value));
            }
        }


        /// <summary>
        /// Multiplier for period defined by SKU validity type.
        /// </summary>
        [DatabaseField]
        public int SKUValidFor
        {
            get
            {
                return GetIntegerValue("SKUValidFor", 0);
            }
            set
            {
                SetValue("SKUValidFor", value);
            }
        }


        /// <summary>
        /// Date until which products represented by SKU are valid.
        /// </summary>
        [DatabaseField]
        public DateTime SKUValidUntil
        {
            get
            {
                return GetDateTimeValue("SKUValidUntil", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("SKUValidUntil", value, DateTimeHelper.ZERO_TIME);
            }
        }


        /// <summary>
        /// Indicates how bundle product is removed from inventory.
        /// </summary>
        [DatabaseField(ValueType = typeof(string))]
        public BundleInventoryTypeEnum SKUBundleInventoryType
        {
            get
            {
                return GetStringValue("SKUBundleInventoryType", string.Empty).ToEnum<BundleInventoryTypeEnum>();
            }
            set
            {
                SetValue("SKUBundleInventoryType", value.ToStringRepresentation());
            }
        }


        /// <summary>
        /// Indicates if SKU needs to be shipped. Value is taken from parent product in case of product variant.
        /// </summary>
        [RegisterProperty]
        [DatabaseField]
        public bool SKUNeedsShipping
        {
            get
            {
                if (IsProductVariant)
                {
                    SKUInfo parent = Parent as SKUInfo;

                    if (parent != null)
                    {
                        return parent.SKUNeedsShipping;
                    }
                }

                return GetBooleanValue("SKUNeedsShipping", true);
            }
            set
            {
                SetValue("SKUNeedsShipping", value);
            }
        }


        /// <summary>
        /// Minimum number of units of SKU allowed to be in one order.
        /// </summary>
        [DatabaseField]
        public int SKUMinItemsInOrder
        {
            get
            {
                return GetEffectiveIntegerValue("SKUMinItemsInOrder", 0);
            }
            set
            {
                SetValue("SKUMinItemsInOrder", value);
            }
        }


        /// <summary>
        /// Maximum number of units of SKU allowed to be in one order.
        /// </summary>
        [DatabaseField]
        public int SKUMaxItemsInOrder
        {
            get
            {
                return GetEffectiveIntegerValue("SKUMaxItemsInOrder", 0);
            }
            set
            {
                SetValue("SKUMaxItemsInOrder", value);
            }
        }


        /// <summary>
        /// Date from which the SKU is available in store. Value is taken from parent product in case of product variant.
        /// </summary>
        [RegisterProperty]
        [DatabaseField]
        public DateTime SKUInStoreFrom
        {
            get
            {
                if (IsProductVariant)
                {
                    SKUInfo parent = Parent as SKUInfo;

                    if (parent != null)
                    {
                        return parent.SKUInStoreFrom;
                    }
                }

                return GetDateTimeValue("SKUInStoreFrom", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("SKUInStoreFrom", value, DateTimeHelper.ZERO_TIME);
            }
        }

        #endregion


        #region "Advanced properties"

        /// <summary>
        /// Gets the SKUs option category.
        /// </summary>       
        [RegisterProperty]
        public virtual OptionCategoryInfo SKUOptionCategory
        {
            get
            {
                return IsProductOption ? Parent as OptionCategoryInfo : null;
            }
        }


        /// <summary>
        /// Indicates if current SKU is representing product or product option.
        /// </summary>
        [RegisterProperty]
        public bool IsProductOption
        {
            get
            {
                return (SKUOptionCategoryID > 0);
            }
        }


        /// <summary>
        /// Gets a value indicating whether is accessory product.
        /// </summary>      
        [RegisterProperty]
        public bool IsAccessoryProduct
        {
            get
            {
                return IsProductOption && (SKUOptionCategory.CategoryType == OptionCategoryTypeEnum.Products);
            }
        }


        /// <summary>
        /// Gets a value indicating whether is text attribute.
        /// </summary>      
        [RegisterProperty]
        public bool IsTextAttribute
        {
            get
            {
                return IsProductOption && (SKUOptionCategory.CategoryType == OptionCategoryTypeEnum.Text);
            }
        }


        /// <summary>
        /// Gets a value indicating whether is attribute option (e.g. product color).
        /// </summary>      
        [RegisterProperty]
        public bool IsAttributeOption
        {
            get
            {
                return IsProductOption && (SKUOptionCategory.CategoryType == OptionCategoryTypeEnum.Attribute);
            }
        }


        /// <summary>
        /// Indicates if current SKU is representing product variant.
        /// </summary>
        [RegisterProperty]
        public bool IsProductVariant
        {
            get
            {
                return !IsProductOption && (SKUParentSKUID > 0);
            }
        }


        /// <summary>
        /// Indicates if current SKU is representing product.
        /// </summary>
        [RegisterProperty]
        public bool IsProduct
        {
            get
            {
                return !IsProductOption && !IsProductVariant;
            }
        }


        /// <summary>
        /// Indicates whether SKU has any variants.
        /// </summary>
        [RegisterProperty]
        public bool HasVariants
        {
            get
            {
                if (!IsProduct)
                {
                    return false;
                }

                if (!IsObjectValid || !mHasVariants.HasValue)
                {
                    mHasVariants = SKUInfoProvider.GetSKUs().TopN(1).WhereEquals("SKUParentSKUID", SKUID).Column("SKUID").HasResults();
                }

                return mHasVariants.Value;
            }
        }


        /// <summary>
        /// Gets product sections.
        /// </summary>
        public IEnumerable<int> SectionIDs => Service.Resolve<IProductSectionProvider>().GetSections(SKUID);

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty SKUInfo object.
        /// </summary>
        public SKUInfo()
            : base(TYPEINFOSKU)
        {
        }


        /// <summary>
        /// Constructor - Creates a new SKUInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">Source data row</param>
        public SKUInfo(DataRow dr)
            : base(TYPEINFOSKU, dr)
        {
        }


        /// <summary>
        /// Constructor - Creates a new SKUInfo object from the given DataContainer.
        /// </summary>
        /// <param name="data">Source data</param>
        public SKUInfo(IDataContainer data)
            : base(TYPEINFOSKU, null)
        {
            LoadData(new LoadDataSettings(data));
        }


        /// <summary>
        /// Gets object data.
        /// </summary>
        [SecurityCritical]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue("SKUCustomData", mSKUCustomData);
        }


        /// <summary>
        /// Constructor for deserialization.
        /// </summary>
        /// <param name="info">Serialization inf</param>
        /// <param name="context">Streaming context</param>
        public SKUInfo(SerializationInfo info, StreamingContext context)
            : base(info, context, TYPEINFOSKU, TYPEINFOOPTIONSKU)
        {
            mSKUCustomData = (ContainerCustomData)info.GetValue("SKUCustomData", typeof(ContainerCustomData));
        }

        #endregion


        #region "Other methods"

        /// <summary>
        /// Get effective integer value. Returns value from parent product when value in given column is not specified.
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="defaultValue">Default value</param>
        private int GetEffectiveIntegerValue(string columnName, int defaultValue)
        {
            if (IsProductVariant && GetValue(columnName) == null)
            {
                var parent = Parent as SKUInfo;
                if (parent != null)
                {
                    return parent.GetIntegerValue(columnName, defaultValue);
                }
            }

            return GetIntegerValue(columnName, defaultValue);
        }


        /// <summary>
        /// Get effective double value. Returns value from parent product when value in given column is not specified.
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="defaultValue">Default value</param>
        private double GetEffectiveDoubleValue(string columnName, double defaultValue)
        {
            if (IsProductVariant && GetValue(columnName) == null)
            {
                var parent = Parent as SKUInfo;
                if (parent != null)
                {
                    return parent.GetDoubleValue(columnName, defaultValue);
                }
            }

            return GetDoubleValue(columnName, defaultValue);
        }


        /// <summary>
        /// Get effective decimal value. Returns value from parent product when value in given column is not specified.
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="defaultValue">Default value</param>
        private decimal GetEffectiveDecimalValue(string columnName, decimal defaultValue)
        {
            if (IsProductVariant && GetValue(columnName) == null)
            {
                var parent = Parent as SKUInfo;
                if (parent != null)
                {
                    return parent.GetDecimalValue(columnName, defaultValue);
                }
            }

            return GetDecimalValue(columnName, defaultValue);
        }


        #region "Permissions"

        /// <summary>
        /// Checks the permissions of the object.
        /// </summary>
        /// <param name="permission">Permission type</param>
        /// <param name="siteName">Name of the site</param>
        /// <param name="userInfo"><see cref="IUserInfo"/> object</param>
        /// <param name="exceptionOnFailure">If <c>true</c>, PermissionCheckException is thrown whenever a permission check fails</param>
        protected override bool CheckPermissionsInternal(PermissionsEnum permission, string siteName, IUserInfo userInfo, bool exceptionOnFailure)
        {
            return EcommercePermissions.CheckProductsPermissions(permission, siteName, userInfo, exceptionOnFailure, IsGlobal, base.CheckPermissionsInternal);
        }

        #endregion

        #endregion
    }
}