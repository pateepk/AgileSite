using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.Newsletters;

[assembly: RegisterObjectType(typeof(SubscriberNewsletterListInfo), "newsletter.subscribernewsletterlist")]

namespace CMS.Newsletters
{
    internal class SubscriberNewsletterListInfo : AbstractInfo<SubscriberNewsletterListInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "newsletter.subscribernewsletterlist";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = GetTypeInfo();

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty SubscriberNewsletterListInfo object.
        /// </summary>
        public SubscriberNewsletterListInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new SubscriberNewsletterListInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data</param>
        public SubscriberNewsletterListInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
        

        #region "Methods"
        
        private static ObjectTypeInfo GetTypeInfo()
        {
            var typeInfo = ObjectTypeInfo.CreateListingObjectTypeInfo(OBJECT_TYPE, SubscriberNewsletterInfo.TYPEINFO);
            typeInfo.AllowDataExport = true;

            return typeInfo;
        }


        /// <summary>
        /// Gets the default list of column names for this class
        /// </summary>
        protected override List<string> GetColumnNames()
        {
            return CombineColumnNames(
                "SubscriberID",
                "SubscriberFullName",
                "SubscriberEmail",
                "SubscriptionApproved",
                "NewsletterID",
                "SubscriberType",
                "NewsletterDisplayName",
                "SubscriberRelatedID"
                );
        }


        /// <summary>
        /// Gets the data query for this object type
        /// </summary>
        protected override IDataQuery GetDataQueryInternal()
        {
            return new DataQuery("newsletter.subscribernewsletter", "selectsubscriptions");
        }

        #endregion
    }
}