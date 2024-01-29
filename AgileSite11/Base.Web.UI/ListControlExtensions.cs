using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI.WebControls;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Extension methods for list controls.
    /// </summary>
    public static class ListControlExtensions
    {
        /// <summary>
        /// Returns true when at least one item is selected.
        /// </summary>
        /// <param name="control">Control</param>
        public static bool HasValue(this ListControl control)
        {
            return !string.IsNullOrEmpty(control.SelectedValue);
        }


        /// <summary>
        /// Returns all selected items.
        /// </summary>
        /// <param name="control">Control</param>
        public static IEnumerable<ListItem> GetSelectedItems(this ListControl control)
        {
            return control.Items.OfType<ListItem>().Where(i => i.Selected);
        }


        /// <summary>
        /// Sorts list items alphabetically.
        /// </summary>
        /// <param name="list">List control</param>
        public static void SortItems(this ListControl list)
        {
            List<ListItem> listCopy = new List<ListItem>();
            foreach (ListItem li in list.Items)
            {
                listCopy.Add(li);
            }

            list.Items.Clear();

            foreach (ListItem li in listCopy.OrderBy(item => item.Text))
            {
                list.Items.Add(li);
            }
        }
    }
}
