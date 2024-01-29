using System;
using System.Web.UI;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Page element. Pass in either Page or Control.
    /// </summary>
    public class PageElement
    {
        #region "Properties"

        /// <summary>
        /// Page
        /// </summary>
        public Page Page
        {
            get;
            protected set;
        }


        /// <summary>
        /// Control type
        /// </summary>
        public Control Control
        {
            get;
            protected set;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Constructor
        /// </summary>
        protected PageElement()
        {
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="control">Control</param>
        public PageElement(Control control)
        {
            Control = control;
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="page">Page</param>
        public PageElement(Page page)
        {
            Page = page;
        }
        
        #endregion


        #region "Operators"

        /// <summary>
        /// Implicit conversion from Page to page element
        /// </summary>
        /// <param name="page">Page</param>
        public static implicit operator PageElement(Page page)
        {
            return new PageElement(page);
        }


        /// <summary>
        /// Implicit conversion from Control to page element
        /// </summary>
        /// <param name="control">Control</param>
        public static implicit operator PageElement(Control control)
        {
            return new PageElement(control);
        }

        #endregion
    }
}