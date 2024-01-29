using System;
using System.ComponentModel;
using System.Web.UI;


namespace CMS.DocumentEngine.Web.UI
{
    /// <summary>
    /// CMSBaseProperties class.
    /// </summary>
    [ToolboxItem(false)]
    public sealed class CMSBaseProperties : CMSAbstractBaseProperties, ICMSBaseProperties
    {
        #region "Constructor"

        /// <summary>
        /// BaseProperties constructor.
        /// </summary>
        public CMSBaseProperties()
        {
        }


        /// <summary>
        /// Base properties constructor.
        /// </summary>
        /// <param name="tag">Writer tag</param>
        public CMSBaseProperties(HtmlTextWriterTag tag) : base(tag)
        {
        }

        #endregion
    }
}