using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Web.UI;

using CMS.Base;
using CMS.Base.Web.UI;
using CMS.DataEngine;
using CMS.Globalization;
using CMS.Helpers;

using TimeZoneInfo = CMS.Globalization.TimeZoneInfo;

namespace CMS.FormEngine.Web.UI
{
    /// <summary>
    /// Abstract class for BasicForm.
    /// </summary>
    public abstract class AbstractBasicForm : CMSDynamicWebControl, INamingContainer, ITimeZoneManager, IControlContextManager
    {
        #region "Delegates"

        /// <summary>
        /// Occurs when the particular item is validated.
        /// </summary>
        /// <param name="sender">Item control that is just validated</param>
        /// <param name="errorMessage">Returning errorMessage from the validator</param>
        public delegate void OnItemValidationEventHandler(object sender, ref string errorMessage);

        #endregion


        #region "Events"

        /// <summary>
        /// Occurs when a value is being extracted from a form control.
        /// </summary>
        public event EventHandler<FormEngineUserControlEventArgs> OnGetControlValue;

        #endregion


        #region "Variables"

        /// <summary>
        /// Current control context object.
        /// </summary>
        private ControlContext mContextObject;


        /// <summary>
        /// Time zone type.
        /// </summary>
        private TimeZoneTypeEnum mTimeZoneType = TimeZoneTypeEnum.Inherit;


        /// <summary>
        /// List of the field names to spell check.
        /// </summary>
        private List<string> mSpellCheckFields;

        #endregion


        #region "Properties"

        /// <summary>
        /// Indicates if spell check is allowed. Not allowed by default.
        /// </summary>
        public virtual bool AllowSpellCheck
        {
            get;
            set;
        }


        /// <summary>
        /// List with client Ids of fields that should be spell checked.
        /// </summary>
        public List<string> SpellCheckFields
        {
            get
            {
                return mSpellCheckFields ?? (mSpellCheckFields = new List<string>());
            }
        }


        /// <summary>
        /// Form information.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual FormInfo FormInformation
        {
            get;
            set;
        }


        /// <summary>
        /// Data to be edited.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IDataContainer Data
        {
            get;
            set;
        }


        /// <summary>
        /// Data to be edited.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DataRow DataRow
        {
            get
            {
                if (Data == null)
                {
                    return null;
                }
                var dataRowContainer = Data as DataRowContainer;
                if (dataRowContainer != null)
                {
                    return (dataRowContainer).DataRow;
                }
                throw new Exception("[BasicForm.DataRow]: This property can be read only if it was previously initialized through set.");
            }
            set
            {
                Data = value != null ? new DataRowContainer(value) : null;
            }
        }


        /// <summary>
        /// Indicates if the control is used for inserting a new object.
        /// </summary>
        [Browsable(true), DefaultValue(false), Description("Indicates if the control is used for inserting a new object.")]
        public bool IsInsertMode
        {
            get
            {
                return (Mode == FormModeEnum.Insert) || (Mode == FormModeEnum.InsertNewCultureVersion);
            }
        }


        /// <summary>
        /// Gets or sets the mode of the form.
        /// </summary>
        [Browsable(true), Description("Gets or sets the mode of the form.")]
        public FormModeEnum Mode
        {
            get
            {
                if (ViewState["Mode"] == null)
                {
                    ViewState["Mode"] = FormModeEnum.Insert;
                }
                return (FormModeEnum)ViewState["Mode"];
            }
            set
            {
                ViewState["Mode"] = value;
            }
        }


        /// <summary>
        /// Indicates if disabled fields should be processed (validated and saved). Default value is true in insert mode and false in edit mode.
        /// </summary>
        public bool ProcessDisabledFields
        {
            get
            {
                return ValidationHelper.GetBoolean(ViewState["ProcessDisabledFields"], IsInsertMode);
            }
            set
            {
                ViewState["ProcessDisabledFields"] = value;
            }
        }

        #endregion


        #region "Properties - field controls"

        /// <summary>
        /// List of field names in order they appear in the form.
        /// </summary>
        public List<string> Fields
        {
            get;
            set;
        }


        /// <summary>
        /// FieldLabels.
        /// </summary>
        public StringSafeDictionary<LocalizedLabel> FieldLabels
        {
            get;
            set;
        }


        /// <summary>
        /// Dictionary with FormEngineUserControl objects.
        /// </summary>
        public StringSafeDictionary<FormEngineUserControl> FieldControls
        {
            get;
            set;
        }


        /// <summary>
        /// Dictionary with EditingFormControl objects.
        /// </summary>
        public StringSafeDictionary<EditingFormControl> FieldEditingControls
        {
            get;
            set;
        }


        /// <summary>
        /// FieldErrorLabels.
        /// </summary>
        public StringSafeDictionary<LocalizedLabel> FieldErrorLabels
        {
            get;
            set;
        }


        /// <summary>
        /// Visibility controls hashtable.
        /// </summary>
        public Hashtable FieldVisibilityControls
        {
            get;
            set;
        }


        /// <summary>
        /// Contains FieldActions controls if the form is in design mode (BasicForm.IsDesignMode property).
        /// </summary>
        public StringSafeDictionary<FieldActions> FieldActionsControls
        {
            get;
            set;
        }


        /// <summary>
        /// Contains update panels which surround the fields (whole rows including labels) if the form is in design mode (BasicForm.IsDesignMode property).
        /// </summary>
        public StringSafeDictionary<UpdatePanel> FieldUpdatePanels
        {
            get;
            set;
        }

        #endregion


        #region "IControlContextManager Members"

        /// <summary>
        /// Gets the current control context.
        /// </summary>
        public ControlContext ControlContext
        {
            get
            {
                return mContextObject ?? (mContextObject = new ControlContext());
            }
        }

        #endregion


        #region "ITimeZoneManager Members"

        /// <summary>
        /// Gets or sets time zone type for child controls. Default value is TimeZoneTypeEnum.Inherit.
        /// </summary>
        public TimeZoneTypeEnum TimeZoneType
        {
            get
            {
                return mTimeZoneType;
            }
            set
            {
                mTimeZoneType = value;
            }
        }


        /// <summary>
        /// Gets or sets custom time zone info. This time zone is used when TimeZoneType is 'custom'.
        /// </summary>
        public TimeZoneInfo CustomTimeZone
        {
            get;
            set;
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor.
        /// </summary>
        protected AbstractBasicForm()
        {
            Fields = new List<string>();
        }

        #endregion


        #region "Life cycle methods"

        /// <summary>
        /// Renders the control at design-time.
        /// </summary>
        protected override void Render(HtmlTextWriter output)
        {
            base.Render(output);

            // Init scripts for spell check if allowed
            InitSpellCheck();
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Loads the values from the data container to the form controls.
        /// </summary>
        public virtual void LoadControlValues()
        {
            var data = Data;

            if ((data != null) && (FieldControls != null))
            {
                // Load the controls
                foreach (DictionaryEntry item in FieldControls)
                {
                    string fieldName = item.Key as string;

                    var control = item.Value as IFormControl;

                    if ((control != null) && data.ContainsColumn(fieldName))
                    {
                        control.LoadControlValue(GetDataValue(fieldName));
                        control.LoadOtherValues();
                    }
                }
            }
        }


        /// <summary>
        /// Gets the value of specified field.
        /// </summary>
        /// <param name="ctrl">The control</param>
        public static object GetControlValue(Control ctrl)
        {
            return ((IFormControl)ctrl)?.Value;
        }


        /// <summary>
        /// Returns DataRow value for the specified column.
        /// </summary>
        /// <param name="columnName">Column name</param>
        public object GetDataValue(string columnName)
        {
            return Data == null ? null : DataHelper.GetNull(Data.GetValue(columnName));
        }


        /// <summary>
        /// Checks if item has changed.
        /// </summary>
        /// <param name="columnName">Column</param>
        public virtual bool ItemChanged(string columnName)
        {
            if (Data.ColumnNames.Contains(columnName) && RequestHelper.IsPostBack())
            {
                object value = GetControlValue(columnName);
                return ValidationHelper.GetString(Data.GetValue(columnName), string.Empty) != ValidationHelper.GetString(value, string.Empty);
            }
            return false;
        }

        #endregion


        #region "Protected methods"

        /// <summary>
        /// Saves the control value to the data.
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="ctrl">Control with the value</param>
        protected virtual void SaveControlValue(string columnName, FormEngineUserControl ctrl)
        {
            object value = GetControlValue(columnName, ctrl);
            string strValue = value as string;

            // Convert to appropriate value
            FormFieldInfo ffi = ctrl.FieldInfo;
            if ((ffi == null) && (FormInformation != null))
            {
                ffi = FormInformation.GetFormField(columnName);
            }

            if (ffi != null)
            {
                // Detect empty value
                if (ffi.AllowEmpty && ((value == null) || (strValue == string.Empty)))
                {
                    value = null;
                    strValue = null;
                }
                else
                {
                    // Convert the value to a proper type
                    value = DataTypeManager.ConvertToSystemType(TypeEnum.Field, ffi.DataType, value);
                    strValue = value as string;
                }
            }

            // Trim the value
            if (ctrl.Trim && (strValue != null))
            {
                value = strValue.Trim();
            }

            // Convert to English string if column's type is string
            // (value type can be different for e.g. form control settings or web-part properties as they support non-resolved macro values)
            var drContainer = Data as DataRowContainer;
            DataRow dataRow = drContainer?.DataRow;
            if (dataRow != null && (dataRow.Table.Columns[columnName].DataType == typeof(string)))
            {
                value = Convert.ToString(value, CultureHelper.EnglishCulture);
            }

            SetDataValue(columnName, value);
        }


        /// <summary>
        /// Sets value of the given column in the data.
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="value">Column value</param>
        protected void SetDataValue(string columnName, object value)
        {
            Data?.SetValue(columnName, value);
        }


        /// <summary>
        /// Initializes the spell checker.
        /// </summary>
        protected virtual void InitSpellCheck()
        {
            if (AllowSpellCheck)
            {
                LoadSpellCheckFields();

                if (SpellCheckFields.Count > 0)
                {
                    string script = "if (typeof(spellCheckFields)==='undefined') {var spellCheckFields = new Array();} ";
                    foreach (string field in SpellCheckFields)
                    {
                        script += string.Format("spellCheckFields.push('{0}');", field);
                    }
                    ScriptHelper.RegisterStartupScript(this, typeof(string), ClientID, ScriptHelper.GetScript(script));
                }
            }
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Gets the value from a form control either directly or modified by an event handler.
        /// </summary>
        /// <param name="columnName">Name of the column</param>
        /// <param name="formControl">The form control</param>
        /// <returns>Value of the form control</returns>
        private object GetControlValue(string columnName, FormEngineUserControl formControl)
        {
            var eventArgs = new FormEngineUserControlEventArgs(columnName, formControl, formControl.Value);

            OnGetControlValue?.Invoke(this, eventArgs);

            return eventArgs.Value;
        }


        /// <summary>
        /// Tries to find appropriate value of control.
        /// </summary>
        /// <param name="columnName">Column name of edited object</param>
        private object GetControlValue(string columnName)
        {
            if (FieldControls != null)
            {
                // Try to obtain value from control
                FormEngineUserControl control = FieldControls[columnName];
                if (control != null)
                {
                    if ((control.HasValue) && (control.Enabled || ProcessDisabledFields))
                    {
                        return GetControlValue(columnName, control);
                    }
                }

                // Try to obtain value from other values
                foreach (FormEngineUserControl ctrl in FieldControls.Values)
                {
                    object value = ctrl.GetOtherValue(columnName);
                    if (value != null)
                    {
                        return value;
                    }
                }
            }

            return null;
        }


        /// <summary>
        /// Load client IDs of spell-check fields to SpellCheckFields property.
        /// </summary>
        private void LoadSpellCheckFields()
        {
            if (FieldControls != null)
            {
                foreach (FormEngineUserControl field in FieldControls.Values)
                {
                    if (field != null && field.Visible && field.FieldInfo != null && field.FieldInfo.SpellCheck)
                    {
                        List<string> clientIds = field.GetSpellCheckFields();
                        if (clientIds != null)
                        {
                            SpellCheckFields.AddRange(clientIds);
                        }
                    }
                }
            }
        }

        #endregion
    }
}