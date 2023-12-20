using System;
using System.Data.SqlTypes;

using CMS.DataEngine;
using CMS.Helpers;

namespace CMS.FormEngine
{
    /// <summary>
    /// Provides check if given value matches field's data type.
    /// </summary>
    public class DataTypeIntegrity
    {
        private readonly object mValue;
        private readonly string mDataType;
        private readonly System.Globalization.CultureInfo mCulture;


        /// <summary>
        /// Defines the maximum number of digits which are allowed for decimal value.
        /// </summary>
        /// <remarks>The precision must be a value from 1 through the maximum precision of 38.</remarks>
        public int DecimalPrecision
        {
            get;
            set;
        }


        /// <summary>
        /// Defines the maximum number of decimal places which are allowed for decimal value.
        /// </summary>
        /// <remarks>Scale must be a value from 0 through <see cref="DecimalPrecision"/>. Scale can be specified only if <see cref="DecimalPrecision"/> is specified.</remarks>
        public int DecimalScale
        {
            get;
            set;
        }


        /// <summary>
        /// Defines maximal length of string value.
        /// </summary>
        public int TextLength
        {
            get;
            set;
        }

       
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="value">Control's value</param>
        /// <param name="ffi">Field info</param>
        /// <param name="culture">Allows to specify culture that will be used for culture dependent data types, e.g. double, date-time. Current culture is used if not set.</param>
        public DataTypeIntegrity(object value, FormFieldInfo ffi, System.Globalization.CultureInfo culture = null)
            : this(value, ffi?.DataType, culture)
        {
            if (ffi?.Size > 0)
            {
                DecimalPrecision = ffi.Size;
                TextLength = ffi.Size;
            }
            if (ffi?.Precision > 0)
            {
                DecimalScale = ffi.Precision;
            }
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="value">Control's value</param>
        /// <param name="dataType">Field data type</param>
        /// <param name="culture">Allows to specify culture that will be used for culture dependent data types, e.g. double, date-time. Current culture is used if not set.</param>
        public DataTypeIntegrity(object value, string dataType, System.Globalization.CultureInfo culture = null)
        {
            mValue = value;
            mDataType = dataType;
            mCulture = culture;
        }


        /// <summary>
        /// Checks if value matches field's data type.
        /// </summary>
        /// <returns>Error message if the value doesn't match field's data type; otherwise null.</returns>
        [Obsolete("Use GetValidationResult() method instead.")]
        public string ValidateDataType()
        {
            var result = GetValidationResult();
            return result.Success ? null : result.ErrorMessage;
        }


        /// <summary>
        /// Checks if value matches field's data type.
        /// </summary>
        /// <returns><see cref="DataTypeIntegrityValidationResult"/> object with error message set.</returns>
        public DataTypeIntegrityValidationResult GetValidationResult()
        {
            if ((mDataType != null) && (mValue != null) && (Convert.ToString(mValue) != String.Empty))
            {
                var valueType = DataTypeManager.GetDataType(TypeEnum.Field, mDataType.ToLowerInvariant());
                var isOfType = valueType.IsValueOfType(mValue, mCulture);
                var dataTypeLowerInvariant = mDataType.ToLowerInvariant();

                if (!isOfType)
                {
                    return new DataTypeIntegrityValidationResult(
                        DataTypeIntegrityValidationResultType.TypeError,
                        GetDataTypeErrorMessage(dataTypeLowerInvariant));
                }

                if (dataTypeLowerInvariant == FieldDataType.Text && !FurtherValidateTextLength())
                {
                    return new DataTypeIntegrityValidationResult(
                        DataTypeIntegrityValidationResultType.MaxLengthError,
                        GetValidateTextLengthErrorMessage());
                }

                if (dataTypeLowerInvariant == FieldDataType.Decimal && !FurtherValidateDecimal())
                {
                    return new DataTypeIntegrityValidationResult(
                        DataTypeIntegrityValidationResultType.PrecisionError,
                        GetValidateDecimalErrorMessage());
                }
            }

            return new DataTypeIntegrityValidationResult();
        }


        /// <summary>
        /// Gets error message for data type.
        /// </summary>
        private string GetDataTypeErrorMessage(string dataTypeLowerInvariant)
        {
            switch (dataTypeLowerInvariant)
            {
                // Integer type
                case FieldDataType.Integer:
                    return GetIntErrorMessage();

                // Long integer type
                case FieldDataType.LongInteger:
                    return GetLongErrorMessage();

                // Double and decimal type
                case FieldDataType.Decimal:
                case FieldDataType.Double:
                    return GetNotNumberErrorMessage();

                // DateTime type
                case FieldDataType.DateTime:
                case FieldDataType.Date:
                    return GetDateTimeErrorMessage();

                // TimeSpan type
                case FieldDataType.TimeSpan:
                    return ValidateTimeSpan();

                // GUID type
                case FieldDataType.Guid:
                    return GetGuidErrorMessage();

                default:
                    return null;
            }
        }


        /// <summary>
        /// Gets error message for integer type.
        /// </summary>
        private string GetIntErrorMessage()
        {
            return ResHelper.GetString("BasicForm.ErrorNotInteger");
        }


        /// <summary>
        /// Gets error message for long integer type.
        /// </summary>
        private string GetLongErrorMessage()
        {
            return ResHelper.GetString("BasicForm.ErrorNotLongInteger");
        }


        /// <summary>
        /// Gets error message for double type.
        /// </summary>
        private string GetNotNumberErrorMessage()
        {
            return ResHelper.GetString("BasicForm.ErrorNotNumber");
        }


        /// <summary>
        /// Checks if length of text is not exceeded.
        /// </summary>
        /// <returns>False if text length is exceeded.</returns>
        private bool FurtherValidateTextLength()
        {
            if ((TextLength > 0) && (ValidationHelper.GetString(mValue, String.Empty).Length > TextLength))
            {
                return false;
            }

            return true;
        }


        /// <summary>
        /// Gets error message if text length is exceeded.
        /// </summary>
        private string GetValidateTextLengthErrorMessage()
        {
            return String.Format(ResHelper.GetString("BasicForm.InvalidLength"), TextLength);
        }


        /// <summary>
        /// Further validates value for decimal type.
        /// </summary>
        /// <returns>False if validation failed.</returns>
        private bool FurtherValidateDecimal()
        {
            if (DecimalPrecision > 0)
            {
                var decimalValue = ValidationHelper.GetDecimal(mValue, decimal.MinValue, mCulture);

                // Get SQL decimal
                var sqlDecimal = new SqlDecimal(decimalValue);

                // Check number of decimal places and number of digits
                if ((sqlDecimal.Scale > DecimalScale) || (sqlDecimal.Precision - sqlDecimal.Scale > DecimalPrecision - DecimalScale))
                {
                    return false;
                }
            }

            return true;
        }


        /// <summary>
        /// Gets error message if decimal validation failed
        /// </summary>
        private string GetValidateDecimalErrorMessage()
        {
            return String.Format(ResHelper.GetString("BasicForm.ErrorNotDecimal"), DecimalPrecision, DecimalScale);
        }


        /// <summary>
        /// Gets error message for DateTime type.
        /// </summary>
        private string GetDateTimeErrorMessage()
        {
            return ResHelper.GetString("BasicForm.ErrorNotDateTime");
        }


        /// <summary>
        /// Validates value for TimeSpan type. Value must be less than 24hours to be accepted by Time DB data type.
        /// </summary>
        private string ValidateTimeSpan()
        {
            var time = ValidationHelper.GetTimeSpan(mValue, TimeSpan.MinValue, mCulture);
            if ((time == TimeSpan.MinValue) || (time.Days >= 1))
            {
                return ResHelper.GetString("BasicForm.ErrorNotTimeInterval");
            }
            return null;
        }


        /// <summary>
        /// Gets error message for GUID type.
        /// </summary>
        private string GetGuidErrorMessage()
        {
            return ResHelper.GetString("BasicForm.notguid");
        }
    }
}