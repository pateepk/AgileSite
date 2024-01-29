using System;

using CMS.SiteProvider;

namespace CMS.Activities
{
    /// <summary>
    /// Defines contract for <see cref="ActivityInfo"/>.
    /// </summary>
    public interface IActivityInfo
    {
        /// <summary>
        /// Gets or sets the campaign code name for this activity.
        /// </summary>
        string ActivityCampaign
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the campaign UTM source for this activity.
        /// </summary>
        string ActivityUTMSource
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the campaign UTM content for this activity.
        /// </summary>
        string ActivityUTMContent
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the ID other item detail that relates to this activity.
        /// </summary>
        int ActivityItemDetailID
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the activity ID.
        /// </summary>
        int ActivityID
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the ID of the active contact for this activity.
        /// </summary>
        int ActivityContactID
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the GUID of the active contact for this activity.
        /// </summary>
        Guid ActivityContactGUID
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the node ID that relates to this activity.
        /// </summary>
        int ActivityNodeID
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the activity title.
        /// </summary>
        string ActivityTitle
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the type of the activity.
        /// </summary>
        string ActivityType
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the value of this activity.
        /// </summary>
        string ActivityValue
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the URL of this activity.
        /// </summary>
        string ActivityURL
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets hash result of <see cref="ActivityURL"/>. The hash is needed for <see cref="PredefinedActivityType.PAGE_VISIT"/> activity type created on content only sites <see cref="SiteInfo.SiteIsContentOnly"/>.
        /// </summary>
        long ActivityURLHash
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the date and time the activity was created.
        /// </summary>
        DateTime ActivityCreated
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the site ID for this activity.
        /// </summary>
        int ActivitySiteID
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the ID of the item that relates to this activity.
        /// </summary>
        int ActivityItemID
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the AB test variant name.
        /// </summary>
        string ActivityABVariantName
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the MVT Combination name.
        /// </summary>
        string ActivityMVTCombinationName
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets URL referrer.
        /// </summary>
        string ActivityURLReferrer
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or set additional comment.
        /// </summary>
        string ActivityComment
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the document culture where activity occurred.
        /// </summary>
        string ActivityCulture
        {
            get;
            set;
        }
    }
}