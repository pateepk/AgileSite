using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.Base;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Membership;
using CMS.WorkflowEngine;
using CMS.DocumentEngine;
using CMS.Core;

[assembly: RegisterObjectType(typeof(VersionHistoryInfo), VersionHistoryInfo.OBJECT_TYPE)]

namespace CMS.DocumentEngine
{
    /// <summary>
    /// VersionHistoryInfo data container class.
    /// </summary>
    public class VersionHistoryInfo : AbstractInfo<VersionHistoryInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = PredefinedObjectType.VERSIONHISTORY;


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(VersionHistoryInfoProvider), OBJECT_TYPE, "CMS.VersionHistory", "VersionHistoryID", null, null, null, "DocumentNamePath", null, "NodeSiteID", "DocumentID", TreeNode.OBJECT_TYPE)
        {
            DependsOn = new List<ObjectDependency>
            {
                new ObjectDependency("ModifiedByUserID", UserInfo.OBJECT_TYPE, ObjectDependencyEnum.RequiredHasDefault),
                new ObjectDependency("VersionDeletedByUserID", UserInfo.OBJECT_TYPE, ObjectDependencyEnum.RequiredHasDefault),
                new ObjectDependency("VersionClassID", DocumentTypeInfo.OBJECT_TYPE_DOCUMENTTYPE, ObjectDependencyEnum.Required),
                new ObjectDependency("VersionWorkflowID", WorkflowInfo.OBJECT_TYPE),
                new ObjectDependency("VersionWorkflowStepID", WorkflowStepInfo.OBJECT_TYPE),
            },

            SynchronizationSettings =
            {
                IncludeToSynchronizationParentDataSet = IncludeToParentEnum.None,
                LogSynchronization = SynchronizationTypeEnum.None,
            },
            LogEvents = false,
            TouchCacheDependencies = true,
            SupportsVersioning = false,
            AllowRestore = false,
            LogIntegration = false,
            ModuleName = "cms.content",
            SupportsCloning = false,
            ImportExportSettings = { IncludeToExportParentDataSet = IncludeToParentEnum.None, LogExport = false }
        };

        #endregion


        #region "Variables"

        private IDataContainer mData;

        #endregion


        #region "Public properties"

        /// <summary>
        /// Version data.
        /// </summary>
        [DatabaseMapping(false)]
        public virtual IDataContainer Data
        {
            get
            {
                if (mData != null)
                {
                    return mData;
                }

                var xml = NodeXML;
                if (String.IsNullOrEmpty(xml))
                {
                    return null;
                }

                var data = DataHelper.GetDataSetFromXml(xml);
                if (!DataHelper.DataSourceIsEmpty(data))
                {
                    SetDataInternal(data);
                }

                return mData;
            }
        }


        /// <summary>
        /// Node XML.
        /// </summary>
        public virtual string NodeXML
        {
            get
            {
                return GetStringValue("NodeXML", "");
            }
            set
            {
                SetValue("NodeXML", value);
            }
        }


        /// <summary>
        /// Publish from date.
        /// </summary>
        public virtual DateTime PublishFrom
        {
            get
            {
                return GetDateTimeValue("PublishFrom", DateTime.MinValue);
            }
            set
            {
                SetValue("PublishFrom", value, DateTime.MinValue);
            }
        }


        /// <summary>
        /// To be published date.
        /// </summary>
        public virtual bool ToBePublished
        {
            get
            {
                return GetBooleanValue("ToBePublished", false);
            }
            set
            {
                SetValue("ToBePublished", value);
            }
        }


        /// <summary>
        /// Version history ID.
        /// </summary>
        public virtual int VersionHistoryID
        {
            get
            {
                return GetIntegerValue("VersionHistoryID", 0);
            }
            set
            {
                SetValue("VersionHistoryID", value);
            }
        }


        /// <summary>
        /// Modified by user ID.
        /// </summary>
        public virtual int ModifiedByUserID
        {
            get
            {
                return GetIntegerValue("ModifiedByUserID", 0);
            }
            set
            {
                SetValue("ModifiedByUserID", value, 0);
            }
        }


        /// <summary>
        /// Deleted by user ID (for recycle bin).
        /// </summary>
        public virtual int DeletedByUserID
        {
            get
            {
                return GetIntegerValue("VersionDeletedByUserID", 0);
            }
            set
            {
                SetValue("VersionDeletedByUserID", value, 0);
            }
        }


        /// <summary>
        /// Was published to date.
        /// </summary>
        public virtual DateTime WasPublishedTo
        {
            get
            {
                return GetDateTimeValue("WasPublishedTo", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("WasPublishedTo", value, DateTimeHelper.ZERO_TIME);
            }
        }


        /// <summary>
        /// Site ID of the node.
        /// </summary>
        public virtual int NodeSiteID
        {
            get
            {
                return GetIntegerValue("NodeSiteID", 0);
            }
            set
            {
                SetValue("NodeSiteID", value);
            }
        }


        /// <summary>
        /// Modified when date.
        /// </summary>
        public virtual DateTime ModifiedWhen
        {
            get
            {
                return GetDateTimeValue("ModifiedWhen", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("ModifiedWhen", value);
            }
        }


        /// <summary>
        /// Deleted when date (for recycle bin).
        /// </summary>
        public virtual DateTime DeletedWhen
        {
            get
            {
                return GetDateTimeValue("VersionDeletedWhen", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("VersionDeletedWhen", value, DateTimeHelper.ZERO_TIME);
            }
        }


        /// <summary>
        /// Publish to date.
        /// </summary>
        public virtual DateTime PublishTo
        {
            get
            {
                return GetDateTimeValue("PublishTo", DateTime.MaxValue);
            }
            set
            {
                SetValue("PublishTo", value, DateTime.MaxValue);
            }
        }


        /// <summary>
        /// Version number.
        /// </summary>
        public virtual string VersionNumber
        {
            get
            {
                return GetStringValue("VersionNumber", "");
            }
            set
            {
                SetValue("VersionNumber", value, null);
            }
        }


        /// <summary>
        /// Document name path.
        /// </summary>
        public virtual string DocumentNamePath
        {
            get
            {
                return GetStringValue("DocumentNamePath", "");
            }
            set
            {
                SetValue("DocumentNamePath", value);
            }
        }


        /// <summary>
        /// Version comment.
        /// </summary>
        public virtual string VersionComment
        {
            get
            {
                return GetStringValue("VersionComment", "");
            }
            set
            {
                SetValue("VersionComment", value, null);
            }
        }


        /// <summary>
        /// Was published from date.
        /// </summary>
        public virtual DateTime WasPublishedFrom
        {
            get
            {
                return GetDateTimeValue("WasPublishedFrom", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("WasPublishedFrom", value, DateTimeHelper.ZERO_TIME);
            }
        }


        /// <summary>
        /// Document ID.
        /// </summary>
        public virtual int DocumentID
        {
            get
            {
                return GetIntegerValue("DocumentID", 0);
            }
            set
            {
                SetValue("DocumentID", value, 0);
            }
        }


        /// <summary>
        /// Version document name.
        /// </summary>
        public virtual string VersionDocumentName
        {
            get
            {
                return GetStringValue("VersionDocumentName", "");
            }
            set
            {
                SetValue("VersionDocumentName", value, null);
            }
        }


        /// <summary>
        /// Version document type.
        /// </summary>
        public virtual string VersionDocumentType
        {
            get
            {
                return GetStringValue("VersionDocumentType", "");
            }
            set
            {
                SetValue("VersionDocumentType", value, null);
            }
        }


        /// <summary>
        /// Version class ID.
        /// </summary>
        public virtual int VersionClassID
        {
            get
            {
                return GetIntegerValue("VersionClassID", 0);
            }
            set
            {
                SetValue("VersionClassID", value, 0);
            }
        }


        /// <summary>
        /// Version menu redirection URL.
        /// </summary>
        public virtual string VersionMenuRedirectUrl
        {
            get
            {
                return GetStringValue("VersionMenuRedirectUrl", "");
            }
            set
            {
                SetValue("VersionMenuRedirectUrl", value, null);
            }
        }


        /// <summary>
        /// Version workflow ID.
        /// </summary>
        public virtual int VersionWorkflowID
        {
            get
            {
                return GetIntegerValue("VersionWorkflowID", 0);
            }
            set
            {
                SetValue("VersionWorkflowID", value, 0);
            }
        }


        /// <summary>
        /// Version workflow step ID.
        /// </summary>
        public virtual int VersionWorkflowStepID
        {
            get
            {
                return GetIntegerValue("VersionWorkflowStepID", 0);
            }
            set
            {
                SetValue("VersionWorkflowStepID", value, 0);
            }
        }


        /// <summary>
        /// Version node aliaspath.
        /// </summary>
        public virtual string VersionNodeAliasPath
        {
            get
            {
                return GetStringValue("VersionNodeAliasPath", "");
            }
            set
            {
                SetValue("VersionNodeAliasPath", value, null);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            VersionHistoryInfoProvider.DeleteVersionHistoryInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            VersionHistoryInfoProvider.SetVersionHistoryInfo(this);
        }


        /// <summary>
        /// Sets the field value.
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="value">New value</param>
        public override bool SetValue(string columnName, object value)
        {
            var result = base.SetValue(columnName, value);

            // Special columns treatment
            switch (columnName.ToLowerInvariant())
            {
                // Clear property in memory
                case "nodexml":
                    mData = null;
                    break;
            }

            return result;
        }


        /// <summary>
        /// Returns object name combining object type name and object display name.
        /// </summary>
        protected override string GetObjectName()
        {
            var objectTypeName = (DeletedWhen != DateTimeHelper.ZERO_TIME) ? CoreServices.Localization.GetString("objecttype.cms_versionhistory_recyclebin") : TypeInfo.GetNiceObjectTypeName();
            return String.Format("{0} {1}", objectTypeName, DocumentNamePath);
        }


        /// <summary>
        /// Returns the name of the object within its parent hierarchy.
        /// </summary>
        /// <param name="includeParent">If true, the parent object name is included to the object name</param>
        /// <param name="includeSite">If true, the site information is included if available</param>
        /// <param name="includeGroup">If true, the group information is included if available</param>
        protected override string GetFullObjectName(bool includeParent, bool includeSite, bool includeGroup)
        {
            // Do not include hierarchy information
            return base.GetFullObjectName(false, includeSite, includeGroup);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty VersionHistoryInfo structure.
        /// </summary>
        public VersionHistoryInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates the VersionHistoryInfo object from the given DataRow data.
        /// </summary>
        /// <param name="dr">Datarow with the workflow info data</param>
        public VersionHistoryInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion


        #region "Internal methods"

        /// <summary>
        /// Sets the version data.
        /// </summary>
        /// <param name="node">Document instance with data to set.</param>
        /// <param name="loadMissingDataFromDb">Indicates if missing data should be loaded from the database for the node instance.</param>
        internal void SetData(TreeNode node, bool loadMissingDataFromDb = true)
        {
            if (node == null)
            {
                NodeXML = String.Empty;
                return;
            }

            // Make sure document instance is complete to get full data for the version
            node.MakeComplete(loadMissingDataFromDb);
            var data = node.GetOriginalDataSet();
            NodeXML = data.GetXml();

            SetDataInternal(data);
        }


        /// <summary>
        /// Sets the version data.
        /// </summary>
        /// <param name="data">Data to set</param>
        private void SetDataInternal(DataSet data)
        {
            // Lock the data set to prevent changes in cached version
            DataHelper.LockDataSet(data);

            mData = new DataRowContainer(data.Tables[0].Rows[0]);
        }

        #endregion
    }
}