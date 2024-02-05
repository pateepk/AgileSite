using System.Web.UI.WebControls;

using CMS.Base;

namespace CMS.FormEngine.Web.UI
{
    /// <summary>
    /// Extension methods.
    /// </summary>
    public static class WebUIExtensions
    {
        /// <summary>
        /// Searches the collection for a System.Web.UI.WebControls.ListItem with a System.Web.UI.WebControls.ListItem.Value property that contains the specified value.
        /// </summary>
        /// <param name="collection">Given collection</param>
        /// <param name="value">Value to search for</param>
        /// <param name="ignoreCase">A System.Boolean indicating a case-sensitive or insensitive comparison. (true indicates a case-insensitive comparison.)</param>
        public static ListItem FindByValue(this ListItemCollection collection, string value, bool ignoreCase)
        {
            foreach (ListItem item in collection)
            {
                if (CMSString.Equals(value, item.Value, ignoreCase))
                {
                    return item;
                }
            }

            return null;
        }
    }
}
