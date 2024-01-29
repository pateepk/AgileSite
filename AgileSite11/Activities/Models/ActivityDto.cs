using System;
using System.Data;

using CMS.Base;
using CMS.DataEngine;
using CMS.DataEngine.Internal;
using CMS.Helpers;
using CMS.SiteProvider;
using CMS.Core;

namespace CMS.Activities.Internal
{
    /// <summary>
    /// Represents lightweight version of <see cref="ActivityInfo"/>. This class should be used instead of <see cref="ActivityInfo"/> in places
    /// when dealing with the regular info object has to big performance impact and features provided by <see cref="BaseInfo"/> are not required.
    /// </summary>
    public class ActivityDto : IDataTransferObject, IActivityInfo
    {
        /// <summary>
        /// Instantiate new instance of <see cref="ActivityDto"/>.
        /// </summary>
        public ActivityDto()
        {
        }


        /// <summary>
        /// Instantiate new instance of <see cref="ActivityDto"/>.
        /// </summary>
        /// <param name="dataRow">Data row containing serialized <see cref="ActivityDto"/></param>
        public ActivityDto(DataRow dataRow)
        {
            ActivityID = dataRow["ActivityID"].ToInteger(0);
            ActivityType = dataRow["ActivityType"].ToString(string.Empty);
            ActivityContactID = dataRow["ActivityContactID"].ToInteger(0);
            ActivitySiteID = dataRow["ActivitySiteID"].ToInteger(0);
            ActivityCampaign = dataRow["ActivityCampaign"].ToString(string.Empty);
            ActivityUTMSource = dataRow["ActivityUTMSource"].ToString(string.Empty);
            ActivityUTMContent = dataRow["ActivityUTMContent"].ToString(string.Empty);
            ActivityComment = dataRow["ActivityComment"].ToString(string.Empty);
            ActivityCreated = dataRow["ActivityCreated"].ToDateTime(DateTime.MinValue, null);
            ActivityCulture = dataRow["ActivityCulture"].ToString(string.Empty);
            ActivityItemDetailID = dataRow["ActivityItemDetailID"].ToInteger(0);
            ActivityItemID = dataRow["ActivityItemID"].ToInteger(0);
            ActivityNodeID = dataRow["ActivityNodeID"].ToInteger(0);
            ActivityTitle = dataRow["ActivityTitle"].ToString(string.Empty);
            ActivityURL = dataRow["ActivityURL"].ToString(string.Empty);
            ActivityURLHash = CoreServices.Conversion.GetValue<long>(dataRow["ActivityURLHash"], 0);
            ActivityURLReferrer = dataRow["ActivityURLReferrer"].ToString(string.Empty);
            ActivityValue = dataRow["ActivityValue"].ToString(string.Empty);
            ActivityABVariantName = dataRow["ActivityABVariantName"].ToString(string.Empty);
            ActivityMVTCombinationName = dataRow["ActivityMVTCombinationName"].ToString(string.Empty);
        }


        /// <summary>
        /// Instantiate new instance of <see cref="ActivityDto"/>.
        /// </summary>
        /// <param name="activity"><see cref="ActivityInfo"/> to be converted to <see cref="ActivityDto"/></param>
        /// <exception cref="ArgumentNullException"><paramref name="activity"/> is <c>null</c></exception>
        public ActivityDto(ActivityInfo activity)
        {
            if (activity == null)
            {
                throw new ArgumentNullException(nameof(activity));
            }

            ActivityID = activity.ActivityID;
            ActivityType = activity.ActivityType;
            ActivityContactID = activity.ActivityContactID;
            ActivityContactGUID = activity.ActivityContactGUID;
            ActivitySiteID = activity.ActivitySiteID;
            ActivityCampaign = activity.ActivityCampaign;
            ActivityUTMSource = activity.ActivityUTMSource;
            ActivityUTMContent = activity.ActivityUTMContent;
            ActivityComment = activity.ActivityComment;
            ActivityCreated = activity.ActivityCreated;
            ActivityCulture = activity.ActivityCulture;
            ActivityItemDetailID = activity.ActivityItemDetailID;
            ActivityItemID = activity.ActivityItemID;
            ActivityNodeID = activity.ActivityNodeID;
            ActivityTitle = activity.ActivityTitle;
            ActivityURL = activity.ActivityURL;
            ActivityURLHash = activity.ActivityURLHash;
            ActivityURLReferrer = activity.ActivityURLReferrer;
            ActivityValue = activity.ActivityValue;
            ActivityABVariantName = activity.ActivityABVariantName;
            ActivityMVTCombinationName = activity.ActivityMVTCombinationName;
        }


        /// <summary>
        /// Creates new instance of <see cref="ActivityInfo"/> from the current <see cref="ActivityDto"/>.
        /// </summary>
        /// <remarks>
        /// This method should be used with caution. Once called, the result should not take any part in performance critical methods.
        /// </remarks>
        /// <returns><see cref="ActivityInfo"/> created from current <see cref="ActivityDto"/></returns>
        public ActivityInfo ToActivityInfo()
        {
            return new ActivityInfo
            {
                ActivityID = ActivityID,
                ActivityType = ActivityType,
                ActivityContactID = ActivityContactID,
                ActivityContactGUID = ActivityContactGUID,
                ActivitySiteID = ActivitySiteID,
                ActivityCampaign = ActivityCampaign,
                ActivityUTMSource = ActivityUTMSource,
                ActivityUTMContent = ActivityUTMContent,
                ActivityComment = ActivityComment,
                ActivityCreated = ActivityCreated,
                ActivityCulture = ActivityCulture,
                ActivityItemDetailID = ActivityItemDetailID,
                ActivityItemID = ActivityItemID,
                ActivityNodeID = ActivityNodeID,
                ActivityTitle = ActivityTitle,
                ActivityURL = ActivityURL,
                ActivityURLHash = ActivityURLHash,
                ActivityURLReferrer = ActivityURLReferrer,
                ActivityValue = ActivityValue,
                ActivityABVariantName = ActivityABVariantName,
                ActivityMVTCombinationName = ActivityMVTCombinationName
        };
        }
    

        /// <summary>
        /// Gets or sets the campaign code name for this activity.
        /// </summary>
        public string ActivityCampaign
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the campaign UTM source for this activity.
        /// </summary>
        public string ActivityUTMSource
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the campaign UTM content for this activity.
        /// </summary>
        public string ActivityUTMContent
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the ID othe item detail that relates to this activity.
        /// </summary>
        public int ActivityItemDetailID
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the activity ID.
        /// </summary>
        public int ActivityID
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the ID of the active contact for this activity.
        /// </summary>
        public int ActivityContactID
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the GUID of the active contact for this activity.
        /// </summary>
        public Guid ActivityContactGUID
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the node ID that relates to this activity.
        /// </summary>
        public int ActivityNodeID
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the activity title.
        /// </summary>
        public string ActivityTitle
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the type of the activity.
        /// </summary>
        public string ActivityType
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the value of this activity.
        /// </summary>
        public string ActivityValue
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the URL of this activity.
        /// </summary>
        public string ActivityURL
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets hash result of <see cref="IActivityInfo.ActivityURL"/>. The hash is needed for <see cref="PredefinedActivityType.PAGE_VISIT"/> activity type created on content only sites <see cref="SiteInfo.SiteIsContentOnly"/>.
        /// </summary>
        public long ActivityURLHash
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the date and time the activity was created.
        /// </summary>
        public DateTime ActivityCreated
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the site ID for this activity.
        /// </summary>
        public int ActivitySiteID
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the ID of the item that relates to this activity.
        /// </summary>
        public int ActivityItemID
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the AB test variant name.
        /// </summary>
        public string ActivityABVariantName
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the MVT Combination name.
        /// </summary>
        public string ActivityMVTCombinationName
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets URL referrer.
        /// </summary>
        public string ActivityURLReferrer
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or set additional comment.
        /// </summary>
        public string ActivityComment
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the document culture where activity occurred.
        /// </summary>
        public string ActivityCulture
        {
            get;
            set;
        }


        /// <summary>
        /// Fills given <paramref name="dataContainer"/> with values from current <see cref="ActivityDto"/>.
        /// </summary>
        /// <param name="dataContainer">Data container to be filled</param>
        /// <exception cref="ArgumentNullException"><paramref name="dataContainer"/> is <c>null</c></exception>
        public void FillDataContainer(IDataContainer dataContainer)
        {
            if (dataContainer == null)
            {
                throw new ArgumentNullException(nameof(dataContainer));
            }
            
            dataContainer["ActivityID"] = ActivityID;
            dataContainer["ActivityType"] = TextHelper.LimitLength(ActivityType, 250, String.Empty);
            dataContainer["ActivityContactID"] = ActivityContactID;
            dataContainer["ActivitySiteID"] = ActivitySiteID;
            dataContainer["ActivityCampaign"] = TextHelper.LimitLength(ActivityCampaign, 200, String.Empty);
            dataContainer["ActivityUTMSource"] = TextHelper.LimitLength(ActivityUTMSource, 200, String.Empty);
            dataContainer["ActivityUTMContent"] = TextHelper.LimitLength(ActivityUTMContent, 200, String.Empty);
            dataContainer["ActivityComment"] = ActivityComment;
            dataContainer["ActivityCreated"] = ActivityCreated;
            dataContainer["ActivityCulture"] = TextHelper.LimitLength(ActivityCulture, 10, String.Empty);
            dataContainer["ActivityItemDetailID"] = ActivityItemDetailID;
            dataContainer["ActivityItemID"] = ActivityItemID;
            dataContainer["ActivityNodeID"] = ActivityNodeID;
            dataContainer["ActivityTitle"] = TextHelper.LimitLength(ActivityTitle, 250, String.Empty);
            dataContainer["ActivityURL"] = ActivityURL;
            dataContainer["ActivityURLHash"] = ActivityURLHash;
            dataContainer["ActivityURLReferrer"] = ActivityURLReferrer;
            dataContainer["ActivityValue"] = TextHelper.LimitLength(ActivityValue, 250, String.Empty);
            dataContainer["ActivityABVariantName"] = TextHelper.LimitLength(ActivityABVariantName, 200, string.Empty);
            dataContainer["ActivityMVTCombinationName"] = TextHelper.LimitLength(ActivityMVTCombinationName, 200, string.Empty);
        }
    }
}
