using System;
using System.ComponentModel;
using System.Web.UI;

namespace CMS.DocumentEngine.Web.UI
{
    /// <summary>
    /// Base controls properties.
    /// </summary>
    [ToolboxItem(false)]
    public sealed class CMSControlProperties : CMSAbstractControlProperties, ICMSControlProperties
    {
        #region "Constructors"

        /// <summary>
        /// ControlProperties constructor.
        /// </summary>
        public CMSControlProperties()
        {
        }


        /// <summary>
        /// Control properties constructor.
        /// </summary>
        /// <param name="tag">Writer tag</param>
        public CMSControlProperties(HtmlTextWriterTag tag)
            : base(tag)
        {
        }

        #endregion
    }
}