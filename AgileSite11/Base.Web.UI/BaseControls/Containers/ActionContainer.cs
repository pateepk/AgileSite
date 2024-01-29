using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Container control that is supposed to be used for some input with additional actions.
    /// </summary>
    public class ActionContainer : CompositeControl
    {
        #region "Variables"

        private Control mInputControl;
        private readonly WebControl mActionsContainer = new CMSPanel();
        private bool mDisplayActions = true;

        #endregion


        #region "Properties"

        /// <summary>
        /// Input template.
        /// </summary>
        [TemplateContainer(typeof(ActionContainer))]
        [PersistenceMode(PersistenceMode.InnerProperty)]
        [Description("Template for input control.")]
        [DefaultValue(null)]
        [Browsable(false)]
        [TemplateInstance(TemplateInstance.Single)]
        public virtual ITemplate InputTemplate
        {
            get;
            set;
        }


        /// <summary>
        /// Actions template.
        /// </summary>
        [TemplateContainer(typeof(ActionContainer))]
        [PersistenceMode(PersistenceMode.InnerProperty)]
        [Description("Template for action controls.")]
        [DefaultValue(null)]
        [Browsable(false)]
        [TemplateInstance(TemplateInstance.Single)]
        public virtual ITemplate ActionsTemplate
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets whether actions should be displayed.
        /// </summary>
        public bool DisplayActions
        {
            get
            {
                return mDisplayActions;
            }
            set
            {
                mDisplayActions = value;
            }
        }


        /// <summary>
        /// Container containing actions.
        /// </summary>
        [Browsable(false)]
        public virtual WebControl ActionsContainer
        {
            get
            {
                return mActionsContainer;
            }
        }

        #endregion


        #region "Overridden properties"

        /// <summary>
        /// HTML tag for this control.
        /// </summary>
        protected override HtmlTextWriterTag TagKey
        {
            get
            {
                return HtmlTextWriterTag.Div;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Loads the content container explicitly.
        /// </summary>
        public void LoadContainer()
        {
            EnsureChildControls();
        }


        /// <summary>
        /// OnInit event.
        /// </summary>
        protected override void OnInit(EventArgs e)
        {
            EnsureChildControls();
            base.OnInit(e);
        }


        /// <summary>
        /// OnPreRender event.
        /// </summary>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            this.AddCssClass("cms-input-group");
        }


        /// <summary>
        /// Renders the control.
        /// </summary>
        /// <param name="writer">HTML text writer.</param>
        protected override void Render(HtmlTextWriter writer)
        {
            if (DisplayActions)
            {
                base.Render(writer);
            }
            else
            {
                mInputControl.RenderControl(writer);
            }
        }


        /// <summary>
        /// Creates child controls.
        /// </summary>
        protected override void CreateChildControls()
        {
            Controls.Clear();

            // Add input template
            if (InputTemplate != null)
            {
                mInputControl = new Control();
                InputTemplate.InstantiateIn(mInputControl);
                Controls.Add(mInputControl);
            }

            // Add actions template
            if (DisplayActions)
            {
                if (ActionsTemplate != null)
                {
                    ActionsContainer.AddCssClass("input-group-btn");
                    ActionsTemplate.InstantiateIn(ActionsContainer);
                    Controls.Add(ActionsContainer);
                }
            }
        }

        #endregion
    }
}
