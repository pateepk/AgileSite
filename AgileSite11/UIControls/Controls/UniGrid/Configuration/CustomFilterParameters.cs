using System.Collections.Generic;
using System.Web.UI;
using System.Xml.Linq;

using CMS.Helpers;

namespace CMS.UIControls.UniGridConfig
{
    /// <summary>
    /// UniGrid filter parameters definition.
    /// </summary>
    [ParseChildren(typeof(FilterParameter), DefaultProperty = "Parameters", ChildrenAsProperties = true)]
    public class CustomFilterParameters
    {
        #region "Properties"

        /// <summary>
        /// List of filter parameters.
        /// </summary>
        public List<FilterParameter> Parameters
        {
            get;
            set;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Empty constructor.
        /// </summary>
        public CustomFilterParameters()
        {
            Parameters = new List<FilterParameter>();
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="element">Custom filter parameters XML element</param>
        public CustomFilterParameters(XElement element)
            : this()
        {
            foreach (var child in element.GetElements("filterparameter"))
            {
                Parameters.Add(new FilterParameter(child));
            }
        }

        #endregion
    }
}
