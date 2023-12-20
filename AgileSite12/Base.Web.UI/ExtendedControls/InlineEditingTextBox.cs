using System;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;

using CMS.Helpers;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Control used for inline editing a value, for example in unigrid.
    /// Best way to use it in columns that have sorting enabled is
    /// to place parent unigrid inside update panel 
    /// and reload unigrid's data after successful update of edited value.
    /// </summary>
    [DefaultProperty("Text")]
    public class InlineEditingTextBox : CMSWebControl, INamingContainer
    {
        #region "Private fields"

        private Literal mStaticTextContent;
        private LinkButton mStaticTextLinkButton;
        private CMSTextBox mEditingTextBox;
        private CMSAccessibleLinkButton mSubmitButton;
        private CMSPanel mEditingPanel;
        private bool mEditClick = true;
        private bool mSubmitClick = true;

        #endregion


        #region "Private properties"

        /// <summary>
        /// Text to be displayed
        /// </summary>
        private Literal StaticTextContent
        {
            get
            {
                if (mStaticTextContent == null)
                {
                    mStaticTextContent = new Literal { EnableViewState = false };
                }
                return mStaticTextContent;
            }
        }


        /// <summary>
        /// Text box control used for editing.
        /// </summary>
        private CMSTextBox EditingTextBox
        {
            get
            {
                if (mEditingTextBox == null)
                {
                    mEditingTextBox = new CMSTextBox { CssClass = "editing-textbox" };
                }
                return mEditingTextBox;
            }
        }


        /// <summary>
        /// Helper property.
        /// </summary>
        private string Value
        {
            get;
            set;
        }


        /// <summary>
        /// Helper property.
        /// </summary>
        private bool Error
        {
            get
            {
                return !String.IsNullOrEmpty(ErrorText);
            }
        }


        /// <summary>
        /// Gets the HtmlTextWriterTag.Div enum.
        /// </summary>        
        protected override HtmlTextWriterTag TagKey
        {
            get
            {
                return HtmlTextWriterTag.Div;
            }
        }

        #endregion


        #region "Public properties"

        /// <summary>
        /// Forces manual postback handling in case control is used in unigrid with DelayedReload
        /// </summary>
        public bool DelayedReload
        {
            get;
            set;
        }


        /// <summary>
        /// Enables encoding of inline control output. By default is true.
        /// </summary>
        public bool EnableEncode
        {
            get;
            set;
        } = true;


        /// <summary>
        /// Represents text to be edited. Use FormattedText property, to display different text in non-editing mode.
        /// </summary>
        public string Text
        {
            get
            {
                return Value;
            }
            set
            {
                Value = value;
                EditingTextBox.Text = value;
            }
        }


        /// <summary>
        /// Represents text to be displayed in non-editing mode. Set this in Formatting event handler.
        /// If not defined, Text property will be used.
        /// </summary>
        public string FormattedText
        {
            get
            {
                return StaticTextContent.Text;
            }
            set
            {
                StaticTextContent.Text = (EnableEncode) ? HTMLHelper.HTMLEncode(value) : value;
            }
        }


        /// <summary>
        /// Text displayed in tooltip in case of error. Set this in Update event handler in case of error.
        /// </summary>
        public string ErrorText
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the maximum number of characters allowed in the text box. Default is 200.
        /// </summary>
        public int MaxLength
        {
            get
            {
                return EditingTextBox.MaxLength;
            }
            set
            {
                EditingTextBox.MaxLength = value;
            }
        }


        /// <summary>
        /// Gets or sets a value indicating whether to use update panel.
        /// It is recommended to set it to true only in columns that have sorting disabled.
        /// Setting this to true, will speed up inline editing because of less data needed to be transfered during post back.
        /// Default value is false.
        /// </summary>
        public bool UseUpdatePanel
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the additional CSS class for control.
        /// </summary>        
        public string AdditionalCssClass
        {
            get;
            set;
        }


        /// <summary>
        /// If <c>true</c> edit mode can be reached by clicking the formatted text, otherwise the formatted text is not clickable and it's necessary to call <see cref="SwitchToEditMode"/> method explicitly.
        /// </summary>
        public bool EnableEditingOnTextClick
        {
            get;
            set;
        } = true;

        #endregion


        #region "Events"

        /// <summary>
        /// Formatting event. Use this event to set FormattedText property.
        /// </summary>
        public event EventHandler Formatting;


        /// <summary>
        /// Update event. Use this event to save edited data, set ErrorText property in case of error
        /// and reload unigrid's data in case of using InlineEditingTextBox in columns that have sorting enabled.
        /// </summary>
        public event EventHandler Update;

        #endregion


        #region "Methods"

        /// <summary>
        /// Raises the formatting event.
        /// </summary>
        /// <param name="e">The EventArgs instance containing the event data.</param>
        protected virtual void OnFormatting(EventArgs e)
        {
            Formatting?.Invoke(this, e);
        }


        /// <summary>
        /// Raises the update event.
        /// </summary>
        /// <param name="e">The EventArgs instance containing the event data.</param>
        protected virtual void OnUpdate(EventArgs e)
        {
            Update?.Invoke(this, e);
        }

        #endregion


        #region "Lifecycle"

        /// <summary>
        /// Ensures initialization of child controls.
        /// </summary>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            EnsureChildControls();

            this.AddCssClass("inline-editing-textbox");

            if (!String.IsNullOrEmpty(AdditionalCssClass))
            {
                this.AddCssClass(AdditionalCssClass);
            }
        }


        /// <summary>
        /// Ensures action events.
        /// </summary>
        protected override void OnLoad(EventArgs ev)
        {
            base.OnLoad(ev);

            // Ensure correct handling of edit and submit action (needed if unigrid uses DelayedReload)
            if (DelayedReload)
            {
                // Find control which made postback 
                Control ctrl = ControlsHelper.GetPostBackControl(Page);

                // Check if inline editing cell should not be switched to editing mode
                if (EnableEditingOnTextClick && (ctrl != null) && (ctrl.UniqueID == mStaticTextLinkButton.UniqueID))
                {
                    SwitchToEditMode();
                }

                // Check if submit button was not clicked
                if ((ctrl != null) && (ctrl.UniqueID == mSubmitButton.UniqueID))
                {
                    SubmitClick();
                }
            }
        }


        /// <summary>
        /// Initializes a new instance of the InlineEditingTextBox class.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            // Static panel
            var staticTextPanel = new CMSPanel();
            staticTextPanel.AddCssClass("static-textpanel");
            staticTextPanel.Controls.Add(StaticTextContent);

            if (EnableEditingOnTextClick)
            {
                staticTextPanel.AddCssClass("form-control");

                // Static link button, on click shows editing mode
                mStaticTextLinkButton = new LinkButton { ID = "btnStaticText" };
                mStaticTextLinkButton.Click += (s, e) => SwitchToEditMode();
                mStaticTextLinkButton.Controls.Add(staticTextPanel);
            }

            mSubmitButton = new CMSAccessibleLinkButton
            {
                ID = "btnSubmit",
                CssClass = "inline-edit-submit",
                ScreenReaderDescription = ResHelper.GetString("general.savechanges"),
                IconCssClass = "icon-check-circle",
                EnableViewState = false
            };
            mSubmitButton.Click += (s, e) => SubmitClick();

            var cancelButton = new CMSAccessibleLinkButton
            {
                ID = "btnCancel",
                CssClass = "inline-edit-cancel",
                ScreenReaderDescription = ResHelper.GetString("general.cancel"),
                IconCssClass = "icon-times-circle",
                EnableViewState = false
            };

            mEditingPanel = new CMSPanel();
            mEditingPanel.AddCssClass("editing-panel");
            mEditingPanel.Controls.Add(EditingTextBox);
            mEditingPanel.Controls.Add(mSubmitButton);
            mEditingPanel.Controls.Add(cancelButton);
            mEditingPanel.Visible = false;

            if (UseUpdatePanel)
            {
                var updatePanel = new CMSUpdatePanel();
                updatePanel.ContentTemplateContainer.Controls.Add(mStaticTextLinkButton as Control ?? staticTextPanel);
                updatePanel.ContentTemplateContainer.Controls.Add(mEditingPanel);
                Controls.Add(updatePanel);
            }
            else
            {
                Controls.Add(mStaticTextLinkButton as Control ?? staticTextPanel);
                Controls.Add(mEditingPanel);
            }
        }


        /// <summary>
        /// Raises the System.Web.UI.Control.PreRender event.
        /// </summary>
        /// <param name="e">The System.EventArgs object that contains the event data.</param> 
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            OnFormatting(EventArgs.Empty);

            // Set default MaxLength if none specified
            if (MaxLength == 0)
            {
                MaxLength = 200;
            }

            // If formatted text is not defined, display unformatted text
            if (String.IsNullOrEmpty(FormattedText))
            {
                FormattedText = Text;
            }

            RegisterScripts();
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Switches control into editing mode.
        /// </summary>
        public void SwitchToEditMode()
        {
            if (mEditClick)
            {
                mEditingPanel.Visible = true;
                EditingTextBox.Focus();
            }

            // Control is already in editing mode
            mEditClick = false;
        }


        /// <summary>
        /// Submits value of editing text box.
        /// </summary>
        private void SubmitClick()
        {
            if (mSubmitClick)
            {
                Value = Page.Request.Form[EditingTextBox.UniqueID] ?? EditingTextBox.Text;

                OnUpdate(EventArgs.Empty);

                if (Error)
                {
                    // Display textbox with Error class and error text in tooltip
                    mEditingPanel.Visible = true;
                    EditingTextBox.AddCssClass("inline-textbox-error");
                    EditingTextBox.ToolTip = ErrorText;

                    // Ensure that invalid value will be visible to user from page request
                    EditingTextBox.Text = Value;
                }
            }

            // Control already submitted data
            mSubmitClick = false;
        }


        private void RegisterScripts()
        {
            ScriptHelper.RegisterJQuery(Page);
            ScriptHelper.RegisterStartupScript(Page, typeof(Page), "InlineEdit", ScriptHelper.GetScript(@"
$cmsj(function() {  
    $cmsj('.inline-editing-textbox .editing-textbox').on('keypress', function(e) {
        // Submit on enter
        if (e.which == 13) {
            $cmsj(this).siblings('.inline-edit-submit')[0].click();
            // Prevent event bubbling
            e.preventDefault();
        }
    });

    $cmsj('.inline-editing-textbox .editing-panel').each(function() {
        var staticPanel = $cmsj(this).parent().find('.static-textpanel');
        staticPanel.hide();
    });
});"));
        }

        #endregion
    }
}
