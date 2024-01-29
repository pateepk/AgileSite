using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.ContactManagement;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Scheduler;
using CMS.WebAnalytics;

[assembly: RegisterObjectType(typeof(ContactGroupInfo), ContactGroupInfo.OBJECT_TYPE)]

namespace CMS.ContactManagement
{
    /// <summary>
    /// ContactGroupInfo data container class.
    /// </summary>
    public class ContactGroupInfo : AbstractInfo<ContactGroupInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = PredefinedObjectType.CONTACTGROUP;


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(ContactGroupInfoProvider), OBJECT_TYPE, "OM.ContactGroup", "ContactGroupID", "ContactGroupLastModified", "ContactGroupGUID", "ContactGroupName", "ContactGroupDisplayName", null, null, null, null)
        {
            Extends = new List<ExtraColumn>()
            {
                new ExtraColumn(PredefinedObjectType.NEWSLETTERCONTACTGROUP, "SubscriberRelatedID", ObjectDependencyEnum.Required),
            },
            SynchronizationSettings =
            {
                LogSynchronization = SynchronizationTypeEnum.LogSynchronization,
                ObjectTreeLocations = new List<ObjectTreeLocation>()
                {
                    new ObjectTreeLocation(GLOBAL, WebAnalyticsModule.ONLINEMARKETING),
                }
            },
            ContinuousIntegrationSettings =
            {
                Enabled = true
            },
            SerializationSettings =
            {
                ExcludedFieldNames = { "ContactGroupStatus" }
            },
            LogEvents = false,
            TouchCacheDependencies = true,
            AllowRestore = false,
            ModuleName = ModuleName.CONTACTMANAGEMENT,
            EnabledColumn = "ContactGroupEnabled",
            ImportExportSettings =
            {
                LogExport = true,
                IsExportable = true,
                ObjectTreeLocations = new List<ObjectTreeLocation>()
                {
                    new ObjectTreeLocation(GLOBAL, WebAnalyticsModule.ONLINEMARKETING),
                },
            },
            Feature = FeatureEnum.SimpleContactManagement
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets or sets the display name of the contact group.
        /// </summary>
        public virtual string ContactGroupDisplayName
        {
            get
            {
                return GetStringValue("ContactGroupDisplayName", "");
            }
            set
            {
                SetValue("ContactGroupDisplayName", value);
            }
        }


        /// <summary>
        /// Gets or sets the description of the contact group.
        /// </summary>
        public virtual string ContactGroupDescription
        {
            get
            {
                return GetStringValue("ContactGroupDescription", "");
            }
            set
            {
                SetValue("ContactGroupDescription", value);
            }
        }


        /// <summary>
        /// Gets or sets the ID of the contact group.
        /// </summary>
        public virtual int ContactGroupID
        {
            get
            {
                return GetIntegerValue("ContactGroupID", 0);
            }
            set
            {
                SetValue("ContactGroupID", value);
            }
        }


        /// <summary>
        /// Gets or sets the name of the contact group.
        /// </summary>
        public virtual string ContactGroupName
        {
            get
            {
                return GetStringValue("ContactGroupName", "");
            }
            set
            {
                SetValue("ContactGroupName", value);
            }
        }


        /// <summary>
        /// Gets or sets the macro condition of given group.
        /// </summary>
        public virtual string ContactGroupDynamicCondition
        {
            get
            {
                return GetStringValue("ContactGroupDynamicCondition", "");
            }
            set
            {
                SetValue("ContactGroupDynamicCondition", value);
            }
        }


        /// <summary>
        /// Gets or sets if the contact group is enabled.
        /// </summary>
        public virtual bool ContactGroupEnabled
        {
            get
            {
                return GetBooleanValue("ContactGroupEnabled", true);
            }
            set
            {
                SetValue("ContactGroupEnabled", value);
            }
        }


        /// <summary>
        /// Gets or sets the date and time when the contact group was last modified.
        /// </summary>
        public virtual DateTime ContactGroupLastModified
        {
            get
            {
                return GetDateTimeValue("ContactGroupLastModified", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("ContactGroupLastModified", value);
            }
        }


        /// <summary>
        /// Gets or sets the contact group's unique identifier.
        /// </summary>
        public virtual Guid ContactGroupGUID
        {
            get
            {
                return GetGuidValue("ContactGroupGUID", Guid.Empty);
            }
            set
            {
                SetValue("ContactGroupGUID", value);
            }
        }


        /// <summary>
        /// Gets or sets status of dynamic contact group.
        /// </summary>
        public virtual ContactGroupStatusEnum ContactGroupStatus
        {
            get
            {
                return (ContactGroupStatusEnum)GetIntegerValue("ContactGroupStatus", -1);
            }
            set
            {
                SetValue("ContactGroupStatus", (int)value, ((int)value >= 0));
            }
        }


        /// <summary>
        /// Gets whether the contact group is being automatically rebuilt.
        /// </summary>
        public virtual bool IsRebuildScheduled
        {
            get
            {
                var task = ContactGroupRebuildTaskManager.GetScheduledTask(this);
                return (task != null) && task.TaskEnabled;
            }
        }

        #endregion


        #region "GeneralizedInfo properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            ContactGroupInfoProvider.DeleteContactGroupInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            ContactGroupInfoProvider.SetContactGroupInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty ContactGroupInfo object.
        /// </summary>
        public ContactGroupInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new ContactGroupInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data</param>
        public ContactGroupInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion


        #region "Permissions"

        /// <summary>
        /// Returns name of the permission used for checking global permissions.
        /// </summary>
        /// <param name="permissionName">Name of the original permission</param>
        protected override string GetGlobalPermissionName(string permissionName)
        {
            // There is no special global permission in contact management
            return permissionName;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Removes dependencies when deleting contact group
        /// </summary>
        protected override void RemoveObjectDependencies(bool deleteAll = false, bool clearHashtables = true)
        {
            // Delete dependant scheduled task
            var task = ContactGroupRebuildTaskManager.GetScheduledTask(this);
            if (task != null)
            {
                task.Generalized.LogSynchronization = SynchronizationTypeEnum.LogSynchronization;
                task.Delete();
            }

            var parameters = new QueryDataParameters();
            parameters.Add("@contactGroupID", ContactGroupID);
            ConnectionHelper.ExecuteQuery("om.contactgroup.removeotherdependencies", parameters); 

            base.RemoveObjectDependencies(deleteAll, clearHashtables);
        }


        /// <summary>
        /// Inserts cloned object to DB.
        /// </summary>
        /// <param name="settings">Cloning settings</param>
        /// <param name="result">Cloning result</param>
        /// <param name="originalObject">Original source BaseInfo (object being cloned)</param>
        protected override void InsertAsCloneInternal(CloneSettings settings, CloneResult result, BaseInfo originalObject)
        {
            // Reset contact group status of the clone when contact group is dynamic
            if (!string.IsNullOrEmpty(ContactGroupDynamicCondition))
            {
                ContactGroupStatus = ContactGroupStatusEnum.ConditionChanged;
            }

            bool contacts = true;
            bool accounts = true;

            Hashtable customParameters = settings.CustomParameters;
            if (customParameters != null)
            {
                contacts = ValidationHelper.GetBoolean(customParameters[PredefinedObjectType.CONTACTGROUP + ".contacts"], true);
                accounts = ValidationHelper.GetBoolean(customParameters[PredefinedObjectType.CONTACTGROUP + ".accounts"], true);
            }

            // Clone contacts if requested
            if (contacts)
            {
                if (settings.ExcludedOtherBindingTypes.Contains(ContactGroupMemberInfo.OBJECT_TYPE_CONTACT))
                {
                    settings.ExcludedOtherBindingTypes.Remove(ContactGroupMemberInfo.OBJECT_TYPE_CONTACT);
                }
            }
            else
            {
                settings.ExcludedOtherBindingTypes.Add(ContactGroupMemberInfo.OBJECT_TYPE_CONTACT);
            }

            // Clone accounts if requested
            if (accounts)
            {
                if (settings.ExcludedOtherBindingTypes.Contains(ContactGroupMemberInfo.OBJECT_TYPE_ACCOUNT))
                {
                    settings.ExcludedOtherBindingTypes.Remove(ContactGroupMemberInfo.OBJECT_TYPE_ACCOUNT);
                }
            }
            else
            {
                settings.ExcludedOtherBindingTypes.Add(ContactGroupMemberInfo.OBJECT_TYPE_ACCOUNT);
            }

            Insert();
        }


        /// <summary>
        /// Register the custom properties
        /// </summary>
        protected sealed override void RegisterProperties()
        {
            base.RegisterProperties();

            RegisterProperty("IsRebuildScheduled", c => c.IsRebuildScheduled);
        }

        #endregion
    }
}