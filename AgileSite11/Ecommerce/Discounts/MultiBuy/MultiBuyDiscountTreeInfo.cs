using System;
using System.Data;
using System.Runtime.Serialization;
using System.Collections.Generic;

using CMS;
using CMS.Core;
using CMS.DataEngine;
using CMS.Ecommerce;
using CMS.Helpers;

[assembly: RegisterObjectType(typeof(MultiBuyDiscountTreeInfo), MultiBuyDiscountTreeInfo.OBJECT_TYPE)]

namespace CMS.Ecommerce
{
    /// <summary>
    /// Data container class for <see cref="MultiBuyDiscountTreeInfo"/>.
    /// </summary>
    [Serializable]
    public class MultiBuyDiscountTreeInfo : AbstractInfo<MultiBuyDiscountTreeInfo>
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public const string OBJECT_TYPE = "ecommerce.multibuydiscounttree";


        /// <summary>
        /// Type information.
        /// </summary>
        public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(MultiBuyDiscountTreeInfoProvider), OBJECT_TYPE, "Ecommerce.MultiBuyDiscountTree", null, null, null, null, null, null, null, "MultiBuyDiscountID", MultiBuyDiscountInfo.OBJECT_TYPE)
        {
            ModuleName = ModuleName.ECOMMERCE,
            Feature = FeatureEnum.Ecommerce,
            LogEvents = false,
            AllowRestore = false,
            TouchCacheDependencies = true,
            DependsOn = new List<ObjectDependency>()
            {
                new ObjectDependency("NodeID", PredefinedObjectType.NODE, ObjectDependencyEnum.Binding),
            },
            ImportExportSettings = { LogExport = false },
            IsBinding = true,
            RegisterAsBindingToObjectTypes = new List<string>{ MultiBuyDiscountInfo.OBJECT_TYPE, MultiBuyDiscountInfo.OBJECT_TYPE_PRODUCT_COUPON },
            ContinuousIntegrationSettings =
            {
                Enabled = true
            }
        };


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
        /// Node ID
        /// </summary>
		[DatabaseField]
        public virtual int NodeID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("NodeID"), 0);
            }
            set
            {
                SetValue("NodeID", value);
            }
        }


        /// <summary>
        /// Node included
        /// </summary>
		[DatabaseField]
        public virtual bool NodeIncluded
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("NodeIncluded"), true);
            }
            set
            {
                SetValue("NodeIncluded", value);
            }
        }


        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            MultiBuyDiscountTreeInfoProvider.DeleteMultiBuyDiscountTreeInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            MultiBuyDiscountTreeInfoProvider.SetMultiBuyDiscountTreeInfo(this);
        }


        /// <summary>
        /// Loads the default data to the object.
        /// </summary>
        protected override void LoadDefaultData()
        {
            base.LoadDefaultData();

            NodeIncluded = true;
        }


        /// <summary>
        /// Constructor for de-serialization.
        /// </summary>
        /// <param name="info">Serialization info.</param>
        /// <param name="context">Streaming context.</param>
        protected MultiBuyDiscountTreeInfo(SerializationInfo info, StreamingContext context)
            : base(info, context, TYPEINFO)
        {
        }


        /// <summary>
        /// Creates an empty instance of the <see cref="MultiBuyDiscountTreeInfo"/> class.
        /// </summary>
        public MultiBuyDiscountTreeInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Creates a new instance of the <see cref="MultiBuyDiscountTreeInfo"/> class from the given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public MultiBuyDiscountTreeInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }
    }
}