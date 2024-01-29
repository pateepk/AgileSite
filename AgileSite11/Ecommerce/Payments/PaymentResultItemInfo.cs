using System.Xml;

using CMS.Helpers;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Class representing payment result item (= one payment result xml node).
    /// </summary>
    public class PaymentResultItemInfo
    {
        /// <summary>
        /// Payment result item header text.
        /// </summary>
        public string Header
        {
            get;
            set;
        } = "";


        /// <summary>
        /// Payment result item code name.
        /// </summary>
        public string Name
        {
            get;
            set;
        } = "";


        /// <summary>
        /// Payment result item value - inner interpretation.
        /// </summary>
        public string Value
        {
            get;
            set;
        } = "";


        /// <summary>
        /// Payment result item value text interpretation - it is visible to user.
        /// </summary>
        public string Text
        {
            get;
            set;
        } = "";


        /// <summary>
        /// Creates payment result item from Xml node.
        /// </summary>
        public PaymentResultItemInfo(XmlNode node)
            : this()
        {
            if (node?.Attributes != null)
            {
                Header = XmlHelper.GetXmlAttributeValue(node.Attributes["header"], "");
                Name = XmlHelper.GetXmlAttributeValue(node.Attributes["name"], "");
                Value = XmlHelper.GetXmlAttributeValue(node.Attributes["value"], "");
                Text = XmlHelper.GetXmlAttributeValue(node.Attributes["text"], "");
            }
        }


        /// <summary>
        /// Creates empty payment result item.
        /// </summary>
        public PaymentResultItemInfo()
        {
        }
    }
}