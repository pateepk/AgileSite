using System;
using System.Text;
using System.Web.UI;


namespace CMS.Base.Web.UI
{
    /// <summary>
    /// HTML utility methods.
    /// </summary>
    public static class PageExtensions
    {
        /// <summary>
        /// Adds the given HTML code to the header of the page.
        /// </summary>
        /// <param name="page">Page</param>
        /// <param name="html">HTML to add</param>
        public static void AddToHeader(this Page page, string html)
        {
            if ((page != null && page.Header != null) && !String.IsNullOrEmpty(html))
            {
                // Add css to page header
                LiteralControl ltl = new LiteralControl(html) { EnableViewState = false };
                page.Header.Controls.Add(ltl);
            }
        }
    }
}
