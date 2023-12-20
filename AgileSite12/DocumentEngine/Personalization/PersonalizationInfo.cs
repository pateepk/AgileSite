using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Membership;
using CMS.PortalEngine;
using CMS.DocumentEngine;

[assembly: RegisterObjectType(typeof(PersonalizationInfo), PersonalizationInfo.OBJECT_TYPE)]
[assembly: RegisterObjectType(typeof(PersonalizationInfo), PersonalizationInfo.OBJECT_TYPE_DASHBOARD)]

namespace CMS.DocumentEngine
{
    /// <summary>
    /// PersonalizationInfo data container class.
    /// </summary>
    public class PersonalizationInfo : AbstractInfo<PersonalizationInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = PredefinedObjectType.PERSONALIZATION;


        /// <summary>
        /// Object type for dashboard
        /// </summary>
        public const string OBJECT_TYPE_DASHBOARD = PredefinedObjectType.DASHBOARD;


        /// <summary>
        /// User type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(PersonalizationInfoProvider), OBJECT_TYPE, "CMS.Personalization", "PersonalizationID", "PersonalizationLastModified", "PersonalizationGUID", null, null, null, null, "PersonalizationUserID", UserInfo.OBJECT_TYPE)
        {
            DependsOn = new List<ObjectDependency>
            {
                new ObjectDependency("PersonalizationDocumentID", TreeNode.OBJECT_TYPE, ObjectDependencyEnum.Required),
            },
            TypeCondition = new TypeCondition().WhereIsNull("PersonalizationDashboardName"),
            ImportExportSettings = { IsExportable = true, IsAutomaticallySelected = true, IncludeToExportParentDataSet = IncludeToParentEnum.None },
            TouchCacheDependencies = true,
            SynchronizationSettings =
            {
                LogSynchronization = SynchronizationTypeEnum.None,
                IncludeToSynchronizationParentDataSet = IncludeToParentEnum.None
            },
            SerializationSettings =
            {
              StructuredFields = new IStructuredField[]
                {
                    new StructuredField("PersonalizationWebParts")
                }
            },
            ContinuousIntegrationSettings =
            {
                Enabled = true
            }
        };


        /// <summary>
        /// Type information for dashboard.
        /// </summary>
        public static ObjectTypeInfo DASHBOARD = new ObjectTypeInfo(typeof(PersonalizationInfoProvider), OBJECT_TYPE_DASHBOARD, "CMS.Personalization", "PersonalizationID", "PersonalizationLastModified", "PersonalizationGUID", null, "PersonalizationDashboardName", null, "PersonalizationSiteID", "PersonalizationUserID", UserInfo.OBJECT_TYPE)
        {
            MacroCollectionName = "CMS.Dashboard",
            TypeCondition = new TypeCondition().WhereIsNotNull("PersonalizationDashboardName"),
            ImportExportSettings = { IsExportable = true, IsAutomaticallySelected = true, IncludeToExportParentDataSet = IncludeToParentEnum.None },
            SupportsGlobalObjects = true,
            TouchCacheDependencies = true,
            SynchronizationSettings =
            {
                LogSynchronization = SynchronizationTypeEnum.None,
                IncludeToSynchronizationParentDataSet = IncludeToParentEnum.None
            },
            SerializationSettings =
            {
              StructuredFields = new IStructuredField[]
                {
                    new StructuredField("PersonalizationWebParts")
                }
            },
            ContinuousIntegrationSettings =
            {
                Enabled = true,
                ObjectFileNameFields = { "PersonalizationDashboardName" }
            }
        };

        #endregion


        #region "Variables"

        /// <summary>
        /// User widgets template instance.
        /// </summary>
        protected PageTemplateInstance mTemplateInstance = null;

        #endregion


        #region "Properties"

        /// <summary>
        /// Personalization user ID.
        /// </summary>
        public virtual int PersonalizationUserID
        {
            get
            {
                return GetIntegerValue("PersonalizationUserID", 0);
            }
            set
            {
                SetValue("PersonalizationUserID", value);
            }
        }


        /// <summary>
        /// Personalization ID.
        /// </summary>
        public virtual int PersonalizationID
        {
            get
            {
                return GetIntegerValue("PersonalizationID", 0);
            }
            set
            {
                SetValue("PersonalizationID", value);
            }
        }


        /// <summary>
        /// Personalization GUID.
        /// </summary>
        public virtual Guid PersonalizationGUID
        {
            get
            {
                return GetGuidValue("PersonalizationGUID", Guid.Empty);
            }
            set
            {
                SetValue("PersonalizationGUID", value);
            }
        }


        /// <summary>
        /// Personalization document ID.
        /// </summary>
        public virtual int PersonalizationDocumentID
        {
            get
            {
                return GetIntegerValue("PersonalizationDocumentID", 0);
            }
            set
            {

                SetValue("PersonalizationDocumentID", value, value > 0);
            }
        }


        /// <summary>
        /// Personalization site ID.
        /// </summary>
        public virtual int PersonalizationSiteID
        {
            get
            {
                return GetIntegerValue("PersonalizationSiteID", 0);
            }
            set
            {
                SetValue("PersonalizationSiteID", value, value > 0);
            }
        }


        /// <summary>
        /// Personalization last modified.
        /// </summary>
        public virtual DateTime PersonalizationLastModified
        {
            get
            {
                return GetDateTimeValue("PersonalizationLastModified", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("PersonalizationLastModified", value);
            }
        }


        /// <summary>
        /// Personalization dashboard name.
        /// </summary>
        public virtual string PersonalizationDashboardName
        {
            get
            {
                return GetStringValue("PersonalizationDashboardName", String.Empty);
            }
            set
            {
                SetValue("PersonalizationDashboardName", value);
            }
        }


        /// <summary>
        /// Gets or sets the page template WebParts.
        /// </summary>
        public string WebParts
        {
            get
            {
                return TemplateInstance.GetZonesXML();
            }
            set
            {
                SetValue("PersonalizationWebParts", value);
                TemplateInstance.LoadFromXml(value);
            }
        }


        /// <summary>
        /// User widgets template instance.
        /// </summary>
        public virtual PageTemplateInstance TemplateInstance
        {
            get
            {
                if (mTemplateInstance == null)
                {
                    // Prepare the template instance
                    string webPartsXml = ValidationHelper.GetString(GetValue("PersonalizationWebParts"), "<page></page>");
                    mTemplateInstance = new PageTemplateInstance(webPartsXml);
                }
                return mTemplateInstance;
            }
            set
            {
                mTemplateInstance = value;
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Type information.
        /// </summary>
        public override ObjectTypeInfo TypeInfo
        {
            get
            {
                return String.IsNullOrEmpty(PersonalizationDashboardName) ? TYPEINFO : DASHBOARD;
            }
        }


        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            PersonalizationInfoProvider.DeletePersonalizationInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            PersonalizationInfoProvider.SetPersonalizationInfo(this);
        }


        /// <summary>
        /// Returns object name combining object type name and object display name.
        /// </summary>
        protected override string GetObjectName()
        {
            if (PersonalizationDocumentID <= 0)
            {
                return base.GetObjectName();
            }

            var document = new TreeProvider().SelectNodes()
                                             .Columns("DocumentCulture", "DocumentNamePath", "NodeSiteID")
                                             .WhereEquals("DocumentID", PersonalizationDocumentID)
                                             .Result;

            var node = TreeNode.New(document.Tables[0].Rows[0]);

            return String.Format("{0} '{1}'", TypeInfo.GetNiceObjectTypeName(), node.Generalized.GetFullObjectName(includeGroup: false));
        }


        /// <summary>
        /// Gets a where condition to find an existing object based on current object. Existing dashboard object is identified by an user, a site and a dashboard name. 
        /// The default implementation uses GUID which may be different if a dashboard was created and exported on a different site.
        /// </summary>
        protected override WhereCondition GetExistingWhereCondition()
        {
            if (TypeInfo == DASHBOARD)
            {
                return GetSiteWhereCondition()
                        .WhereID(TypeInfo.ParentIDColumn, ObjectParentID)
                        .WhereEquals(TypeInfo.DisplayNameColumn, ObjectDisplayName);
            }

            return base.GetExistingWhereCondition();
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty PersonalizationInfo object.
        /// </summary>
        public PersonalizationInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new PersonalizationInfo object from the given DataRow.
        /// </summary>
        public PersonalizationInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}