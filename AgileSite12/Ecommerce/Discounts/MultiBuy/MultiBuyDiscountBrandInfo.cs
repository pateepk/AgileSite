using System;
using System.Data;
using System.Runtime.Serialization;
using System.Collections.Generic;

using CMS;
using CMS.Core;
using CMS.DataEngine;
using CMS.Ecommerce;

[assembly: RegisterObjectType(typeof(MultiBuyDiscountBrandInfo), MultiBuyDiscountBrandInfo.OBJECT_TYPE)]

namespace CMS.Ecommerce
{
    /// <summary>
    /// Data container class for <see cref="MultiBuyDiscountBrandInfo"/>.
    /// </summary>
    [Serializable]
    public class MultiBuyDiscountBrandInfo : AbstractInfo<MultiBuyDiscountBrandInfo>
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public const string OBJECT_TYPE = "ecommerce.multibuydiscountbrand";


        /// <summary>
        /// Type information.
        /// </summary>
        public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(MultiBuyDiscountBrandInfoProvider), OBJECT_TYPE, "Ecommerce.MultiBuyDiscountBrand", null, null, null, null, null, null, null, "MultiBuyDiscountID", MultiBuyDiscountInfo.OBJECT_TYPE)
        {
            DependsOn = new List<ObjectDependency>()
            {
                new ObjectDependency("BrandID", BrandInfo.OBJECT_TYPE, ObjectDependencyEnum.Binding),
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


        /// <summary>
        /// Multi buy discount ID.
        /// </summary>
		[DatabaseField]
        public virtual int MultiBuyDiscountID
        {
            get
            {
                return GetIntegerValue("MultiBuyDiscountID", 0);
            }
            set
            {
                SetValue("MultiBuyDiscountID", value);
            }
        }


        /// <summary>
        /// Brand ID.
        /// </summary>
		[DatabaseField]
        public virtual int BrandID
        {
            get
            {
                return GetIntegerValue("BrandID", 0);
            }
            set
            {
                SetValue("BrandID", value);
            }
        }


        /// <summary>
        /// Brand included.
        /// </summary>
		[DatabaseField]
        public virtual bool BrandIncluded
        {
            get
            {
                return GetBooleanValue("BrandIncluded", true);
            }
            set
            {
                SetValue("BrandIncluded", value);
            }
        }


        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            MultiBuyDiscountBrandInfoProvider.DeleteMultiBuyDiscountBrandInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            MultiBuyDiscountBrandInfoProvider.SetMultiBuyDiscountBrandInfo(this);
        }


        /// <summary>
        /// Loads the default data to the object.
        /// </summary>
        protected override void LoadDefaultData()
        {
            base.LoadDefaultData();

            BrandIncluded = true;
        }


        /// <summary>
        /// Constructor for de-serialization.
        /// </summary>
        /// <param name="info">Serialization info.</param>
        /// <param name="context">Streaming context.</param>
        protected MultiBuyDiscountBrandInfo(SerializationInfo info, StreamingContext context)
            : base(info, context, TYPEINFO)
        {
        }


        /// <summary>
        /// Creates an empty instance of the <see cref="MultiBuyDiscountBrandInfo"/> class.
        /// </summary>
        public MultiBuyDiscountBrandInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Creates a new instance of the <see cref="MultiBuyDiscountBrandInfo"/> class from the given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public MultiBuyDiscountBrandInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }
    }
}