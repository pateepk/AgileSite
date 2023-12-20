using System;
using System.Web.UI.HtmlControls;

namespace CMS.OnlineMarketing.Web.UI
{
    /// <summary>
    /// Encapsulates Kentico submit button web component.
    /// </summary>
    internal class SubmitButton : HtmlGenericControl
    {
        /// <summary>
        /// Css class indicating green primary button.
        /// </summary>
        private const string PRIMARY_BUTTON_CLASS_STYLE = "ktc-btn ktc-btn-primary";

        /// <summary>
        /// Css class indicating grey default button.
        /// </summary>
        private const string DEFAULT_BUTTON_CLASS_STYLE = "ktc-btn ktc-btn-default";


        private string mText;
        private Guid mIdentifier;
        private bool mIsPrimaryButton;


        /// <summary>
        /// Gets or set button text.
        /// </summary>
        public string Text
        {
            get
            {
                return mText;
            }

            set
            {
                Attributes.Add("button-text", value);
                mText = value;
            }
        }


        /// <summary>
        /// Gets or sets the text displayed on mouseover.
        /// </summary>
        public string Tooltip { get; set; }


        /// <summary>
        /// Gets or sets button guid identifier.
        /// </summary>
        public Guid Identifier
        {
            get
            {
                return mIdentifier;
            }
            set
            {
                Attributes.Add("id", value.ToString());
                mIdentifier = value;
            }
        }


        /// <summary>
        /// Gets or sets a value which indicates whether button is primary. Otherewise is default.
        /// </summary>
        /// <remarks>Primary button represents green button and default represents grey button.</remarks>
        public bool IsPrimaryButton
        {
            get
            {
                return mIsPrimaryButton;
            }
            set
            {
                string buttonStyle = value ? PRIMARY_BUTTON_CLASS_STYLE : DEFAULT_BUTTON_CLASS_STYLE;
                Attributes.Add("button-style", buttonStyle);

                mIsPrimaryButton = value;
            }
        }


        /// <summary>
        /// Initializes a new instance of <see cref="SubmitButton"/>.
        /// </summary>
        public SubmitButton() : base("kentico-submit-button")
        {
        }


        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            Attributes.Add("button-Tooltip", Tooltip);
        }
    }
}
