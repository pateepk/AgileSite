using System;
using System.Data;
using System.Collections.Generic;

using CMS;
using CMS.Activities;
using CMS.DataEngine;
using CMS.ContactManagement;

[assembly: RegisterObjectType(typeof(ActivityListInfo), "om.activitylist")]

namespace CMS.ContactManagement
{
    /// <summary>
    /// Provides method to determining whether the <see cref="ContactInfo"/> assigned to the given <see cref="IActivityInfo"/> exists and is monitored or not.
    /// </summary>
    internal class ContactActivityLogValidator : IActivityLogValidator
    {
        /// <summary>
        /// Determines whether the <see cref="ContactInfo"/> assigned to given <paramref name="activity"/> exists and is monitored, i.e. 
        /// <see cref="ContactInfo.ContactMonitored"/> property is set to <c>true</c>.
        /// </summary>
        /// <param name="activity">Activity to be validated</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="activity"/> is <c>null</c></exception>
        /// <exception cref="ArgumentException">Thrown when <see cref="IActivityInfo.ActivityContactID"/> of given <paramref name="activity"/> has to be greater than <c>0</c></exception>
        /// <returns>True if assigned <see cref="ContactInfo"/> exists and is monitored; otherwise, false</returns>
        public bool IsValid(IActivityInfo activity)
        {
            if (activity == null)
            {
                throw new ArgumentNullException("activity");
            }
            if (activity.ActivityContactID <= 0)
            {
                throw new ArgumentException("[ContactActivityLogValidator.ValidateActivity]: Contact ID has to be specified", "activity");
            }
            
            var contact = ContactInfoProvider.GetContactInfo(activity.ActivityContactID);
            if (contact == null)
            {
                return false;
            }

            return contact.ContactMonitored;
        }
    }
    

    /// <summary>
    /// "Virtual" object for loading activities for particular contact (not merged).
    /// </summary>
    internal class ActivityListInfo : AbstractInfo<ActivityListInfo>
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
        public ActivityListInfo(DataRow dr)
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
        /// Gets the data query for this object type
        /// </summary>
        protected override IDataQuery GetDataQueryInternal()
        {
            return new DataQuery("om.activity.selectactivitylist");
        }

        #endregion
    }
}