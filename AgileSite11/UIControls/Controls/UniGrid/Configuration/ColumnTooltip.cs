using System.ComponentModel;
using System.Xml.Linq;

using CMS.Helpers;

namespace CMS.UIControls.UniGridConfig
{
    /// <summary>
    /// Column tooltip settings.
    /// </summary>
    public class ColumnTooltip : Component
    {
        #region "Properties"

        /// <summary>
        /// Indicates whether the output of the tooltip should be encoded.
        /// </summary>
        public bool Encode
        {
            get;
            set;
        }


        /// <summary>
        /// Name used in the OnExternalDataBound event for changing the appearance of the tooltip. This can be used to create complex tooltips including images, panels etc.
        /// </summary>
        public string ExternalSourceName
        {
            get;
            set;
        }


        /// <summary>
        /// Name of the column from the data source of the UniGrid that is used as the source of the tooltip.
        /// </summary>
        public string Source
        {
            get;
            set;
        }


        /// <summary>
        /// Determines the width of the tooltip.
        /// </summary>
        public string Width
        {
            get;
            set;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Empty constructor.
        /// </summary>
        public ColumnTooltip()
        {
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="element">Tooltip XML element</param>
        public ColumnTooltip(XElement element)
            : this()
        {
            Source = element.GetAttributeStringValue("source");
            ExternalSourceName = element.GetAttributeStringValue("externalsourcename");
            Width = element.GetAttributeStringValue("width");
            Encode = element.GetAttributeValue("encode", true);
        }

        #endregion
    }
}