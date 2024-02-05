using System;
using System.ComponentModel;
using System.Web.UI;

namespace CMS.DocumentEngine.Web.UI
{
    /// <summary>
    /// Base menu controls properties.
    /// </summary>
    [ToolboxItem(false)]
    public sealed class CMSMenuProperties : CMSAbstractMenuProperties, ICMSMenuProperties
    {
        #region "Constructors"

        /// <summary>
        /// Menu properties constructor.
        /// </summary>
        public CMSMenuProperties()
        {
        }


        /// <summary>
        /// Menu properties constructor.
        /// </summary>
        /// <param name="tag">Writer tag</param>
        public CMSMenuProperties(HtmlTextWriterTag tag)
            : base(tag)
        {
        }

        #endregion
    }
}