using System;
using System.Xml;

namespace CMS.ContactManagement
{
    /// <summary>
    /// RuleItem class.
    /// </summary>
    internal abstract class RuleItem
    {
        #region "Methods"

        /// <summary>
        /// Clones current object and returns copy of it.
        /// </summary>
        /// <returns>Returns clone of either RuleAttributeInfo or RuleActivityInfo depending on current object</returns>
        public abstract RuleItem Clone();


        /// <summary>
        /// Returns XML representation of current object.
        /// </summary>
        /// <param name="doc">XML document with other items</param>
        /// <returns>Returns XmlNode</returns>
        public abstract XmlNode GetXmlNode(XmlDocument doc);

        #endregion
    }
}