using System;
using System.Data;

using CMS;
using CMS.Core;
using CMS.Helpers;
using CMS.DataEngine;
using CMS.EventManager;

[assembly: RegisterObjectType(typeof(EventAttendeeInfo), EventAttendeeInfo.OBJECT_TYPE)]

namespace CMS.EventManager
{
    /// <summary>
    /// EventAttendeeInfo data container class.
    /// </summary>
    public class EventAttendeeInfo : AbstractInfo<EventAttendeeInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = PredefinedObjectType.EVENTATTENDEE;


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(EventAttendeeInfoProvider), OBJECT_TYPE, "CMS.EventAttendee", "AttendeeID", "AttendeeLastModified", "AttendeeGUID", null, "AttendeeEmail", null, null, "AttendeeEventNodeID", PredefinedObjectType.NODE)
                                              {
                                                  TouchCacheDependencies = true,
                                                  AllowRestore = false,
                                                  ModuleName = ModuleName.EVENTMANAGER,
                                                  SupportsCloning = false,
                                                  ImportExportSettings = { IncludeToExportParentDataSet = IncludeToParentEnum.None, },
                                                  SynchronizationSettings =
                                                  {
                                                      IncludeToSynchronizationParentDataSet = IncludeToParentEnum.None,
                                                      LogSynchronization = SynchronizationTypeEnum.None,
                                                  }
                                              };

        #endregion


        #region "Properties"

        /// <summary>
        /// ID of the attendee.
        /// </summary>
        public virtual int AttendeeID
        {
            get
            {
                return GetIntegerValue("AttendeeID", 0);
            }
            set
            {
                SetValue("AttendeeID", value);
            }
        }


        /// <summary>
        /// Last name of the attendee.
        /// </summary>
        public virtual string AttendeeLastName
        {
            get
            {
                return GetStringValue("AttendeeLastName", "");
            }
            set
            {
                SetValue("AttendeeLastName", value);
            }
        }


        /// <summary>
        /// First name of the attendee.
        /// </summary>
        public virtual string AttendeeFirstName
        {
            get
            {
                return GetStringValue("AttendeeFirstName", "");
            }
            set
            {
                SetValue("AttendeeFirstName", value);
            }
        }


        /// <summary>
        /// Email of the attendee.
        /// </summary>
        public virtual string AttendeeEmail
        {
            get
            {
                return GetStringValue("AttendeeEmail", "");
            }
            set
            {
                SetValue("AttendeeEmail", value);
            }
        }


        /// <summary>
        /// ID of event node.
        /// </summary>
        public virtual int AttendeeEventNodeID
        {
            get
            {
                return GetIntegerValue("AttendeeEventNodeID", 0);
            }
            set
            {
                SetValue("AttendeeEventNodeID", value);
            }
        }


        /// <summary>
        /// Phone number of the attendee.
        /// </summary>
        public virtual string AttendeePhone
        {
            get
            {
                return GetStringValue("AttendeePhone", "");
            }
            set
            {
                SetValue("AttendeePhone", value);
            }
        }


        /// <summary>
        /// Attendee GUID.
        /// </summary>
        public virtual Guid AttendeeGUID
        {
            get
            {
                return GetGuidValue("AttendeeGUID", Guid.Empty);
            }
            set
            {
                SetValue("AttendeeGUID", value, Guid.Empty);
            }
        }


        /// <summary>
        /// Object last modified.
        /// </summary>
        public virtual DateTime AttendeeLastModified
        {
            get
            {
                return GetDateTimeValue("AttendeeLastModified", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("AttendeeLastModified", value, DateTimeHelper.ZERO_TIME);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            EventAttendeeInfoProvider.DeleteEventAttendeeInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            EventAttendeeInfoProvider.SetEventAttendeeInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty EventAttendeeInfo object.
        /// </summary>
        public EventAttendeeInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new EventAttendeeInfo object from the given DataRow.
        /// </summary>
        public EventAttendeeInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}