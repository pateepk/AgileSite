using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.Core;
using CMS.DataEngine;
using CMS.Forums;

[assembly: RegisterObjectType(typeof(ForumSubscriptionWithPostInfo), "forums.forumsubscriptionwithpost")]

namespace CMS.Forums
{
    internal class ForumSubscriptionWithPostInfo : AbstractInfo<ForumSubscriptionWithPostInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "forums.forumsubscriptionwithpost";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = GetTypeInfo();

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty ForumSubscriptionWithPostInfo object.
        /// </summary>
        public ForumSubscriptionWithPostInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new ForumSubscriptionWithPostInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data</param>
        public ForumSubscriptionWithPostInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion


        #region "Methods"

        private static ObjectTypeInfo GetTypeInfo()
        {
            var typeInfo = ObjectTypeInfo.CreateListingObjectTypeInfo(OBJECT_TYPE, ForumSubscriptionInfo.TYPEINFO);
            typeInfo.ModuleName = ModuleName.FORUMS;

            return typeInfo;
        }


        /// <summary>
        /// Gets the default list of column names for this class
        /// </summary>
        protected override List<string> GetColumnNames()
        {
            return CombineColumnNames(
                "SubscriptionID",
                "SubscriptionEmail",
                "PostSubject",
                "SubscriptionApproved"
                );
        }


        /// <summary>
        /// Gets the data query for this object type
        /// </summary>
        protected override IDataQuery GetDataQueryInternal()
        {
            return new DataQuery("forums.forumsubscription", "selectallwithpost");
        }

        #endregion
    }
}