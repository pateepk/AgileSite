using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

using CMS.Helpers;

namespace CMS.BannerManagement
{
    /// <summary>
    /// Helper class for Banner management.
    /// </summary>
    public static class BannerManagementHelper
    {
        #region "Public methods"

        /*
        <image>
            <src>cesta</src>
            <title>cesta</title>
            <alt>cesta</alt>
            <class>cesta</class>
            <style>cesta</style>
        </image>
        */


        /// <summary>
        /// Serializes data from BannerImageAttributes and returns XmlDocument.
        /// <param name="bannerImg">Banner image attributes</param>
        /// </summary>
        public static XmlDocument SerializeBannerImageAttributes(BannerImageAttributes bannerImg)
        {
            XmlDocument doc = new XmlDocument();
            XmlElement xmlRoot = doc.CreateElement("image");
            doc.AppendChild(xmlRoot);

            Dictionary<string, object> properties = new Dictionary<string, object>
            {
                {"src", bannerImg.Src},
                {"title", bannerImg.Title},
                {"alt", bannerImg.Alt},
                {"class", bannerImg.Class},
                {"style", bannerImg.Style}
            };

            xmlRoot.AddChildElements(properties, removeEmptyEntries: false);

            return doc;
        }


        /// <summary>
        /// Serializes data from BannerImageAttributes and returns XML as a string.
        /// </summary>
        /// <param name="bannerImg">Banner image attributes</param>
        /// <returns>Banner image attributes serialized in XML.</returns>
        public static string SerializeBannerImageAttributesToString(BannerImageAttributes bannerImg)
        {
            return SerializeBannerImageAttributes(bannerImg)
                    .ToFormattedXmlString(true);
        }


        /// <summary>
        /// Deserializes the passed XML in string and returns BannerImageAttributes.
        /// </summary>
        /// <param name="xmlString">XML</param>
        public static BannerImageAttributes DeserializeBannerImageAttributes(string xmlString)
        {
#pragma warning disable BH1014 // Do not use System.IO
            using (var stringReader = new StringReader(xmlString))
#pragma warning restore BH1014 // Do not use System.IO
            {
                using (var xmlReader = XmlReader.Create(stringReader, new XmlReaderSettings()))
                {
                    XmlDocument doc = new XmlDocument();
                    try
                    {
                        doc.Load(xmlReader);

                        return DeserializeBannerImageAttributes(doc);
                    }
                    catch
                    {
                        return new BannerImageAttributes();
                    }
                }
            }
        }


        /// <summary>
        /// Deserializes the passed XmlDocument and returns BannerImageAttributes.
        /// </summary>
        /// <param name="xmlDocument">XML document with banner image</param>
        public static BannerImageAttributes DeserializeBannerImageAttributes(XmlDocument xmlDocument)
        {
            try
            {
                BannerImageAttributes bannerImg = new BannerImageAttributes();

                XmlElement rootElement = xmlDocument.DocumentElement;

                bannerImg.Src = rootElement["src"].InnerText;
                bannerImg.Title = rootElement["title"].InnerText;
                bannerImg.Alt = rootElement["alt"].InnerText;
                bannerImg.Class = rootElement["class"].InnerText;
                bannerImg.Style = rootElement["style"].InnerText;

                return bannerImg;
            }
            catch (Exception)
            {
                return new BannerImageAttributes();
            }
        }

        #endregion
    }
}
