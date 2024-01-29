using System;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Synchronization;

[assembly: RegisterObjectType(typeof(IntegrationSyncLogInfo), IntegrationSyncLogInfo.OBJECT_TYPE)]

namespace CMS.Synchronization
{
    /// <summary>
    /// IntegrationSyncLogInfo data container class.
    /// </summary>
    public class IntegrationSyncLogInfo : AbstractInfo<IntegrationSyncLogInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "integration.synclog";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(IntegrationSyncLogInfoProvider), OBJECT_TYPE, "Integration.SyncLog", "SyncLogID", "SyncLogTime", null, null, null, null, null, "SyncLogSynchronizationID", IntegrationSynchronizationInfo.OBJECT_TYPE)
        {
            SupportsVersioning = false,
            LogIntegration = false,
            AllowRestore = false,
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Synchronization log identifier.
        /// </summary>
        public virtual int SyncLogID
        {
            get
            {
                return GetIntegerValue("SyncLogID", 0);
            }
            set
            {
                SetValue("SyncLogID", value);
            }
        }


        /// <summary>
        /// Error message (stored when synchronization fails).
        /// </summary>
        public virtual string SyncLogErrorMessage
        {
            get
            {
                return GetStringValue("SyncLogErrorMessage", "");
            }
            set
            {
                SetValue("SyncLogErrorMessage", value);
            }
        }


        /// <summary>
        /// Synchronization identifier which is the synchronization log bound to.
        /// </summary>
        public virtual int SyncLogSynchronizationID
        {
            get
            {
                return GetIntegerValue("SyncLogSynchronizationID", 0);
            }
            set
            {
                SetValue("SyncLogSynchronizationID", value);
            }
        }


        /// <summary>
        /// Time of synchronization log event.
        /// </summary>
        public virtual DateTime SyncLogTime
        {
            get
            {
                return GetDateTimeValue("SyncLogTime", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("SyncLogTime", value);
            }
        }

        #endregion


        #region "GeneralizedInfo properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            IntegrationSyncLogInfoProvider.DeleteIntegrationSyncLogInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            IntegrationSyncLogInfoProvider.SetIntegrationSyncLogInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty IntegrationSyncLogInfo object.
        /// </summary>
        public IntegrationSyncLogInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new IntegrationSyncLogInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data</param>
        public IntegrationSyncLogInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}