using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.DocumentEngine;

[assembly: RegisterObjectType(typeof(VersionAttachmentInfo), VersionAttachmentInfo.OBJECT_TYPE)]

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Version attachment binding info.
    /// </summary>
    public class VersionAttachmentInfo : AbstractInfo<VersionAttachmentInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type.
        /// </summary>
        public const string OBJECT_TYPE = "cms.versionattachment";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(VersionAttachmentInfoProvider), OBJECT_TYPE, "CMS.VersionAttachment", null, null, null, null, null, null, null, "AttachmentHistoryID", AttachmentHistoryInfo.OBJECT_TYPE)
                                              {
                                                  DependsOn = new List<ObjectDependency>
                                                      {
                                                          new ObjectDependency("VersionHistoryID", VersionHistoryInfo.OBJECT_TYPE, ObjectDependencyEnum.Binding)
                                                      },
                                                  LogEvents = false,
                                                  TouchCacheDependencies = true,
                                                  SupportsVersioning = false,
                                                  ModuleName = "cms.content",
                                                  RegisterAsBindingToObjectTypes = new List<string> { AttachmentHistoryInfo.OBJECT_TYPE, AttachmentHistoryInfo.OBJECT_TYPE_VARIANT },
                                                  ImportExportSettings = { LogExport = false }
                                              };

        #endregion


        #region "Properties"

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
        /// Attachment history ID.
        /// </summary>
        public virtual int AttachmentHistoryID
        {
            get
            {
                return GetIntegerValue("AttachmentHistoryID", 0);
            }
            set
            {
                SetValue("AttachmentHistoryID", value);
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty CultureSiteInfo object.
        /// </summary>
        public VersionAttachmentInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new CultureSiteInfo object from the given DataRow.
        /// </summary>
        public VersionAttachmentInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Sets object
        /// </summary>
        protected override void SetObject()
        {
            VersionAttachmentInfoProvider.SetVersionAttachmentInfo(this);
        }


        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            VersionAttachmentInfoProvider.DeleteVersionAttachmentInfo(this);
        }

        #endregion
    }
}