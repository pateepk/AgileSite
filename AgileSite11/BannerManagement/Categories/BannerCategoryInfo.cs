using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.BannerManagement;
using CMS.WebAnalytics;

[assembly: RegisterObjectType(typeof(BannerCategoryInfo), BannerCategoryInfo.OBJECT_TYPE)]

namespace CMS.BannerManagement
{
    /// <summary>
    /// BannerCategoryInfo data container class.
    /// </summary>
    public class BannerCategoryInfo : AbstractInfo<BannerCategoryInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "cms.bannercategory";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(BannerCategoryInfoProvider), OBJECT_TYPE, "CMS.BannerCategory", "BannerCategoryID", "BannerCategoryLastModified", "BannerCategoryGuid", "BannerCategoryName", "BannerCategoryDisplayName", null, "BannerCategorySiteID", null, null)
        {
            ModuleName = ModuleName.BANNERMANAGEMENT,
            SynchronizationSettings =
            {
                LogSynchronization = SynchronizationTypeEnum.LogSynchronization,
                ObjectTreeLocations = new List<ObjectTreeLocation>
                {
                    new ObjectTreeLocation(SITE, WebAnalyticsModule.ONLINEMARKETING),
                    new ObjectTreeLocation(GLOBAL, WebAnalyticsModule.ONLINEMARKETING)
                }
            },
            LogEvents = true,
            TouchCacheDependencies = true,
            SupportsVersioning = false,
            IsCategory = true,
            AllowRestore = false,
            SupportsGlobalObjects = true,
            ImportExportSettings =
            {
                LogExport = false,
                IsExportable = false
            },
            EnabledColumn = "BannerCategoryEnabled",
            ContinuousIntegrationSettings =
            {
                Enabled = true
            },
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Display name of the banner category.
        /// </summary>
        public virtual string BannerCategoryDisplayName
        {
            get
            {
                return GetStringValue("BannerCategoryDisplayName", "");
            }
            set
            {
                SetValue("BannerCategoryDisplayName", value);
            }
        }


        /// <summary>
        /// Code name of the banner category.
        /// </summary>
        public virtual string BannerCategoryName
        {
            get
            {
                return GetStringValue("BannerCategoryName", "");
            }
            set
            {
                SetValue("BannerCategoryName", value);
            }
        }


        /// <summary>
        /// State of banner category. True if enabled; false if disabled.
        /// </summary>
        public virtual bool BannerCategoryEnabled
        {
            get
            {
                return GetBooleanValue("BannerCategoryEnabled", false);
            }
            set
            {
                SetValue("BannerCategoryEnabled", value);
            }
        }


        /// <summary>
        /// ID of the banner category.
        /// </summary>
        public virtual int BannerCategoryID
        {
            get
            {
                return GetIntegerValue("BannerCategoryID", 0);
            }
            set
            {
                SetValue("BannerCategoryID", value);
            }
        }


        /// <summary>
        /// Banner category GUID.
        /// </summary>
        public virtual Guid BannerCategoryGuid
        {
            get
            {
                return GetGuidValue("BannerCategoryGuid", Guid.Empty);
            }
            set
            {
                SetValue("BannerCategoryGuid", value);
            }
        }


        /// <summary>
        /// Time of last modification of this banner category.
        /// </summary>
        public virtual DateTime BannerCategoryLastModified
        {
            get
            {
                return GetDateTimeValue("BannerCategoryLastModified", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("BannerCategoryLastModified", value);
            }
        }


        /// <summary>
        /// ID of the site where banner category is located (or null if this is a global category).
        /// </summary>
        public virtual int? BannerCategorySiteID
        {
            get
            {
                object value = GetValue("BannerCategorySiteID");

                if (value != null)
                {
                    return ValidationHelper.GetInteger(value, 0);
                }

                return null;
            }
            set
            {
                SetValue("BannerCategorySiteID", value, (value > 0));
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            BannerCategoryInfoProvider.DeleteBannerCategoryInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            BannerCategoryInfoProvider.SetBannerCategoryInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty BannerCategoryInfo object.
        /// </summary>
        public BannerCategoryInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new BannerCategoryInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public BannerCategoryInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Inserts banners from the cloned category to the new one.
        /// 
        /// Objects of type Category cannot have a child-parent relationship with its children, co cloning will not clone banners.
        /// </summary>
        /// <param name="settings">Cloning settings</param>
        /// <param name="result">Result of the cloning - messages in this object will be altered by processing this method</param>
        /// <param name="originalObject">Original source BaseInfo (object being cloned)</param>
        protected override void InsertAsClonePostprocessing(CloneSettings settings, CloneResult result, BaseInfo originalObject)
        {
            bool copyBanners = settings.CustomParameters != null && ValidationHelper.GetBoolean(settings.CustomParameters["cms.bannercategory" + ".banner"], false);

            if (copyBanners)
            {
                var banners = BannerInfoProvider.GetBanners().Where("BannerCategoryID", QueryOperator.Equals, originalObject.Generalized.ObjectID);
                if (!DataHelper.DataSourceIsEmpty(banners))
                {
                    int originalParentId = settings.ParentID;
                    string originalCodeName = settings.CodeName;
                    string originalDisplayName = settings.DisplayName;

                    settings.ParentID = 0;

                    // Generate code names automatically
                    settings.CodeName = null;

                    // Take display name from the cloned object
                    settings.DisplayName = null;

                    foreach (BannerInfo banner in banners)
                    {
                        banner.BannerCategoryID = BannerCategoryID;
                        banner.Generalized.InsertAsClone(settings, result);
                    }

                    // Set original values to the settings object
                    settings.ParentID = originalParentId;
                    settings.CodeName = originalCodeName;
                    settings.DisplayName = originalDisplayName;
                }
            }
        }

        #endregion
    }
}
