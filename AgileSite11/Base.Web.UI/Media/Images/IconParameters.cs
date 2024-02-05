using System;
using System.Linq;
using System.Text;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Container class used in situations when you need to display icon
    /// (document type, mime type, ...) and it is possible to display it
    /// as image or font icon based on data and circumstances.
    /// </summary>
    public class IconParameters
    {
        #region "Image properties"

        /// <summary>
        /// Image URL.
        /// </summary>
        public string Url
        {
            get;
            set;
        }

        #endregion


        #region "Font icon properties"

        /// <summary>
        /// Font icon CSS class.
        /// </summary>
        public string IconClass
        {
            get;
            set;
        }


        /// <summary>
        /// Font icon size.
        /// </summary>
        public FontIconSizeEnum IconSize
        {
            get;
            set;
        }

        #endregion
    }
}
