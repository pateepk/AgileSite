using System;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Interface for controls which open some of the CMS dialogs (Insert image or media, Insert link, Insert URL etc.).
    /// </summary>
    public interface IDialogControl
    {
        /// <summary>
        /// Configuration of the dialog which is opened by the control.
        /// </summary>
        DialogConfiguration DialogConfig
        {
            get;
            set;
        }
    }
}