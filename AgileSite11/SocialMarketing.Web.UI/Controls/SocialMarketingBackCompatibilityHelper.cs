using System;
using System.Xml.Serialization;

using CMS.Helpers;
using CMS.IO;


namespace CMS.SocialMarketing.Web.UI
{
    /// <summary>
    /// Helper methods for serializing and deserializing data used for social network posting - it is used for backward compatibility only.
    /// </summary>
    public static class SocialMarketingBackCompatibilityHelper
    {

        #region "Inner structures"

        /// <summary>
        /// Carries data needed for publishing info about documents on social network. It is used for backward compatibility only.
        /// </summary>
        [Serializable]
        [XmlRoot("socialAutoPostElement")]
        public struct SocialMarketingPublishingElement
        {
            /// <summary>
            /// Type of social network
            /// </summary>
            [XmlElement("socialNetworkType")]
            public SocialNetworkTypeEnum SocialNetworkType;


            /// <summary>
            /// Template
            /// </summary>
            [XmlElement("template")]
            public string Template;


            /// <summary>
            /// Indicates whether item should be automatically sent to social network after publishing.
            /// </summary>
            [XmlElement("autoPostAfterPublishing")]
            public string AutoPostAfterPublishingString;


            /// <summary>
            /// Indicates whether item should be automatically sent to social network after publishing.
            /// </summary>
            public bool AutoPostAfterPublishing
            {
                get
                {
                    return ValidationHelper.GetBoolean(AutoPostAfterPublishingString, false);
                }
                set
                {
                    AutoPostAfterPublishingString = value.ToString();
                }
            }


            /// <summary>
            /// Indicates whether item is already published.
            /// </summary>
            [XmlElement("isPublished")]
            public string IsPublishedString;


            /// <summary>
            /// Indicates whether item is already published.
            /// </summary>
            public bool IsPublished
            {
                get
                {
                    return ValidationHelper.GetBoolean(IsPublishedString, false);
                }
                set
                {
                    IsPublishedString = value.ToString();
                }
            }


            /// <summary>
            /// Url to item posted on desired social networking service.
            /// </summary>
            [XmlElement("postURL")]
            public string PostURL;
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Deserializes the passed string with XML into SocialMarketingPublishingElement and returns it. 
        /// Throws exception if it is not possible to deserialize publishing element from given XML.
        /// </summary>
        /// <param name="serializedElement">String with SocialMarketingPublishingElement serialized to XML.</param>
        /// <exception cref="Exception">When it is not possible to deserialize publishing element from given XML.</exception>
        /// <returns>Deserialized SocialMarketingPublishingElement.</returns>
        public static SocialMarketingPublishingElement DeserializePublishingElement(string serializedElement)
        {
            SocialMarketingPublishingElement elemenet;

            XmlSerializer serializer = new XmlSerializer(typeof(SocialMarketingPublishingElement));
            using (var reader = new StringReader(serializedElement))
            {
                elemenet = (SocialMarketingPublishingElement)serializer.Deserialize(reader);
            }

            return elemenet;
        }

        #endregion

    }
}
