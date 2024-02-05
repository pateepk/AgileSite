using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Relationships;

[assembly: RegisterObjectType(typeof(RelationshipNameInfo), RelationshipNameInfo.OBJECT_TYPE)]

namespace CMS.Relationships
{
    /// <summary>
    /// Relationship name info data container class.
    /// </summary>
    public class RelationshipNameInfo : AbstractInfo<RelationshipNameInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = PredefinedObjectType.RELATIONSHIPNAME;


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(RelationshipNameInfoProvider), OBJECT_TYPE, "CMS.RelationshipName", "RelationshipNameID", "RelationshipLastModified", "RelationshipGUID", "RelationshipName", "RelationshipDisplayName", null, null, null, null)
        {
            ImportExportSettings =
            {
                LogExport = true,
                IsExportable = true,
                IncludeToExportParentDataSet = IncludeToParentEnum.None,
                WhereCondition = "(RelationshipNameIsAdHoc IS NULL OR RelationshipNameIsAdHoc = 0)",
                ObjectTreeLocations = new List<ObjectTreeLocation>
                {
                    new ObjectTreeLocation(GLOBAL, CONFIGURATION),
                },
            },
            SynchronizationSettings = 
            {
                LogSynchronization = SynchronizationTypeEnum.LogSynchronization,
                ObjectTreeLocations = new List<ObjectTreeLocation>
                    {
                        new ObjectTreeLocation(GLOBAL, CONFIGURATION),
                    },

                IncludeToSynchronizationParentDataSet = IncludeToParentEnum.None,
            },
            LogEvents = true,
            TouchCacheDependencies = true,
            ContinuousIntegrationSettings = 
            {
                Enabled = true
            }
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets or sets the relationship name ID.
        /// </summary>
        [DatabaseField("RelationshipNameID")]
        public int RelationshipNameId
        {
            get
            {
                return GetIntegerValue("RelationshipNameID", 0);
            }
            set
            {
                SetValue("RelationshipNameID", value);
            }
        }


        /// <summary>
        /// Gets or sets the relationship display name.
        /// </summary>
        [DatabaseField]
        public string RelationshipDisplayName
        {
            get
            {
                return GetStringValue("RelationshipDisplayName", "");
            }
            set
            {
                SetValue("RelationshipDisplayName", value);
            }
        }


        /// <summary>
        /// Gets or sets the relationship name.
        /// </summary>
        [DatabaseField]
        public string RelationshipName
        {
            get
            {
                return GetStringValue("RelationshipName", "");
            }
            set
            {
                SetValue("RelationshipName", value);
            }
        }


        /// <summary>
        /// Gets or sets the relationship allowed objects.
        /// </summary>
        [DatabaseField]
        public string RelationshipAllowedObjects
        {
            get
            {
                return GetStringValue("RelationshipAllowedObjects", "");
            }
            set
            {
                SetValue("RelationshipAllowedObjects", value);
            }
        }

        
        /// <summary>
        /// Indicates if relationship name is ad-hoc.
        /// </summary>
        [DatabaseField]
        public bool RelationshipNameIsAdHoc
        {
            get
            {
                return GetBooleanValue("RelationshipNameIsAdHoc", false);
            }
            set
            {
                SetValue("RelationshipNameIsAdHoc", value);
            }
        }


        /// <summary>
        /// Relationship GUID.
        /// </summary>
        [DatabaseField]
        public virtual Guid RelationshipGUID
        {
            get
            {
                return GetGuidValue("RelationshipGUID", Guid.Empty);
            }
            set
            {
                SetValue("RelationshipGUID", value, Guid.Empty);
            }
        }


        /// <summary>
        /// Object last modified.
        /// </summary>
        [DatabaseField]
        public virtual DateTime RelationshipLastModified
        {
            get
            {
                return GetDateTimeValue("RelationshipLastModified", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("RelationshipLastModified", value, DateTimeHelper.ZERO_TIME);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Indicates if the object supports deleting to recycle bin.
        /// </summary>
        protected override bool AllowRestore
        {
            get
            {
                // Do not delete ad-hoc relationship name to the recycle bin
                return base.AllowRestore && !RelationshipNameIsAdHoc;
            }
            set
            {
                base.AllowRestore = value;
            }
        }


        /// <summary>
        /// If true, Events tasks are logged on the object update.
        /// </summary>
        protected override bool LogEvents
        {
            get
            {
                // Do not log events for ad-hoc relationship name
                return base.LogEvents && !RelationshipNameIsAdHoc;
            }
            set
            {
                base.LogEvents = value;
            }
        }


        /// <summary>
        /// Indicates how should be handled the logging of synchronization tasks on the object update.
        /// </summary>
        protected override SynchronizationTypeEnum LogSynchronization
        {
            get
            {
                // Do not log synchronization for ad-hoc relationship name
                if (RelationshipNameIsAdHoc)
                {
                    return SynchronizationTypeEnum.None;
                }

                return base.LogSynchronization;
            }
            set
            {
                base.LogSynchronization = value;
            }
        }


        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            RelationshipNameInfoProvider.DeleteRelationshipName(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            RelationshipNameInfoProvider.SetRelationshipNameInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor, creates an empty RelationshipNameInfo structure.
        /// </summary>
        public RelationshipNameInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor, creates an empty RelationshipNameInfo object from the given DataRow data.
        /// </summary>
        /// <param name="dr">Datarow with the class info data</param>
        public RelationshipNameInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Check whether relationship is in site given by id.
        /// </summary>
        /// <param name="siteId">ID of site to check</param>
        /// <returns>Returns true if relationship name already exists</returns>
        public Boolean IsOnSite(int siteId)
        {
            return RelationshipNameSiteInfoProvider.IsRelationShipOnSite(RelationshipNameId, siteId);
        }

        #endregion
    }
}