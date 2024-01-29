using System;
using System.Data;
using System.Runtime.Serialization;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.ModuleUsageTracking;

[assembly: RegisterObjectType(typeof(ModuleUsageCounterInfo), ModuleUsageCounterInfo.OBJECT_TYPE)]
   
namespace CMS.ModuleUsageTracking
{
    /// <summary>
    /// ModuleUsageCounterInfo data container class.
    /// </summary>
	[Serializable]
    internal class ModuleUsageCounterInfo : AbstractInfo<ModuleUsageCounterInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "cms.moduleusagecounter";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(ModuleUsageCounterInfoProvider), OBJECT_TYPE, "CMS.ModuleUsageCounter", "ModuleUsageCounterID", null, null, "ModuleUsageCounterName", null, null, null, null, null)
        {
            ModuleName = "CMS.ModuleUsageTracking",
            TouchCacheDependencies = true,
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Module usage counter ID
        /// </summary>
        [DatabaseField]
        public virtual int ModuleUsageCounterID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("ModuleUsageCounterID"), 0);
            }
            set
            {
                SetValue("ModuleUsageCounterID", value);
            }
        }


        /// <summary>
        /// Module usage counter name
        /// </summary>
        [DatabaseField]
        public virtual string ModuleUsageCounterName
        {
            get
            {
                return ValidationHelper.GetString(GetValue("ModuleUsageCounterName"), String.Empty);
            }
            set
            {
                SetValue("ModuleUsageCounterName", value);
            }
        }


        /// <summary>
        /// Module usage counter value
        /// </summary>
        [DatabaseField]
        public virtual long ModuleUsageCounterValue
        {
            get
            {
                return ValidationHelper.GetLong(GetValue("ModuleUsageCounterValue"), 0);
            }
            set
            {
                SetValue("ModuleUsageCounterValue", value);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            ModuleUsageCounterInfoProvider.DeleteModuleUsageCounterInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            ModuleUsageCounterInfoProvider.SetModuleUsageCounterInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor for de-serialization.
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Streaming context</param>
        protected ModuleUsageCounterInfo(SerializationInfo info, StreamingContext context)
            : base(info, context, TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates an empty ModuleUsageCounterInfo object.
        /// </summary>
        public ModuleUsageCounterInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new ModuleUsageCounterInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data</param>
        public ModuleUsageCounterInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}