using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;

using AjaxControlToolkit;

using CMS.EventLog;
using CMS.Helpers;
using CMS.MacroEngine;
using CMS.Membership;
using CMS.PortalEngine;
using CMS.SiteProvider;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// CMSTextBox, inherited from System.Web.UI.WebControls.TextBox. Handles macro security parameters.
    /// </summary>
    [ToolboxItem(false)]
    public class CMSTextBox : TextBox
    {
        #region "Variables"

        // Watermark extender
        private TextBoxWatermarkExtender mExWatermark;

        // IsLiveSite backing field
        private bool? mIsLiveSite;

        #endregion


        #region "Properties"

        /// <summary>
        /// Dictionary containing macro identity options.
        /// </summary>
        private Dictionary<string, MacroIdentityOption> Signatures
        {
            get
            {
                var signatures = (Dictionary<string, MacroIdentityOption>)ViewState["Signatures"];
                if (signatures == null)
                {
                    signatures = new Dictionary<string, MacroIdentityOption>();
                    ViewState["Signatures"] = signatures;
                }
                return signatures;
            }
        }


        /// <summary>
        /// Indicates if security parameters in macros are handled automatically.
        /// </summary>
        public bool ProcessMacroSecurity
        {
            get;
            set;
        } = true;


        /// <summary>
        /// Indicates if macro expressions should be decoded before security parameters are computed. Default <c>False</c>.
        /// </summary>
        internal bool DecodeMacrosBeforeSign
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if control is used on live site.
        /// </summary>
        public virtual bool IsLiveSite
        {
            get
            {
                if (mIsLiveSite == null)
                {
                    // Try to get the property value from parent controls
                    mIsLiveSite = ControlsHelper.GetParentProperty<AbstractUserControl, bool>(this, s => s.IsLiveSite, !(Page is IAdminPage));
                }

                return mIsLiveSite.Value;
            }
            set
            {
                mIsLiveSite = value;
            }
        }


        /// <summary>
        /// If true, the value is processed as a macro (signed as a whole)
        /// </summary>
        public bool ValueIsMacro
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the text content of the System.Web.UI.WebControls.TextBox control.
        /// If ProcessMacroSecurity is true, macro security parameters are processed.       
        /// </summary>
        /// <remarks>
        /// IMPORTANT: keyword "new" is at use to hide macro signatures from output. 
        /// In case of "override" keyword, macros in CMSTextArea controls (such as the transformation editor) will be display with full signature (hash).
        /// </remarks>
        public new string Text
        {
            get
            {
                var output = base.Text;

                // Return empty string if text is equal to watermark text
                if (String.Equals(output, MacroResolver.Resolve(WatermarkText), StringComparison.InvariantCulture))
                {
                    return string.Empty;
                }

                if (ProcessMacroSecurity && !MacroStaticSettings.AllowOnlySimpleMacros && !string.IsNullOrEmpty(output))
                {
                    // Add the macro signatures
                    output = AddMacroSignatures(output, out var containsMacro);

                    // Remove the signature and sign as anonymous if the signed macro exceeds the MaxLength 
                    if (containsMacro && MaxLength > 0 && output.Length > MaxLength)
                    {
                        EventLogProvider.LogWarning("MacroResolver", "SIGNATURE", null, SiteContext.CurrentSiteID, $"Content of textbox '{ID}' exceeds the maximum length {MaxLength} after macro signing: '{output}'. Macros were saved as anonymous macros. This can result in security issues during macro resolution or other unexpected behavior.");

                        output = MacroSecurityProcessor.RemoveSecurityParametersAndAnonymize(output, Signatures);
                    }
                }

                output = XmlHelper.RemoveIllegalCharacters(output);

                return output;
            }
            set
            {
                // Remove macro signatures
                var input = RemoveMacroSignatures(value, true);
                base.Text = input;
            }
        }


        /// <summary>
        /// Gets or sets whether autocomplete is enabled.
        /// </summary>
        public bool EnableAutoComplete
        {
            get;
            set;
        } = true;

        #endregion


        #region "Watermark properties"

        /// <summary>
        /// The text to show when the control has no value.
        /// </summary>
        public string WatermarkText
        {
            get;
            set;
        }


        /// <summary>
        /// The CSS class to apply to the TextBox when it has no value (e.g. the watermark text is shown).
        /// </summary>
        public string WatermarkCssClass
        {
            get;
            set;
        } = "WatermarkText";

        #endregion


        #region "Page events"

        /// <summary>
        /// Initializes control
        /// </summary>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            // Ensure form control class
            this.AddCssClass("form-control");
        }


        /// <summary>
        /// OnPreRender override
        /// </summary>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            HandleWatermark();

            // Register tooltip for macro length warning
            if (ProcessMacroSecurity && !MacroStaticSettings.AllowOnlySimpleMacros && !IsLiveSite)
            {
                ScriptHelper.RegisterBootstrapTooltip(Page, ".macro-length-warning i");
            }

            if (!EnableAutoComplete)
            {
                Attributes.Add("AutoComplete", "Off");
            }

            // Ensure form control class
            this.AddCssClass("form-control");
        }


        /// <summary>
        /// Renders the text box control. If length exceeds MaxLength because of macros, warning icon is displayed.
        /// </summary>
        /// <param name="writer">HtmlTextWriter object</param>
        protected override void Render(HtmlTextWriter writer)
        {
            // Render watermark extender explicitly if is defined
            mExWatermark?.RenderControl(writer);

            // Render textbox
            base.Render(writer);

            // Macro length warning processing
            if ((MaxLength > 0) && ProcessMacroSecurity && !MacroStaticSettings.AllowOnlySimpleMacros && !IsLiveSite)
            {
                string text = Text;
                if (MacroProcessor.ContainsMacro(text) && (text.Length > MaxLength))
                {
                    string warning = ResHelper.GetString("cmstextbox.macrotoolong");
                    var macroWarning = String.Format(@"<span class=""form-control-text macro-length-warning"" >{0}</span>", UIHelper.GetAccessibleIconTag("icon-exclamation-triangle	", warning, FontIconSizeEnum.NotDefined, "warning-icon"));
                    writer.Write(macroWarning);
                }
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Handles the watermark functionality
        /// </summary>
        private void HandleWatermark()
        {
            // Watermark extender
            // Disable watermark extender for nonempty fields (issue with value which is same as the watermark text)
            string resolvedWatermarkText = MacroResolver.Resolve(WatermarkText);
            if (!string.IsNullOrEmpty(WatermarkText) && !String.IsNullOrEmpty(resolvedWatermarkText) && !CMSString.Equals(Text, WatermarkText))
            {
                // Ensure the script manager on the page
                ControlsHelper.EnsureScriptManager(Page);

                // Create extender
                mExWatermark = new TextBoxWatermarkExtender();
                mExWatermark.ID = ID + "_exWatermark";
                mExWatermark.TargetControlID = ID;
                mExWatermark.EnableViewState = false;

                Controls.Add(mExWatermark);

                // Initialize extender
                mExWatermark.WatermarkText = resolvedWatermarkText;

                if (!String.IsNullOrEmpty(WatermarkCssClass))
                {
                    mExWatermark.WatermarkCssClass = CssClass + " " + WatermarkCssClass;
                }
            }
        }


        /// <summary>
        /// Removes the macro signatures from the input text
        /// </summary>
        /// <param name="input">Input text</param>
        /// <param name="replaceWithHash">If true, macros which contained security information will be returned with hash at the end</param>
        private string RemoveMacroSignatures(string input, bool replaceWithHash)
        {
            if (ProcessMacroSecurity)
            {
                if (!string.IsNullOrEmpty(input))
                {
                    bool containsMacro;

                    // If value is macro, convert to full macro expression
                    if (ValueIsMacro)
                    {
                        input = "{%" + input + "%}";
                        containsMacro = true;
                    }
                    else
                    {
                        containsMacro = input.Contains("{");
                    }

                    // Remove the signatures
                    if (containsMacro)
                    {
                        input = MacroSecurityProcessor.RemoveSecurityParameters(input, replaceWithHash, Signatures);
                    }
                    else
                    {
                        Signatures.Clear();
                    }

                    // If value is macro, trim back the full expression
                    if (ValueIsMacro)
                    {
                        input = input.Substring(2, input.Length - 4);
                    }
                }
                else
                {
                    Signatures.Clear();
                }
            }
            return input;
        }


        /// <summary>
        /// Adds macro signatures to the output
        /// </summary>
        private string AddMacroSignatures(string output, out bool containsMacro)
        {
            // If value is macro, convert to full macro expression
            if (ValueIsMacro)
            {
                output = "{%" + output + "%}";
                containsMacro = true;
            }
            else
            {
                containsMacro = output.Contains("{");
            }

            if (containsMacro)
            {
                // Add security parameters
                var identityOption = MacroIdentityOption.FromUserInfo(MembershipContext.AuthenticatedUser);
                output = MacroSecurityProcessor.AddSecurityParameters(output, identityOption, Signatures, DecodeMacrosBeforeSign);
            }

            // If value is macro, trim back the full expression
            if (ValueIsMacro)
            {
                output = output.Substring(2, output.Length - 4);
            }

            return output;
        }

        #endregion
    }
}
