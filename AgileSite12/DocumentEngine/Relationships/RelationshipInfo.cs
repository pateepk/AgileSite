using System.Collections.Generic;
using System.Data;
using System.Linq;

using CMS;
using CMS.Base;
using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.DocumentEngine.Internal;
using CMS.Helpers;
using CMS.Relationships;

[assembly: RegisterObjectType(typeof(RelationshipInfo), RelationshipInfo.OBJECT_TYPE)]
[assembly: RegisterObjectType(typeof(RelationshipInfo), RelationshipInfo.OBJECT_TYPE_ADHOC)]

namespace CMS.DocumentEngine
{
    /// <summary>
    /// RelationshipInfo data container class.
    /// </summary>
    public class RelationshipInfo : AbstractInfo<RelationshipInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "cms.relationship";

        /// <summary>
        /// Object type for ordered relationships with ad-hoc relationship name.
        /// </summary>
        public const string OBJECT_TYPE_ADHOC = "cms.adhocrelationship";

        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(RelationshipInfoProvider), OBJECT_TYPE, "CMS.Relationship", "RelationshipID", null, null, null, null, null, null, "LeftNodeID", DocumentNodeDataInfo.OBJECT_TYPE)
        {
            DependsOn = new List<ObjectDependency>
            {
                new ObjectDependency("RightNodeID", DocumentNodeDataInfo.OBJECT_TYPE, ObjectDependencyEnum.Binding),
                new ObjectDependency("RelationshipNameID", RelationshipNameInfo.OBJECT_TYPE, ObjectDependencyEnum.Binding)
            },

            SynchronizationSettings =
            {
                LogSynchronization = SynchronizationTypeEnum.Default,
            },
            TypeCondition = new TypeCondition().WhereEqualsOrNull("RelationshipIsAdHoc", false),
            LogEvents = true,
            TouchCacheDependencies = true,
            SupportsVersioning = false,
            IsBinding = true,
            AllowRestore = false,
            ModuleName = "cms.content",
            // RequiredObject has to be set to false because of cross-site related documents
            RequiredObject = false,
            ImportExportSettings =
            {
                LogExport = false
            },
            ContinuousIntegrationSettings =
            {
                Enabled = true
            }
        };


        /// <summary>
        /// Type information for ordered (ad hoc) relationship.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO_ADHOC = new ObjectTypeInfo(typeof(RelationshipInfoProvider), OBJECT_TYPE_ADHOC, "CMS.Relationship", "RelationshipID", null, null, null, null, null, null, "LeftNodeID", DocumentNodeDataInfo.OBJECT_TYPE)
        {
            MacroCollectionName = "AdHocRelationship",
            OriginalTypeInfo = TYPEINFO,

            DependsOn = new List<ObjectDependency>
            {
                new ObjectDependency("RightNodeID", DocumentNodeDataInfo.OBJECT_TYPE, ObjectDependencyEnum.Binding),
                new ObjectDependency("RelationshipNameID", RelationshipNameInfo.OBJECT_TYPE, ObjectDependencyEnum.Binding)
            },

            SynchronizationSettings =
            {
                LogSynchronization = SynchronizationTypeEnum.Default,
            },
            OrderColumn = "RelationshipOrder",
            TypeCondition = new TypeCondition().WhereEquals("RelationshipIsAdHoc", true),
            LogEvents = true,
            TouchCacheDependencies = true,
            SupportsVersioning = false,
            IsBinding = true,
            AllowRestore = false,
            ModuleName = "cms.content",
            // RequiredObject has to be set to false because of cross-site related documents
            RequiredObject = false,
            ImportExportSettings =
            {
                LogExport = false
            },
            ContinuousIntegrationSettings =
            {
                Enabled = true
            }
        };

        #endregion


        #region "Variables"

        /// <summary>
        /// Custom data.
        /// </summary>
        private ContainerCustomData mRelationshipCustomData;

        #endregion


        #region "Properties"

        /// <summary>
        /// Relationship order. Returns 0 for standard relationships which cannot be sorted. Only ah-hoc relationships can be sorted.
        /// </summary>
        [DatabaseField]
        public virtual int RelationshipOrder
        {
            get
            {
                return GetIntegerValue("RelationshipOrder", 0);
            }
            set
            {
                SetValue("RelationshipOrder", value, (value > 0));
            }
        }


        /// <summary>
        /// Returns true if relationship is ad-hoc (available since v 9.0), false for standard relationships.
        /// Ad-hoc relationship is a left side relationship created between node and selected related node.
        /// Multiple ad-hoc relationships can be sorted in the group which has common left node and ad-hoc relationship name.
        /// </summary>
        [DatabaseField]
        public virtual bool RelationshipIsAdHoc
        {
            get
            {
                return GetBooleanValue("RelationshipIsAdHoc", false);
            }
            set
            {
                SetValue("RelationshipIsAdHoc", value);
            }
        }


        /// <summary>
        /// Relationship ID.
        /// </summary>
        [DatabaseField]
        public virtual int RelationshipID
        {
            get
            {
                return GetIntegerValue("RelationshipID", 0);
            }
            set
            {
                SetValue("RelationshipID", value);
            }
        }

        /// <summary>
        /// Right node ID.
        /// </summary>
        [DatabaseField("RightNodeID")]
        public virtual int RightNodeId
        {
            get
            {
                return GetIntegerValue("RightNodeID", 0);
            }
            set
            {
                SetValue("RightNodeID", value);
            }
        }


        /// <summary>
        /// Relationship name ID.
        /// </summary>
        [DatabaseField("RelationshipNameID")]
        public virtual int RelationshipNameId
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
        /// Left node ID.
        /// </summary>
        [DatabaseField("LeftNodeID")]
        public virtual int LeftNodeId
        {
            get
            {
                return GetIntegerValue("LeftNodeID", 0);
            }
            set
            {
                SetValue("LeftNodeID", value);
            }
        }


        /// <summary>
        /// Relationship custom data.
        /// </summary>
        [DatabaseField(ValueType = typeof(string))]
        public ContainerCustomData RelationshipCustomData
        {
            get
            {
                return mRelationshipCustomData ?? (mRelationshipCustomData = new ContainerCustomData(this, "RelationshipCustomData"));
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Type info.
        /// </summary>
        public override ObjectTypeInfo TypeInfo
        {
            get
            {
                return RelationshipIsAdHoc ? TYPEINFO_ADHOC : TYPEINFO;
            }
        }


        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            RelationshipInfoProvider.DeleteRelationshipInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            RelationshipInfoProvider.SetRelationshipInfo(this);
        }


        /// <summary>
        /// Creates subset for ordering functionality. The group is defined by ad-hoc relationship name ID and the left node ID, under which is relationship created.
        /// </summary>
        protected override WhereCondition GetSiblingsWhereCondition()
        {
            return new WhereCondition().WhereEquals("RelationshipNameID", RelationshipNameId)
                                       .WhereEquals("LeftNodeID", LeftNodeId);

        }


        /// <summary>
        /// Ensures synchronization of left side node if order of ad-hoc relationships was changed.
        /// This method is called only for ad-hoc relationships with OrderColumn defined in the type info.
        /// This method can be removed in case node as a parent of relationship will be defined in the type info and import/export and staging will work properly.
        /// </summary>
        protected override void SetObjectOrderPostprocessing()
        {
            base.SetObjectOrderPostprocessing();

            var tree = new TreeProvider();
            var node = tree.SelectSingleNode(LeftNodeId);

            DocumentSynchronizationHelper.LogDocumentChange(node, TaskTypeEnum.UpdateDocument, node.TreeProvider);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty RelationshipInfo object.
        /// </summary>
        public RelationshipInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new RelationshipInfo object from the given DataRow.
        /// </summary>
        public RelationshipInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Checks whether the specified user has permissions for this object. This method is called automatically after CheckPermissions event was fired.
        /// </summary>
        /// <param name="permission">Permission to perform this operation will be checked</param>
        /// <param name="siteName">Permissions on this site will be checked</param>
        /// <param name="userInfo">Permissions of this user will be checked</param>
        /// <param name="exceptionOnFailure">If true, PermissionCheckException is thrown whenever a permission check fails</param>
        /// <returns>True if user is allowed to perform specified operation on the this object; otherwise false</returns>
        protected override bool CheckPermissionsInternal(PermissionsEnum permission, string siteName, IUserInfo userInfo, bool exceptionOnFailure)
        {
            if (LeftNodeId > 0 && RelationshipIsAdHoc)
            {
                var tree = new TreeProvider();
                var node = tree.SelectNodes()
                    .WhereEquals("NodeID", LeftNodeId)
                    .OnSite(siteName)
                    .CombineWithAnyCulture()
                    .TopN(1)
                    .FirstOrDefault();

                if (node != null)
                {
                    return DocumentSecurityHelper.IsAuthorizedPerDocument(node, GetPermissionToCheck(permission), userInfo, siteName);
                }
            }

            return base.CheckPermissionsInternal(permission, siteName, userInfo, exceptionOnFailure);
        }

        #endregion
    }
}