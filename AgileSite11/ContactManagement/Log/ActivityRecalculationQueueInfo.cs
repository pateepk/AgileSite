using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime.Serialization;

using CMS;
using CMS.ContactManagement;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;

[assembly: RegisterObjectType(typeof(ActivityRecalculationQueueInfo), ActivityRecalculationQueueInfo.OBJECT_TYPE)]
    
namespace CMS.ContactManagement
{
    /// <summary>
    /// ActivityRecalculationQueueInfo data container class.
    /// </summary>
	[Serializable]
    internal class ActivityRecalculationQueueInfo : AbstractInfo<ActivityRecalculationQueueInfo>, IActivityRecalculationQueueInfo
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "om.activityrecalculationqueue";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(ActivityRecalculationQueueInfoProvider), OBJECT_TYPE, "OM.ActivityRecalculationQueue", "ActivityRecalculationQueueID", null, null, null, null, null, null, null, null)
        {
            DependsOn = new List<ObjectDependency> 
			{
			    new ObjectDependency("ActivityRecalculationQueueActivityID", "om.activity", ObjectDependencyEnum.Required), 
            },
            SynchronizationSettings =
            {
                IncludeToSynchronizationParentDataSet = IncludeToParentEnum.None,
                LogSynchronization = SynchronizationTypeEnum.None
            },
            LogEvents = false,
            AllowRestore = false,
            ModuleName = ModuleName.CONTACTMANAGEMENT,
            ImportExportSettings = { IncludeToExportParentDataSet = IncludeToParentEnum.None, LogExport = false },
            Feature = FeatureEnum.FullContactManagement,
            ContainsMacros = false,
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Activity recalculation queue ID
        /// </summary>
        [DatabaseField]
        public virtual int ActivityRecalculationQueueID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("ActivityRecalculationQueueID"), 0);
            }
            set
            {
                SetValue("ActivityRecalculationQueueID", value);
            }
        }


        /// <summary>
        /// Activity recalculation queue activity ID
        /// </summary>
        [DatabaseField]
        public virtual int ActivityRecalculationQueueActivityID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("ActivityRecalculationQueueActivityID"), 0);
            }
            set
            {
                SetValue("ActivityRecalculationQueueActivityID", value);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            ActivityRecalculationQueueInfoProvider.DeleteActivityRecalculationQueueInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            ActivityRecalculationQueueInfoProvider.SetActivityRecalculationQueueInfo(this);
        }

        #endregion


        #region "Constructors"

		/// <summary>
        /// Constructor for de-serialization.
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Streaming context</param>
        public ActivityRecalculationQueueInfo(SerializationInfo info, StreamingContext context)
            : base(info, context, TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates an empty ActivityRecalculationQueueInfo object.
        /// </summary>
        public ActivityRecalculationQueueInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new ActivityRecalculationQueueInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data</param>
        public ActivityRecalculationQueueInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}