using System;
using System.Web.UI;

using CMS.Core;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Control extender
    /// </summary>
    public abstract class ControlExtender
    {
        /// <summary>
        /// Gets the extended control.
        /// </summary>
        public Control Control
        {
            get;
            private set;
        }


        /// <summary>
        /// Initializes the extender using the specified control.
        /// </summary>
        /// <param name="control">Control to be extended</param>
        /// <param name="runOnInit">Indicates if OnInit method will be called besides basic initialization.</param>
        public virtual void Init(Control control, bool runOnInit = true)
        {
            if (control == null)
            {
                throw new ArgumentNullException("control");
            }

            Control = control;

            if (runOnInit)
            {
                OnInit();
            }
        }


        /// <summary>
        /// Initializes the extender.
        /// </summary>
        public abstract void OnInit();
    }


    /// <summary>
    /// Generic control extender 
    /// </summary>
    /// <typeparam name="TControl">Type of the control to be extended</typeparam>
    public abstract class ControlExtender<TControl> : ControlExtender where TControl : Control
    {
        private bool initialized = false;
        private bool initPostponed = false;
        private bool isLastPostponedInitEvent = false;


        /// <summary>
        /// Gets the extended control.
        /// </summary>
        public new TControl Control
        {
            get;
            private set;
        }


        /// <summary>
        /// Initializes the extender using the specified control.
        /// </summary>
        /// <param name="control">Control to be extended</param>
        /// <param name="runOnInit">Indicates if OnInit method will be called besides basic initialization.</param>
        public override void Init(Control control, bool runOnInit = true)
        {
            if (control == null)
            {
                throw new ArgumentNullException("control");
            }

            if (initialized)
            {
                return;
            }

            if (control is TControl)
            {
                // Cast the control to the required type
                Control = (TControl)control;
            }
            else
            {
                // Try to find the control of the required type
                Control = ControlsHelper.GetChildControl<TControl>(control, false);
            }

            if (Control == null)
            {
                if (!initPostponed)
                {
                    // Postpone init, bind to control lifecycle events
                    control.Init += (sender, args) => Init(control, runOnInit);
                    control.Load += (sender, args) => Init(control, runOnInit);
                    control.PreRender += (sender, args) =>
                    {
                        isLastPostponedInitEvent = true;

                        try
                        {
                            Init(control, runOnInit);
                        }
                        catch (Exception ex)
                        {
                            // Postponed init failed
                            CoreServices.EventLog.LogException("ControlExtender", "OnInit", ex);
                        }
                    };

                    initPostponed = true;
                    return;
                }
                else if (!isLastPostponedInitEvent)
                {
                    // Postpone init further
                    return;
                }

                // Init failed
                throw GetInitFailedException(control);
            }

            if (runOnInit)
            {
                OnInit();
            }

            initialized = true;
        }


        private Exception GetInitFailedException(Control control)
        {
            var message = string.Format("[{0}.Init]: The specified control '{1}' with ID '{2}' is not of the required type '{3}' and does not contain the control of such type. Please review the extender settings.", GetType().FullName, control.GetType().FullName, control.ID, typeof(TControl).FullName);
            return new Exception(message);
        }
    }
}