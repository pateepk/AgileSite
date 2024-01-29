using System.Collections.Generic;
using System.Linq;

using CMS;
using CMS.DataEngine;
using CMS.DocumentEngine;

[assembly: RegisterObjectType(typeof(AttachmentListInfo), AttachmentListInfo.OBJECT_TYPE)]

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Represents a list info for list of all attachments in the system
    /// </summary>
    internal class AttachmentListInfo : AbstractInfo<AttachmentListInfo>
    {
        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "cms.attachmentlist";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = ObjectTypeInfo.CreateListingObjectTypeInfo(OBJECT_TYPE, AttachmentInfo.TYPEINFO);


        /// <summary>
        /// Constructor - Creates an empty AttachmentListInfo object.
        /// </summary>
        public AttachmentListInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Gets the data query for this object type
        /// </summary>
        protected override IDataQuery GetDataQueryInternal()
        {
            string attachmentTableName = AttachmentInfo.TYPEINFO.ClassStructureInfo.TableName;

            var query = AttachmentInfoProvider.GetAttachments()
                                              .Source(s => s.LeftJoin(new QuerySourceTable(attachmentTableName, "parent"), "AttachmentVariantParentID", "AttachmentID"))
                                              .Columns(string.Format("{0}.*", attachmentTableName))
                                              .AddColumn(new QueryColumn("parent.AttachmentGUID").As("ParentGUID"))
                                              .AddColumn(new QueryColumn("parent.AttachmentName").As("ParentName"))
                                              .AsNested();

            return query;
        }


        /// <summary>
        /// Gets the default list of column names for this class
        /// </summary>
        protected override List<string> GetColumnNames()
        {
            string[] viewColumns = { "ParentGUID", "ParentName" };
            return viewColumns.Union(base.GetColumnNames().ToArray()).ToList();
        }
    }
}