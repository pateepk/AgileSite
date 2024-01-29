using System;
using System.Collections.Generic;

using CMS.Base;

namespace CMS.DataEngine
{
    /// <summary>    
    /// Class representing synchronization (staging) settings in the type information of objects.
    /// </summary>
    /// <remarks>
    /// Use in the <see cref="ObjectTypeInfo.SynchronizationSettings"/> property of ObjectTypeInfo.
    /// </remarks>
    [Serializable]
    public class SynchronizationSettings : AbstractDataContainer<SynchronizationSettings>
    {
        /// <summary>
        /// Logs synchronization.
        /// </summary>
        private SynchronizationTypeEnum mLogSynchronization = SynchronizationTypeEnum.None;

        /// <summary>
        /// Parent data inclusion for synchronization.
        /// </summary>
        private IncludeToParentEnum mIncludeToSynchronizationParentDataSet = IncludeToParentEnum.None;


        /// <summary>
        /// Determines whether the system logs staging tasks for objects of the type. Select one of the SynchronizationTypeEnum values.
        /// </summary>
        [RegisterColumn]
        public SynchronizationTypeEnum LogSynchronization
        {
            get
            {
                if (!ObjectTypeInfo.GlobalLogSynchronization)
                {
                    return SynchronizationTypeEnum.None;
                }

                return mLogSynchronization;
            }
            set
            {
                mLogSynchronization = value;
            }
        }


        /// <summary>
        /// Adds the object type to the object tree on the Objects tab of the Staging application.
        /// </summary>
        public List<ObjectTreeLocation> ObjectTreeLocations
        {
            get;
            set;
        }


        /// <summary>
        /// Condition that determines which objects of the given type generate synchronization tasks. If set, only objects that match the condition are synchronized.
        /// </summary>
        public Func<BaseInfo, bool> LogCondition
        {
            get;
            set;
        }


        /// <summary>
        /// Determines whether the data of parent objects automatically includes child objects of the given type during synchronization (for example staging).
        /// Applies to object types that have a parent. 
        /// Set one of the values from IncludeToParentEnum. The default value is Complete for object types that have the ParentObjectType and ParentID properties set in their type info.
        /// </summary>
        [RegisterColumn]
        public IncludeToParentEnum IncludeToSynchronizationParentDataSet
        {
            get
            {
                return mIncludeToSynchronizationParentDataSet;
            }
            set
            {
                if (value == IncludeToParentEnum.Default)
                {
                    throw new NotSupportedException("Setting property to default value is not supported.");
                }

                mIncludeToSynchronizationParentDataSet = value;
            }
        }


        /// <summary>
        /// Gets or sets the list of columns that are excluded when staging objects.
        /// </summary>
        /// <remarks>
        /// The property does NOT limit the actual data of staging tasks, only the operations that take place on the target server.
        /// </remarks>
        public IList<string> ExcludedStagingColumns
        {
            get;
            set;
        }


        /// <summary>
        /// Copies the settings properties to another settings instance
        /// </summary>
        /// <param name="settings"></param>
        internal void CopyPropertiesTo(SynchronizationSettings settings)
        {
            settings.mLogSynchronization = mLogSynchronization;
            settings.mIncludeToSynchronizationParentDataSet = mIncludeToSynchronizationParentDataSet;
            settings.ObjectTreeLocations = ObjectTreeLocations;
            settings.LogCondition = LogCondition;
            settings.ExcludedStagingColumns = ExcludedStagingColumns;
        }
    }
}
