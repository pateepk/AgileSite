using System;
using System.Data;
using System.Runtime.Serialization;
using System.Collections.Generic;

using CMS;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Ecommerce;

[assembly: RegisterObjectType(typeof(MultiBuyDiscountSKUInfo), MultiBuyDiscountSKUInfo.OBJECT_TYPE)]

namespace CMS.Ecommerce
{
    /// <summary>
    /// MultiBuyDiscountSKUInfo data container class.
    /// </summary>
    [Serializable]
    public class MultiBuyDiscountSKUInfo : AbstractInfo<MultiBuyDiscountSKUInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "ecommerce.multibuydiscountsku";


        /// <summary>
        /// Type information.
        /// </summary>
        public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(MultiBuyDiscountSKUInfoProvider), OBJECT_TYPE, "Ecommerce.MultiBuyDiscountSKU", null, null, null, null, null, null, null, "MultiBuyDiscountID", MultiBuyDiscountInfo.OBJECT_TYPE)
        {
            DependsOn = new List<ObjectDependency>
            {
                new ObjectDependency("SKUID", SKUInfo.OBJECT_TYPE_SKU, ObjectDependencyEnum.Binding)
            },

            LogEvents = false,
            AllowRestore = false,
            TouchCacheDependencies = true,
            ModuleName = ModuleName.ECOMMERCE,
            Feature = FeatureEnum.Ecommerce,
            ImportExportSettings = { LogExport = false },
            RegisterAsBindingToObjectTypes = new List<string> { MultiBuyDiscountInfo.OBJECT_TYPE, MultiBuyDiscountInfo.OBJECT_TYPE_PRODUCT_COUPON },
            ContinuousIntegrationSettings =
            {
                Enabled = true
            }
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Multi buy discount ID
        /// </summary>
        [DatabaseField]
        public virtual int MultiBuyDiscountID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("MultiBuyDiscountID"), 0);
            }
            set
            {
                SetValue("MultiBuyDiscountID", value);
            }
        }


        /// <summary>
        /// SKUID
        /// </summary>
        [DatabaseField]
        public virtual int SKUID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("SKUID"), 0);
            }
            set
            {
                SetValue("SKUID", value);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            MultiBuyDiscountSKUInfoProvider.DeleteMultiBuyDiscountSKUInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            MultiBuyDiscountSKUInfoProvider.SetMultiBuyDiscountSKUInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor for de-serialization.
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Streaming context</param>
        public MultiBuyDiscountSKUInfo(SerializationInfo info, StreamingContext context)
            : base(info, context, TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates an empty MultiBuyDiscountSKUInfo object.
        /// </summary>
        public MultiBuyDiscountSKUInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new MultiBuyDiscountSKUInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data</param>
        public MultiBuyDiscountSKUInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}