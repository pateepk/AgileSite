using System;
using System.Linq;
using System.Text;
using System.Web.UI;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Extensible edit menu interface
    /// </summary>
    public interface IExtensibleEditMenu
    {
        /// <summary>
        /// Adds additional control to pnlAdditionalControls panel that shows this control in right part of panel.
        /// </summary>
        /// <param name="control">Control to be added</param>
        /// <exception cref="ArgumentNullException"><paramref name="control"/> is null</exception>
        void AddAdditionalControl(Control control);
    }
}
