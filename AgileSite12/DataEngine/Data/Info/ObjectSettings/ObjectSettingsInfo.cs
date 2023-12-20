using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.Helpers;
using CMS.DataEngine;

[assembly: RegisterObjectType(typeof(ObjectSettingsInfo), ObjectSettingsInfo.OBJECT_TYPE)]

namespace CMS.DataEngine
{
    /// <summary>
    /// ObjectSettingsInfo data container class.
    /// </summary>
    public class ObjectSettingsInfo : AbstractInfo<ObjectSettingsInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "cms.objectsettings";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(ObjectSettingsInfoProvider), OBJECT_TYPE, "CMS.ObjectSettings", "ObjectSettingsID", null, null, null, null, null, null, null, null)
        {
            DependsOn = new List<ObjectDependency>()
            {
                new ObjectDependency("ObjectSettingsObjectID", null, ObjectDependencyEnum.Required, "ObjectSettingsObjectType"),
            },
            SupportsInvalidation = true,
            IsMainObject = false,
            ContainsMacros = false,
            LogIntegration = false
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Related object ID.
        /// </summary>
        public virtual int ObjectSettingsObjectID
        {
            get
            {
                return GetIntegerValue("ObjectSettingsObjectID", 0);
            }
            set
            {
                SetValue("ObjectSettingsObjectID", value);
            }
        }


        /// <summary>
        /// Time stamp when the object was checked out.
        /// </summary>
        public virtual DateTime ObjectCheckedOutWhen
        {
            get
            {
                return GetDateTimeValue("ObjectCheckedOutWhen", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("ObjectCheckedOutWhen", value, DateTimeHelper.ZERO_TIME);
            }
        }


        /// <summary>
        /// Version of the object that is checked out.
        /// </summary>
        public virtual int ObjectCheckedOutVersionHistoryID
        {
            get
            {
                return GetIntegerValue("ObjectCheckedOutVersionHistoryID", 0);
            }
            set
            {
                SetValue("ObjectCheckedOutVersionHistoryID", value, (value > 0));
            }
        }


        /// <summary>
        /// ID of the settings object.
        /// </summary>
        public virtual int ObjectSettingsID
        {
            get
            {
                return GetIntegerValue("ObjectSettingsID", 0);
            }
            set
            {
                SetValue("ObjectSettingsID", value);
            }
        }


        /// <summary>
        /// Related object type.
        /// </summary>
        public virtual string ObjectSettingsObjectType
        {
            get
            {
                return GetStringValue("ObjectSettingsObjectType", "");
            }
            set
            {
                SetValue("ObjectSettingsObjectType", value);
            }
        }


        /// <summary>
        /// ID of the user who has currently checked out the object for editing.
        /// </summary>
        public virtual int ObjectCheckedOutByUserID
        {
            get
            {
                return GetIntegerValue("ObjectCheckedOutByUserID", 0);
            }
            set
            {
                SetValue("ObjectCheckedOutByUserID", value, (value > 0));
            }
        }


        /// <summary>
        /// Current workflow step ID of the object.
        /// </summary>
        public virtual int ObjectWorkflowStepID
        {
            get
            {
                return GetIntegerValue("ObjectWorkflowStepID", 0);
            }
            set
            {
                SetValue("ObjectWorkflowStepID", value, (value > 0));
            }
        }


        /// <summary>
        /// Object tags.
        /// </summary>
        public virtual string ObjectTags
        {
            get
            {
                return GetStringValue("ObjectTags", "");
            }
            set
            {
                SetValue("ObjectTags", value);
            }
        }


        /// <summary>
        /// Object comments.
        /// </summary>
        public virtual string ObjectComments
        {
            get
            {
                return GetStringValue("ObjectComments", "");
            }
            set
            {
                SetValue("ObjectComments", value);
            }
        }


        /// <summary>
        /// Version of the object that is currently published.
        /// </summary>
        public virtual int ObjectPublishedVersionHistoryID
        {
            get
            {
                return GetIntegerValue("ObjectPublishedVersionHistoryID", 0);
            }
            set
            {
                SetValue("ObjectPublishedVersionHistoryID", value, (value > 0));
            }
        }


        /// <summary>
        /// Indicates if workflow e-mails should be send from workflow process.
        /// </summary>
        public virtual bool ObjectWorkflowSendEmails
        {
            get
            {
                return GetBooleanValue("ObjectWorkflowSendEmails", true);
            }
            set
            {
                SetValue("ObjectWorkflowSendEmails", value);
            }
        }


        /// <summary>
        /// Object full name if defined
        /// </summary>
        protected override string ObjectFullName
        {
            get
            {
                return ObjectHelper.BuildFullName(ObjectSettingsObjectType, ObjectSettingsObjectID.ToString());
            }
        }


        /// <summary>
        /// Returns true if the object is checked out
        /// </summary>
        protected override bool IsCheckedOut
        {
            get
            {
                return (ObjectCheckedOutByUserID > 0);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            ObjectSettingsInfoProvider.DeleteObjectSettingsInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            ObjectSettingsInfoProvider.SetObjectSettingsInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty ObjectSettingsInfo object.
        /// </summary>
        public ObjectSettingsInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new ObjectSettingsInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public ObjectSettingsInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}
