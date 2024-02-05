using System;
using System.Linq;
using System.Xml;

using CMS.Helpers;

namespace CMS.DataEngine.Serialization
{
    /// <summary>
    /// Contains all information necessary to identify an object in database except for its ID. Pass it to the <see cref="TranslationHelper"/> to obtain the current ID.
    /// </summary>
    public class TranslationReference
    {
        #region "Properties"

        /// <summary>
        /// Foreign object type.
        /// </summary>
        public string ObjectType
        {
            get;
            set;
        }


        /// <summary>
        /// Foreign object code name.
        /// </summary>
        public string CodeName
        {
            get;
            set;
        }


        /// <summary>
        /// Foreign object GUID.
        /// </summary>
        public Guid GUID
        {
            get;
            set;
        }


        /// <summary>
        /// Foreign object site.
        /// </summary>
        public TranslationReference Site
        {
            get;
            set;
        }


        /// <summary>
        /// Foreign object parent translation reference.
        /// </summary>
        public TranslationReference Parent
        {
            get;
            set;
        }


        /// <summary>
        /// Foreign object group translation reference.
        /// </summary>
        public TranslationReference Group
        {
            get;
            set;
        }

        #endregion


        /// <summary>
        /// Constructor for <see cref="TranslationReferenceLoader"/>.
        /// </summary>
        internal TranslationReference()
        {
        }


        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        public override string ToString()
        {
            return TextHelper.MergeIfNotEmpty(
                "_",
                ObjectType,
                CodeName ?? (GUID == Guid.Empty ? null : GUID.ToString()),
                Parent,
                Site,
                Group);
        }


        /// <summary>
        /// Adds the translation nodes to the serialization
        /// </summary>
        /// <param name="element">Parent element for translation data</param>
        public void WriteToXmlElement(XmlNode element)
        {
            WriteTranslationNodeCodeName(element);

            WriteTranslationNodeGuid(element);

            WriteTranslationNodeObjectType(element);

            WriteTranslationNodeSiteName(element);

            WriteTranslationNodeParentId(element);

            WriteTranslationNodeGroupId(element);
        }


        /// <summary>
        /// Writes code name to the foreign key element
        /// </summary>
        /// <param name="element">Parent element for translation data (foreign key element)</param>
        private void WriteTranslationNodeCodeName(XmlNode element)
        {
            if (!String.IsNullOrEmpty(CodeName))
            {
                AppendSimpleNode("CodeName", CodeName, element);
            }
        }


        /// <summary>
        /// Writes GUID to the foreign key element
        /// </summary>
        /// <param name="element">Parent element for translation data (foreign key element)</param>
        private void WriteTranslationNodeGuid(XmlNode element)
        {
            if (GUID != default(Guid))
            {
                AppendSimpleNode("GUID", GUID.ToString(), element);
            }
        }


        /// <summary>
        /// Writes object type to the foreign key element
        /// </summary>
        /// <param name="element">Parent element for translation data (foreign key element)</param>
        private void WriteTranslationNodeObjectType(XmlNode element)
        {
            if (!String.IsNullOrEmpty(ObjectType))
            {
                AppendSimpleNode("ObjectType", ObjectType, element);
            }
        }


        /// <summary>
        /// Writes site name to the foreign key element
        /// </summary>
        /// <param name="element">Parent element for translation data (foreign key element)</param>
        private void WriteTranslationNodeSiteName(XmlNode element)
        {
            if (Site == null)
            {
                // No translation available
                return;
            }

            var siteElement = element.OwnerDocument.CreateElement("Site");
            element.AppendChild(siteElement);

            Site.WriteToXmlElement(siteElement);
        }


        /// <summary>
        /// Writes parent identifier to the foreign key element
        /// </summary>
        /// <param name="element">Parent element for translation data (foreign key element)</param>
        private void WriteTranslationNodeParentId(XmlNode element)
        {
            if (Parent == null)
            {
                // No translation available
                return;
            }

            var groupElement = element.OwnerDocument.CreateElement("Parent");
            element.AppendChild(groupElement);

            Parent.WriteToXmlElement(groupElement);
        }


        /// <summary>
        /// Writes group identifier to the foreign key element
        /// </summary>
        /// <param name="element">Parent element for translation data (foreign key element)</param>
        private void WriteTranslationNodeGroupId(XmlNode element)
        {
            if (Group == null)
            {
                // No translation available
                return;
            }

            var groupElement = element.OwnerDocument.CreateElement("Group");
            element.AppendChild(groupElement);

            Group.WriteToXmlElement(groupElement);
        }


        /// <summary>
        /// Appends a simple value node to the given node
        /// </summary>
        /// <param name="name">Node name</param>
        /// <param name="value">Value</param>
        /// <param name="parent">Parent node</param>
        private static void AppendSimpleNode(string name, string value, XmlNode parent)
        {
            if (String.IsNullOrEmpty(value))
            {
                return;
            }

            var element = parent.OwnerDocument.CreateElement(name);
            element.InnerText = value;

            parent.AppendChild(element);
        }
    }
}
