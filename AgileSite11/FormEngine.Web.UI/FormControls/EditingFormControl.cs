using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using CMS.Base;
using CMS.Base.Web.UI;
using CMS.DataEngine;
using CMS.EventLog;
using CMS.Helpers;
using CMS.MacroEngine;
using CMS.Modules;
using CMS.Protection;
using CMS.SiteProvider;

namespace CMS.FormEngine.Web.UI
{
    /// <summary>
    /// EditingFormControl control.
    /// </summary>
    [ToolboxItem(false)]
    public class EditingFormControl : FormEngineWebControl, ICallbackEventHandler
    {
        #region "Variables"

        private const string LABEL_CONTROL = "LabelControl";

        private CMSAccessibleButton mMacroEditButton;
        private CMSAccessibleButton mRemoveMacroButton;

        private readonly Panel mNestedControlPanel = new Panel();

        private Control mNestedControl;
        private FormEngineUserControl mMacroTextBox;
        private Panel mMacroTextBoxPanel;
        private Hashtable mMacroTable;
        private bool mIsLiveSite = true;
        private string mItemName;
        private string mResolverName;

        private BadWordActionEnum mBadWordsResult = BadWordActionEnum.None;
        private readonly Hashtable mBadWordsList = null;

        private object mValue = DBNull.Value;
        private MacroResolver mContextResolver;

        /// <summary>
        /// Indicates that control initialization should be called on  Page.InitComplete event.
        /// </summary>
        private bool mInitOnPageInitComplete;


        /// <summary>
        /// Indicates if the control initialization was completed.
        /// </summary>
        private bool mInitCompleted;

        #endregion


        #region "Properties"

        /// <summary>
        /// Enabled.
        /// </summary>
        public override bool Enabled
        {
            get
            {
                return base.Enabled;
            }
            set
            {
                base.Enabled = value;

                // Enable / disable nested control
                var control = NestedControl as IFormControl;
                if (control != null)
                {
                    control.Enabled = value;
                }
            }
        }


        /// <summary>
        /// Determines whether the value is macro or not.
        /// </summary>
        public bool IsMacro
        {
            get
            {
                return ((AllowMacroEditing || IsDefaultValueAndMacro) && (mMacroTextBox != null) && !String.IsNullOrEmpty(Convert.ToString(mMacroTextBox.Value)));
            }
        }


        /// <summary>
        /// Result of bad word check.
        /// </summary>
        public BadWordActionEnum BadWordsResult
        {
            get
            {
                return mBadWordsResult;
            }
        }


        /// <summary>
        /// List of found bad words.
        /// </summary>
        public Hashtable BadWordsList
        {
            get
            {
                return mBadWordsList;
            }
        }


        /// <summary>
        /// Determines whether to display the "Edit macro" button.
        /// </summary>
        public bool AllowMacroEditing
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets macro resolver name.
        /// </summary>
        public string ResolverName
        {
            get
            {
                if (string.IsNullOrEmpty(mResolverName))
                {
                    mResolverName = ContextResolver.ResolverName;
                }

                return mResolverName;
            }
            set
            {
                mResolverName = value;
                if (mMacroTextBox != null)
                {
                    mMacroTextBox.ResolverName = mResolverName;
                }
            }
        }


        /// <summary>
        /// Gets or sets macro resolver for given control.
        /// </summary>
        public virtual MacroResolver ContextResolver
        {
            get
            {
                return mContextResolver ?? (mContextResolver = CreateContextResolver(GetParentResolver(), true));
            }
            set
            {
                mContextResolver = CreateContextResolver(value, false);
            }
        }


        /// <summary>
        /// Gets or sets the value to the nested control.
        /// </summary>
        public override object Value
        {
            get
            {
                if (IsMacro)
                {
                    // Return macro value if present
                    return mMacroTextBox.Value;
                }

                return AbstractBasicForm.GetControlValue(mNestedControl) ?? mValue;
            }
            set
            {
                SetControlValue(mNestedControl, value);
            }
        }


        /// <summary>
        /// Returns value prepared for validation.
        /// </summary>
        public override object ValueForValidation
        {
            get
            {
                if (!IsMacro)
                {
                    IFormControl control = NestedControl as IFormControl;
                    if (control != null)
                    {
                        return control.ValueForValidation;
                    }
                }

                return base.ValueForValidation;
            }
        }


        /// <summary>
        /// Original control value
        /// </summary>
        protected virtual object OriginalValue
        {
            get
            {
                return ViewState["OriginalValue"];
            }
            set
            {
                ViewState["OriginalValue"] = value;
            }
        }


        /// <summary>
        /// Redirects to NestedControl GetOtherValues() or returns null if nested control is not FormEngineUserControl.
        /// </summary>
        public override object[,] GetOtherValues()
        {
            IFormControl control = NestedControl as IFormControl;
            if (control != null)
            {
                return control.GetOtherValues();
            }
            return null;
        }


        /// <summary>
        /// Gets the nested control.
        /// </summary>
        public Control NestedControl
        {
            get
            {
                return mNestedControl;
            }
        }


        /// <summary>
        /// Gets the button control which shows the modal dialog with macro editor.
        /// </summary>
        public CMSAccessibleButton MacroEditButton
        {
            get
            {
                return mMacroEditButton;
            }
        }


        /// <summary>
        /// Gets the button control for removing macro.
        /// </summary>
        public CMSAccessibleButton RemoveMacroButton
        {
            get
            {
                return mRemoveMacroButton;
            }
        }


        /// <summary>
        /// Gets the Panel where the nested control is placed.
        /// </summary>
        public Panel NestedControlPanel
        {
            get
            {
                return mNestedControlPanel;
            }
        }


        /// <summary>
        /// Macro table.
        /// </summary>
        public Hashtable MacroTable
        {
            get
            {
                if ((mMacroTable == null) && (Form != null))
                {
                    mMacroTable = Form.MacroTable;
                }
                return mMacroTable;
            }
        }


        /// <summary>
        /// Data to be edited.
        /// </summary>
        public IDataContainer Data
        {
            get
            {
                if (Form != null)
                {
                    return Form.Data;
                }

                return null;
            }
        }


        /// <summary>
        /// Data value.
        /// </summary>
        public object DataValue
        {
            get
            {
                if (Form != null && FieldInfo != null)
                {
                    return Form.GetDataValue(FieldInfo.Name);
                }

                return null;
            }
        }


        /// <summary>
        /// Is insert mode.
        /// </summary>
        public bool IsInsertMode
        {
            get
            {
                if (Form != null)
                {
                    return Form.IsInsertMode;
                }

                return false;
            }
        }


        /// <summary>
        /// Mode of the form.
        /// </summary>
        public FormModeEnum Mode
        {
            get
            {
                if (Form != null)
                {
                    return Form.Mode;
                }

                return FormModeEnum.Update;
            }
        }


        /// <summary>
        /// Field type.
        /// </summary>
        public FormFieldControlTypeEnum FieldType
        {
            get
            {
                return FieldInfo.FieldType;
            }

            set
            {
                FieldInfo.FieldType = value;
            }
        }


        /// <summary>
        /// Indicates if the control is on live site.
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
        /// Gets ClientID of the control from which the Value is retrieved or 
        /// null if such a control can't be specified.
        /// </summary>
        public override string ValueElementID
        {
            get
            {
                if (mNestedControl != null)
                {
                    return mNestedControl.ClientID;
                }
                return null;
            }
        }


        /// <summary>
        /// Returns true if default value of formfield is macro expression and current value is the default value.
        /// </summary>
        public bool IsDefaultValueAndMacro
        {
            get
            {
                if ((FieldInfo != null) && (Form != null))
                {
                    bool isMacro;
                    string defValue = FieldInfo.GetPropertyValue(FormFieldPropertyEnum.DefaultValue, out isMacro);

                    return ((isMacro || MacroProcessor.ContainsMacro(defValue)) && (MacroSecurityProcessor.RemoveSecurityParameters(defValue, false, null) == MacroSecurityProcessor.RemoveSecurityParameters(ValidationHelper.GetString(Form.GetDataValue(FieldInfo.Name), null), false, null)));
                }

                return false;
            }
        }


        /// <summary>
        /// Field name to which the field belongs. This property is only supported in templated form.
        /// </summary>
        public string Field
        {
            get;
            set;
        }


        /// <summary>
        /// Form control code name.
        /// </summary>
        public string FormControlName
        {
            get
            {
                return GetControlValue("controlname");
            }
            set
            {
                ViewState["controlname"] = value;
            }
        }


        /// <summary>
        /// Form control path.
        /// </summary>
        public string FormControlPath
        {
            get
            {
                return GetControlValue("controlpath");
            }
            set
            {
                ViewState["controlpath"] = value;
            }
        }


        /// <summary>
        /// Name of the edited item.
        /// </summary>
        protected string ItemName
        {
            get
            {
                if (!string.IsNullOrEmpty(mItemName))
                {
                    return mItemName;
                }

                if (ViewState["ItemName"] != null)
                {
                    mItemName = Convert.ToString(ViewState["ItemName"]);
                    return mItemName;
                }
                
                if (!string.IsNullOrEmpty(FieldInfo.Name))
                {
                    mItemName = FieldInfo.Name.ToLowerInvariant();
                    return mItemName;
                }

                mItemName = Guid.NewGuid().ToString();
                return mItemName;
            }
            set
            {
                mItemName = value;
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor.
        /// </summary>
        public EditingFormControl()
        {
            FieldInfo = new FormFieldInfo
            {
                AllowEmpty = true
            };

            FieldType = FormFieldControlTypeEnum.CustomUserControl;
            mMacroTable = new Hashtable(StringComparer.InvariantCultureIgnoreCase);
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="ffi">Form field info</param>
        /// <param name="form">Parent form</param>
        public EditingFormControl(FormFieldInfo ffi, BasicForm form)
        {
            FieldInfo = ffi;
            Form = form;
        }

        #endregion


        #region "Control events"

        /// <summary>
        /// Init event handler.
        /// </summary>
        protected override void OnInit(EventArgs e)
        {
            if (!RequestHelper.IsPostBack())
            {
                mInitOnPageInitComplete = true;
                EnsureChildControls();
            }

            base.OnInit(e);
        }


        /// <summary>
        /// Customized LoadViewState.
        /// </summary>
        protected override void LoadViewState(object savedState)
        {
            base.LoadViewState(((Pair)savedState).First);

            EnsureChildControls();
        }


        /// <summary>
        /// Customized SaveViewState.
        /// </summary>
        protected override object SaveViewState()
        {
            // Store the item name in the view state
            ViewState["ItemName"] = ItemName;

            return new Pair(base.SaveViewState(), null);
        }


        /// <summary>
        /// Renders the control at run-time.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            if (mInitOnPageInitComplete)
            {
                PageContext.BeforeInitComplete += Page_InitComplete;
            }
            else
            {
                InitializeControl();
            }
        }


        /// <summary>
        /// Event handler of page's init complete.
        /// </summary>
        protected void Page_InitComplete(object sender, EventArgs e)
        {
            InitializeControl();
        }


        /// <summary>
        /// PreRender event handler.
        /// </summary>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            // Register scripts only if macro editing is enabled
            if (AllowMacroEditing && (MacroTable != null))
            {
                string url = Page.ResolveUrl("~/CMSAdminControls") + "/EditingFormControl.aspx";

                // Create the script for editing the macro value
                string setMacroScript = @"
function setNestedControlValue(macroTextBoxId, controlPanelId, controlValue, macroTextBoxPanelId) {
    var macroTextBox = document.getElementById(macroTextBoxId);
    var macroTextBoxPanel = document.getElementById(macroTextBoxPanelId);
    var controlPanel = document.getElementById(controlPanelId);
    if(macroTextBoxPanel != null){
        var macroEditButton = document.getElementById(macroTextBoxPanelId+'_meib');
        var removeMacroButton = document.getElementById(macroTextBoxPanelId+'_rmib');
        if ((macroTextBox != null) && (controlPanel != null) && (macroEditButton != null) && (removeMacroButton != null)) {
            if (controlValue == null || controlValue == '') {
                controlPanel.style.display = 'inline';
                macroTextBoxPanel.style.display = 'none';
                macroTextBox.value = '';
                macroEditButton.style.display = 'inline';
                removeMacroButton.style.display = 'none';
            } else {
                controlPanel.style.display = 'none';
                macroTextBoxPanel.style.display = 'inline';
                macroTextBox.value = controlValue;
                macroEditButton.style.display = 'none';
                removeMacroButton.style.display = 'inline';
            }
        }
    }
}
function getNestedControlValue(macroTextBoxId) {
    var macroTextBox = document.getElementById(macroTextBoxId);
    if (macroTextBox != null) {
        return macroTextBox.value;
    }
    return '';
}
function removeMacro(selId, controlPanelId, selPanelId) {
    setNestedControlValue(selId, controlPanelId, '', selPanelId);
}
function openMacroEditorDialog(queryParameters){
    modalDialog('" + url + @"' + queryParameters, 'EditValue', 800, 450);
}";

                // Script that binds setting nested control value to macro text box change event
                const string macroRefreshScript = @"
(function (macroTextBoxId, controlPanelId, macroTextBoxPanelId) {{
    var macroTextBox = document.getElementById(macroTextBoxId);
    if (macroTextBox != null) {{
        macroTextBox.onchange = function () {{
            var macroValue = getNestedControlValue(macroTextBoxId);
            setNestedControlValue(macroTextBoxId, controlPanelId, macroValue, macroTextBoxPanelId);
        }}
    }}
}})({0}, {1}, {2})";

                mMacroEditButton.OnClientClick = Page.ClientScript.GetCallbackEventReference(this, null, "openMacroEditorDialog", null) + ";return false;";
                mRemoveMacroButton.OnClientClick = "removeMacro(" + ScriptHelper.GetString(mMacroTextBox.ValueElementID) + ", " + ScriptHelper.GetString(mNestedControlPanel.ClientID) + ", " + ScriptHelper.GetString(mMacroTextBoxPanel.ClientID) + ");return false;";

                // Register the scripts
                ScriptHelper.RegisterDialogScript(Page);
                ScriptHelper.RegisterClientScriptBlock(this, typeof(string), "EditingFormControl_SetMacroScript", ScriptHelper.GetScript(setMacroScript));
                ScriptHelper.RegisterStartupScript(this, typeof(string), "EditingFormControl_BindMacroTextBoxChanged_" + ScriptHelper.GetString(mMacroTextBox.ValueElementID), ScriptHelper.GetScript(string.Format(macroRefreshScript, ScriptHelper.GetString(mMacroTextBox.ValueElementID), ScriptHelper.GetString(mNestedControlPanel.ClientID), ScriptHelper.GetString(mMacroTextBoxPanel.ClientID))));

                // Set correct visibility of controls according to macro settings
                EnsureMacros();
            }
        }


        /// <summary>
        /// Renders the control at design-time.
        /// </summary>
        protected override void Render(HtmlTextWriter output)
        {
            // For design mode display control ID
            if (Context == null)
            {
                output.Write("[EditingFormControl: " + ID + "]");
                return;
            }

            if (FieldInfo != null)
            {
                // Add Content before
                string contentBefore = ContextResolver.ResolveMacros(FieldInfo.GetPropertyValue(FormFieldPropertyEnum.ContentBefore));
                if (!String.IsNullOrEmpty(contentBefore))
                {
                    var literal = new LiteralControl(contentBefore);
                    literal.RenderControl(output);
                }
            }

            if (AllowMacroEditing && (MacroTable != null))
            {
                // Layout of macro edit control is created using literals, because controls that should render are chosen at design time.
                output.Write("<div class=\"macro-edit-cell\">");

                // Add macro edit buttons
                mMacroEditButton.RenderControl(output);
                mRemoveMacroButton.RenderControl(output);

                output.Write("</div><div class=\"macro-edit-control-cell\">");

                // Add control
                mNestedControlPanel.RenderControl(output);
                mMacroTextBoxPanel.RenderControl(output);

                output.Write("</div>");
            }
            else
            {
                // Render only panel with nested control
                mNestedControlPanel.RenderControl(output);
            }

            if (FieldInfo != null)
            {
                // Add explanation text div
                string explanationText = ContextResolver.ResolveMacros(FieldInfo.GetPropertyValue(FormFieldPropertyEnum.ExplanationText));
                if (!String.IsNullOrEmpty(explanationText))
                {
                    var literal = new LiteralControl(String.Format("<div class=\"{1}\">{0}</div>", explanationText, (Form != null) ? Form.ExplanationTextCssClass : "ExplanationText"));
                    literal.RenderControl(output);
                }

                // Add Content after
                string contentAfter = ContextResolver.ResolveMacros(FieldInfo.GetPropertyValue(FormFieldPropertyEnum.ContentAfter));
                if (!String.IsNullOrEmpty(contentAfter))
                {
                    var literal = new LiteralControl(contentAfter);
                    literal.RenderControl(output);
                }
            }
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Gets the control value based on the given key
        /// </summary>
        /// <param name="key">Value key</param>
        private string GetControlValue(string key)
        {
            string controlName = null;

            if (FieldInfo.Settings[key] != null)
            {
                controlName = ValidationHelper.GetString(FieldInfo.Settings[key], null);
            }
            else if (ViewState[key] != null)
            {
                controlName = ValidationHelper.GetString(ViewState[key], null);
            }

            return controlName;
        }


        /// <summary>
        /// Creates a context resolver for this form control and registers the control functionality within this resolver.
        /// </summary>
        /// <param name="resolver">Parent resolver</param>
        /// <param name="createAsChild">If true, the resolver if created as a child of the given resolver</param>
        private MacroResolver CreateContextResolver(MacroResolver resolver, bool createAsChild)
        {
            if (resolver == null)
            {
                throw new ArgumentNullException("resolver");
            }

            // Make sure this control uses its own resolver
            if (createAsChild)
            {
                resolver = resolver.CreateChild();
            }

            // Set current culture for resolving
            resolver.Culture = Thread.CurrentThread.CurrentUICulture.ToString();

            return resolver;
        }


        /// <summary>
        /// Gets the parent resolver for this control resolver
        /// </summary>
        private MacroResolver GetParentResolver()
        {
            // Get resolver from form or from current context based on availability
            return ((Form != null) ? Form.ContextResolver : MacroContext.CurrentResolver);
        }


        /// <summary>
        /// Initializes editing form control.
        /// </summary>
        /// <param name="force">Indicates if the initialization should be forced, e.g. during reload</param>
        private void InitializeControl(bool force = false)
        {
            if (mInitCompleted && !force)
            {
                return;
            }

            NestedControlPanel.Controls.Clear();
            Controls.Clear();

            mNestedControlPanel.ID = "ncp" + ItemName;

            // Initialize macro editing controls
            mMacroTextBoxPanel = new Panel();
            mMacroTextBoxPanel.ID = "mtp" + ItemName;
            Controls.Add(mMacroTextBoxPanel);

            // Create macro editing controls only if macro editing is enabled
            if (AllowMacroEditing && (MacroTable != null))
            {
                // If the default value is macro and should not be resolved, load it into the macro table
                string macroValue = ValidationHelper.GetString(MacroTable[ItemName], null);

                bool resolveMacros = (FieldInfo != null) && FieldInfo.GetResolveDefaultValue(!AllowMacroEditing);

                if (String.IsNullOrEmpty(macroValue) && IsDefaultValueAndMacro && !resolveMacros)
                {
                    if (FieldInfo != null)
                    {
                        MacroTable[ItemName] = FieldInfo.GetPropertyValue(FormFieldPropertyEnum.DefaultValue);
                    }
                }

                mMacroTextBox = Page.LoadUserControl("~/CMSFormControls/Inputs/LargeTextArea.ascx") as FormEngineUserControl;
                if (mMacroTextBox != null)
                {
                    mMacroTextBox.ID = "m" + ItemName;
                    mMacroTextBox.ResolverName = ResolverName;
                    mMacroTextBoxPanel.Controls.Add(mMacroTextBox);
                }

                // Edit button
                mMacroEditButton = new CMSAccessibleButton
                {
                    ID = mMacroTextBoxPanel.ID + "_meib",
                    ToolTip = ResHelper.GetString("EditingFormControl.EditValue"),
                    CssClass = "macro-edit-remove-button",
                    IconCssClass = "macro-edit-remove-icon icon-caret-right",
                    EnableViewState = false,
                    IconOnly = true
                };

                // Remove button
                mRemoveMacroButton = new CMSAccessibleButton
                {
                    ID = mMacroTextBoxPanel.ID + "_rmib",
                    ToolTip = ResHelper.GetString("EditingFormControl.RemoveMacro"),
                    CssClass = "macro-edit-remove-button",
                    IconCssClass = "macro-edit-remove-icon icon-times",
                    EnableViewState = false,
                    IconOnly = true
                };

                Controls.Add(mMacroEditButton);
                Controls.Add(mRemoveMacroButton);
            }

            // Create nested control, put it into panel and add it to Controls collection
            mNestedControl = CreateControl(FieldInfo);

            if (NestedControl != null)
            {
                // Add marker for macro
                var nestedCtrl = NestedControl as FormEngineUserControl;
                if (nestedCtrl != null)
                {
                    string marker = nestedCtrl.GetControlInfoMarker();

                    if (!String.IsNullOrEmpty(marker))
                    {
                        mMacroTextBoxPanel.Controls.Add(new LiteralControl(marker));
                    }
                }

                NestedControlPanel.CssClass = "EditingFormControlNestedControl editing-form-control-nested-control";
                NestedControlPanel.Controls.Add(NestedControl);

                Controls.Add(NestedControlPanel);

                // Enable / disable the nested control
                var nested = NestedControl as IFormControl;
                if (nested != null)
                {
                    nested.Enabled = Enabled;
                }

                mInitCompleted = true;
            }
        }


        /// <summary>
        /// Creates the field control.
        /// </summary>
        /// <param name="ffi">Form field info</param>
        private Control CreateControl(FormFieldInfo ffi)
        {
            if (ffi == null)
            {
                return null;
            }

            FormEngineUserControl usrControl;
            FormUserControlInfo ci = null;

            string controlName = null;

            try
            {
                var controlPath = FormControlPath;

                if (String.IsNullOrEmpty(controlPath))
                {
                    // Load based on name
                    controlName = (ffi.FieldType == FormFieldControlTypeEnum.CustomUserControl) ? FormControlName : Enum.GetName(typeof(FormFieldControlTypeEnum), ffi.FieldType);

                    ci = FormUserControlInfoProvider.GetFormUserControlInfo(controlName);

                    usrControl = InitializeFormControl(ci, controlName, ffi);
                }
                else
                {
                    // Load based on path
                    usrControl = InitializeFormControl(controlPath, ffi);

                    controlName = controlPath;
                }
            }
            catch (Exception ex)
            {
                return ReportFormControlError(controlName, ci, ex);
            }

            return usrControl;
        }


        /// <summary>
        /// Reports the form control creation error
        /// </summary>
        /// <param name="controlName">Control name</param>
        /// <param name="ci">Form control info</param>
        /// <param name="ex">Logged exception</param>
        private static Control ReportFormControlError(string controlName, FormUserControlInfo ci, Exception ex)
        {
            // Report the error
            FormControlError ctrlError = new FormControlError();
            ctrlError.FormControlName = controlName;

            bool logException = true;

            if ((ci != null) && !ResourceInfoProvider.IsResourceAvailable(ci.UserControlResourceID))
            {
                // Get the module
                var ri = ResourceInfoProvider.GetResourceInfo(ci.UserControlResourceID);
                if (ri != null)
                {
                    // Module not available
                    var error = HTMLHelper.HTMLEncode(String.Format(ResHelper.GetString("FormControl.NotInstalled"), ci.UserControlDisplayName, ri.ResourceDisplayName));
                    ctrlError.ErrorTitle = error;
                    logException = false;
                }
            }

            if (logException)
            {
                // Log the exception to event log
                ctrlError.InnerException = ex;
                EventLogProvider.LogException("FormEngine", "LoadFormControl", ex);
            }

            return ctrlError;
        }


        /// <summary>
        /// Creates and initializes form control by given path and field information.
        /// </summary>
        /// <param name="controlPath">Control path</param>
        /// <param name="ffi">FormFieldInfo with settings for control</param>
        /// <returns>Returns initialized form control</returns>
        private FormEngineUserControl InitializeFormControl(string controlPath, FormFieldInfo ffi)
        {
            var usrControl = LoadFormControl(controlPath);

            InitializeControl(ffi, usrControl);

            return usrControl;
        }


        /// <summary>
        /// Creates and initializes form control by given form control code name and initializes it by provided parameters.
        /// </summary>
        /// <param name="ci">Control info to load</param>
        /// <param name="controlCodeName">FormUserControlInfo code name</param>
        /// <param name="ffi">FormFieldInfo with settings for control</param>
        /// <returns>Returns initialized form control</returns>
        private FormEngineUserControl InitializeFormControl(FormUserControlInfo ci, string controlCodeName, FormFieldInfo ffi)
        {
            FormEngineUserControl ctrl = null;

            // Load form control with default values
            if (ci != null)
            {
                string formProperties = ci.UserControlMergedParameters;
                string originalControlName = ci.UserControlCodeName;

                if (ci.UserControlParentID > 0)
                {
                    // Get parent control
                    ci = FormUserControlInfoProvider.GetFormUserControlInfo(ci.UserControlParentID);
                    originalControlName = ci.UserControlCodeName;
                }

                ctrl = FormUserControlLoader.LoadFormControl(Page, controlCodeName, ffi.Name, Form, false);

                InitializeControl(ffi, ctrl);

                // Load default values
                FormInfo fi = FormHelper.GetFormControlParameters(controlCodeName, formProperties, false);
                ctrl.LoadDefaultProperties(fi, false);

                if (!ffi.IsDummyField)
                {
                    // Initialize control's value immediately
                    InitControlValue(ctrl, ffi, originalControlName, DataValue);
                }
                else
                {
                    // Initialize dummy field control's value on form's DummyFieldDataLoad event
                    Form.DummyFieldDataLoad += (senderObj, eventArts) => { InitControlValue(ctrl, ffi, originalControlName); };
                }
            }

            return ctrl;
        }


        /// <summary>
        /// Loads the control from the given URL
        /// </summary>
        /// <param name="url">URL to load</param>
        private FormEngineUserControl LoadFormControl(string url)
        {
            var ctrl = Page.LoadUserControl(url);

            // All OK when form control already
            var formControl = ctrl as FormEngineUserControl;
            if (formControl != null)
            {
                return formControl;
            }

            // Compatibility for filter controls
            var filterCtrl = ctrl as IFilterControl;
            if (filterCtrl != null)
            {
                return new FilterFormControlWrapper(filterCtrl);
            }

            // Try to cast
            return (FormEngineUserControl)ctrl;
        }


        /// <summary>
        /// Initializes the control with the given form field information
        /// </summary>
        /// <param name="ffi">Form field information</param>
        /// <param name="usrControl">Form control</param>
        private void InitializeControl(FormFieldInfo ffi, FormEngineUserControl usrControl)
        {
            if ((usrControl != null) && (ffi != null))
            {
                string fieldName = DataHelper.GetNotEmpty(ffi.Name, ID + "_ctrl");

                usrControl.ID = fieldName;
                usrControl.Field = fieldName;
                usrControl.FieldInfo = ffi;
                usrControl.Form = Form;
                usrControl.Data = Data;
                usrControl.ContextResolver = ContextResolver;

                // Add field to fields list
                if (Form != null)
                {
                    if (!Form.Fields.Contains(fieldName))
                    {
                        Form.Fields.Add(fieldName);
                    }
                }

                usrControl.IsLiveSite = IsLiveSite;

                // Load other control properties
                usrControl.LoadControlFromFFI();

                // Ensure basic styles within form
                if ((usrControl.MessagesPlaceHolder != null) && usrControl.UsesLocalMessagesPlaceHolder)
                {
                    usrControl.MessagesPlaceHolder.BasicStyles = true;
                }
            }
        }


        /// <summary>
        /// Initializes value of the form control.
        /// </summary>
        /// <param name="usrControl">Initialized form control</param>
        /// <param name="ffi">FormFieldInfo with settings for control</param>
        /// <param name="formControlName">Original form control name; it may differ for inherited form controls</param>
        /// <param name="value">Data value</param>
        internal void InitControlValue(FormEngineUserControl usrControl, FormFieldInfo ffi, string formControlName, object value = null)
        {
            if (usrControl == null || ffi == null)
            {
                return;
            }

            string fieldName = ffi.Name;
            object defaultValue = null;

            if (IsInsertMode || ffi.IsDummyField)
            {
                // Get field default value and resolve macros using control resolver if allowed
                bool resolveMacros = ffi.GetResolveDefaultValue(!AllowMacroEditing);
                if ((value == null) || (resolveMacros && AllowMacroEditing))
                {
                    defaultValue = ffi.GetPropertyValue(FormFieldPropertyEnum.DefaultValue, resolveMacros ? usrControl.ContextResolver : null);

                    // Macro-based values are converted to the strong-type object due to conversion differences (system culture vs resolver culture)
                    if (resolveMacros && ffi.IsMacro(FormFieldPropertyEnum.DefaultValue))
                    {
                        CultureInfo ci = CultureHelper.GetCultureInfo(usrControl.ContextResolver.Culture);
                        defaultValue = DataTypeManager.ConvertToSystemType(TypeEnum.Field, ffi.DataType, defaultValue, ci);
                    }
                }
            }

            object labelValue;

            if ((defaultValue != null) &&
                (!ffi.AllowEmpty || (Convert.ToString(defaultValue) != string.Empty)))
            {
                // Allow empty default value only if the field does not allow nulls
                SetControlValue(usrControl, defaultValue);
                labelValue = defaultValue;
            }
            else
            {
                SetControlValue(usrControl, value);
                labelValue = value;
            }

            // Special case for label (check the original code name for inherited controls)
            if ((labelValue != null) && LABEL_CONTROL.Equals(formControlName, StringComparison.OrdinalIgnoreCase))
            {
                if (Data != null)
                {
                    // Convert the value to a proper type
                    var convertedValue = DataTypeManager.ConvertToSystemType(TypeEnum.Field, ffi.DataType, labelValue);

                    // Save the value
                    Data.SetValue(fieldName, convertedValue);
                }
            }
        }


        /// <summary>
        /// Sets correct visibility of controls according to macro settings.
        /// </summary>
        private void EnsureMacros()
        {
            if ((RequestHelper.IsPostBack() && (mMacroTextBox.Value.ToString() != string.Empty)) || ((MacroTable != null) && (MacroTable[ItemName] != null) && (IsMacro)))
            {
                mMacroTextBoxPanel.Attributes.Add("style", "display: inline;");
                mNestedControlPanel.Attributes.Add("style", "display: none;");
                mRemoveMacroButton.Attributes.Add("style", "display: inline;");
                mMacroEditButton.Attributes.Add("style", "display: none;");
            }
            else
            {
                mMacroTextBoxPanel.Attributes.Add("style", "display: none;");
                mNestedControlPanel.Attributes.Add("style", "display: inline;");
                mRemoveMacroButton.Attributes.Add("style", "display: none;");
                mMacroEditButton.Attributes.Add("style", "display: inline;");
            }
        }


        /// <summary>
        /// Loads the value to the control.
        /// </summary>
        /// <param name="c">Control to load</param>
        /// <param name="value">Value</param>
        private void SetControlValue(Control c, object value)
        {
            if (mMacroTextBox != null)
            {
                mMacroTextBox.Value = null;
            }

            // First try to set the macro value
            if (MacroTable != null)
            {
                object macroVal = MacroTable[ItemName];
                if (macroVal != null)
                {
                    if (mMacroTextBox != null)
                    {
                        mMacroTextBox.Value = ValidationHelper.GetString(macroVal, "");
                    }

                    // Remember original value if the control requires that
                    var formControl = c as FormEngineUserControl;
                    if ((formControl != null) && formControl.RememberOriginalValue)
                    {
                        OriginalValue = value;
                        formControl.Value = null;
                    }

                    return;
                }
            }

            // Set the value of the given form control
            var control = c as IFormControl;
            if (control != null)
            {
                control.Value = value;
            }
            else
            {
                mValue = value;
            }
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Ensures the inner controls.
        /// </summary>
        public void EnsureControls()
        {
            EnsureChildControls();
        }


        /// <summary>
        /// Reloads the control.
        /// </summary>
        [Obsolete("Use method CMS.FormEngine.Web.UI.FormEngineUserControl.ReloadControl() instead")]
        public void Reload()
        {
            InitializeControl(true);
        }


        /// <summary>
        /// Perform bad words check.
        /// </summary>
        public void CheckBadWords()
        {
            string siteName = SiteContext.CurrentSiteName;
            if (siteName != null)
            {
                siteName += ".";
            }

            // Perform bad words check
            if (BadWordsHelper.PerformBadWordsCheck(siteName))
            {
                // Supported only for listed controls
                switch (FieldInfo.FieldType)
                {
                    case FormFieldControlTypeEnum.LabelControl:
                    case FormFieldControlTypeEnum.TextBoxControl:
                    case FormFieldControlTypeEnum.CustomUserControl:
                    case FormFieldControlTypeEnum.TextAreaControl:
                    case FormFieldControlTypeEnum.HtmlAreaControl:
                        if (Value != null)
                        {
                            // Check bad words
                            string textToCheck = (string)Value;
                            mBadWordsResult = BadWordInfoProvider.CheckAllBadWords(null, SiteContext.CurrentSiteName, ref textToCheck, mBadWordsList);
                            Value = textToCheck;
                        }
                        break;
                    default:
                        throw new Exception("[EditingFormControl.CheckBadWords]: The bad words check is not available for this data type.");
                }
            }
        }


        /// <summary>
        /// Validation of the control.
        /// </summary>
        public override bool IsValid()
        {
            if (mBadWordsResult == BadWordActionEnum.Deny)
            {
                return false;
            }

            var ctrl = mNestedControl as FormEngineUserControl;
            if (ctrl != null)
            {
                // Validate the macro value through the user control
                if ((IsMacro && !ctrl.ValidateMacroValue(mMacroTextBox.Text, (string)OriginalValue)) || !ctrl.IsValid())
                {
                    ValidationError = ctrl.ValidationError;

                    return false;
                }
            }

            return base.IsValid();
        }


        /// <summary>
        /// Gets client ID of first inner input control which is not already associated with some label.
        /// </summary>
        /// <param name="controls">Collection of controls</param>
        /// <returns>Returns string with client ID of first control which has no associated label</returns>
        public static string GetInputClientID(ControlCollection controls)
        {
            var associatedControls = new Dictionary<string, string>();
            var key = GetAssociatedInputControlKey(controls, associatedControls);
            return key != null ? associatedControls[key] : null;
        }


        /// <summary>
        /// Gets control ID of first inner input control which is not already associated with some label.
        /// </summary>
        /// <param name="controls">Collection of controls</param>
        /// <returns>Returns string with control ID of first control which has no associated label</returns>
        public static string GetInputControlID(ControlCollection controls)
        {
            var associatedControls = new Dictionary<string, string>();
            return GetAssociatedInputControlKey(controls, associatedControls);
        }


        private static string GetAssociatedInputControlKey(ControlCollection controls, IDictionary<string, string> associatedControls)
        {
            if ((controls != null) && controls.Count > 0)
            {
                GetInputClientIDInternal(controls, associatedControls);

                // Return first control which has no associated label
                return associatedControls.FirstOrDefault(i => i.Value != null).Key;
            }
            return null;
        }


        /// <summary>
        /// Goes through controls collection and insert into provided hashtable input, select and file controls without associated label.
        /// </summary>
        /// <param name="controls">ControlCollection with controls to be searched</param>
        /// <param name="associatedControls">Hashtable where control client ID will be saved</param>
        private static void GetInputClientIDInternal(ControlCollection controls, IDictionary<string, string> associatedControls)
        {
            foreach (Control ctrl in controls.OfType<Control>().Where(i => i.Visible))
            {
                // For input controls
                if ((ctrl is TextBox) || (ctrl is HtmlTextArea) || (ctrl is RadioButton) || (ctrl is CheckBox && !(ctrl.Parent is CheckBoxList))
                    || (ctrl is ListBox) || (ctrl is DropDownList) || (ctrl is IInputControl))
                {
                    // Check if associated label doesn't exist in temporary table
                    if (ctrl.ID != null)
                    {
                        if (!associatedControls.ContainsKey(ctrl.ID))
                        {
                            associatedControls.Add(ctrl.ID, ctrl.ClientID);
                        }
                        else
                        {
                            associatedControls[ctrl.ID] = ctrl.ClientID;
                        }
                    }
                }
                else
                {
                    // For label control which has associated control
                    var label = ctrl as Label;
                    if (label != null)
                    {
                        string associatedLabel = label.AssociatedControlID;
                        if (!String.IsNullOrEmpty(associatedLabel))
                        {
                            // Mark in table that associated label found
                            associatedControls[associatedLabel] = null;
                        }
                    }
                    // Try to find not-associated nested control
                    else if (ctrl.Controls.Count > 0)
                    {
                        GetInputClientIDInternal(ctrl.Controls, associatedControls);
                    }
                }
            }
        }


        /// <summary>
        /// Sets control value.
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="isMacro">Indicates if the value is macro. Default value is FALSE.</param>
        public void SetValue(object value, bool isMacro = false)
        {
            if (MacroTable != null)
            {
                MacroTable[ItemName] = isMacro ? value : null;
            }

            SetControlValue(mNestedControl, value);
        }

        #endregion


        #region "ICallbackEventHandler Members"

        /// <summary>
        /// Gets callback result.
        /// </summary>
        public string GetCallbackResult()
        {
            Hashtable parameters = new Hashtable();

            parameters["controlpanelid"] = mNestedControlPanel.ClientID;
            parameters["selectorid"] = mMacroTextBox.ValueElementID;
            parameters["selectorpanelid"] = mMacroTextBoxPanel.ClientID;

            if (!string.IsNullOrEmpty(ResolverName))
            {
                parameters["resolvername"] = ResolverName;
            }

            WindowHelper.Add(ItemName, parameters);

            string queryString = "?params=" + ItemName;
            queryString = URLHelper.AddParameterToUrl(queryString, "hash", QueryHelper.GetHash(queryString));
            return queryString;
        }


        /// <summary>
        /// Raise callback method.
        /// </summary>
        public void RaiseCallbackEvent(string eventArgument)
        {
        }

        #endregion
    }
}