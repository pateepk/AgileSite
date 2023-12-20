using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime.Serialization;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.ContactManagement;
using CMS.Core;

[assembly: RegisterObjectType(typeof(ContactChangeRecalculationQueueInfo), ContactChangeRecalculationQueueInfo.OBJECT_TYPE)]

namespace CMS.ContactManagement
{
    /// <summary>
    /// ContactChangeRecalculationQueueInfo data container class.
    /// </summary>
	[Serializable]
    internal class ContactChangeRecalculationQueueInfo : AbstractInfo<ContactChangeRecalculationQueueInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = PredefinedObjectType.CONTACTCHANGERECALCULATIONQUEUEINFO;


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(ContactChangeRecalculationQueueInfoProvider), OBJECT_TYPE, "OM.ContactChangeRecalculationQueue", "ContactChangeRecalculationQueueID", null, null, null, null, null, null, null, null)
        {
            DependsOn = new List<ObjectDependency>
            {
                new ObjectDependency("ContactChangeRecalculationQueueContactID", PredefinedObjectType.CONTACT, ObjectDependencyEnum.Required)
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
        /// Contact change recalculation queue ID
        /// </summary>
        [DatabaseField]
        public virtual int ContactChangeRecalculationQueueID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("ContactChangeRecalculationQueueID"), 0);
            }
            set
            {
                SetValue("ContactChangeRecalculationQueueID", value);
            }
        }


        /// <summary>
        /// Contact change recalculation queue contact ID
        /// </summary>
        [DatabaseField]
        public virtual int ContactChangeRecalculationQueueContactID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("ContactChangeRecalculationQueueContactID"), 0);
            }
            set
            {
                SetValue("ContactChangeRecalculationQueueContactID", value);
            }
        }


        /// <summary>
        /// Contact change recalculation queue changed columns
        /// </summary>
        [DatabaseField]
        public virtual string ContactChangeRecalculationQueueChangedColumns
        {
            get
            {
                return ValidationHelper.GetString(GetValue("ContactChangeRecalculationQueueChangedColumns"), String.Empty);
            }
            set
            {
                SetValue("ContactChangeRecalculationQueueChangedColumns", value);
            }
        }


        /// <summary>
        /// Contact change recalculation queue contact is new
        /// </summary>
        [DatabaseField]
        public virtual bool ContactChangeRecalculationQueueContactIsNew
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("ContactChangeRecalculationQueueContactIsNew"), false);
            }
            set
            {
                SetValue("ContactChangeRecalculationQueueContactIsNew", value);
            }
        }


        /// <summary>
        /// Contact change recalculation queue contact was merged or split
        /// </summary>
        [DatabaseField]
        public virtual bool ContactChangeRecalculationQueueContactWasMerged
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("ContactChangeRecalculationQueueContactWasMerged"), false);
            }
            set
            {
                SetValue("ContactChangeRecalculationQueueContactWasMerged", value);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            ContactChangeRecalculationQueueInfoProvider.DeleteContactChangeRecalculationQueueInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            ContactChangeRecalculationQueueInfoProvider.SetContactChangeRecalculationQueueInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor for de-serialization.
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Streaming context</param>
        public ContactChangeRecalculationQueueInfo(SerializationInfo info, StreamingContext context)
            : base(info, context, TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates an empty ContactChangeRecalculationQueueInfo object.
        /// </summary>
        public ContactChangeRecalculationQueueInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new ContactChangeRecalculationQueueInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data</param>
        public ContactChangeRecalculationQueueInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}