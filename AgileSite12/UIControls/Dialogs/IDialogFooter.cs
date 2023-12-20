using System;
using System.Linq;
using System.Text;
using System.Web.UI;


namespace CMS.UIControls
{
    /// <summary>
    /// DialogFooter
    /// </summary>
    public interface IDialogFooter
    {
        /// <summary>
        /// Hides cancel button
        /// </summary>
        void HideCancelButton();


        /// <summary>
        /// Makes sure a cancel button is rendered on the page.
        /// </summary>
        void ShowCancelButton();


        /// <summary>
        /// Adds a control to the footer.
        /// </summary>
        /// <param name="control">Control to be added.</param>
        void AddControl(Control control);
    }
}