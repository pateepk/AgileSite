using System;
using System.Web.UI;
using System.Web.UI.WebControls;

using CMS.EventLog;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// CMS Update panel - override of update panel which can handle events in layout.
    /// </summary>
    public class CMSUpdatePanel : UpdatePanel, IAttributeAccessor
    {
        #region "Variables"

        private bool mContainerLoaded = false;
        private bool mCanUnload = false;
        private PlaceHolder plcProgress = new PlaceHolder();

        #endregion


        #region "Properties"

        /// <summary>
        /// If true, the control handles errors caused by the control manipulation.
        /// </summary>
        public bool CatchErrors
        {
            get;
            set;
        } = true;


        /// <summary>
        /// If true, the progress information is shown.
        /// </summary>
        public bool ShowProgress
        {
            get;
            set;
        }


        /// <summary>
        /// HTML code of the progress information.
        /// </summary>
        public string ProgressHTML
        {
            get;
            set;
        }

        #endregion


        #region "Methods"


        /// <summary>
        /// Loads the content container explicitly.
        /// </summary>
        public void LoadContainer()
        {
            try
            {
                ContentTemplateContainer.Controls.Clear();
                ContentTemplate.InstantiateIn(ContentTemplateContainer);
                mContainerLoaded = true;
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("CMSUpdatePanel", "LOADCONTAINER", ex);
            }
        }


        /// <summary>
        /// Init event.
        /// </summary>
        protected override void OnInit(EventArgs e)
        {
            ControlsHelper.EnsureScriptManager(Page);

            if (mContainerLoaded && CatchErrors)
            {
                // If container was loaded, Init event will fail
                try
                {
                    base.OnInit(e);
                }
                catch
                {
                }
            }
            else
            {
                base.OnInit(e);
            }
        }


        /// <summary>
        /// Creates the child controls in the control
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            ContentTemplateContainer.Controls.Add(plcProgress);
        }


        /// <summary>
        /// OnPreRender.
        /// </summary>
        /// <param name="e">Event args</param>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            mCanUnload = true;

            if (ShowProgress)
            {
                // Add the progress container
                UpdateProgress up = new UpdateProgress
                {
                    ID = ID + "up",
                    DisplayAfter = 1000,
                    AssociatedUpdatePanelID = ID
                };

                string html = ProgressHTML;
                if (String.IsNullOrEmpty(html))
                {
                    html = ScriptHelper.GetLoaderHtml();
                }

                up.Controls.Add(new LiteralControl(html));

                plcProgress.Controls.AddAt(0, up);
            }
        }


        /// <summary>
        /// OnUnload.
        /// </summary>
        /// <param name="e">Event args</param>
        protected override void OnUnload(EventArgs e)
        {
            if (mCanUnload)
            {
                base.OnUnload(e);
            }
        }

        #endregion
    }
}