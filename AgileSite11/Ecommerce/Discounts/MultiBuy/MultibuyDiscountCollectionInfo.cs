using System;
using System.Data;
using System.Runtime.Serialization;
using System.Collections.Generic;

using CMS;
using CMS.Core;
using CMS.DataEngine;
using CMS.Ecommerce;

[assembly: RegisterObjectType(typeof(MultiBuyDiscountCollectionInfo), MultiBuyDiscountCollectionInfo.OBJECT_TYPE)]

namespace CMS.Ecommerce
{
    /// <summary>
    /// Data container class for <see cref="MultiBuyDiscountCollectionInfo"/>.
    /// </summary>
    [Serializable]
    public class MultiBuyDiscountCollectionInfo : AbstractInfo<MultiBuyDiscountCollectionInfo>
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public const string OBJECT_TYPE = "ecommerce.multibuydiscountcollection";


        /// <summary>
        /// Type information.
        /// </summary>
        public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(MultiBuyDiscountCollectionInfoProvider), OBJECT_TYPE, "Ecommerce.MultiBuyDiscountCollection", null, null, null, null, null, null, null, "MultibuyDiscountID", MultiBuyDiscountInfo.OBJECT_TYPE)
        {
            DependsOn = new List<ObjectDependency>()
            {
                new ObjectDependency("CollectionID", CollectionInfo.OBJECT_TYPE, ObjectDependencyEnum.Binding),
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
        /// Multibuy discount ID
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
        /// Collection ID
        /// </summary>
        [DatabaseField]
        public virtual int CollectionID
        {
            get
            {
                return GetIntegerValue("CollectionID", 0);
            }
            set
            {
                SetValue("CollectionID", value);
            }
        }


        /// <summary>
        /// Collection included
        /// </summary>
        [DatabaseField]
        public virtual bool CollectionIncluded
        {
            get
            {
                return GetBooleanValue("CollectionIncluded", true);
            }
            set
            {
                SetValue("CollectionIncluded", value);
            }
        }


        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            MultiBuyDiscountCollectionInfoProvider.DeleteMultiBuyDiscountCollectionInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            MultiBuyDiscountCollectionInfoProvider.SetMultiBuyDiscountCollectionInfo(this);
        }


        /// <summary>
        /// Loads the default data to the object.
        /// </summary>
        protected override void LoadDefaultData()
        {
            base.LoadDefaultData();

            CollectionIncluded = true;
        }


        /// <summary>
        /// Constructor for de-serialization.
        /// </summary>
        /// <param name="info">Serialization info.</param>
        /// <param name="context">Streaming context.</param>
        protected MultiBuyDiscountCollectionInfo(SerializationInfo info, StreamingContext context)
            : base(info, context, TYPEINFO)
        {
        }


        /// <summary>
        /// Creates an empty instance of the <see cref="MultiBuyDiscountCollectionInfo"/> class.
        /// </summary>
        public MultiBuyDiscountCollectionInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Creates a new instance of the <see cref="MultiBuyDiscountCollectionInfo"/> class from the given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public MultiBuyDiscountCollectionInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }
    }
}