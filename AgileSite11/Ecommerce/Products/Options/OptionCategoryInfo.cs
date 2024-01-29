using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.Core;
using CMS.Helpers;
using CMS.Base;
using CMS.DataEngine;
using CMS.Ecommerce;

[assembly: RegisterObjectType(typeof(OptionCategoryInfo), OptionCategoryInfo.OBJECT_TYPE)]

namespace CMS.Ecommerce
{
    /// <summary>
    /// OptionCategoryInfo data container class.
    /// </summary>
    public class OptionCategoryInfo : AbstractInfo<OptionCategoryInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = PredefinedObjectType.OPTIONCATEGORY;


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(OptionCategoryInfoProvider), OBJECT_TYPE, "ECommerce.OptionCategory", "CategoryID", "CategoryLastModified", "CategoryGUID", "CategoryName", "CategoryDisplayName", null, "CategorySiteID", null, null)
        {
            // Child object types
            // Object dependencies
            // - None

            // Binding object types
            // Synchronization
            SynchronizationSettings =
            {
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
            NameGloballyUnique = true,
            SupportsGlobalObjects = true,
            SupportsCloneToOtherSite = false,
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
            EnabledColumn = "CategoryEnabled",
            ContinuousIntegrationSettings =
            {
                Enabled = true
            }
        };

        #endregion


        #region "Public properties"

        /// <summary>
        /// Category Display Name (simple text or localizable string).
        /// </summary>
        [DatabaseField]
        public virtual string CategoryDisplayName
        {
            get
            {
                return GetStringValue("CategoryDisplayName", "");
            }
            set
            {
                SetValue("CategoryDisplayName", value);
            }
        }


        /// <summary>
        /// Category Code Name.
        /// </summary>
        [DatabaseField]
        public virtual string CategoryName
        {
            get
            {
                return GetStringValue("CategoryName", "");
            }
            set
            {
                SetValue("CategoryName", value);
            }
        }


        /// <summary>
        /// Category Display Name for live site (simple text or localizable string).
        /// </summary>
        [DatabaseField]
        public virtual string CategoryLiveSiteDisplayName
        {
            get
            {
                return GetStringValue("CategoryLiveSiteDisplayName", "");
            }
            set
            {
                SetValue("CategoryLiveSiteDisplayName", value);
            }
        }


        /// <summary>
        /// Gets category live site display name when specified. Returns value of CategoryDisplayName otherwise.
        /// </summary>
        [DatabaseField]
        public virtual string CategoryTitle
        {
            get
            {
                return string.IsNullOrEmpty(CategoryLiveSiteDisplayName) ? CategoryDisplayName : CategoryLiveSiteDisplayName;
            }
        }


        /// <summary>
        /// Gets informative category name consisting of category display name and category live site display name when available.
        /// </summary>
        [DatabaseField]
        public virtual string CategoryFullName
        {
            get
            {
                if (!string.IsNullOrEmpty(CategoryLiveSiteDisplayName))
                {
                    return ResHelper.LocalizeString(string.Format("{0} ({1})", CategoryDisplayName, CategoryLiveSiteDisplayName));
                }

                return ResHelper.LocalizeString(CategoryDisplayName);
            }
        }


        /// <summary>
        /// Category ID.
        /// </summary>
        [DatabaseField]
        public virtual int CategoryID
        {
            get
            {
                return GetIntegerValue("CategoryID", 0);
            }
            set
            {
                SetValue("CategoryID", value);
            }
        }


        /// <summary>
        /// Category site ID. Set to 0 for global category.
        /// </summary>
        [DatabaseField]
        public virtual int CategorySiteID
        {
            get
            {
                return GetIntegerValue("CategorySiteID", 0);
            }
            set
            {
                SetValue("CategorySiteID", value, (value > 0));
            }
        }


        /// <summary>        
        /// Indicates if category is global or site-specific.
        /// </summary>
        [DatabaseField]
        public virtual bool CategoryIsGlobal
        {
            get
            {
                return (CategorySiteID <= 0);
            }
        }


        /// <summary>
        /// Type of the category
        /// </summary>
        [DatabaseField(ValueType = typeof(String))]
        public virtual OptionCategoryTypeEnum CategoryType
        {
            get
            {
                return GetStringValue("CategoryType", "PRODUCTS").ToEnum<OptionCategoryTypeEnum>();
            }
            set
            {
                SetValue("CategoryType", value.ToStringRepresentation());
            }
        }


        /// <summary>
        /// Type of the control which is used for configuring product options of the category
        /// </summary>
        [DatabaseField(ValueType = typeof(String))]
        public virtual OptionCategorySelectionTypeEnum CategorySelectionType
        {
            get
            {
                return GetStringValue("CategorySelectionType", string.Empty).ToEnum<OptionCategorySelectionTypeEnum>();
            }
            set
            {
                SetValue("CategorySelectionType", value.ToStringRepresentation());
            }
        }


        /// <summary>
        /// Category default options.
        /// </summary>
        [DatabaseField]
        public virtual string CategoryDefaultOptions
        {
            get
            {
                return GetStringValue("CategoryDefaultOptions", "");
            }
            set
            {
                SetValue("CategoryDefaultOptions", value);
            }
        }


        /// <summary>
        /// Category description (simple text or localizable string).
        /// </summary>
        [DatabaseField]
        public virtual string CategoryDescription
        {
            get
            {
                return GetStringValue("CategoryDescription", "");
            }
            set
            {
                SetValue("CategoryDescription", value);
            }
        }


        /// <summary>
        /// Default record text (simple text or localizable string).
        /// </summary>
        [DatabaseField]
        public virtual string CategoryDefaultRecord
        {
            get
            {
                return GetStringValue("CategoryDefaultRecord", "");
            }
            set
            {
                SetValue("CategoryDefaultRecord", value);
            }
        }


        /// <summary>
        /// Indicates whether option category is enabled.
        /// </summary>
        [DatabaseField]
        public virtual bool CategoryEnabled
        {
            get
            {
                return GetBooleanValue("CategoryEnabled", false);
            }
            set
            {
                SetValue("CategoryEnabled", value);
            }
        }


        /// <summary>
        /// CategoryOption GUID.
        /// </summary>
        [DatabaseField]
        public virtual Guid CategoryGUID
        {
            get
            {
                return GetGuidValue("CategoryGUID", Guid.Empty);
            }
            set
            {
                SetValue("CategoryGUID", value, Guid.Empty);
            }
        }


        /// <summary>
        /// Object last modified.
        /// </summary>
        [DatabaseField]
        public virtual DateTime CategoryLastModified
        {
            get
            {
                return GetDateTimeValue("CategoryLastModified", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("CategoryLastModified", value, DateTimeHelper.ZERO_TIME);
            }
        }


        /// <summary>
        /// Indicates whether the option price should be displayed with option.
        /// </summary>
        [DatabaseField]
        public bool CategoryDisplayPrice
        {
            get
            {
                return GetBooleanValue("CategoryDisplayPrice", true);
            }
            set
            {
                SetValue("CategoryDisplayPrice", value);
            }
        }

        /// <summary>
        /// Maximum length of the text product option text. It has no effect, if the category has no text product option.
        /// </summary>
        [DatabaseField]
        public virtual int CategoryTextMaxLength
        {
            get
            {
                return GetIntegerValue("CategoryTextMaxLength", 0);
            }
            set
            {
                SetValue("CategoryTextMaxLength", value);
            }
        }


        /// <summary>
        /// Minimum length of the text product option text. It has no effect, if the category has no text product option.
        /// </summary>
        [DatabaseField]
        public virtual int CategoryTextMinLength
        {
            get
            {
                return GetIntegerValue("CategoryTextMinLength", 0);
            }
            set
            {
                SetValue("CategoryTextMinLength", value);
            }
        }



        /// <summary>
        /// Indicates whether option category type can be changed.
        /// </summary>
        [RegisterProperty]
        public bool CategoryOptionHasDependencies
        {
            get
            {
                var options = SKUInfoProvider.GetSKUOptions(CategoryID, false);

                if (!DataHelper.DataSourceIsEmpty(options))
                {
                    // Check if any option in edited category has dependencies 
                    foreach (SKUInfo option in options)
                    {
                        if (option.Generalized.CheckDependencies())
                        {
                            // One of options in category has dependencies
                            return true;
                        }
                    }

                    // Check if option is used in variants
                    var variants = VariantOptionInfoProvider.GetVariantOptions()
                                       .TopN(1)
                                       .Column("VariantSKUID")
                                       .WhereIn("OptionSKUID", DataHelper.GetIntegerValues(options.Tables[0], "SKUID"));

                    return !DataHelper.DataSourceIsEmpty(variants);
                }

                return false;
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            OptionCategoryInfoProvider.DeleteOptionCategoryInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            OptionCategoryInfoProvider.SetOptionCategoryInfo(this);
        }


        /// <summary>
        /// Checks the object dependencies. Returns true if there is at least one dependency.
        /// </summary>
        /// <param name="reportAll">If false, only required dependencies (without default value) are checked, if true required ObjectDependency constraint is ignored</param>
        protected override bool CheckDependencies(bool reportAll = true)
        {
            // Special handling for text options
            if ((CategorySelectionType == OptionCategorySelectionTypeEnum.TextArea) ||
                (CategorySelectionType == OptionCategorySelectionTypeEnum.TextBox))
            {
                // Get options from checked category
                DataSet ds = SKUInfoProvider.GetSKUs().WhereEquals("SKUOptionCategoryID", CategoryID).Column("SKUID");
                if (!DataHelper.DataSourceIsEmpty(ds))
                {
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        int skuId = ValidationHelper.GetInteger(dr["SKUID"], 0);
                        if (SKUInfoProvider.CheckDependencies(skuId))
                        {
                            return true;
                        }
                    }
                }
            }
            else
            {
                // Ordinary option category handling
                return base.CheckDependencies(reportAll);
            }

            return false;
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty OptionCategoryInfo object.
        /// </summary>
        public OptionCategoryInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new OptionCategoryInfo object from the given DataRow.
        /// </summary>
        public OptionCategoryInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion


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
            switch (permission)
            {
                case PermissionsEnum.Read:
                    return ECommerceHelper.IsUserAuthorizedForPermission(EcommercePermissions.PRODUCTS_READ, siteName, userInfo, exceptionOnFailure);

                case PermissionsEnum.Create:
                case PermissionsEnum.Delete:
                case PermissionsEnum.Modify:
                    return OptionCategoryInfoProvider.IsUserAuthorizedToModifyOptionCategory(IsGlobal, siteName, userInfo, exceptionOnFailure);
                    
                default:
                    return base.CheckPermissionsInternal(permission, siteName, userInfo, exceptionOnFailure);
            }
        }

        #endregion
    }
}