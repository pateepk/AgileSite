using CMS.IO;
using System;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace CMS.SocialMarketing
{
    /// <summary>
    /// Represents data of FacebookInsightsCollectionTask.
    /// </summary>
    [XmlRoot("data")]
    [Serializable]
    public class FacebookInsightsCollectionTaskData
    {

        #region "Fields"

        private FacebookInsightsState mFacebookInsightsState = new FacebookInsightsState();

        #endregion


        #region "Public properties"

        /// <summary>
        /// Application ID to which the task belongs.
        /// </summary>
        [XmlElement("applicationId")]
        public int ApplicationId
        {
            get;
            set;
        }


        /// <summary>
        /// Insights state.
        /// </summary>
        [XmlElement("state")]
        public FacebookInsightsState FacebookInsightsState
        {
            get
            {
                return mFacebookInsightsState;
            }
            set
            {
                mFacebookInsightsState = value;
            }
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Gets XML string representation of given taks data object.
        /// </summary>
        /// <param name="data">Data to serialize.</param>
        /// <returns>XML string representation.</returns>
        public static string ToXmlString(FacebookInsightsCollectionTaskData data)
        {
            StringWriter taskData = null;
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(FacebookInsightsCollectionTaskData));
                taskData = new StringWriter();
                serializer.Serialize(taskData, data);

                return taskData.ToString();
            }
            finally
            {
                if (taskData != null)
                {
                    taskData.Close();
                }
            }
        }


        /// <summary>
        /// Creates task data object from XML string representation.
        /// </summary>
        /// <param name="xmlString">XML string representation.</param>
        /// <returns>Task data object.</returns>
        public static FacebookInsightsCollectionTaskData FromXmlString(string xmlString)
        {
            StringReader taskData = null;
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(FacebookInsightsCollectionTaskData));
                taskData = new StringReader(xmlString);
                
                return (FacebookInsightsCollectionTaskData)serializer.Deserialize(taskData);
            }
            finally
            {
                if (taskData != null)
                {
                    taskData.Close();
                }
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Creates empty task data.
        /// </summary>
        public FacebookInsightsCollectionTaskData()
        {
        }


        /// <summary>
        /// Creates task data with given application ID.
        /// </summary>
        /// <param name="applicationId">Application ID.</param>
        public FacebookInsightsCollectionTaskData(int applicationId)
        {
            ApplicationId = applicationId;
        }

        #endregion
    }
}
