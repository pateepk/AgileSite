using System;
using System.ComponentModel;
using System.Web.UI;

using CMS.Base.Web.UI;

namespace CMS.PortalEngine.Web.UI
{
    /// <summary>
    /// Frames manager class which ensures communication between frames in a modal dialog.
    /// </summary>
    [ToolboxItem(false)]
    public class WebPartFramesManager : Control, IPostBackEventHandler
    {
        #region "Event handler"

        /// <summary>
        /// Saves the object.
        /// </summary>
        public delegate bool SaveEventHandler(object sender, EventArgs e);


        /// <summary>
        /// Raised when the Save action is required.
        /// </summary>
        public event SaveEventHandler OnSave;


        /// <summary>
        /// Raised when the Apply action is required.
        /// </summary>
        public event SaveEventHandler OnApply;

        #endregion


        #region "Public methods"

        /// <summary>
        /// Gets the OK script.
        /// </summary>
        public string GetOKScript()
        {
            return "SendEvent('ok', (window.GetRefreshStatus != null) ? GetRefreshStatus() : true); return false;";
        }

        /// <summary>
        /// Gets the cancel script.
        /// </summary>
        public string GetCancelScript()
        {
            return "SendEvent('close'); return false;";
        }

        /// <summary>
        /// Gets the apply script.
        /// </summary>
        public string GetApplyScript()
        {
            return "SendEvent('apply', (window.GetRefreshStatus != null) ? GetRefreshStatus() : true); return false;";
        }

        #endregion


        #region "Page methods"

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // Register the event sending/receiving scripts. (Allows to send events between frames)
            ScriptHelper.RegisterScriptFile(Page, "DesignMode/WebPartProperties.js");
        }


        /// <summary>
        /// PreRender event handler
        /// </summary>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            // Register the "Ok", "Apply" events which are fired by the footer buttons (OK, Apply).
            if (OnSave != null)
            {
                RegisterOnSaveHandlerScripts();
            }
        }


        /// <summary>
        /// Registers the on save event which is fired by the footer buttons (OK, Apply). It is necessary to override the OnSave method to ensure the save action.
        /// </summary>
        private void RegisterOnSaveHandlerScripts()
        {
            // Trigger the OnClick events when the buttons work as event receivers (they are not visible)
            string script = @"
                var okButtonProcessed = false;
                function OnOkHandler() { if (okButtonProcessed) { return false; } okButtonProcessed = true; " + ControlsHelper.GetPostBackEventReference(this, "ok") + @"; }
                function OnApplyHandler() { " + ControlsHelper.GetPostBackEventReference(this, "apply") + ";} ";

            ScriptHelper.RegisterStartupScript(this, typeof(string), "SaveScript", script, true);
        }

        #endregion


        #region IPostBackEventHandler Members

        /// <summary>
        /// Processes an event raised when a form is posted to the server.
        /// </summary>
        public void RaisePostBackEvent(string eventArgument)
        {
            bool applyButtonPressed = (eventArgument == "apply");
            bool okButtonPressed = (eventArgument == "ok");

            if (okButtonPressed || applyButtonPressed)
            {
                // OK button was pressed
                if (okButtonPressed)
                {
                    // Raise the OnSave event 
                    if ((OnSave != null) && OnSave(this, null))
                    {
                        // Close dialog when the save action was successful
                        ScriptHelper.RegisterStartupScript(this, typeof(string), "CloseDialogScript", "SendEvent('close');", true);
                    }
                }
                // Raise OnApply event
                else
                {
                    // Raise the OnSave event 
                    if (OnApply != null)
                    {
                        // Raise the OnApply event 
                        OnApply(this, null);
                    }
                    else if (OnSave != null)
                    {
                        // Raise the OnSave event when OnApply is not handled
                        OnSave(this, null);
                    }
                }
            }
        }

        #endregion
    }
}
