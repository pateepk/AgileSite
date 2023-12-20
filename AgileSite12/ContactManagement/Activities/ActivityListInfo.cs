using System.Collections.Generic;

using CMS;
using CMS.Activities;
using CMS.DataEngine;
using CMS.ContactManagement;

[assembly: RegisterObjectType(typeof(ActivityListInfo), "om.activitylist")]

namespace CMS.ContactManagement
{
    /// <summary>
    /// "Virtual" object for loading activities for particular contact (not merged).
    /// </summary>
    public class ActivityListInfo : AbstractInfo<ActivityListInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "om.activitylist";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = ObjectTypeInfo.CreateListingObjectTypeInfo(OBJECT_TYPE, ActivityInfo.TYPEINFO);

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty ActivityListInfo object.
        /// </summary>
        public ActivityListInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new ActivityListInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data</param>
        public ActivityListInfo(System.Data.DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Gets the default list of column names for this class
        /// </summary>
        protected override List<string> GetColumnNames()
        {
            return CombineColumnNames(
                "ActivityID",
                "ActivityContactID",
                "ActivityCreated",
                "ActivityType",
                "ActivityItemID",
                "ActivityItemDetailID",
                "ActivityValue",
                "ActivityURL",
                "ActivityTitle",
                "ActivitySiteID",
                "ActivityComment",
                "ActivityCampaign",
                "ActivityURLReferrer",
                "ActivityCulture",
                "ActivityNodeID",
                "ActivityUTMSource",
                "ActivityABVariantName",
                "ActivityMVTCombinationName",
                "ActivityURLHash",
                "ActivityUTMContent",
                "ContactFirstName",
                "ContactMiddleName",
                "ContactLastName");
        }


        /// <summary>
        /// Gets the data query for this object type.
        /// </summary>
        protected override IDataQuery GetDataQueryInternal()
        {

            return ActivityInfoProvider.GetActivities()
                                       .Source(s => s.LeftJoin<ContactInfo>(nameof(ActivityInfo.ActivityContactID), nameof(ContactInfo.ContactID)))
                                       .Columns(GetColumnNames());
        }

        #endregion
    }
}
