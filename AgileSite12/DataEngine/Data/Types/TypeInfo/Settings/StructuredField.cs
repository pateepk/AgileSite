using System;
using System.Linq;
using System.Text;
using System.Xml;

using CMS.Core;
using CMS.DataEngine.Serialization;

namespace CMS.DataEngine
{
    /// <summary>
    /// Definition of the structured field configuration
    /// </summary>
    /// <typeparam name="TImplementationType">Implementation type, must implement IStructuredData</typeparam>
    public class StructuredField<TImplementationType> : IStructuredField
        where TImplementationType : IStructuredData
    {
        /// <summary>
        /// Info field name
        /// </summary>
        public string FieldName
        {
            get;
            private set;
        }


        /// <summary>
        /// Implementation type
        /// </summary>
        public Type ImplementationType
        {
            get;
            private set;
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Info field name</param>
        public StructuredField(string name)
        {
            if (String.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Field name cannot be empty", "name");
            }

            FieldName = name;
            ImplementationType = typeof(TImplementationType);
        }


        /// <summary>
        /// Creates the object from the given XML value
        /// </summary>
        public IStructuredData CreateStructuredValue(string xmlValue)
        {
            if (String.IsNullOrEmpty(xmlValue))
            {
                return null;
            }

            var data = (IStructuredData)ObjectFactory.New(ImplementationType);

            // Load the XML to the object
            var doc = new XmlDocument();
            doc.LoadXml(xmlValue);

            data.LoadFromXmlElement(doc.DocumentElement);
            
            return data;
        }
    }


    /// <summary>
    /// Definition of the structured field for simple xml values with fixed order of xml elements. 
    /// For advanced fields use generic variant. 
    /// </summary>
    public class StructuredField : StructuredField<StructuredData>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Info field name</param>
        public StructuredField(string name)
            : base (name)
        {
        }
    }
}
