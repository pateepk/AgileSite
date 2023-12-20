using System;
using System.Web.UI.WebControls;

namespace CMS.UIControls
{
    /// <summary>
    /// Summary description for CMSModalPage.
    /// </summary>
    public abstract class CMSModalPage : CMSDeskPage
    {
        #region "Variables"

        private bool mRegisterESCScript = true;

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets or sets the value which determines whether the ESC script for closing a window is registered.
        /// </summary>
        public bool RegisterESCScript
        {
            get
            {
                return mRegisterESCScript;
            }
            set
            {
                mRegisterESCScript = value;
            }
        }


        /// <summary>
        /// Fired when 'Save and Close' button is clicked and the content should be saved. Pages that use this event should use MasterPage that implements
        /// from <see cref="ICMSModalMasterPage"/> interface, which contains this event.
        /// This event is similar to <see cref="Button.OnClick"/> event.
        /// </summary>
        /// <remarks>
        /// Adding a handler to this event will also display 'Save and Close' button at the bottom of the page.
        /// </remarks>
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

        #endregion


        #region "Methods"

        /// <summary>
        /// Constructor
        /// </summary>
        protected CMSModalPage()
        {
            Init += CMSModalPage_Init;
            IsDialog = true;
        }


        /// <summary>
        /// Init event handler
        /// </summary>
        private void CMSModalPage_Init(object sender, EventArgs e)
        {
            RequireSite = false;
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
                throw new InvalidOperationException("[CMSModalPage.SetSaveResourceString]: To use this method, used master page must implement ICMSModalMasterPage interface.");
            }
            master.SetSaveResourceString(resourceString);
        }


        /// <summary>
        /// PreRender event handler
        /// </summary>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            if (RegisterESCScript)
            {
                RegisterEscScript();
            }
            RegisterModalPageScripts();
        }

        #endregion
    }
}