using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.EventManager;

[assembly: RegisterObjectType(typeof(BookingEventListInfo), "cms.bookingeventlist")]

namespace CMS.EventManager
{
    internal class BookingEventListInfo : AbstractInfo<BookingEventListInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "cms.bookingeventlist";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = GetTypeInfo();

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty BookingEventListInfo object.
        /// </summary>
        public BookingEventListInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new BookingEventListInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data</param>
        public BookingEventListInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion


        #region "Methods"

        private static ObjectTypeInfo GetTypeInfo()
        {
            var typeInfo = ObjectTypeInfo.CreateListingObjectTypeInfo(OBJECT_TYPE, TreeNode.TYPEINFO);
            typeInfo.ModuleName = "cms.content";

            return typeInfo;
        }


        protected override List<string> GetColumnNames()
        {
            if ((mCombinedColumnNames == null) || TypeInfo.ColumnsInvalidated)
            {
                // View-specific columns
                string[] viewColumns =
                {
                    "ClassName",
                    "ClassDisplayName",
                    "NodeID",
                    "NodeAliasPath",
                    "NodeName",
                    "NodeLinkedNodeID",
                    "NodeAlias",
                    "NodeGroupID",
                    "NodeSiteID",
                    "DocumentID",
                    "DocumentName",
                    "DocumentNamePath",
                    "DocumentCulture",
                    "DocumentForeignKeyValue",
                    "AttendeesCount"
                };

                IDataClass dataClass = DataClassFactory.NewDataClass("CMS.BookingEvent");

                CombineColumnNames(viewColumns, dataClass.ColumnNames);
            }
            return mCombinedColumnNames;
        }


        /// <summary>
        /// Gets the data query for this object type
        /// </summary>
        protected override IDataQuery GetDataQueryInternal()
        {
            return EventProvider.GetBookingEventQuery(null);
        }

        #endregion
    }
}