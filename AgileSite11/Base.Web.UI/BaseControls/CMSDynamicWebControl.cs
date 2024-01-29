using System;
using System.ComponentModel;
using System.Web.UI;
using CMS.Base.Web.UI;
using CMS.Helpers;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Base class for dynamic Web controls
    /// </summary>
    public abstract class CMSDynamicWebControl : CMSWebControl
    {
        #region "Variables"

        private bool initOnPageInitComplete;

        #endregion


        #region "Properties"

        /// <summary>
        /// If true, the control initializes on BeforeInitComplete, otherwise on InitComplete event.
        /// </summary>
        protected bool InitEarly
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if processing of the code of this control should continue. It can be used after raising an event.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool StopProcessing
        {
            get;
            set;
        }


        /// <summary>
        /// If true, the initialization of the control has already completed and won't repeat. Set to true in your initialization code.
        /// </summary>
        public bool InitCompleted
        {
            get;
            set;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Init event handler.
        /// </summary>
        protected override void OnInit(EventArgs e)
        {
            if (!RequestHelper.IsPostBack())
            {
                initOnPageInitComplete = true;
                EnsureChildControls();
            }

            base.OnInit(e);
        }


        /// <summary>
        /// Customized LoadViewState.
        /// </summary>
        protected override void LoadViewState(object savedState)
        {
            base.LoadViewState(((Pair)savedState).First);

            EnsureInitialization();
        }


        /// <summary>
        /// Customized SaveViewState.
        /// </summary>
        protected override object SaveViewState()
        {
            return new Pair(base.SaveViewState(), null);
        }


        /// <summary>
        /// Renders the control at run-time.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            if (initOnPageInitComplete)
            {
                if (InitEarly)
                {
                    PageContext.BeforeInitComplete += Page_InitComplete;
                }
                else
                {
                    PageContext.InitComplete += Page_InitComplete;
                }
            }
            else
            {
                EnsureInitialization();
            }
        }


        /// <summary>
        /// Event handler of page's init complete.
        /// </summary>
        protected void Page_InitComplete(object sender, EventArgs e)
        {
            EnsureInitialization();
        }


        /// <summary>
        /// Load event handler.
        /// </summary>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // Ensure first form initialization if added dynamically
            if (initOnPageInitComplete)
            {
                EnsureInitialization();
            }
        }


        /// <summary>
        /// Initializes the control. Is called within InitComplete or 
        /// </summary>
        protected void EnsureInitialization()
        {
            EnsureChildControls();

            if (!InitCompleted && !StopProcessing)
            {
                InitializeControl();
            }
        }


        /// <summary>
        /// Initializes the control. Is called within InitComplete or BeforeInitComplete (based on property InitEarly), or LoadViewState.
        /// You need to make sure to set the InitCompleted property once your control is properly initialized to avoid multiple initializations.
        /// </summary>
        protected abstract void InitializeControl();

        #endregion
    }
}