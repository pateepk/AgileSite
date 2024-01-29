using System;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace CMS.SocialMarketing
{
    /// <summary>
    /// Represents state of one object for which Facebook Insights are collected.
    /// </summary>
    [Serializable]
    public class FacebookInsightsStateItem
    {

        #region "Properties"

        /// <summary>
        /// Object ID to which the state item belongs.
        /// </summary>
        [XmlElement("oid", IsNullable = false)]
        public string ObjectId
        {
            get;
            set;
        }


        /// <summary>
        /// The latest collected end time.
        /// </summary>
        [XmlElement("cet", IsNullable = true)]
        public DateTime? CollectedEndTime
        {
            get;
            set;
        }


        /// <summary>
        /// Till when the collection of this object ID's Insights can be skipped.
        /// </summary>
        [XmlElement("scu", IsNullable = true)]
        public DateTime? SkipCollectionUntil
        {
            get;
            set;
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Creates empty FacebookInsightsStateItem.
        /// </summary>
        public FacebookInsightsStateItem()
        {
        }


        /// <summary>
        /// Creates FacebookInsightsStateItem with all properties set to null except object ID.
        /// </summary>
        /// <param name="objectId">Object ID of the Insights object.</param>
        public FacebookInsightsStateItem(string objectId)
        {
            ObjectId = objectId;
        }


        /// <summary>
        /// Create FacebookInsightsStateItem with given properties set.
        /// </summary>
        /// <param name="objectId">Object ID of the Insights object.</param>
        /// <param name="collectedEndTime">Latest end time which has been retrieved from Facebook for this object's Insights.</param>
        /// <param name="skipCollectionUntil">Until when the collection of new Insights can be skipped.</param>
        public FacebookInsightsStateItem(string objectId, DateTime? collectedEndTime, DateTime? skipCollectionUntil)
        {
            ObjectId = objectId;
            CollectedEndTime = collectedEndTime;
            SkipCollectionUntil = skipCollectionUntil;
        }

        #endregion

    }
}
