using System;
using System.Web.UI.WebControls;

namespace CMS.UIControls
{
    /// <summary>
    /// Interface for modal master pages.
    /// </summary>
    public interface ICMSModalMasterPage
    {
        /// <summary>
        /// Fired when 'save and close' button is clicked and the content should be saved. Pages that use this master page should add handler to this event
        /// alike binding to <see cref="Button.OnClick"/> event.
        /// </summary>
        event EventHandler Save;


        /// <summary>
        /// Sets JavaScript to a Save and Close button.
        /// </summary>
        /// <param name="javaScript">JavaScript to add to the Save and Close button</param>
        void SetSaveJavaScript(string javaScript);


        /// <summary>
        /// Sets JavaScript to be processed when user clicks the Close button or the grey area around the modal window.
        /// </summary>
        /// <param name="javaScript">JavaScript to process when user clicks the Close button or the grey area around the modal window</param>
        void SetCloseJavaScript(string javaScript);


        /// <summary>
        /// Sets Save and Close button resource string.
        /// </summary>
        /// <param name="resourceString">Resource string</param>
        void SetSaveResourceString(string resourceString);


        /// <summary>
        /// Shows generic save and close button in the modal dialog.
        /// </summary>
        void ShowSaveAndCloseButton();
    }
}