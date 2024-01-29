using System;

using CMS.Base;
using CMS.Base.Web.UI;
using CMS.DataEngine;
using CMS.EventLog;
using CMS.Helpers;

namespace CMS.FormEngine.Web.UI
{
    /// <summary>
    /// Base class for the text filter control.
    /// </summary>
    public abstract class TextFilterControl: FormEngineUserControl
    {
        #region "Constants"

        // Default format of filter format, it is used when {3} is filled in
        private const string DEFAULT_CUSTOM_CONDITION_FORMAT = "[{0}] {2} {1}";

        #endregion


        #region "Variables"

        private string mOperatorFieldName;

        #endregion


        #region "Properties"

        /// <summary>
        /// Textbox control
        /// </summary>
        protected abstract CMSTextBox TextBoxControl
        {
            get;
        }


        /// <summary>
        /// Drop down list control
        /// </summary>
        protected abstract CMSDropDownList OperatorControl
        {
            get;
        }


        /// <summary>
        /// Gets or sets value.
        /// </summary>
        public override object Value
        {
            get
            {
                return TextBoxControl.Text;
            }
            set
            {
                TextBoxControl.Text = ValidationHelper.GetString(value, null);
            }
        }


        /// <summary>
        /// Default value for the operator value.
        /// </summary>
        protected TextCompareOperatorEnum DefaultOperator
        {
            get
            {
                var val = ValidationHelper.GetString(GetValue("DefaultOperator"), "");
                TextCompareOperatorEnum enumVal;

                if (Enum.TryParse(val, true, out enumVal))
                {
                    return enumVal;
                }

                return TextCompareOperatorEnum.Like;
            }
            set
            {
                SetValue("DefaultOperator", value);
            }
        }


        /// <summary>
        /// Gets name of the field for operator value. Default value is '{FieldName}Operator' where {FieldName} is name of the current field.
        /// </summary>
        protected string OperatorFieldName
        {
            get
            {
                if (string.IsNullOrEmpty(mOperatorFieldName))
                {
                    // Get name of the field for operator value
                    mOperatorFieldName = DataHelper.GetNotEmpty(GetValue("OperatorFieldName"), Field + "Operator");
                }

                return mOperatorFieldName;
            }
        }


        #endregion


        #region "Methods"

        /// <summary>
        /// Returns other values related to this form control.
        /// </summary>
        /// <returns>Returns an array where first dimension is attribute name and the second dimension is its value.</returns>
        public override object[,] GetOtherValues()
        {
            if (Form.Data is DataRowContainer)
            {
                if (!ContainsColumn(OperatorFieldName))
                {
                    Form.DataRow.Table.Columns.Add(OperatorFieldName);
                }
            }

            // Set properties names
            object[,] values = new object[1, 2];

            values[0, 0] = OperatorFieldName;
            values[0, 1] = OperatorControl.SelectedValue;

            return values;
        }


        /// <summary>
        /// Creates child controls
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            CheckFieldEmptiness = false;
            InitFilterDropDown();

            LoadOtherValues();
        }
        

        /// <summary>
        /// OnPreRender event handler
        /// </summary>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            // Disable text field for 'Empty' and 'Not Empty' values
            TextCompareOperatorEnum op = (TextCompareOperatorEnum)Enum.Parse(typeof(TextCompareOperatorEnum), OperatorControl.SelectedValue);
            TextBoxControl.Enabled = (op != TextCompareOperatorEnum.Empty) && (op != TextCompareOperatorEnum.NotEmpty);
            if (!TextBoxControl.Enabled)
            {
                TextBoxControl.Text = String.Empty;
            }

            OperatorControl.Attributes.Add("onchange", "enableInput(this.value, '" + TextBoxControl.ClientID + "');");

            RegisterScripts();
        }


        private void RegisterScripts()
        {
            string script = String.Format(@"
function enableInput(selectedValue, inputId) {{
    var inputElem = document.getElementById(inputId);
    inputElem.disabled = (selectedValue == {0} || selectedValue == {1});
    if (inputElem.disabled) {{
        inputElem.value = '';
    }}
}}", (int)TextCompareOperatorEnum.Empty, (int)TextCompareOperatorEnum.NotEmpty);

            ScriptHelper.RegisterClientScriptBlock(this, typeof(string), "TextFilter", script, true);
        }


        /// <summary>
        /// Loads the other fields values to the state of the form control.
        /// </summary>
        public override void LoadOtherValues()
        {
            OperatorControl.SelectedValue = ValidationHelper.GetString(GetColumnValue(OperatorFieldName), DefaultOperator.ToString("D"));
        }


        /// <summary>
        /// Initializes operator filter drop-down list.
        /// </summary>
        private void InitFilterDropDown()
        {
            if (OperatorControl.Items.Count == 0)
            {
                ControlsHelper.FillListControlWithEnum<TextCompareOperatorEnum>(OperatorControl, "filter");
            }
        }


        /// <summary>
        /// Gets where condition.
        /// </summary>
        public override string GetWhereCondition()
        {
            EnsureChildControls();

            TextCompareOperatorEnum op = (TextCompareOperatorEnum)Enum.Parse(typeof(TextCompareOperatorEnum), OperatorControl.SelectedValue);
            string tempVal = ValidationHelper.GetString(Value, string.Empty);
            string escapedVal = EscapeValue(tempVal, op);

            if (string.IsNullOrEmpty(tempVal) && ((op != TextCompareOperatorEnum.Empty) && (op != (TextCompareOperatorEnum.NotEmpty))))
            {
                // Value isn't set (doesn't have to be for empty and not empty)
                return null;
            }

            string field = (FieldInfo != null) ? FieldInfo.Name : Field;
            string textOp = null;
            WhereCondition condition = new WhereCondition();

            switch (op)
            {
                case TextCompareOperatorEnum.Like:
                    condition.WhereContains(field, tempVal);

                    textOp = WhereBuilder.LIKE;
                    tempVal = string.Format("N'%{0}%'", escapedVal);
                    break;

                case TextCompareOperatorEnum.NotLike:
                    condition.WhereNotContains(field, tempVal);

                    textOp = WhereBuilder.NOT_LIKE;
                    tempVal = string.Format("N'%{0}%'", escapedVal);
                    break;

                case TextCompareOperatorEnum.StartsWith:
                    condition.WhereStartsWith(field, tempVal);

                    textOp = WhereBuilder.LIKE;
                    tempVal = string.Format("N'{0}%'", escapedVal);
                    break;

                case TextCompareOperatorEnum.NotStartsWith:
                    condition.WhereNotStartsWith(field, tempVal);

                    textOp = WhereBuilder.NOT_LIKE;
                    tempVal = string.Format("N'{0}%'", escapedVal);
                    break;

                case TextCompareOperatorEnum.EndsWith:
                    condition.WhereEndsWith(field, tempVal);

                    textOp = WhereBuilder.LIKE;
                    tempVal = string.Format("N'%{0}'", escapedVal);
                    break;

                case TextCompareOperatorEnum.NotEndsWith:
                    condition.WhereNotEndsWith(field, tempVal);

                    textOp = WhereBuilder.NOT_LIKE;
                    tempVal = string.Format("N'%{0}'", escapedVal);
                    break;

                case TextCompareOperatorEnum.Equals:
                    condition.WhereEquals(field, tempVal);

                    textOp = WhereBuilder.EQUAL;
                    tempVal = string.Format("N'{0}'", escapedVal);
                    break;

                case TextCompareOperatorEnum.NotEquals:
                    condition.WhereNotEquals(field, tempVal);

                    textOp = WhereBuilder.NOT_EQUAL;
                    tempVal = string.Format("N'{0}'", escapedVal);
                    break;

                case TextCompareOperatorEnum.Empty:
                    condition.WhereEmpty(field);
                    break;

                case TextCompareOperatorEnum.NotEmpty:
                    condition.WhereNotEmpty(field);
                    break;
            }

            switch (op)
            {
                // If operator is starts with not (except NotEmpty), include also empty records which also fulfill the condition
                case TextCompareOperatorEnum.NotEndsWith:
                case TextCompareOperatorEnum.NotEquals:
                case TextCompareOperatorEnum.NotLike:
                case TextCompareOperatorEnum.NotStartsWith:
                    condition.Or().WhereEmpty(field);
                    break;
            }

            if (String.IsNullOrEmpty(WhereConditionFormat) || !SupportsWhereConditionFormat(op))
            {
                // Return default where condition
                return condition.ToString(true);
            }

            try
            {
                // If where condition format contains default format, we can use condition object instead of manual creation using the String.Format method 
                if (WhereConditionFormat.Contains(DEFAULT_CUSTOM_CONDITION_FORMAT))
                {
                    return WhereConditionFormat.Replace(DEFAULT_CUSTOM_CONDITION_FORMAT, condition.ToString(true));
                }

                // Return custom where condition
                return string.Format(WhereConditionFormat, field, tempVal, textOp);
            }
            catch (Exception ex)
            {
                // Log exception
                EventLogProvider.LogException("TextFilter", "GetWhereCondition", ex);

                ShowError("Failed to generate the filter where condition. See event log for more details. Event name 'TextFilter'.");
            }

            return null;
        }


        /// <summary>
        /// Indicates if where condition format is supported by the defined operator
        /// </summary>
        /// <param name="op">Current operator</param>
        private bool SupportsWhereConditionFormat(TextCompareOperatorEnum op)
        {
            switch (op)
            {
                case TextCompareOperatorEnum.Empty:
                case TextCompareOperatorEnum.NotEmpty:
                    return false;

                default:
                    return true;
            }
        }


        /// <summary>
        /// Escapes given value based on used operator.
        /// </summary>
        /// <param name="value">Value to be escaped</param>
        /// <param name="op">Operator which will be used</param>
        private string EscapeValue(string value, TextCompareOperatorEnum op)
        {
            value = SqlHelper.EscapeQuotes(value);

            switch (op)
            {
                case TextCompareOperatorEnum.NotEndsWith:
                case TextCompareOperatorEnum.NotLike:
                case TextCompareOperatorEnum.NotStartsWith:
                case TextCompareOperatorEnum.EndsWith:
                case TextCompareOperatorEnum.Like:
                case TextCompareOperatorEnum.StartsWith:
                    return SqlHelper.EscapeLikeText(value);
            }

            return value;
        }

        #endregion
    }
}