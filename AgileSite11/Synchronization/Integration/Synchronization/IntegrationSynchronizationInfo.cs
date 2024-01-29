using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Synchronization;

[assembly: RegisterObjectType(typeof(IntegrationSynchronizationInfo), IntegrationSynchronizationInfo.OBJECT_TYPE)]

namespace CMS.Synchronization
{
    /// <summary>
    /// IntegrationSynchronizationInfo data container class.
    /// </summary>
    public class IntegrationSynchronizationInfo : AbstractInfo<IntegrationSynchronizationInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "integration.synchronization";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(IntegrationSynchronizationInfoProvider), OBJECT_TYPE, "Integration.Synchronization", "SynchronizationID", "SynchronizationLastRun", null, null, null, null, null, "SynchronizationTaskID", IntegrationTaskInfo.OBJECT_TYPE)
        {
            DependsOn = new List<ObjectDependency> { new ObjectDependency("SynchronizationConnectorID", IntegrationConnectorInfo.OBJECT_TYPE, ObjectDependencyEnum.Required) },
            SupportsVersioning = false,
            LogIntegration = false,
            AllowRestore = false,
            DeleteObjectWithAPI = true
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Error message (stored when synchronization fails).
        /// </summary>
        public virtual string SynchronizationErrorMessage
        {
            get
            {
                return GetStringValue("SynchronizationErrorMessage", "");
            }
            set
            {
                SetValue("SynchronizationErrorMessage", value);
            }
        }


        /// <summary>
        /// Synchronization identifier.
        /// </summary>
        public virtual int SynchronizationID
        {
            get
            {
                return GetIntegerValue("SynchronizationID", 0);
            }
            set
            {
                SetValue("SynchronizationID", value);
            }
        }


        /// <summary>
        /// Identifier of connector the synchronization is bound to.
        /// </summary>
        public virtual int SynchronizationConnectorID
        {
            get
            {
                return GetIntegerValue("SynchronizationConnectorID", 0);
            }
            set
            {
                SetValue("SynchronizationConnectorID", value);
            }
        }


        /// <summary>
        /// Identifier of task the synchronization is bound to.
        /// </summary>
        public virtual int SynchronizationTaskID
        {
            get
            {
                return GetIntegerValue("SynchronizationTaskID", 0);
            }
            set
            {
                SetValue("SynchronizationTaskID", value);
            }
        }


        /// <summary>
        /// Last run date of synchronization.
        /// </summary>
        public virtual DateTime SynchronizationLastRun
        {
            get
            {
                return GetDateTimeValue("SynchronizationLastRun", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("SynchronizationLastRun", value);
            }
        }


        /// <summary>
        /// Whether the synchronization of the related task is running.
        /// </summary>
        public virtual bool SynchronizationIsRunning
        {
            get
            {
                return GetBooleanValue("SynchronizationIsRunning", false);
            }
            set
            {
                SetValue("SynchronizationIsRunning", value);
            }
        }

        #endregion


        #region "GeneralizedInfo properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            IntegrationSynchronizationInfoProvider.DeleteIntegrationSynchronizationInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            IntegrationSynchronizationInfoProvider.SetIntegrationSynchronizationInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty IntegrationSynchronizationInfo object.
        /// </summary>
        public IntegrationSynchronizationInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new IntegrationSynchronizationInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data</param>
        public IntegrationSynchronizationInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}