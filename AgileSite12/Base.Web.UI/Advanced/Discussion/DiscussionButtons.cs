using System;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;

using CMS.Helpers;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// BBCode buttons control.
    /// </summary>
    [ToolboxItem(false)]
    public class DiscussionButtons : Panel
    {
        #region "Variables"

        private bool mShowQuote = true;
        private bool mShowImage = true;
        private bool mShowUrl = true;
        private bool mShowAdvancedImage = true;
        private bool mShowAdvancedURL = true;
        private bool mShowBold = true;
        private bool mShowItalic = true;
        private bool mShowUnderline = true;
        private bool mShowStrike = true;
        private bool mShowColor = true;
        private bool mShowCode = true;
        private bool mUsePromptDialog = true;
        private bool mEnabled = true;
        private string mControlID = "";
        private string mButtonsCssClass = "DiscussionButton";
        private DialogConfiguration mImageDialogConfig = null;
        private DialogConfiguration mURLDialogConfig = null;
        private bool mIsLiveSite = true;


        /// <summary>
        /// Insert quote button.
        /// </summary>
        protected ImageButton btnQuote = new ImageButton();

        /// <summary>
        /// Insert image button.
        /// </summary>
        protected ImageButton btnImage = new ImageButton();

        /// <summary>
        /// Insert URL button.
        /// </summary>
        protected ImageButton btnUrl = new ImageButton();

        /// <summary>
        /// Insert media button.
        /// </summary>
        protected ImageButton btnAdvancedImage = new ImageButton();

        /// <summary>
        /// Insert link button.
        /// </summary>
        protected ImageButton btnAdvancedUrl = new ImageButton();

        /// <summary>
        /// Sets text font style to bold button.
        /// </summary>
        protected ImageButton btnBold = new ImageButton();

        /// <summary>
        /// Sets text font style to italics button.
        /// </summary>
        protected ImageButton btnItalic = new ImageButton();

        /// <summary>
        /// Sets text font style to underlined button.
        /// </summary>
        protected ImageButton btnUnderline = new ImageButton();

        /// <summary>
        /// Sets text font style to strikedthrough button.
        /// </summary>
        protected ImageButton btnStrike = new ImageButton();

        /// <summary>
        /// Insert code button.
        /// </summary>
        protected ImageButton btnCode = new ImageButton();

        /// <summary>
        /// Sets text color button.
        /// </summary>
        protected ImageButton btnColor = new ImageButton();

        #endregion


        #region "Public properties"

        /// <summary>
        /// Indicates if the control is used on the live site.
        /// </summary>
        public bool IsLiveSite
        {
            get
            {
                return mIsLiveSite;
            }
            set
            {
                mIsLiveSite = value;
            }
        }


        /// <summary>
        /// Configuration of the dialog for inserting Images.
        /// </summary>
        public DialogConfiguration ImageDialogConfig
        {
            get
            {
                if (mImageDialogConfig == null)
                {
                    mImageDialogConfig = new DialogConfiguration();
                }
                return mImageDialogConfig;
            }
            set
            {
                mImageDialogConfig = value;
            }
        }


        /// <summary>
        /// Configuration of the dialog for inserting URLs.
        /// </summary>
        public DialogConfiguration URLDialogConfig
        {
            get
            {
                if (mURLDialogConfig == null)
                {
                    mURLDialogConfig = new DialogConfiguration();
                }
                return mURLDialogConfig;
            }
            set
            {
                mURLDialogConfig = value;
            }
        }


        /// <summary>
        /// Gets or sets the ClientID of the control (TextArea) where the buttons will insert the macros.
        /// </summary>
        public string ControlID
        {
            get
            {
                return mControlID;
            }
            set
            {
                mControlID = value;
            }
        }


        /// <summary>
        /// Gets or sets the value which determines whether to display button for Quote tag insertion.
        /// </summary>
        public bool ShowQuote
        {
            get
            {
                return mShowQuote;
            }
            set
            {
                mShowQuote = value;
                btnQuote.Visible = value;
            }
        }


        /// <summary>
        /// Gets or sets the value which determines whether to display button for URL tag insertion.
        /// </summary>
        public bool ShowURL
        {
            get
            {
                return mShowUrl;
            }
            set
            {
                mShowUrl = value;
                btnUrl.Visible = value;
            }
        }


        /// <summary>
        /// Gets or sets the value which determines whether to display button for Image tag insertion.
        /// </summary>
        public bool ShowImage
        {
            get
            {
                return mShowImage;
            }
            set
            {
                mShowImage = value;
                btnImage.Visible = value;
            }
        }


        /// <summary>
        /// Gets or sets the value which determines whether to display button for link insertion.
        /// </summary>
        public bool ShowAdvancedURL
        {
            get
            {
                return mShowAdvancedURL;
            }
            set
            {
                mShowAdvancedURL = value;
                btnAdvancedUrl.Visible = value;
            }
        }


        /// <summary>
        /// Gets or sets the value which determines whether to display button for media insertion.
        /// </summary>
        public bool ShowAdvancedImage
        {
            get
            {
                return mShowAdvancedImage;
            }
            set
            {
                mShowAdvancedImage = value;
                btnAdvancedImage.Visible = value;
            }
        }


        /// <summary>
        /// Gets or sets the value which determines whether to display button for Bold font style tag insertion.
        /// </summary>
        public bool ShowBold
        {
            get
            {
                return mShowBold;
            }
            set
            {
                mShowBold = value;
                btnBold.Visible = value;
            }
        }


        /// <summary>
        /// Gets or sets the value which determines whether to display button for Italic font style tag insertion.
        /// </summary>
        public bool ShowItalic
        {
            get
            {
                return mShowItalic;
            }
            set
            {
                mShowItalic = value;
                btnItalic.Visible = value;
            }
        }


        /// <summary>
        /// Gets or sets the value which determines whether to display button for Underline font style tag insertion.
        /// </summary>
        public bool ShowUnderline
        {
            get
            {
                return mShowUnderline;
            }
            set
            {
                mShowUnderline = value;
                btnUnderline.Visible = value;
            }
        }


        /// <summary>
        /// Gets or sets the value which determines whether to display button for Strikedthrough font style tag insertion.
        /// </summary>
        public bool ShowStrike
        {
            get
            {
                return mShowStrike;
            }
            set
            {
                mShowStrike = value;
                btnStrike.Visible = value;
            }
        }


        /// <summary>
        /// Gets or sets the value which determines whether to display button for Font color style tag insertion.
        /// </summary>
        public bool ShowColor
        {
            get
            {
                return mShowColor;
            }
            set
            {
                mShowColor = value;
                btnColor.Visible = value;
            }
        }


        /// <summary>
        /// Gets or sets the value which determines whether to display button for Code tag insertion.
        /// </summary>
        public bool ShowCode
        {
            get
            {
                return mShowCode;
            }
            set
            {
                mShowCode = value;
                btnCode.Visible = value;
            }
        }


        /// <summary>
        /// Gets or sets the value which determines whether the prompt dialog will be used.
        /// Prompt dialog doesn't work properly with modal window.
        /// </summary>
        public bool UsePromptDialog
        {
            get
            {
                return mUsePromptDialog;
            }
            set
            {
                mUsePromptDialog = value;
            }
        }


        /// <summary>
        /// Determines whether the control is enabled or disabled.
        /// </summary>
        public override bool Enabled
        {
            get
            {
                return mEnabled;
            }
            set
            {
                mEnabled = value;
                InititlaizeButtons(value);
            }
        }


        /// <summary>
        /// Gets or sets buttons CSS class.
        /// </summary>
        public string ButtonsCssClass
        {
            get
            {
                return mButtonsCssClass;
            }
            set
            {
                mButtonsCssClass = value;
            }
        }

        #endregion


        #region "Protected methods"

        /// <summary>
        /// Initialize buttons.
        /// </summary>
        /// <param name="enabled">Indicates if control is enabled</param>
        protected void InititlaizeButtons(bool enabled)
        {
            if (!enabled)
            {
                btnUrl.OnClientClick = string.Empty;
                btnImage.OnClientClick = string.Empty;
                btnQuote.OnClientClick = string.Empty;
                btnBold.OnClientClick = string.Empty;
                btnCode.OnClientClick = string.Empty;
                btnColor.OnClientClick = string.Empty;
                btnItalic.OnClientClick = string.Empty;
                btnStrike.OnClientClick = string.Empty;
                btnUnderline.OnClientClick = string.Empty;
                btnAdvancedImage.OnClientClick = string.Empty;
                btnAdvancedUrl.OnClientClick = string.Empty;

                btnUrl.Style.Remove(HtmlTextWriterStyle.Cursor);
                btnImage.Style.Remove(HtmlTextWriterStyle.Cursor);
                btnQuote.Style.Remove(HtmlTextWriterStyle.Cursor);
                btnBold.Style.Remove(HtmlTextWriterStyle.Cursor);
                btnCode.Style.Remove(HtmlTextWriterStyle.Cursor);
                btnColor.Style.Remove(HtmlTextWriterStyle.Cursor);
                btnItalic.Style.Remove(HtmlTextWriterStyle.Cursor);
                btnStrike.Style.Remove(HtmlTextWriterStyle.Cursor);
                btnUnderline.Style.Remove(HtmlTextWriterStyle.Cursor);
                btnAdvancedImage.Style.Remove(HtmlTextWriterStyle.Cursor);
                btnAdvancedUrl.Style.Remove(HtmlTextWriterStyle.Cursor);

                btnUrl.Style.Add(HtmlTextWriterStyle.Cursor, "default");
                btnImage.Style.Add(HtmlTextWriterStyle.Cursor, "default");
                btnQuote.Style.Add(HtmlTextWriterStyle.Cursor, "default");
                btnBold.Style.Add(HtmlTextWriterStyle.Cursor, "default");
                btnCode.Style.Add(HtmlTextWriterStyle.Cursor, "default");
                btnColor.Style.Add(HtmlTextWriterStyle.Cursor, "default");
                btnItalic.Style.Add(HtmlTextWriterStyle.Cursor, "default");
                btnStrike.Style.Add(HtmlTextWriterStyle.Cursor, "default");
                btnUnderline.Style.Add(HtmlTextWriterStyle.Cursor, "default");
                btnAdvancedImage.Style.Add(HtmlTextWriterStyle.Cursor, "default");
                btnAdvancedUrl.Style.Add(HtmlTextWriterStyle.Cursor, "default");

                btnUrl.CssClass = CssClass;
                btnImage.CssClass = CssClass;
                btnQuote.CssClass = CssClass;
                btnBold.CssClass = CssClass;
                btnCode.CssClass = CssClass;
                btnColor.CssClass = CssClass;
                btnItalic.CssClass = CssClass;
                btnStrike.CssClass = CssClass;
                btnUnderline.CssClass = CssClass;
                btnAdvancedImage.CssClass = CssClass;
                btnAdvancedUrl.CssClass = CssClass;
            }
            btnUrl.Enabled = enabled;
            btnImage.Enabled = enabled;
            btnQuote.Enabled = enabled;
            btnBold.Enabled = enabled;
            btnCode.Enabled = enabled;
            btnColor.Enabled = enabled;
            btnItalic.Enabled = enabled;
            btnStrike.Enabled = enabled;
            btnUnderline.Enabled = enabled;
            btnAdvancedImage.Enabled = enabled;
            btnAdvancedUrl.Enabled = enabled;
        }


        /// <summary>
        /// Renders the control at run-time.
        /// </summary>
        protected override void CreateChildControls()
        {
            RegisterScripts();

            Controls.Add(btnUrl);
            Controls.Add(btnAdvancedUrl);
            Controls.Add(btnImage);
            Controls.Add(btnAdvancedImage);
            Controls.Add(btnQuote);
            Controls.Add(btnCode);

            // Put separator here if needed
            if ((ShowURL || ShowImage || ShowQuote || ShowCode || ShowAdvancedImage || ShowAdvancedURL) &&
                (ShowBold || ShowItalic || ShowUnderline || ShowStrike || ShowColor))
            {
                Controls.Add(new LiteralControl("<span class=\"DiscussionButtonsSeparator\">&nbsp;</span>"));
            }

            Controls.Add(btnBold);
            Controls.Add(btnItalic);
            Controls.Add(btnUnderline);
            Controls.Add(btnStrike);
            Controls.Add(btnColor);

            btnUrl.Visible = ShowURL;
            btnImage.Visible = ShowImage;
            btnQuote.Visible = ShowQuote;
            btnBold.Visible = ShowBold;
            btnItalic.Visible = ShowItalic;
            btnUnderline.Visible = ShowUnderline;
            btnStrike.Visible = ShowStrike;
            btnColor.Visible = ShowColor;
            btnCode.Visible = ShowCode;
            btnAdvancedImage.Visible = ShowAdvancedImage;
            btnAdvancedUrl.Visible = ShowAdvancedURL;

            btnUrl.ImageUrl = UIHelper.GetImageUrl(Page, "Design/Controls/DiscussionButtons/inserturl.gif");
            btnImage.ImageUrl = UIHelper.GetImageUrl(Page, "Design/Controls/DiscussionButtons/insertimage.gif");
            btnQuote.ImageUrl = UIHelper.GetImageUrl(Page, "Design/Controls/DiscussionButtons/insertquote.gif");
            btnBold.ImageUrl = UIHelper.GetImageUrl(Page, "Design/Controls/DiscussionButtons/insertbold.gif");
            btnItalic.ImageUrl = UIHelper.GetImageUrl(Page, "Design/Controls/DiscussionButtons/insertitalic.gif");
            btnUnderline.ImageUrl = UIHelper.GetImageUrl(Page, "Design/Controls/DiscussionButtons/insertunderline.gif");
            btnStrike.ImageUrl = UIHelper.GetImageUrl(Page, "Design/Controls/DiscussionButtons/insertstrike.gif");
            btnColor.ImageUrl = UIHelper.GetImageUrl(Page, "Design/Controls/DiscussionButtons/insertcolor.gif");
            btnCode.ImageUrl = UIHelper.GetImageUrl(Page, "Design/Controls/DiscussionButtons/insertcode.gif");
            btnAdvancedUrl.ImageUrl = UIHelper.GetImageUrl(Page, "Design/Controls/DiscussionButtons/inserturl.gif");
            btnAdvancedImage.ImageUrl = UIHelper.GetImageUrl(Page, "Design/Controls/DiscussionButtons/insertimage.gif");

            btnUrl.Style.Add(HtmlTextWriterStyle.Cursor, "pointer");
            btnImage.Style.Add(HtmlTextWriterStyle.Cursor, "pointer");
            btnQuote.Style.Add(HtmlTextWriterStyle.Cursor, "pointer");
            btnBold.Style.Add(HtmlTextWriterStyle.Cursor, "pointer");
            btnItalic.Style.Add(HtmlTextWriterStyle.Cursor, "pointer");
            btnUnderline.Style.Add(HtmlTextWriterStyle.Cursor, "pointer");
            btnStrike.Style.Add(HtmlTextWriterStyle.Cursor, "pointer");
            btnColor.Style.Add(HtmlTextWriterStyle.Cursor, "pointer");
            btnCode.Style.Add(HtmlTextWriterStyle.Cursor, "pointer");
            btnAdvancedImage.Style.Add(HtmlTextWriterStyle.Cursor, "pointer");
            btnAdvancedUrl.Style.Add(HtmlTextWriterStyle.Cursor, "pointer");

            btnUrl.ToolTip = ResHelper.GetString("bbeditor.inserturl");
            btnImage.ToolTip = ResHelper.GetString("bbeditor.insertimage");
            btnQuote.ToolTip = ResHelper.GetString("bbeditor.insertquote");
            btnBold.ToolTip = ResHelper.GetString("bbeditor.insertbold");
            btnItalic.ToolTip = ResHelper.GetString("bbeditor.insertitalic");
            btnUnderline.ToolTip = ResHelper.GetString("bbeditor.insertunderline");
            btnStrike.ToolTip = ResHelper.GetString("bbeditor.insertstrike");
            btnColor.ToolTip = ResHelper.GetString("bbeditor.insertcolor");
            btnCode.ToolTip = ResHelper.GetString("bbeditor.insertcode");
            btnAdvancedImage.ToolTip = ResHelper.GetString("bbeditor.insertimage");
            btnAdvancedUrl.ToolTip = ResHelper.GetString("bbeditor.inserturl");

            btnUrl.AlternateText = ResHelper.GetString("bbeditor.inserturl");
            btnImage.AlternateText = ResHelper.GetString("bbeditor.insertimage");
            btnQuote.AlternateText = ResHelper.GetString("bbeditor.insertquote");
            btnBold.AlternateText = ResHelper.GetString("bbeditor.insertbold");
            btnItalic.AlternateText = ResHelper.GetString("bbeditor.insertitalic");
            btnUnderline.AlternateText = ResHelper.GetString("bbeditor.insertunderline");
            btnStrike.AlternateText = ResHelper.GetString("bbeditor.insertstrike");
            btnColor.AlternateText = ResHelper.GetString("bbeditor.insertcolor");
            btnCode.AlternateText = ResHelper.GetString("bbeditor.insertcode");
            btnAdvancedImage.AlternateText = ResHelper.GetString("bbeditor.insertimage");
            btnAdvancedUrl.AlternateText = ResHelper.GetString("bbeditor.inserturl");

            btnUrl.OnClientClick = "InsertTwoPrompts('" + ControlID + "'," + ((UsePromptDialog) ? "1" : "0") +
                                   ", 'url', " + ScriptHelper.GetLocalizedString("bbeditor.inserturl.url") + ", '" +
                                   ResHelper.GetString("bbeditor.inserturl.desc") + "', true); return false;";
            btnImage.OnClientClick = "InsertOnePrompt('" + ControlID + "'," + ((UsePromptDialog) ? "1" : "0") +
                                     ", 'img', " + ScriptHelper.GetLocalizedString("bbeditor.insertimage.url") + ", GetDiscussionMacroSelection('" + ControlID + "','http://'), true, false); return false;";
            btnColor.OnClientClick = "InsertOnePrompt('" + ControlID + "'," + ((UsePromptDialog) ? "1" : "0") +
                                     ", 'color', " + ScriptHelper.GetLocalizedString("bbeditor.insertimage.color") + ", '', false, true); return false;";
            btnQuote.OnClientClick = "InsertNoPrompt('" + ControlID + "', 'quote', false); return false;";
            btnBold.OnClientClick = "InsertNoPrompt('" + ControlID + "','b', false); return false;";
            btnItalic.OnClientClick = "InsertNoPrompt('" + ControlID + "','i', false); return false;";
            btnStrike.OnClientClick = "InsertNoPrompt('" + ControlID + "','s', false); return false;";
            btnUnderline.OnClientClick = "InsertNoPrompt('" + ControlID + "', 'u', false); return false;";
            btnCode.OnClientClick = "InsertNoPrompt('" + ControlID + "','code', false); return false;";

            // Configure Image dialog
            ImageDialogConfig.OutputFormat = OutputFormatEnum.BBMedia;
            ImageDialogConfig.SelectableContent = SelectableContentEnum.OnlyImages;
            ImageDialogConfig.EditorClientID = ControlID;
            string width = ImageDialogConfig.DialogWidth.ToString();
            string height = ImageDialogConfig.DialogHeight.ToString();
            if (ImageDialogConfig.UseRelativeDimensions)
            {
                width += "%";
                height += "%";
            }

            string url = CMSDialogHelper.GetDialogUrl(ImageDialogConfig, IsLiveSite, false, null, false);
            btnAdvancedImage.OnClientClick = string.Format("modalDialog('{0}', 'InsertImage', '{1}', '{2}'); return false;", url, width, height);

            // Configure URL dialog
            URLDialogConfig.OutputFormat = OutputFormatEnum.BBLink;
            ImageDialogConfig.SelectableContent = SelectableContentEnum.AllContent;
            URLDialogConfig.EditorClientID = ControlID;
            width = URLDialogConfig.DialogWidth.ToString();
            height = URLDialogConfig.DialogHeight.ToString();
            if (URLDialogConfig.UseRelativeDimensions)
            {
                width += "%";
                height += "%";
            }
            url = CMSDialogHelper.GetDialogUrl(URLDialogConfig, IsLiveSite, false, null, false);
            btnAdvancedUrl.OnClientClick = string.Format("modalDialog('{0}', 'InsertLink', '{1}', '{2}'); return false;", url, width, height);
        }


        /// <summary>
        /// OnInit event.
        /// </summary>
        /// <param name="e">Event arguments</param>
        protected override void OnInit(EventArgs e)
        {
            // Initialize buttons
            InititlaizeButtons(Enabled);

            if (CssClass == "")
            {
                CssClass = "DiscussionButtons";
            }
            base.OnInit(e);
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Registers necessary scripts for macro inserting.
        /// </summary>
        private void RegisterScripts()
        {
            // Register general script for text insertion into TextArea
            string script =
                // Gets the currently selected text
                "function GetDiscussionMacroSelection(objId, defaultValue) {\n" +
                "   var obj = document.getElementById(objId);\n" +
                "   var text = null;\n" +
                "   if (obj != null) {\n" +
                "      if (document.selection) {\n" +
                // IE
                "           obj.focus();\n" +
                "           var range = document.selection.createRange();\n" +
                "           if (range.parentElement() != obj) {\n" +
                "               return defaultValue;\n" +
                "           }\n" +
                "           return range.text;\n" +
                "       } else {\n" +
                // Firefox
                "           var start = obj.selectionStart;\n" +
                "           var end   = obj.selectionEnd;\n" +
                "           return obj.value.substr(start, end - start);\n" +
                "           }\n" +
                "       }\n" +
                "   return defaultValue; " +
                "}\n" +
                // Surrounds the selected text with textBefore and textAfer
                "function InsertDiscussionMacro(textBefore, textAfter, objId, replaceSelection) {\n" +
                "   var obj = document.getElementById(objId);\n" +
                "   var text = null;\n" +
                "   if (obj != null) {\n" +
                "      if (document.selection) {\n" +
                // IE
                "           obj.focus();\n" +
                "           var range = document.selection.createRange();\n" +
                "           if (range.parentElement() != obj) {\n" +
                "               return false;\n" +
                "           }\n" +
                "           if (replaceSelection) {\n" +
                "              range.text = textBefore + textAfter;\n" +
                "           } else {\n" +
                "              range.text = textBefore + range.text + textAfter;\n" +
                "           }\n" +
                "       } else {\n" +
                // Firefox
                "           var start = obj.selectionStart;\n" +
                "           var end   = obj.selectionEnd;\n" +
                "           if (replaceSelection) {\n" +
                "               obj.value = obj.value.substr(0, start) + textBefore + textAfter + obj.value.substr(end, obj.value.length);\n" +
                "           } else {\n" +
                "               obj.value = obj.value.substr(0, start) + textBefore + obj.value.substr(start, end - start) + textAfter + obj.value.substr(end, obj.value.length);\n" +
                "           }\n" +
                "           if (start != null) {\n" +
                "               setCaretTo(obj, end + textBefore.length + textAfter.length);\n" +
                "           } else {\n" +
                "               obj.value += textBefore + textAfter;\n" +
                "           }\n" +
                "       }\n" +
                "   }\n" +
                "}\n" +
                // Sets the location of the cursor in specified object to given position
                "function setCaretTo(obj, pos) {	\n" +
                "if (obj != null) {\n" +
                "if (obj.createTextRange) {\n" +
                "var range = obj.createTextRange();\n" +
                "range.move('character', pos);\n" +
                "range.select();\n" +
                "} else if(obj.selectionStart) {\n" +
                "obj.focus();\n" +
                "obj.setSelectionRange(pos, pos);\n" +
                "}\n" +
                "}\n" +
                "}\n" +
                // URL-like script (two prompts)
                "function InsertTwoPrompts(objId, usePrompt, tagName, promptText1, promptText2, replaceSelection) {	\n" +
                "  if (usePrompt == 1) { \n" +
                "    var url = prompt(promptText1, 'http://'); \n" +
                "    if (url != null) { \n" +
                "        var desc = prompt(promptText2, GetDiscussionMacroSelection(objId,'')); \n" +
                "        if (desc == null) { desc = '' } \n" +
                "        InsertDiscussionMacro('[' + tagName + '=' + url + ']' + desc + '[/' + tagName + ']', '', objId, true); \n" +
                "    }\n" +
                "  } else {\n" +
                "    InsertDiscussionMacro('[' + tagName + '=' + GetDiscussionMacroSelection(objId,'') + ']', '[/' + tagName + ']', objId, replaceSelection); \n" +
                "  } \n" +
                "}\n" +
                // Image-like script (one prompt)
                "function InsertOnePrompt(objId, usePrompt, tagName, promptText, promptDefaultValue, replaceSelection, parameterPrompt) {	\n" +
                "  if (usePrompt == 1) { \n" +
                "    var url = prompt(promptText, promptDefaultValue); \n" +
                "    if (url != null) { \n" +
                "        if (parameterPrompt) { \n" +
                "            InsertDiscussionMacro('[' + tagName + '=' + url + ']', '[/' + tagName + ']', objId, replaceSelection); \n" +
                "        } else { \n" +
                "            InsertDiscussionMacro('[' + tagName + ']' + url, '[/' + tagName + ']', objId, replaceSelection); \n" +
                "        } \n" +
                "    }\n" +
                "  } else {\n" +
                "     InsertDiscussionMacro('[' + tagName + '='+ promptDefaultValue +']', '[/' + tagName + ']', objId, replaceSelection); \n" +
                "  } \n" +
                "}\n" +
                // Quote-like script (no prompt)
                "function InsertNoPrompt(objId, tagName, replaceSelection) {	\n" +
                "   InsertDiscussionMacro('[' + tagName + ']', '[/' + tagName + ']', objId, replaceSelection); \n" +
                "}\n";

            ScriptHelper.RegisterClientScriptBlock(this, typeof(string), "DiscussionMacroButtons", ScriptHelper.GetScript(script));
            ScriptHelper.RegisterDialogScript(Page);
        }

        #endregion
    }
}
