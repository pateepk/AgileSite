using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime.Serialization;

using CMS;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Ecommerce;

[assembly: RegisterObjectType(typeof(CarrierInfo), CarrierInfo.OBJECT_TYPE)]

namespace CMS.Ecommerce
{
    /// <summary>
    /// CarrierInfo data container class.
    /// </summary>
    [Serializable]
    public class CarrierInfo : AbstractInfo<CarrierInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "ecommerce.carrier";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(CarrierInfoProvider), OBJECT_TYPE, "Ecommerce.Carrier", "CarrierID", "CarrierLastModified", "CarrierGUID", "CarrierName", "CarrierDisplayName", null, "CarrierSiteID", null, null)
        {
            // Synchronization
            SynchronizationSettings =
            {
                LogSynchronization = SynchronizationTypeEnum.LogSynchronization,
                ObjectTreeLocations = new List<ObjectTreeLocation>
                {
                    new ObjectTreeLocation(SITE, ECommerceModule.ECOMMERCE),
                }
            },

            // Others
            SupportsCloning = false,
            AllowDataExport = false,
            LogEvents = true,
            TouchCacheDependencies = true,
            ModuleName = ModuleName.ECOMMERCE,
            Feature = FeatureEnum.Ecommerce,
            SupportsInvalidation = true,
            AssemblyNameColumn = "CarrierAssemblyName",
            ImportExportSettings =
            {
                LogExport = true,
                IsExportable = true,
                ObjectTreeLocations = new List<ObjectTreeLocation>
                {
                    new ObjectTreeLocation(SITE, ECommerceModule.ECOMMERCE),
                },
            },
            ContinuousIntegrationSettings =
            {
                Enabled = true
            }
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Carrier ID
        /// </summary>
        [DatabaseField]
        public virtual int CarrierID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("CarrierID"), 0);
            }
            set
            {
                SetValue("CarrierID", value);
            }
        }


        /// <summary>
        /// Carrier display name
        /// </summary>
        [DatabaseField]
        public virtual string CarrierDisplayName
        {
            get
            {
                return ValidationHelper.GetString(GetValue("CarrierDisplayName"), String.Empty);
            }
            set
            {
                SetValue("CarrierDisplayName", value, String.Empty);
            }
        }


        /// <summary>
        /// Carrier name
        /// </summary>
        [DatabaseField]
        public virtual string CarrierName
        {
            get
            {
                return ValidationHelper.GetString(GetValue("CarrierName"), String.Empty);
            }
            set
            {
                SetValue("CarrierName", value, String.Empty);
            }
        }


        /// <summary>
        /// Carrier site ID
        /// </summary>
        [DatabaseField]
        public virtual int CarrierSiteID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("CarrierSiteID"), 0);
            }
            set
            {
                SetValue("CarrierSiteID", value, 0);
            }
        }


        /// <summary>
        /// Carrier GUID
        /// </summary>
        [DatabaseField]
        public virtual Guid CarrierGUID
        {
            get
            {
                return ValidationHelper.GetGuid(GetValue("CarrierGUID"), Guid.Empty);
            }
            set
            {
                SetValue("CarrierGUID", value, Guid.Empty);
            }
        }


        /// <summary>
        /// Carrier last modified
        /// </summary>
        [DatabaseField]
        public virtual DateTime CarrierLastModified
        {
            get
            {
                return ValidationHelper.GetDateTime(GetValue("CarrierLastModified"), DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("CarrierLastModified", value);
            }
        }


        /// <summary>
        /// Carrier assembly name
        /// </summary>
        [DatabaseField]
        public virtual string CarrierAssemblyName
        {
            get
            {
                return ValidationHelper.GetString(GetValue("CarrierAssemblyName"), String.Empty);
            }
            set
            {
                SetValue("CarrierAssemblyName", value, String.Empty);
            }
        }


        /// <summary>
        /// Carrier class name
        /// </summary>
        [DatabaseField]
        public virtual string CarrierClassName
        {
            get
            {
                return ValidationHelper.GetString(GetValue("CarrierClassName"), String.Empty);
            }
            set
            {
                SetValue("CarrierClassName", value, String.Empty);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            CarrierInfoProvider.DeleteCarrierInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            CarrierInfoProvider.SetCarrierInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor for de-serialization.
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Streaming context</param>
        public CarrierInfo(SerializationInfo info, StreamingContext context)
            : base(info, context, TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates an empty CarrierInfo object.
        /// </summary>
        public CarrierInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new CarrierInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data</param>
        public CarrierInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}