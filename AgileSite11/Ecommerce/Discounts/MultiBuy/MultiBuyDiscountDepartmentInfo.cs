using System;
using System.Data;
using System.Runtime.Serialization;
using System.Collections.Generic;

using CMS;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Ecommerce;

[assembly: RegisterObjectType(typeof(MultiBuyDiscountDepartmentInfo), MultiBuyDiscountDepartmentInfo.OBJECT_TYPE)]

namespace CMS.Ecommerce
{
    /// <summary>
    /// MultiBuyDiscountDepartmentInfo data container class.
    /// </summary>
    [Serializable]
    public class MultiBuyDiscountDepartmentInfo : AbstractInfo<MultiBuyDiscountDepartmentInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "ecommerce.multibuydiscountdepartment";


        /// <summary>
        /// Type information.
        /// </summary>
        public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(MultiBuyDiscountDepartmentInfoProvider), OBJECT_TYPE, "Ecommerce.MultiBuyDiscountDepartment", null, null, null, null, null, null, null, "MultiBuyDiscountID", MultiBuyDiscountInfo.OBJECT_TYPE)
        {
            DependsOn = new List<ObjectDependency> 
			{
			    new ObjectDependency("DepartmentID", DepartmentInfo.OBJECT_TYPE, ObjectDependencyEnum.Binding) 
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
        /// Department ID
        /// </summary>
        [DatabaseField]
        public virtual int DepartmentID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("DepartmentID"), 0);
            }
            set
            {
                SetValue("DepartmentID", value);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            MultiBuyDiscountDepartmentInfoProvider.DeleteMultiBuyDiscountDepartmentInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            MultiBuyDiscountDepartmentInfoProvider.SetMultiBuyDiscountDepartmentInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor for de-serialization.
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Streaming context</param>
        public MultiBuyDiscountDepartmentInfo(SerializationInfo info, StreamingContext context)
            : base(info, context, TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates an empty MultiBuyDiscountDepartmentInfo object.
        /// </summary>
        public MultiBuyDiscountDepartmentInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new MultiBuyDiscountDepartmentInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data</param>
        public MultiBuyDiscountDepartmentInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}