using System;
using System.Web.UI;
using System.Web.UI.WebControls;

using CMS.Helpers;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Wraps label with additional markup to create alert box.
    /// </summary>
    public class AlertLabel : Label
    {
        #region "Variables"

        private string mResourceString;

        #endregion


        #region "Properties"

        /// <summary>
        /// Indicates whether control should be generated as simple label
        /// </summary>
        internal bool BasicStyles 
        { 
            get; 
            set; 
        }


        /// <summary>
        /// Indicates whether the alert should render as error, warning or information panel.
        /// </summary>
        public MessageTypeEnum AlertType
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets a value that indicates whether a server control is rendered as UI on the page.
        /// </summary>
        public override bool Visible
        {
            get
            {
                if (!IsVisibilityInitialized)
                {
                    base.Visible = !String.IsNullOrEmpty(Text);
                }
                return base.Visible;
            }
            set
            {
                base.Visible = value;
                IsVisibilityInitialized = true;
            }
        }


        /// <summary>
        /// Name of a resource string used for text. Property Text has higher priority.
        /// </summary>
        public string ResourceString
        {
            get
            {
                return mResourceString;
            }
            set
            {
                mResourceString = value;
                if (String.IsNullOrEmpty(Text))
                {
                    Text = ResHelper.GetString(mResourceString);
                }
            }
        }


        /// <summary>
        ///  Gets the HTML tag that is used to render the Label control.
        /// </summary>
        protected override HtmlTextWriterTag TagKey
        {
            get
            {
                if (!BasicStyles)
                {
                    return HtmlTextWriterTag.Div;
                }

                return HtmlTextWriterTag.Span;
            }
        }


        private bool IsVisibilityInitialized
        {
            get;
            set;
        }


        /// <summary>
        /// Icon leading the alert message.
        /// </summary>
        private CMSIcon AlertIcon
        {
            get;
            set;
        }


        /// <summary>
        /// Returns the client ID of the label element.
        /// </summary>
        public string LabelClientID
        {
            get
            {
                return BasicStyles ? ClientID : ClientID + "_lbl";
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Renders the alert icon and wraps label content in a span element.
        /// </summary>
        /// <param name="writer">Output writer.</param>
        protected override void RenderContents(HtmlTextWriter writer)
        {
            if (!BasicStyles)
            {
                writer.Write("<span class=\"alert-icon\">");

                AlertIcon.RenderControl(writer);

                writer.Write("</span><div id=\"" + LabelClientID + "\" class=\"alert-label\">");

                base.RenderContents(writer);

                writer.Write("</div>");
            }
            else
            {
                base.RenderContents(writer);
            }
        }


        /// <summary>
        /// Initializes the alert icon and CSS classes and renders the alert label to given writer.
        /// </summary>
        /// <param name="writer">Output writer</param>
        protected override void Render(HtmlTextWriter writer)
        {
            Initialize();

            base.Render(writer);
        }


        private void Initialize()
        {
            EnableViewState = false;
            
            if (!BasicStyles)
            {
                string alertCssClass = null;

                switch (AlertType)
                {
                    case MessageTypeEnum.Information:
                        alertCssClass = "alert-info";
                        AlertIcon = new CMSIcon { CssClass = "icon-i-circle", AlternativeText = ResHelper.GetString("general.info") };
                        break;

                    case MessageTypeEnum.Confirmation:
                        alertCssClass = "alert-success";
                        AlertIcon = new CMSIcon { CssClass = "icon-check-circle", AlternativeText = ResHelper.GetString("general.success") };
                        break;

                    case MessageTypeEnum.Warning:
                        alertCssClass = "alert-warning";
                        AlertIcon = new CMSIcon { CssClass = "icon-exclamation-triangle", AlternativeText = ResHelper.GetString("general.warning") };
                        break;

                    case MessageTypeEnum.Error:
                        alertCssClass = "alert-error";
                        AlertIcon = new CMSIcon { CssClass = "icon-times-circle", AlternativeText = ResHelper.GetString("general.error") };
                        break;
                }
                CssClass = CssHelper.EnsureClass(CssClass, CssHelper.EnsureClass(alertCssClass, "alert"));
            }
        }

        #endregion
    }
}
