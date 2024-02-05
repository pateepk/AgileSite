using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace CMS.SocialMarketing
{
    /// <summary>
    /// Represents state of Facebook Insights collection.
    /// </summary>
    [Serializable]
    public class FacebookInsightsState
    {

        #region "Fields"

        private List<FacebookInsightsStateItem> mStateItemArrayList = new List<FacebookInsightsStateItem>();

        #endregion


        #region "Public properties"

        /// <summary>
        /// Contains FacebookInsightsStateItem for purpose of serialization.
        /// </summary>
        [XmlArray("insights")]
        [XmlArrayItem("i")]
        public List<FacebookInsightsStateItem> StateItemArrayList
        {
            get
            {
                return mStateItemArrayList;
            }
            set
            {
                mStateItemArrayList = value;
            }
        }

        #endregion


        #region "Public Methods"

        /// <summary>
        /// Tells you whether FacebookInsightsState contains item with given object ID.
        /// </summary>
        /// <param name="objectId">Object ID of FacebookinsightsStateItem.</param>
        /// <returns>True if objectId is contained, false otherwise.</returns>
        public bool ContainsStateItem(string objectId)
        {
            return (StateItemArrayList.Count(i => i.ObjectId == objectId) > 0);
        }


        /// <summary>
        /// Gets FacebookInsightsStateItem with given object ID. Creates one, if not exists.
        /// </summary>
        /// <param name="objectId">Object ID of FacebookinsightsStateItem to get.</param>
        /// <returns>FacebookinsightsStateItem with given object ID.</returns>
        public FacebookInsightsStateItem GetOrCreateStateItem(string objectId)
        {
            FacebookInsightsStateItem stateItem = StateItemArrayList.Where(i => i.ObjectId == objectId).FirstOrDefault();
            if (stateItem == null)
            {
                stateItem = new FacebookInsightsStateItem(objectId);
                StateItemArrayList.Add(stateItem);
            }
            return stateItem;
        }

        #endregion


    }
}
