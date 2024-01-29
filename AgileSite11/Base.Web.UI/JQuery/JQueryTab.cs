using System;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// JQuery tabs container class.
    /// </summary>
    [ToolboxItem(false)]
    [ParseChildren(true)]
    public class JQueryTab : WebControl
    {
        #region "Variables"

        private ITemplate mContentTemplate = null;
        private Control mHeaderControl;
        private ITemplate mHeaderTemplate = null;
        private JQueryTabContainer mOwner = null;

        #endregion


        #region "Properties"

        internal bool Selected
        {
            get;
            set;
        }

        /// <summary>
        /// Index of the tab
        /// </summary>
        [Browsable(false)]
        public int Index
        {
            get;
            set;
        }


        /// <summary>
        /// Content template.
        /// </summary>
        [PersistenceMode(PersistenceMode.InnerProperty), TemplateInstance(TemplateInstance.Single), Browsable(false)]
        public ITemplate ContentTemplate
        {
            get
            {
                return mContentTemplate;
            }
            set
            {
                mContentTemplate = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether themes apply to this control.
        /// </summary>
        [Category("Behavior"), DefaultValue(true)]
        public override bool Enabled
        {
            get
            {
                return base.Enabled;
            }
            set
            {
                base.Enabled = value;
            }
        }


        /// <summary>
        /// Tabs header template.
        /// </summary>
        [PersistenceMode(PersistenceMode.InnerProperty), TemplateInstance(TemplateInstance.Single), Browsable(false)]
        public ITemplate HeaderTemplate
        {
            get
            {
                return mHeaderTemplate;
            }
            set
            {
                mHeaderTemplate = value;
            }
        }


        /// <summary>
        /// Tabs header text.
        /// </summary>
        [Category("Appearance"), DefaultValue("")]
        public string HeaderText
        {
            get
            {
                return (ViewState["HeaderText"] ?? string.Empty).ToString();
            }
            set
            {
                ViewState["HeaderText"] = value;
            }
        }


        /// <summary>
        /// Specifies the visibility and position of scroll bars.
        /// </summary>
        [Category("Behavior"), DefaultValue(0)]
        public ScrollBars ScrollBars
        {
            get
            {
                if (ViewState["ScrollBars"] != null)
                {
                    return (ScrollBars)ViewState["ScrollBars"];
                }
                return ScrollBars.None;
            }
            set
            {
                ViewState["ScrollBars"] = value;
            }
        }

        #endregion


        #region "Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        public JQueryTab()
        {
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        public JQueryTab(HtmlTextWriterTag tag)
            : base(tag)
        {
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        public JQueryTab(string tag)
            : base(tag)
        {
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Raises the System.Web.UI.Control.Init event.
        /// </summary>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            if (HeaderTemplate != null)
            {
                mHeaderControl = new Control();
                HeaderTemplate.InstantiateIn(mHeaderControl);
                Controls.Add(mHeaderControl);
            }
            if (ContentTemplate != null)
            {
                Control container = new Control();
                ContentTemplate.InstantiateIn(container);
                Controls.Add(container);
            }
            base.OnInit(e);
        }


        /// <summary>
        /// Renders the control to the specified HTML writer.
        /// </summary>
        protected override void Render(HtmlTextWriter writer)
        {
            if (mHeaderControl != null)
            {
                mHeaderControl.Visible = false;
            }
            AddAttributesToRender(writer);

            writer.RenderBeginTag(HtmlTextWriterTag.Div);
            RenderChildren(writer);
            writer.RenderEndTag();
        }


        /// <summary>
        /// Renders tab header.
        /// </summary>
        protected internal virtual void RenderHeader(HtmlTextWriter writer)
        {
            if (!String.IsNullOrEmpty(HeaderText) || (mHeaderControl != null))
            {
                writer.Write("<li id={0}_head>", ClientID);
                writer.Write("<a href=\"#{0}\">", ClientID);

                if (mHeaderControl != null)
                {
                    mHeaderControl.RenderControl(writer);
                }
                else
                {
                    writer.Write(HeaderText);
                }
                writer.Write("</a>");
                writer.Write("</li>");
            }
        }


        /// <summary>
        /// Sets current tab owner.
        /// </summary>
        internal void SetOwner(JQueryTabContainer owner, int index)
        {
            mOwner = owner;
            Index = index;
        }

        #endregion
    }
}