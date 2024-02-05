using System.Xml.Linq;

using CMS.Helpers;

namespace CMS.UIControls.UniGridConfig
{
    /// <summary>
    /// UniGrid custom filter parameter.
    /// </summary>    
    public class FilterParameter : AbstractConfiguration
    {
        #region "Properties"

        /// <summary>
        /// Specifies the name of custom filter class property.
        /// </summary>
        public string Name
        {
            get;
            set;
        }


        /// <summary>
        /// Specifies the value for custom filter class property specified by Name
        /// </summary>
        public string Value
        {
            get;
            set;
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor.
        /// </summary>
        public FilterParameter()
        { }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="element">XML element with the column definition</param>
        public FilterParameter(XElement element)
            : this()
        {
            Name = element.GetAttributeStringValue("name");
            Value = element.GetAttributeStringValue("value");
        }

        #endregion
    }
}
