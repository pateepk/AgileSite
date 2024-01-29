using System;

using CMS.DataEngine;
using CMS.Helpers;
using CMS.MacroEngine;

namespace CMS.FormEngine.Web.UI
{
    internal class FormControlValidation
    {
        #region "Variables"

        private readonly EditingFormControl validatedControl;
        private readonly FormEngineUserControl formControl;
        private readonly FormFieldInfo ffi;
        private readonly string errorMessage;
        private readonly bool checkFieldEmptiness;
        private object value;
        private string validationResult;
        private MacroResolver mContextResolver;
        private bool valueIsEmpty;
        private bool customErrorMessage;

        #endregion


        #region "Properties"

        /// <summary>
        /// Macro resolver
        /// </summary>
        private MacroResolver ContextResolver
        {
            get
            {
                if (mContextResolver == null)
                {
                    mContextResolver = MacroContext.CurrentResolver;
                }
                return mContextResolver;
            }
            set
            {
                mContextResolver = value;
            }
        }

        #endregion


        #region "Public methods and constructors"

        /// <summary>
        /// Constructor for validation of control's value.
        /// </summary>
        /// <param name="validatedControl">EditingFormControl to be validated</param>
        /// <param name="formControl">FormEngineUserControl representing user's control</param>
        /// <param name="ffi">FormFieldInfo with field settings</param>
        /// <param name="checkFieldEmptiness">Indicates if control should be checked for field emptiness</param>
        /// <param name="resolver">Macro resolver</param>
        public FormControlValidation(EditingFormControl validatedControl, FormEngineUserControl formControl, FormFieldInfo ffi, bool checkFieldEmptiness, MacroResolver resolver)
        {
            // Init variables
            this.validatedControl = validatedControl;
            this.formControl = formControl;
            this.ffi = ffi;
            this.checkFieldEmptiness = checkFieldEmptiness;
            ContextResolver = resolver;

            // Prepare error message
            if (ffi != null)
            {
                errorMessage = ffi.GetPropertyValue(FormFieldPropertyEnum.ValidationErrorMessage, ContextResolver);
            }

            if (!string.IsNullOrEmpty(errorMessage))
            {
                errorMessage = ResHelper.LocalizeString(errorMessage);
                customErrorMessage = true;
            }
            else
            {
                errorMessage = ResHelper.GetString("BasicForm.InvalidInput");
            }
            formControl.ErrorMessage = errorMessage;
        }


        /// <summary>
        /// Validates control.
        /// </summary>
        /// <returns>Returns error message when validation failed.</returns>
        public string Validate()
        {
            LoadControlValue();

            // Check field emptiness
            if (!ValidateFieldEmptiness())
            { }
            // For macro check only field emptiness and return
            else if ((validatedControl != null) && validatedControl.IsMacro)
            {
                return validationResult;
            }
            // Validate editing control
            else if (!CheckEditingControlValidity())
            { }
            // Validate form control
            else if (!CheckFormControlValidity())
            { }
            else
            {
                // Validate text length
                ValidateTextFieldLength();
            }

            // Check data type integrity if necessary
            CheckDataTypeIntegrity();

            // Check field validation rules if specified
            CheckMacroRules();

            return validationResult;
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Loads value from appropriate control.
        /// </summary>
        private void LoadControlValue()
        {
            var control = GetAppropriateControl();

            value = control.ValueForValidation;
            valueIsEmpty = FormControlContainsEmptyValue(control);
        }


        /// <summary>
        /// Returns <see cref="validatedControl"/> or <see cref="formControl"/> if <see cref="validatedControl"/> does not exists.
        /// </summary>
        private IFormControl GetAppropriateControl()
        {
            if (validatedControl != null)
            {
                return validatedControl;
            }

            return formControl;
        }


        /// <summary>
        /// Returns true, if <paramref name="control"/> has empty value, without any data.
        /// </summary>
        private bool FormControlContainsEmptyValue(IFormControl control)
        {
            var result = String.IsNullOrWhiteSpace(control.ValueForValidation as string) ? control.Value : control.ValueForValidation;

            return (result == null) || String.IsNullOrWhiteSpace(Convert.ToString(result));
        }


        /// <summary>
        /// Add validation error to validation result.
        /// </summary>
        /// <param name="error">Error to be appended to validation result</param>
        private void AppendValidationError(string error = null)
        {
            if (!String.IsNullOrEmpty(validationResult))
            {
                validationResult += " ";
            }

            if (!String.IsNullOrEmpty(error))
            {
                // Append validation error message
                validationResult += error;
            }
            else
            {
                // Apply default error message                                            
                validationResult += errorMessage;
            }
        }

        #endregion


        #region "Emptiness validation"

        /// <summary>
        /// Checks content of the field and if empty value is allowed.
        /// </summary>
        protected bool ValidateFieldEmptiness()
        {
            if (EmptinessValidationEnabled())
            {
                if (valueIsEmpty)
                {
                    AppendValidationError((customErrorMessage) ? errorMessage : ResHelper.GetString("BasicForm.ErrorEmptyValue"));
                    return false;
                }
            }
            return true;
        }


        /// <summary>
        /// Returns if control should be validated for emptiness.
        /// </summary>
        private bool EmptinessValidationEnabled()
        {
            // Check if control should be validated for emptiness
            return (checkFieldEmptiness && (ffi != null) && !ffi.AllowEmpty &&
                // Check for empty macro
                (((validatedControl != null) && validatedControl.IsMacro) ||
                // Check for empty value
                formControl.CheckFieldEmptiness));
        }

        #endregion


        #region "Control validation"

        /// <summary>
        /// Validate EditingFormControl.
        /// </summary>
        private bool CheckEditingControlValidity()
        {
            // Nested FormEngineUserControl is also validated in case the value is not macro
            if ((validatedControl != null) && !validatedControl.IsValid())
            {
                AppendValidationError(validatedControl.ValidationError);
                return false;
            }

            return true;
        }


        /// <summary>
        /// Validate FormEngineUserControl.
        /// </summary>
        private bool CheckFormControlValidity()
        {
            // Validate stand-alone form control (not nested in EditingFormControl)
            if ((validatedControl == null) && !formControl.IsValid())
            {
                AppendValidationError(formControl.ValidationError);
                return false;
            }

            return true;
        }

        #endregion


        #region "Length validation"

        /// <summary>
        /// Checks if text field's content is not longer than maximum field size (FormFieldInfo.Size may be database related).
        /// </summary>
        private void ValidateTextFieldLength()
        {
            if ((ffi != null) && (ffi.DataType == FieldDataType.Text) && ValidationHelper.GetString(value, "").Length > ffi.Size)
            {
                AppendValidationError(String.Format(ResHelper.GetString("BasicForm.InvalidLength"), ffi.Size));
            }
        }

        #endregion


        #region "Data type integrity"

        /// <summary>
        /// Checks if control's value matches data type.
        /// </summary>
        private void CheckDataTypeIntegrity()
        {
            if ((ffi != null) && !FormHelper.IsFieldOfType(ffi, FormFieldControlTypeEnum.LabelControl))
            {
                var checkType = new DataTypeIntegrity(value, ffi);
                string result = checkType.ValidateDataType();
                if (!String.IsNullOrEmpty(result))
                {
                    AppendValidationError(result);
                }
            }
        }

        #endregion


        #region "Macro rule validation"

        /// <summary>
        /// Checks field's validation macro rules.
        /// </summary>
        private void CheckMacroRules()
        {
            if (!valueIsEmpty && (ffi != null) && (ffi.FieldMacroRules.Count > 0))
            {
                ContextResolver.SetNamedSourceData("Value", value);

                foreach (var rule in ffi.FieldMacroRules)
                {
                    if ((rule != null) && !ValidationHelper.GetBoolean(ContextResolver.ResolveMacros(rule.MacroRule), false))
                    {
                        AppendValidationError(ResHelper.LocalizeString(rule.ErrorMessage));
                        break;
                    }
                }
            }
        }

        #endregion
    }
}