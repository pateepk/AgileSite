using System;
using System.Web.UI.WebControls;

using CMS.Base;

namespace CMS.UIControls
{
    /// <summary>
    /// Base page for the design pages.
    /// </summary>
    public abstract class CMSModalDesignPage : CMSDesignPage
    {
        /// <summary>
        /// Fired when 'save and close' button is clicked and the content should be saved. Pages that use this event should use MasterPage that implements
        /// from <see cref="ICMSModalMasterPage"/> interface, which contains this event.
        /// This event is similar to <see cref="Button.OnClick"/> event.
        /// <remarks>
        /// Adding a handler to this event will also display 'Save and Close' button at the bottom of the page.
        /// </remarks>
        /// </summary>
        public event EventHandler Save
        {
            add
            {
                var master = Master as ICMSModalMasterPage;
                if (master == null)
                {
                    throw new InvalidOperationException("Binding to Save event is possible only when using Master Page that inherits from ICMSModalMasterPage");
                }
                master.ShowSaveAndCloseButton();
                master.Save += value;

            }
            remove
            {
                var master = Master as ICMSModalMasterPage;
                if (master == null)
                {
                    throw new InvalidOperationException("Binding to Save event is possible only when using Master Page that inherits from ICMSModalMasterPage");
                }
                master.Save -= value;
            }
        }


        /// <summary>
        /// Constructors
        /// </summary>
        protected CMSModalDesignPage()
        {
            Load += CMSModalDesignPage_Load;
        }


        /// <summary>
        /// Page load event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void CMSModalDesignPage_Load(object sender, EventArgs e)
        {
            RedirectToSecured();
            RegisterEscScript();
        }


        /// <summary>
        /// PreRender event handler
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            RegisterModalPageScripts();
        }


        /// <summary>
        /// Sets JavaScript to a Save and Close button. This method can only be called when master page implements <see cref="ICMSModalMasterPage"/>.
        /// </summary>
        /// <param name="javaScript">JavaScript to add to the Save and Close button</param>
        /// <exception cref="InvalidOperationException">Used master page is not <see cref="ICMSModalMasterPage"/></exception>
        /// <remarks>
        /// By using this method, the master page will also display 'Save and Close' button in the bottom of the page.
        /// </remarks>
        protected void SetSaveJavascript(string javaScript)
        {
            var master = Master as ICMSModalMasterPage;
            if (master == null)
            {
                throw new InvalidOperationException("To use this method, used master page must implement ICMSModalMasterPage interface.");
            }
            master.ShowSaveAndCloseButton();
            master.SetSaveJavaScript(javaScript);
        }


        /// <summary>
        /// Sets JavaScript to be processed when user clicks the Close button or the area around the modal window.
        /// This method can only be called when master page implements <see cref="ICMSModalMasterPage"/>.
        /// </summary>
        /// <param name="javaScript">JavaScript to be processed when user clicks the Close button or the area around the modal window</param>
        /// <exception cref="InvalidOperationException">Used master page is not <see cref="ICMSModalMasterPage"/></exception>
        protected void SetCloseJavascript(string javaScript)
        {
            var master = Master as ICMSModalMasterPage;
            if (master == null)
            {
                throw new InvalidOperationException("To use this method, used master page must implement ICMSModalMasterPage interface.");
            }
            master.SetCloseJavaScript(javaScript);
        }


        /// <summary>
        /// Sets Save and Close button resource string.
        /// </summary>
        /// <param name="resourceString">Resource string</param>
        /// <exception cref="InvalidOperationException">Used master page is not <see cref="ICMSModalMasterPage"/></exception>
        protected void SetSaveResourceString(string resourceString)
        {
            var master = Master as ICMSModalMasterPage;
            if (master == null)
            {
                throw new InvalidOperationException("To use this method, used master page must implement ICMSModalMasterPage interface.");
            }
            master.SetSaveResourceString(resourceString);
        }
    }
}