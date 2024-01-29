using System;
using System.Web.UI;

using CMS.Base.Web.UI;
using CMS.FormEngine.Web.UI;
using CMS.MacroEngine;

namespace CMS.PortalEngine.Web.UI
{
    /// <summary>
    /// Layout to supplement the layout that failed to load.
    /// </summary>
    internal class TextLayout : CMSAbstractLayout
    {
        #region "Variables"

        /// <summary>
        /// Layout code.
        /// </summary>
        protected string mLayoutCode;


        private MacroResolver mContextResolver;


        private MacroResolver ContextResolver
        {
            get
            {
                if (mContextResolver == null)
                {
                    mContextResolver = PortalUIHelper.GetControlResolver(Page, UIContextHelper.GetUIContext(this));
                }

                return mContextResolver;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="code">Layout code</param>
        public TextLayout(string code)
        {
            mLayoutCode = code;
        }


        /// <summary>
        /// Init event handler.
        /// </summary>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            EnsureChildControls();
        }


        /// <summary>
        /// Creates child controls.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            string code = ContextResolver.ResolveMacros(mLayoutCode);

            // Add the text and resolve controls in it
            Controls.Add(new LiteralControl(code));

            ControlsHelper.ResolveDynamicControls(this);
        }

        #endregion
    }
}