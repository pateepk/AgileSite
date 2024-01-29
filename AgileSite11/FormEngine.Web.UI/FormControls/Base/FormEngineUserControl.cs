using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Web.UI;
using System.Web.UI.WebControls;

using CMS.Base;
using CMS.DataEngine;
using CMS.Base.Web.UI;
using CMS.Helpers;
using CMS.MacroEngine;

using TreeNode = CMS.DocumentEngine.TreeNode;

namespace CMS.FormEngine.Web.UI
{
    /// <summary>
    /// Common interface for custom user controls used in forms.
    /// </summary>
    [ValidationProperty("Value")]
    public abstract class FormEngineUserControl : InlineUserControl, IFormControl
    {
        #region "Delegates & events"

        /// <summary>
        /// Fired on the change to the form control.
        /// </summary>
        public event EventHandler Changed;

        #endregion


        #region "Variables"

        private string mInputClientID;
        private string mInputControlID;
        private bool mCheckFieldEmptiness = true;

        private bool mHasDependingFields;

        private BasicForm mForm;
        private FormFieldInfo mFieldInfo;
        private IDataContainer mData;

        private string mValidationError;
        private bool mCheckMinMaxLength;
        private bool mCheckRegularExpression;
        private string mErrorMessage;
        private bool mDependsOnAnotherField;
        private string mField;
        private bool mCheckUnique;
        private bool mRememberOriginalValue = true;
        private MacroResolver mContextResolver;
        private UIContext mUIContext;

        // Set of properties which values are macros
        private readonly HashSet<string> mMacroProperties = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);

        #endregion


        #region "Properties"

        /// <summary>
        /// Control's UI Context
        /// </summary>
        protected UIContext UIContext
        {
            get
            {
                return mUIContext ?? (mUIContext = UIContextHelper.GetUIContext(this));
            }
        }


        /// <summary>
        /// Control's edited object
        /// </summary>
        public object EditedObject
        {
            get
            {
                return UIContext.EditedObject;
            }
            set
            {
                UIContext.EditedObject = value;
            }
        }


        /// <summary>
        /// Underlying form control, if provided, the form control automatically redirects all properties to that control
        /// </summary>
        protected virtual FormEngineUserControl UnderlyingFormControl => null;


        /// <summary>
        /// Parent form.
        /// </summary>
        public BasicForm Form
        {
            get
            {
                if (UnderlyingFormControl != null)
                {
                    return UnderlyingFormControl.Form;
                }

                return mForm;
            }
            set
            {
                if (UnderlyingFormControl != null)
                {
                    UnderlyingFormControl.Form = value;
                }
                else
                {
                    var formChanged = (value != mForm);

                    mForm = value;

                    // Connect to the form if form has changed
                    if (formChanged)
                    {
                        ConnectToForm(value);
                    }
                }
            }
        }


        /// <summary>
        /// Field info object.
        /// </summary>
        public virtual FormFieldInfo FieldInfo
        {
            get
            {
                if (UnderlyingFormControl != null)
                {
                    return UnderlyingFormControl.FieldInfo;
                }

                return mFieldInfo;
            }
            set
            {
                if (UnderlyingFormControl != null)
                {
                    UnderlyingFormControl.FieldInfo = value;
                }
                else
                {
                    mFieldInfo = value;
                }
            }
        }


        /// <summary>
        /// Gets ClientID of the control from which the Value is retrieved or 
        /// null if such a control can't be specified.
        /// </summary>
        public virtual string ValueElementID
        {
            get
            {
                return UnderlyingFormControl?.ValueElementID;
            }
        }


        /// <summary>
        /// Gets or sets field value. You need to override this method to make the control work properly with the form.
        /// </summary>
        public abstract object Value
        {
            get;
            set;
        }


        /// <summary>
        /// Returns value prepared for validation.
        /// </summary>
        public virtual object ValueForValidation
        {
            get
            {
                return Value;
            }
        }


        /// <summary>
        /// Returns true if the control has value, if false, the value from the control should not be used within the form to update the data
        /// </summary>
        public virtual new bool HasValue
        {
            get
            {
                if (UnderlyingFormControl != null)
                {
                    return UnderlyingFormControl.HasValue;
                }

                return Visible;
            }
        }


        /// <summary>
        /// Gets the display name of the value item. Returns null if display name is not available.
        /// </summary>
        public virtual string ValueDisplayName
        {
            get
            {
                return UnderlyingFormControl?.ValueDisplayName;
            }
        }


        /// <summary>
        /// Gets or sets Value converted to string.
        /// </summary>
        public virtual string Text
        {
            get
            {
                return (string)Value;
            }
            set
            {
                Value = value;
            }
        }


        /// <summary>
        /// Node data. This property is used only for passing values to the control.
        /// </summary>
        public virtual IDataContainer Data
        {
            get
            {
                if (UnderlyingFormControl != null)
                {
                    return UnderlyingFormControl.Data;
                }

                return mData;
            }
            set
            {
                if (UnderlyingFormControl != null)
                {
                    UnderlyingFormControl.Data = value;
                }
                else
                {
                    mData = value;
                }
            }
        }


        /// <summary>
        /// Gets or sets state enable.
        /// </summary>
        public virtual bool Enabled
        {
            get
            {
                if (UnderlyingFormControl != null)
                {
                    return UnderlyingFormControl.Enabled;
                }

                return ValidationHelper.GetBoolean(ViewState["Enabled"], true);
            }
            set
            {
                if (UnderlyingFormControl != null)
                {
                    UnderlyingFormControl.Enabled = value;
                }
                else
                {
                    ViewState["Enabled"] = value;
                }
            }
        }


        /// <summary>
        /// Validation error string shown when the control is not valid.
        /// </summary>
        public string ValidationError
        {
            get
            {
                if (UnderlyingFormControl != null)
                {
                    return UnderlyingFormControl.ValidationError;
                }

                return mValidationError;
            }
            set
            {
                if (UnderlyingFormControl != null)
                {
                    UnderlyingFormControl.ValidationError = value;
                }
                else
                {
                    mValidationError = value;
                }
            }
        }


        /// <summary>
        /// If true, control does not process the data.
        /// </summary>
        public override bool StopProcessing
        {
            get
            {
                if (UnderlyingFormControl != null)
                {
                    return UnderlyingFormControl.StopProcessing;
                }

                return ValidationHelper.GetBoolean(ViewState["StopProcessing"], false);
            }
            set
            {
                if (UnderlyingFormControl != null)
                {
                    UnderlyingFormControl.StopProcessing = value;
                }
                else
                {
                    ViewState["StopProcessing"] = value;
                }
            }
        }


        /// <summary>
        /// Helper property to use custom parameter in form control.
        /// </summary>
        public virtual object FormControlParameter
        {
            get
            {
                return UnderlyingFormControl?.FormControlParameter;
            }
            set
            {
                if (UnderlyingFormControl != null)
                {
                    UnderlyingFormControl.FormControlParameter = value;
                }
            }
        }


        /// <summary>
        /// Client ID of primary input control. If not explicitly set, first client ID of inner control of the form control is returned.
        /// </summary>
        public virtual string InputClientID
        {
            get
            {
                if (UnderlyingFormControl != null)
                {
                    return UnderlyingFormControl.InputClientID;
                }

                return mInputClientID ?? (mInputClientID = GetInputClientID());
            }
        }


        /// <summary>
        /// Control ID of primary input control. If not explicitly set, first control ID of inner control of the form control is returned.
        /// </summary>
        public virtual string InputControlID
        {
            get
            {
                if (UnderlyingFormControl != null)
                {
                    return UnderlyingFormControl.InputControlID;
                }

                return mInputControlID ?? (mInputControlID = GetInputControlID());
            }
            set
            {
                if (UnderlyingFormControl != null)
                {
                    UnderlyingFormControl.InputControlID = value;
                }
                else
                {
                    mInputControlID = value;
                }
            }
        }


        /// <summary>
        /// Indicates that field should be checked on emptiness in validation step by BasicForm. Default TRUE. 
        /// It is preferable to set to FALSE for controls with complex input such as file uploaders. Field emptiness
        /// validation then must be placed in custom form control in IsValid() method.
        /// </summary>
        public virtual bool CheckFieldEmptiness
        {
            get
            {
                if (UnderlyingFormControl != null)
                {
                    return UnderlyingFormControl.CheckFieldEmptiness;
                }

                return mCheckFieldEmptiness;
            }
            set
            {
                if (UnderlyingFormControl != null)
                {
                    UnderlyingFormControl.CheckFieldEmptiness = value;
                }
                else
                {
                    mCheckFieldEmptiness = value;
                }
            }
        }


        /// <summary>
        /// Indicates that field should be checked on min/max input length in validation step by BasicForm. Default FALSE.
        /// It is preferable to set to TRUE for simple text input controls such as TextBox.
        /// </summary>
        public virtual bool CheckMinMaxLength
        {
            get
            {
                if (UnderlyingFormControl != null)
                {
                    return UnderlyingFormControl.CheckMinMaxLength;
                }

                return mCheckMinMaxLength;
            }
            set
            {
                if (UnderlyingFormControl != null)
                {
                    UnderlyingFormControl.CheckMinMaxLength = value;
                }
                else
                {
                    mCheckMinMaxLength = value;
                }
            }
        }


        /// <summary>
        /// Indicates that field should be checked on regular expressions in validation step  by BasicForm. Default FALSE.
        /// It is preferable to set to TRUE for controls where user should be able to enter text such as TextBox.
        /// </summary>
        public virtual bool CheckRegularExpression
        {
            get
            {
                if (UnderlyingFormControl != null)
                {
                    return UnderlyingFormControl.CheckRegularExpression;
                }

                return mCheckRegularExpression;
            }
            set
            {
                if (UnderlyingFormControl != null)
                {
                    UnderlyingFormControl.CheckRegularExpression = value;
                }
                else
                {
                    mCheckRegularExpression = value;
                }
            }
        }


        /// <summary>
        /// Control custom error message.
        /// </summary>
        public virtual string ErrorMessage
        {
            get
            {
                if (UnderlyingFormControl != null)
                {
                    return UnderlyingFormControl.ErrorMessage;
                }

                return mErrorMessage;
            }
            set
            {
                if (UnderlyingFormControl != null)
                {
                    UnderlyingFormControl.ErrorMessage = value;
                }
                else
                {
                    mErrorMessage = value;
                }
            }
        }


        /// <summary>
        /// Indicates if another fields depend on this field.
        /// </summary>
        public virtual bool HasDependingFields
        {
            get
            {
                return mHasDependingFields;
            }
            set
            {
                if (mHasDependingFields != value)
                {
                    // Try to set AutoPostBack property for all children only if the setting is changed
                    foreach (Control ctrl in Controls)
                    {
                        SetControlAutoPostback(ctrl, value);
                    }
                }

                mHasDependingFields = value;
            }
        }


        /// <summary>
        /// Indicates if field depends on another field.
        /// </summary>
        public bool DependsOnAnotherField
        {
            get
            {
                if (UnderlyingFormControl != null)
                {
                    return UnderlyingFormControl.DependsOnAnotherField;
                }

                return mDependsOnAnotherField;
            }
            set
            {
                if (UnderlyingFormControl != null)
                {
                    UnderlyingFormControl.DependsOnAnotherField = value;
                }
                else
                {
                    mDependsOnAnotherField = value;
                }
            }
        }


        /// <summary>
        /// Field name to which the field belongs.
        /// </summary>
        public string Field
        {
            get
            {
                if (UnderlyingFormControl != null)
                {
                    return UnderlyingFormControl.Field;
                }

                return mField;
            }
            set
            {
                if (UnderlyingFormControl != null)
                {
                    UnderlyingFormControl.Field = value;
                }
                else
                {
                    mField = value;
                }
            }
        }


        /// <summary>
        /// If true, the field is checked for uniqueness. This property is only supported in templated form.
        /// </summary>
        public bool CheckUnique
        {
            get
            {
                if (UnderlyingFormControl != null)
                {
                    return UnderlyingFormControl.CheckUnique;
                }

                return mCheckUnique;
            }
            set
            {
                if (UnderlyingFormControl != null)
                {
                    UnderlyingFormControl.CheckUnique = value;
                }
                else
                {
                    mCheckUnique = value;
                }
            }
        }


        /// <summary>
        /// If true, the returned value is trimmed. This property is only supported if used inside the form control. Default false.
        /// </summary>
        public bool Trim
        {
            get
            {
                if (UnderlyingFormControl != null)
                {
                    return UnderlyingFormControl.Trim;
                }

                return GetValue("trim", false);
            }
            set
            {
                if (UnderlyingFormControl != null)
                {
                    UnderlyingFormControl.Trim = value;
                }
                else
                {
                    SetValue("trim", value);
                }
            }
        }


        /// <summary>
        /// If true, the control remembers the original value for evaluation
        /// </summary>
        public bool RememberOriginalValue
        {
            get
            {
                if (UnderlyingFormControl != null)
                {
                    return UnderlyingFormControl.RememberOriginalValue;
                }

                return mRememberOriginalValue;
            }
            set
            {
                if (UnderlyingFormControl != null)
                {
                    UnderlyingFormControl.RememberOriginalValue = value;
                }
                else
                {
                    mRememberOriginalValue = value;
                }
            }
        }


        /// <summary>
        /// Gets or sets macro resolver name.
        /// </summary>
        public virtual string ResolverName
        {
            get
            {
                return ContextResolver.ResolverName;
            }
            set
            {
                ContextResolver.ResolverName = value;
            }
        }


        /// <summary>
        /// Gets or sets macro resolver for given control.
        /// </summary>
        public virtual MacroResolver ContextResolver
        {
            get
            {
                if (UnderlyingFormControl != null)
                {
                    return UnderlyingFormControl.ContextResolver;
                }

                return mContextResolver ?? (mContextResolver = CreateContextResolver(GetParentResolver(), true));
            }
            set
            {
                if (UnderlyingFormControl != null)
                {
                    UnderlyingFormControl.ContextResolver = value;
                }
                else
                {
                    mContextResolver = CreateContextResolver(value, false);
                }
            }
        }


        /// <summary>
        /// CSS class of the control.
        /// </summary>
        public string CssClass
        {
            get
            {
                if (UnderlyingFormControl != null)
                {
                    return UnderlyingFormControl.CssClass;
                }

                return ValidationHelper.GetString(ViewState["CssClass"], String.Empty);
            }
            set
            {
                if (UnderlyingFormControl != null)
                {
                    UnderlyingFormControl.CssClass = value;
                }
                else
                {
                    ViewState["CssClass"] = value;
                }
            }
        }


        /// <summary>
        /// CSS style of the control.
        /// </summary>
        public string ControlStyle
        {
            get
            {
                if (UnderlyingFormControl != null)
                {
                    return UnderlyingFormControl.ControlStyle;
                }

                return ValidationHelper.GetString(ViewState["ControlStyle"], String.Empty);
            }
            set
            {
                if (UnderlyingFormControl != null)
                {
                    UnderlyingFormControl.ControlStyle = value;
                }
                else
                {
                    ViewState["ControlStyle"] = value;
                }
            }
        }


        /// <summary>
        /// Format of where condition.
        /// </summary>
        public string WhereConditionFormat
        {
            get
            {
                return GetValue<string>("WhereConditionFormat", null);
            }
            set
            {
                SetValue("WhereConditionFormat", value);
            }
        }


        /// <summary>
        /// Control properties
        /// </summary>
        public FormInfo DefaultProperties
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets resource prefix for custom texts.
        /// </summary>
        public override string ResourcePrefix
        {
            get
            {
                return GetValue("ResourcePrefix", String.Empty);
            }
            set
            {
                SetValue("ResourcePrefix", value);
            }
        }

        #endregion


        #region "Control events"

        /// <summary>
        /// OnInit event.
        /// </summary>
        /// <param name="e">Event arguments</param>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            // If the control has underlying control, hook it's changed method to this one
            if (UnderlyingFormControl != null)
            {
                UnderlyingFormControl.Changed += UnderlyingFormControl_Changed;
            }

            if (Form != null)
            {
                // Ensure basic style for form controls when used in form
                if (UsesLocalMessagesPlaceHolder)
                {
                    MessagesPlaceHolder.BasicStyles = true;
                }

                if (HasDependingFields)
                {
                    // Register change event to the parent form
                    Form.RegisterControlWithDependencies(this);
                }

                if (DependsOnAnotherField)
                {
                    // Hook on change of controls with dependencies
                    Form.ControlWithDependenciesChanged += Form_ControlWithDependenciesChanged;
                }
            }
        }


        /// <summary>
        /// Renders user control.
        /// </summary>
        protected override void Render(HtmlTextWriter writer)
        {
            // Set user control CSS class and style
            if (!String.IsNullOrEmpty(CssClass) || !String.IsNullOrEmpty(ControlStyle))
            {
                string classAtr = String.Empty;
                string styleAtr = String.Empty;

                if (!String.IsNullOrEmpty(CssClass))
                {
                    classAtr = $" class=\"{CssClass}\"";
                }
                if (!String.IsNullOrEmpty(ControlStyle))
                {
                    styleAtr = $" style=\"{ControlStyle}\"";
                }

                // Create "div" around the control
                writer.Write("<div{0}{1}>", classAtr, styleAtr);
                base.Render(writer);
                writer.Write("</div>");
            }
            else
            {
                base.Render(writer);
            }
        }


        /// <summary>
        /// Changed handler.
        /// </summary>
        protected void UnderlyingFormControl_Changed(object sender, EventArgs e)
        {
            RaiseOnChanged();
        }


        /// <summary>
        /// Another form control, which has some dependencies, has changed.
        /// </summary>
        protected void Form_ControlWithDependenciesChanged(object sender, EventArgs e)
        {
            ReloadControl();
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Loads control value.
        /// </summary>
        /// <param name="value">Value to load</param>
        public virtual void LoadControlValue(object value)
        {
            Value = value;
        }


        /// <summary>
        /// Returns the value of the given property.
        /// </summary>
        /// <param name="propertyName">Property name</param>
        public override object GetValue(string propertyName)
        {
            // Get property value from underlying control
            UnderlyingFormControl?.GetValue(propertyName);

            // Get a resolved value if value is a macro
            if (mMacroProperties.Contains(propertyName))
            {
                return GetResolvedValue<object>(propertyName, null);
            }

            return base.GetValue(propertyName);
        }


        /// <summary>
        /// Sets the property value of the control, setting the value affects only local property value.
        /// </summary>
        /// <param name="propertyName">Property name to set</param>
        /// <param name="value">New property value</param>
        public override bool SetValue(string propertyName, object value)
        {
            // By default the value is not a macro
            return SetValue(propertyName, value, false);
        }


        /// <summary>
        /// Sets the property value of the control, setting the value affects only local property value.
        /// </summary>
        /// <param name="propertyName">Property name to set</param>
        /// <param name="value">New property value</param>
        /// <param name="isMacro">If true, the property value is a macro</param>
        public bool SetValue(string propertyName, object value, bool isMacro)
        {
            // Update value in resolver so that it gets priority over other available properties with the same name
            ContextResolver.SetNamedSourceDataCallback(propertyName, ctx => GetValue(propertyName));

            // Propagate value to underlying control
            UnderlyingFormControl?.SetValue(propertyName, value, isMacro);

            // Update the macro status
            if (isMacro)
            {
                mMacroProperties.Add(propertyName);
            }
            else
            {
                mMacroProperties.Remove(propertyName);
            }

            return base.SetValue(propertyName, value);
        }


        /// <summary>
        /// Returns an array of values of any other fields returned by the control.
        /// </summary>
        /// <remarks>It returns an array where first dimension is attribute name and the second dimension is its value.</remarks>
        public virtual object[,] GetOtherValues()
        {
            return UnderlyingFormControl?.GetOtherValues();
        }


        /// <summary>
        /// Loads the other fields values to the state of the form control
        /// </summary>
        public virtual void LoadOtherValues()
        {
            UnderlyingFormControl?.LoadOtherValues();
        }


        /// <summary>
        /// Returns a value of other field with specified returned by the control.
        /// </summary>
        /// <param name="name">Field name</param>
        public virtual object GetOtherValue(string name)
        {
            object[,] returnedValues = GetOtherValues();
            if (returnedValues != null)
            {
                for (int valueIndex = 0; valueIndex <= returnedValues.GetUpperBound(0); valueIndex++)
                {
                    // If found field with requested name
                    var value = returnedValues[valueIndex, 0] as string;
                    if (name != null && name.Equals(value, StringComparison.InvariantCultureIgnoreCase))
                    {
                        return returnedValues[valueIndex, 1];
                    }
                }
            }

            return null;
        }


        /// <summary>
        /// Returns true if entered data is valid. If data is invalid, it returns false and displays an error message.
        /// </summary>
        public virtual bool IsValid()
        {
            if (UnderlyingFormControl != null)
            {
                return UnderlyingFormControl.IsValid();
            }

            return true;
        }


        /// <summary>
        /// Gets the control info marker code
        /// </summary>
        public virtual string GetControlInfoMarker()
        {
            return UnderlyingFormControl?.GetControlInfoMarker();
        }


        /// <summary>
        /// Returns true if the given macro value is valid value for this control
        /// </summary>
        /// <param name="macro">Macro to check</param>
        /// <param name="originalValue">Original value</param>
        public virtual bool ValidateMacroValue(string macro, string originalValue)
        {
            if (UnderlyingFormControl != null)
            {
                return UnderlyingFormControl.ValidateMacroValue(macro, originalValue);
            }

            return true;
        }


        /// <summary>
        /// Returns the list of the field IDs (Client IDs of the inner controls) that should be spell checked.
        /// </summary>
        public virtual List<string> GetSpellCheckFields()
        {
            return UnderlyingFormControl?.GetSpellCheckFields();
        }


        /// <summary>
        /// Gets a where condition. 
        /// Returns where condition formatted by WhereConditionFormat from IFilterFormControl interface if form control implements it.
        /// Otherwise returns where condition in [FieldName] = 'Value' format.
        /// </summary>
        public virtual string GetWhereCondition()
        {
            string value = ValidationHelper.GetString(Value, string.Empty);

            // Check if value is empty
            if ((FieldInfo == null) || (value == string.Empty) || (WhereConditionFormat == "##NONE##"))
            {
                return null;
            }

            // Use filter where condition format
            if (String.IsNullOrEmpty(WhereConditionFormat))
            {
                var convertedValue = DataTypeManager.ConvertToSystemType(TypeEnum.Field, FieldInfo.DataType, value, CultureHelper.EnglishCulture);

                return new WhereCondition(FieldInfo.Name, QueryOperator.Equals, convertedValue).ToString(true);
            }

            if (!String.IsNullOrEmpty(value) && !String.IsNullOrEmpty(WhereConditionFormat))
            {
                // Format where condition
                return String.Format(WhereConditionFormat, FieldInfo.Name, SqlHelper.GetSafeQueryString(value, false), "=");
            }

            return null;
        }


        /// <summary>
        /// Loads the default properties from the given form definition
        /// </summary>
        /// <param name="fi">Form information to load</param>
        /// <param name="forceValues">Indicates if existing parameter values should be overridden</param>
        public void LoadDefaultProperties(FormInfo fi = null, bool forceValues = true)
        {
            // Ensure default properties
            if (fi == null)
            {
                fi = DefaultProperties;
            }
            else
            {
                DefaultProperties = fi;
            }

            // Process all fields
            var fields = fi.GetFields(true, true);
            foreach (FormFieldInfo ffi in fields)
            {
                // Load the default value if set
                var defValue = ffi.GetPropertyValue(FormFieldPropertyEnum.DefaultValue, ContextResolver);

                var propertyName = ffi.Name;
                if (!String.IsNullOrEmpty(defValue) && (forceValues || !HasValue(propertyName)))
                {
                    SetValue(propertyName, defValue);
                }
            }
        }


        /// <summary>
        /// Gets resolved property value
        /// </summary>
        /// <typeparam name="ReturnType">Return type of the property</typeparam>
        /// <param name="name">Name of the property</param>
        /// <param name="defaultValue">Default value</param>
        public ReturnType GetResolvedValue<ReturnType>(string name, ReturnType defaultValue)
        {
            string macro = ValidationHelper.GetString(base.GetValue(name), null);

            // Resolve macros
            macro = ContextResolver.ResolveMacros(macro);

            return ValidationHelper.GetValue(macro, defaultValue);
        }


        /// <summary>
        /// Loads control with values in FormFieldInfo.
        /// </summary>
        public void LoadControlFromFFI()
        {
            var ffi = FieldInfo;
            if (ffi != null)
            {
                // Set other parameters from FormFieldInfo settings
                var settings = ffi.Settings;

                foreach (string key in settings.Keys)
                {
                    SetValue(key, settings[key]);
                }

                // Set values with macros
                var macros = ffi.SettingsMacroTable;
                if (macros != null)
                {
                    foreach (string key in macros.Keys)
                    {
                        var value = macros[key];
                        if (value != null)
                        {
                            SetValue(key, value, true);
                        }
                    }
                }

                HasDependingFields = ffi.HasDependingFields;
                DependsOnAnotherField = ffi.DependsOnAnotherField;

                // Apply styles as the last thing 
                ApplyStyles(ffi);
            }
        }


        /// <summary>
        /// Applies control styles from the given form info
        /// </summary>
        /// <param name="ffi">Form field info</param>
        private void ApplyStyles(FormFieldInfo ffi)
        {
            var resolver = ContextResolver;

            // Apply CSS style
            string inputControlStyle = ffi.GetPropertyValue(FormFieldPropertyEnum.InputControlStyle, resolver);
            if (!String.IsNullOrEmpty(inputControlStyle))
            {
                ControlStyle = inputControlStyle;
            }

            // Apply CSS class
            string controlCssClass = ffi.GetPropertyValue(FormFieldPropertyEnum.ControlCssClass, resolver);
            if (!String.IsNullOrEmpty(controlCssClass))
            {
                CssClass = controlCssClass;
            }
        }


        /// <summary>
        /// Checks if field value's length is not shorter or longer than specified borders.
        /// </summary>
        /// <param name="minControlSize">Minimal size</param>
        /// <param name="maxControlSize">Maximal size</param>
        /// <param name="textLength">Text length</param>
        /// <param name="errorText">Error text which will be returned in case of failure</param>
        /// <param name="errorMsg">Custom error message; it's used if it differs from 'Invalid input' message</param>
        /// <returns>True if field value is not shorter or longer than specified min/max borders.</returns>
        public static bool CheckLength(int minControlSize, int maxControlSize, int textLength, ref string errorText, string errorMsg)
        {
            if (((maxControlSize > 0) && (textLength > maxControlSize)) || ((minControlSize > 0) && (textLength < minControlSize)))
            {
                // Use predefined error messages
                if (!String.IsNullOrEmpty(errorText))
                {
                    errorText += " ";
                }

                if (errorMsg != ResHelper.GetString("BasicForm.InvalidInput"))
                {
                    // Use custom error message
                    errorText += errorMsg;
                }
                else
                {
                    if (textLength > maxControlSize)
                    {
                        errorText += string.Format(ResHelper.GetString("BasicForm.TooLong"), maxControlSize, textLength);
                    }
                    else
                    {
                        errorText += string.Format(ResHelper.GetString("BasicForm.TooShort"), minControlSize, textLength);
                    }
                }

                return false;
            }

            return true;
        }

        #endregion


        #region "Protected methods"

        /// <summary>
        /// Connects the given form control to a form if necessary
        /// </summary>
        /// <param name="form">Form control</param>
        protected virtual void ConnectToForm(BasicForm form)
        {
            // Nothing by default
        }


        /// <summary>
        /// Raises the Changed event.
        /// </summary>
        protected void RaiseOnChanged()
        {
            Changed?.Invoke(this, null);
        }


        /// <summary>
        /// Gets input control based on <see cref="ValueElementID"/> or first inner input control if <see cref="ValueElementID"/> is not set or null if no input control is found.
        /// </summary>
        private Control GetInputControl(ControlCollection controls)
        {
            Control control = null;

            if (!string.IsNullOrEmpty(ValueElementID))
            {
                control = GetValueElement(controls);
            }

            return control ?? FindInputControl(controls);
        }


        /// <summary>
        /// Seacrhes for first inner input control which is not already associated with some label. Returns null if no input control is found.
        /// </summary>
        private Control FindInputControl(ControlCollection controls)
        {
            var controlId = EditingFormControl.GetInputControlID(controls);
            if (!string.IsNullOrEmpty(controlId))
            {
                return FindControl(controlId);
            }

            return null;
        }


        /// <summary>
        /// Tries to get input control based on <see cref="ValueElementID"/>.
        /// </summary>
        private Control GetValueElement(ControlCollection controls)
        {
            Control result = null;

            foreach (Control control in controls)
            {
                if (result != null)
                {
                    break;
                }

                if (control.ClientID.Equals(ValueElementID, StringComparison.OrdinalIgnoreCase))
                {
                    result = control;
                }

                if (result == null)
                {
                    result = GetValueElement(control.Controls);
                }
            }

            return result;
        }


        /// <summary>
        /// Gets client ID of first inner control.
        /// </summary>
        /// <returns>Returns client ID of input control</returns>
        private string GetInputClientID()
        {
            return EditingFormControl.GetInputClientID(Controls);
        }


        /// <summary>
        /// Gets control ID of first inner control.
        /// </summary>
        /// <returns>Returns ID of input control</returns>
        private string GetInputControlID()
        {
            return EditingFormControl.GetInputControlID(Controls);
        }


        /// <summary>
        /// Returns dialog configuration from the specified form control settings.
        /// </summary>       
        protected DialogConfiguration GetDialogConfiguration()
        {
            return GetDialogConfiguration(Properties);
        }


        /// <summary>
        /// Returns dialog configuration from the specified form control settings.
        /// </summary>       
        /// <param name="settings">Form control settings</param>
        private DialogConfiguration GetDialogConfiguration(Hashtable settings)
        {
            DialogConfiguration config = new DialogConfiguration(settings);

            if (Form != null)
            {
                if (Form.IsInsertMode)
                {
                    // Get node parent ID
                    var parent = Form.ParentObject as TreeNode;
                    if (parent != null)
                    {
                        // Add Form GUID                    
                        config.AttachmentFormGUID = Form.FormGUID;
                        config.AttachmentParentID = parent.NodeID;
                    }
                }
                else
                {
                    // If no document ID found add -1 => try to load from context on get event
                    config.AttachmentDocumentID = ValidationHelper.GetInteger(Form.Data.GetValue("DocumentID"), -1);

                    // Get node parent ID
                    config.AttachmentParentID = ValidationHelper.GetInteger(Form.Data.GetValue("NodeParentID"), 0);
                }
            }

            return config;
        }


        /// <summary>
        /// Indicates if form control's DataRow contains requested column.
        /// </summary>
        /// <param name="columnName">Name of column to look for in data row</param>
        /// <returns>Returns TRUE if form control's DataRow contains column with given name. Otherwise returns FALSE.</returns>
        protected bool ContainsColumn(string columnName)
        {
            var data = Form?.Data;
            if (data == null)
            {
                return false;
            }

            return data.ContainsColumn(columnName);
        }


        /// <summary>
        /// Gets the given column value from the form data
        /// </summary>
        /// <param name="columnName">Column name</param>
        protected object GetColumnValue(string columnName)
        {
            var data = Form?.Data;

            return data?.GetValue(columnName);
        }


        /// <summary>
        /// Ensures that the given value is represented as null is empty, or converted to proper type.
        /// </summary>
        /// <param name="value">Value to process</param>
        protected object ConvertInputValue(object value)
        {
            if (FieldInfo == null)
            {
                return value;
            }

            if (value == null)
            {
                return null;
            }

            var strValue = value as string;
            if (FieldInfo.AllowEmpty && strValue == string.Empty)
            {
                return null;
            }

            // Convert the value to a proper type
            value = DataTypeManager.ConvertToSystemType(TypeEnum.Field, FieldInfo.DataType, value, CultureHelper.EnglishCulture);
            TrimDecimalValue(ref value, FieldInfo);            

            return value;
        }


        /// <summary>
        /// Gets the string value from the given value
        /// </summary>
        /// <param name="value">Value to process</param>
        protected string GetStringValue(object value)
        {
            var ffi = FieldInfo;
            var strValue = value as string;

            // Detect empty value
            if (((ffi == null) || ffi.AllowEmpty) && ((value == null) || (strValue == String.Empty)))
            {
                strValue = "";
            }
            else if (ffi != null)
            {
                // Convert the value to a proper type
                strValue = DataTypeManager.GetStringValue(TypeEnum.Field, ffi.DataType, value, CultureHelper.EnglishCulture);
            }
            else
            {
                // Default conversion if field info is not available
                strValue = ValidationHelper.GetString(value, "");
            }

            return strValue;
        }
		
		
        /// <summary>
        /// Reloads form control content.
        /// </summary>
        public void ReloadControl()
        {
            ReloadControlInternal();
        }


        /// <summary>
        /// Virtual method that can be used in specific form control to reload its content. 
        /// It is called after the change of another form control which has some dependencies.
        /// </summary>
        protected virtual void ReloadControlInternal()
        {
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Creates a context resolver for this form control and registers the control functionality within this resolver.
        /// </summary>
        /// <param name="resolver">Parent resolver</param>
        /// <param name="createAsChild">If true, the resolver if created as a child of the given resolver</param>
        private MacroResolver CreateContextResolver(MacroResolver resolver, bool createAsChild)
        {
            if (resolver == null)
            {
                throw new ArgumentNullException(nameof(resolver));
            }

            // Make sure this control uses its own resolver
            if (createAsChild)
            {
                resolver = resolver.CreateChild();
            }

            // Get field name
            string fieldName = (FieldInfo != null) ? FieldInfo.Name : Field;

            if (!string.IsNullOrEmpty(fieldName))
            {
                // Register resolver data - make field available in form context if available
                resolver.SetNamedSourceDataCallback(fieldName, r => new FormControlMacroContainer(this));
            }

            resolver.SetNamedSourceDataCallback("Properties", r => new SafeDictionaryContainer<object>(Properties));
            resolver.SetNamedSourceDataCallback("Value", r => Value);

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
        /// Sets the given control auto postback if available.
        /// </summary>
        /// <param name="ctrl">Control to set</param>
        /// <param name="value">New value</param>
        private void SetControlAutoPostback(Control ctrl, bool value)
        {
            if (ctrl is Panel || ctrl is PlaceHolder || ctrl is UpdatePanel || (ctrl.Parent is UpdatePanel))
            {
                var updatePanel = ctrl as UpdatePanel;
                if (value && updatePanel != null)
                {
                    // Register postback trigger if input control is inside update panel
                    var inputControl = GetInputControl(ctrl.Controls);
                    RegisterPostbackTrigger(updatePanel, inputControl);
                }

                foreach (Control subCtrl in ctrl.Controls)
                {
                    SetControlAutoPostback(subCtrl, value);
                }
            }

            var formEngineUserControl = ctrl as FormEngineUserControl;
            if (formEngineUserControl != null)
            {
                // Propagate setting into inner form control
                formEngineUserControl.HasDependingFields = value;
            }

            if (ctrl is IPostBackDataHandler)
            {
                if (ctrl is CheckBox)
                {
                    var aspControl = (CheckBox)ctrl;
                    aspControl.AutoPostBack = value;
                    aspControl.CheckedChanged += UnderlyingFormControl_Changed;
                }
                if (ctrl is ListControl)
                {
                    var aspControl = (ListControl)ctrl;
                    aspControl.AutoPostBack = value;
                    aspControl.SelectedIndexChanged += UnderlyingFormControl_Changed;
                }
                if (ctrl is TextBox)
                {
                    var aspControl = (TextBox)ctrl;
                    aspControl.AutoPostBack = value;
                    aspControl.TextChanged += UnderlyingFormControl_Changed;
                }
            }
        }


        /// <summary>
        /// Registers input control <paramref name="inputControl"/> as postback trigger of the update panel <paramref name="updatePanel"/>.
        /// </summary>
        /// <param name="updatePanel">Outer update panel</param>
        /// <param name="inputControl">Input control that should be registered</param>
        private void RegisterPostbackTrigger(UpdatePanel updatePanel, Control inputControl)
        {
            if (updatePanel == null || inputControl == null)
            {
                return;
            }

            inputControl.Page.PreRenderComplete += (sender, eventArgs) =>
            {
                updatePanel.Triggers.Add(new PostBackTrigger
                {
                    ControlID = inputControl.ID
                });
            };

            var scriptManager = ScriptManager.GetCurrent(Page);
            scriptManager?.RegisterPostBackControl(inputControl);
        }


        /// <summary>
        /// Trims trailing zero values on decimal value.
        /// </summary>
        protected virtual void TrimDecimalValue(ref object value, IField fieldInfo)
        {
            if (fieldInfo.DataType.Equals(FieldDataType.Decimal, StringComparison.OrdinalIgnoreCase))
            {
                value = ((decimal)value).TrimEnd();
            }
        }

        #endregion
    }
}