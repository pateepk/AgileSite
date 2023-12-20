using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

using CMS.IO;


namespace CMS.SocialMarketing
{
    /// <summary>
    /// Represents data of LinkedInInsightsCollectionTask.
    /// </summary>
    [XmlRoot("data")]
    [Serializable]
    public class LinkedInInsightsCollectionTaskData
    {
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
        /// List of insights collection states of accounts within application.
        /// </summary>
        [XmlArray("accounts")]
        [XmlArrayItem("account")]
        public List<LinkedInAccountInsightsState> AccountInsights
        {
            get;
            set;
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Gets XML string representation of given taks data object.
        /// </summary>
        /// <param name="data">Data to serialize.</param>
        /// <returns>XML string representation.</returns>
        public static string ToXmlString(LinkedInInsightsCollectionTaskData data)
        {
            StringWriter taskData = null;
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof (LinkedInInsightsCollectionTaskData));
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
        public static LinkedInInsightsCollectionTaskData FromXmlString(string xmlString)
        {
            StringReader taskData = null;
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(LinkedInInsightsCollectionTaskData));
                taskData = new StringReader(xmlString);

                return (LinkedInInsightsCollectionTaskData)serializer.Deserialize(taskData);
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
        public LinkedInInsightsCollectionTaskData()
        {
        }


        /// <summary>
        /// Creates task data with given application ID.
        /// </summary>
        /// <param name="applicationId">Application ID.</param>
        public LinkedInInsightsCollectionTaskData(int applicationId)
        {
            ApplicationId = applicationId;
        }

        #endregion
    }
}
