using System;
using System.Xml;

using CMS.Helpers;
using CMS.WebAnalytics.Internal;

namespace CMS.OnlineMarketing
{
    /// <summary>
    /// Represents single conversion setting for an A/B test.
    /// </summary>
    [Serializable]
    public class ABTestConversion
    {
        private const string CONVERSION_NODE = "conversion";
        private const string NAME_ATTR = "name";
        private const string VALUE_ATTR = "value";
        private const string ITEM_ATTR = "item";
        private const string ITEMNAME_ATTR = "itemname";


        /// <summary>
        /// Code name of the conversion which is used for logging.
        /// </summary>
        public string ConversionName
        {
            get;
            set;
        }


        /// <summary>
        /// Numeric value for conversion of type specified in <see cref="ConversionName"/>.
        /// </summary>
        public decimal Value
        {
            get;
            set;
        }


        /// <summary>
        /// Identifier of an item related to the conversion, e.g. newsletter name related to newsletter subscription conversion.
        /// </summary>
        public string RelatedItemIdentifier
        {
            get;
            set;
        }


        /// <summary>
        /// Display name of an item related to the conversion, e.g. newsletter display name related to newsletter subscription conversion.
        /// </summary>
        public string RelatedItemDisplayName
        {
            get;
            set;
        }


        /// <summary>
        /// Original name from conversion definition (<see cref="ABTestConversionDefinition.ConversionName"/>).
        /// </summary>
        public string ConversionOriginalName
        {
            get;
            private set;
        }


        /// <summary>
        /// Initializes a new instance of <see cref="ABTestConversion"/> class.
        /// </summary>
        /// <param name="conversionName">Code name of new conversion</param>
        /// <param name="relatedItemIdentifier">Identifier of related item</param>
        /// <param name="value">Value of new conversion</param>
        public ABTestConversion(string conversionName, string relatedItemIdentifier = null, decimal value = 0)
        {
            ConversionOriginalName = conversionName;
            ConversionName = ABTestConversionHelper.GetConversionFullName(conversionName, relatedItemIdentifier);
            RelatedItemIdentifier = relatedItemIdentifier;
            Value = value;
        }


        /// <summary>
        /// Initializes a new instance of <see cref="ABTestConversion"/> class from its XML representation.
        /// </summary>
        /// <param name="node">XML representation of <see cref="ABTestConversion"/> object</param>
        internal ABTestConversion(XmlNode node)
        {
            if (node == null || !String.Equals(node.Name, CONVERSION_NODE, StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("Parameter is null or is not compatible with the object.");
            }

            LoadFromXml(node);
        }


        /// <summary>
        /// Returns XML node representing the conversion setting.
        /// </summary>
        /// <param name="doc">XML document</param>
        internal XmlNode GetXMLNode(XmlDocument doc)
        {
            var node = doc.CreateElement(CONVERSION_NODE);

            node.SetAttribute(NAME_ATTR, ConversionName);
            node.SetAttribute(VALUE_ATTR, ValidationHelper.GetString(Value, "0", CultureHelper.EnglishCulture));

            if (!String.IsNullOrEmpty(RelatedItemIdentifier))
            {
                node.SetAttribute(ITEM_ATTR, RelatedItemIdentifier);
            }
            if (!String.IsNullOrEmpty(RelatedItemDisplayName))
            {
                node.SetAttribute(ITEMNAME_ATTR, RelatedItemDisplayName);
            }

            return node;
        }


        /// <summary>
        /// Loads conversion setting from XML definition.
        /// </summary>
        /// <param name="node">Conversion setting XML node</param>
        private void LoadFromXml(XmlNode node)
        {
            if (node != null)
            {
                ConversionName = XmlHelper.GetAttributeValue(node, NAME_ATTR);
                Value = ValidationHelper.GetDecimalSystem(XmlHelper.GetAttributeValue(node, VALUE_ATTR, "0"), 0m);
                RelatedItemIdentifier = XmlHelper.GetAttributeValue(node, ITEM_ATTR);
                RelatedItemDisplayName = XmlHelper.GetAttributeValue(node, ITEMNAME_ATTR);
                ConversionOriginalName = ABTestConversionHelper.GetConversionOriginalName(ConversionName);
            }
        }
    }
}
