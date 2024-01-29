using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;
using System;

using CMS.Helpers;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// BBEditor control.
    /// </summary>
    [ToolboxItem(false)]
    [ValidationProperty("Text")]
    public class BBEditor : Panel, INamingContainer
    {
        #region "Variables"

        /// <summary>
        /// Text area for text editing.
        /// </summary>
        protected CMSTextArea txtText = null;

        /// <summary>
        /// DiscussionButtons control for macro editing.
        /// </summary>
        protected DiscussionButtons pnlButtons = new DiscussionButtons();

        #endregion


        #region "Public properties"

        /// <summary>
        /// Sets number of rows to display in containing <see cref="TextArea"/>, see <see cref="TextBox.Rows"/> for more details.
        /// </summary>
        public int Rows
        {
            get
            {
                return TextArea.Rows;
            }
            set
            {
                TextArea.Rows = value;
            }
        }


        /// <summary>
        /// Configuration of the dialog for inserting Images.
        /// </summary>
        public DialogConfiguration ImageDialogConfig
        {
            get
            {
                return pnlButtons.ImageDialogConfig;
            }
            set
            {
                pnlButtons.ImageDialogConfig = value;
            }
        }


        /// <summary>
        /// Configuration of the dialog for inserting URLs.
        /// </summary>
        public DialogConfiguration URLDialogConfig
        {
            get
            {
                return pnlButtons.URLDialogConfig;
            }
            set
            {
                pnlButtons.URLDialogConfig = value;
            }
        }


        /// <summary>
        /// Gets or sets the text of the text area.
        /// </summary>
        public string Text
        {
            get
            {
                return TextArea.Text;
            }
            set
            {
                TextArea.Text = value;
            }
        }


        /// <summary>
        /// Gets the TextArea object.
        /// </summary>
        /// 
        public CMSTextArea TextArea
        {
            get
            {
                if (txtText == null)
                {
                    txtText = new CMSTextArea();
                    txtText.ID = "ctlBBTextBox";
                    Controls.Add(TextArea);
                }
                return txtText;
            }
        }


        /// <summary>
        /// Gets or sets the value which determines whether to display button for Quote tag insertion.
        /// </summary>
        public bool ShowQuote
        {
            get
            {
                return ValidationHelper.GetBoolean(ViewState["ShowQuote"], true);
            }
            set
            {
                ViewState["ShowQuote"] = value;
                pnlButtons.ShowQuote = value;
            }
        }


        /// <summary>
        /// Gets or sets the value which determines whether to display button for URL tag insertion.
        /// </summary>
        public bool ShowURL
        {
            get
            {
                return ValidationHelper.GetBoolean(ViewState["ShowURL"], true);
            }
            set
            {
                ViewState["ShowURL"] = value;
                pnlButtons.ShowURL = value;
            }
        }


        /// <summary>
        /// Gets or sets the value which determines whether to display button for Image tag insertion.
        /// </summary>
        public bool ShowImage
        {
            get
            {
                return ValidationHelper.GetBoolean(ViewState["ShowImage"], true);
            }
            set
            {
                ViewState["ShowImage"] = value;
                pnlButtons.ShowImage = value;
            }
        }


        /// <summary>
        /// Gets or sets the value which determines whether to display button for Image tag insertion  using advanced dialog.
        /// </summary>
        public bool ShowAdvancedImage
        {
            get
            {
                return ValidationHelper.GetBoolean(ViewState["ShowAdvancedImage"], false);
            }
            set
            {
                ViewState["ShowAdvancedImage"] = value;
                pnlButtons.ShowAdvancedImage = value;
            }
        }


        /// <summary>
        /// Gets or sets the value which determines whether to display button for URL tag insertion using advanced dialog.
        /// </summary>
        public bool ShowAdvancedURL
        {
            get
            {
                return ValidationHelper.GetBoolean(ViewState["ShowAdvancedURL"], false);
            }
            set
            {
                ViewState["ShowAdvancedURL"] = value;
                pnlButtons.ShowAdvancedURL = value;
            }
        }


        /// <summary>
        /// Gets or sets the value which determines whether to display button for Bold font stlye tag insertion.
        /// </summary>
        public bool ShowBold
        {
            get
            {
                return ValidationHelper.GetBoolean(ViewState["ShowBold"], true);
            }
            set
            {
                ViewState["ShowBold"] = value;
                pnlButtons.ShowBold = value;
            }
        }


        /// <summary>
        /// Gets or sets the value which determines whether to display button for Italic font stlye tag insertion.
        /// </summary>
        public bool ShowItalic
        {
            get
            {
                return ValidationHelper.GetBoolean(ViewState["ShowItalic"], true);
            }
            set
            {
                ViewState["ShowItalic"] = value;
                pnlButtons.ShowItalic = value;
            }
        }


        /// <summary>
        /// Gets or sets the value which determines whether to display button for Underline font stlye tag insertion.
        /// </summary>
        public bool ShowUnderline
        {
            get
            {
                return ValidationHelper.GetBoolean(ViewState["ShowUnderline"], true);
            }
            set
            {
                ViewState["ShowUnderline"] = value;
                pnlButtons.ShowUnderline = value;
            }
        }


        /// <summary>
        /// Gets or sets the value which determines whether to display button for Strikedthrough font style tag insertion.
        /// </summary>
        public bool ShowStrike
        {
            get
            {
                return ValidationHelper.GetBoolean(ViewState["ShowStrike"], true);
            }
            set
            {
                ViewState["ShowStrike"] = value;
                pnlButtons.ShowStrike = value;
            }
        }


        /// <summary>
        /// Gets or sets the value which determines whether to display button for Font color style tag insertion.
        /// </summary>
        public bool ShowColor
        {
            get
            {
                return ValidationHelper.GetBoolean(ViewState["ShowColor"], true);
            }
            set
            {
                ViewState["ShowColor"] = value;
                pnlButtons.ShowColor = value;
            }
        }


        /// <summary>
        /// Gets or sets the value which determines whether to display button for Code tag insertion.
        /// </summary>
        public bool ShowCode
        {
            get
            {
                return ValidationHelper.GetBoolean(ViewState["ShowCode"], true);
            }
            set
            {
                ViewState["ShowCode"] = value;
                pnlButtons.ShowCode = value;
            }
        }


        /// <summary>
        /// Gets or sets the value which determines whether the prompt dialog will be used.
        /// Prompt dialog doensn't work properly with modal window.
        /// </summary>
        public bool UsePromptDialog
        {
            get
            {
                return ValidationHelper.GetBoolean(ViewState["UsePromptDialog"], true);
            }
            set
            {
                ViewState["UsePromptDialog"] = value;
                pnlButtons.UsePromptDialog = value;
            }
        }


        /// <summary>
        /// Determines whether the control is enabled or disabled.
        /// </summary>
        public override bool Enabled
        {
            get
            {
                return ValidationHelper.GetBoolean(ViewState["Enabled"], true);
            }
            set
            {
                base.Enabled = value;
                ViewState["Enabled"] = value;
                TextArea.Enabled = value;
                pnlButtons.Enabled = value;
            }
        }


        /// <summary>
        /// Indicates if the control is used on the live site.
        /// </summary>
        public bool IsLiveSite
        {
            get
            {
                return ValidationHelper.GetBoolean(ViewState["IsLiveSite"], true);
            }
            set
            {
                ViewState["IsLiveSite"] = value;
                pnlButtons.IsLiveSite = value;
            }
        }


        /// <summary>
        /// Ensure focus on the text area.
        /// </summary>
        public override void Focus()
        {
            base.Focus();
            TextArea.Focus();
        }

        #endregion


        #region "Protected methods"

        /// <summary>
        /// Renders the control at run-time.
        /// </summary>
        protected override void CreateChildControls()
        {
            // Show panle only if any BBCode is enabled
            if (ShowBold || ShowCode || ShowColor || ShowImage || ShowItalic ||
                ShowQuote || ShowStrike || ShowUnderline || ShowURL || ShowAdvancedImage || ShowAdvancedURL)
            {
                pnlButtons.ID = "pnlBBButtons";
                Controls.AddAt(0, pnlButtons);
                pnlButtons.AddCssClass("DicussionPanel");
            }

            if (!Controls.Contains(TextArea))
            {
                Controls.Add(TextArea);
            }

            if (TextArea.CssClass == "")
            {
                TextArea.AddCssClass("TextareaItem");
            }

            pnlButtons.ControlID = TextArea.ClientID;
            pnlButtons.ShowBold = ShowBold;
            pnlButtons.ShowCode = ShowCode;
            pnlButtons.ShowColor = ShowColor;
            pnlButtons.ShowImage = ShowImage;
            pnlButtons.ShowItalic = ShowItalic;
            pnlButtons.ShowQuote = ShowQuote;
            pnlButtons.ShowStrike = ShowStrike;
            pnlButtons.ShowUnderline = ShowUnderline;
            pnlButtons.ShowURL = ShowURL;
            pnlButtons.ShowAdvancedImage = ShowAdvancedImage;
            pnlButtons.ShowAdvancedURL = ShowAdvancedURL;
            pnlButtons.ImageDialogConfig.EditorClientID = TextArea.ClientID;
            pnlButtons.URLDialogConfig.EditorClientID = TextArea.ClientID;
        }

        #endregion
    }
}