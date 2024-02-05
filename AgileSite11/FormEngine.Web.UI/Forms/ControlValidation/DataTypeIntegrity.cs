using System;
using System.Data.SqlTypes;
using System.Globalization;

using CMS.DataEngine;
using CMS.Helpers;

namespace CMS.FormEngine.Web.UI
{
    /// <summary>
    /// Provides check if given value matches field's data type.
    /// </summary>
    public class DataTypeIntegrity
    {
        #region "Variables"

        private readonly object mValue;
        private readonly string mDataType;
        private readonly CultureInfo mCulture;

        #endregion


        #region "Properties"

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

        #endregion


        #region "Public methods and constructors"

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="value">Control's value</param>
        /// <param name="ffi">Field info</param>
        /// <param name="culture">Allows to specify culture that will be used for culture dependent data types, e.g. double, date-time. Current culture is used if not set.</param>
        public DataTypeIntegrity(object value, FormFieldInfo ffi, CultureInfo culture = null)
            : this(value, ffi?.DataType, culture)
        {
            if (ffi?.Size > 0)
            {
                DecimalPrecision = ffi.Size;
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
        public DataTypeIntegrity(object value, string dataType, CultureInfo culture = null)
        {
            mValue = value;
            mDataType = dataType;
            mCulture = culture;
        }


        /// <summary>
        /// Checks if value matches field's data type.
        /// </summary>
        /// <returns>Error message if the value doesn't match field's data type; otherwise null.</returns>
        public string ValidateDataType()
        {
            if ((mDataType != null) && (mValue != null) && (Convert.ToString(mValue) != String.Empty))
            {
                switch (mDataType.ToLowerInvariant())
                {
                    // Integer type
                    case FieldDataType.Integer:
                        return ValidateInteger();

                    // Long integer type
                    case FieldDataType.LongInteger:
                        return ValidateLongInteger();

                    // Double type
                    case FieldDataType.Double:
                        return ValidateDouble();

                    case FieldDataType.Decimal:
                        return ValidateDecimal();

                    // DateTime type
                    case FieldDataType.DateTime:
                    case FieldDataType.Date:
                        return ValidateDateTime();

                    // TimeSpan type
                    case FieldDataType.TimeSpan:
                        return ValidateTimeSpan();

                    // GUID type
                    case FieldDataType.Guid:
                        return ValidateGUID();
                }
            }

            return null;
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Validates value for integer type.
        /// </summary>
        private string ValidateInteger()
        {
            if (!ValidationHelper.IsInteger(mValue))
            {
                return ResHelper.GetString("BasicForm.ErrorNotInteger");
            }
            return null;
        }


        /// <summary>
        /// Validates value for long integer type.
        /// </summary>
        private string ValidateLongInteger()
        {
            if (!ValidationHelper.IsLong(mValue))
            {
                return ResHelper.GetString("BasicForm.ErrorNotLongInteger");
            }
            return null;
        }


        /// <summary>
        /// Validates value for double type.
        /// </summary>
        private string ValidateDouble()
        {
            if (!ValidationHelper.IsDouble(mValue, mCulture?.Name))
            {
                return ResHelper.GetString("BasicForm.ErrorNotNumber");
            }
            return null;
        }


        /// <summary>
        /// Validates value for decimal type.
        /// </summary>
        private string ValidateDecimal()
        {
            if (ValidationHelper.IsDecimal(mValue, mCulture?.Name))
            {
                if (DecimalPrecision > 0)
                {
                    var decimalValue = ValidationHelper.GetDecimal(mValue, decimal.MinValue, mCulture);

                    // Get SQL decimal
                    var sqlDecimal = new SqlDecimal(decimalValue);

                    // Check number of decimal places and number of digits
                    if ((sqlDecimal.Scale > DecimalScale) || (sqlDecimal.Precision - sqlDecimal.Scale > DecimalPrecision - DecimalScale))
                    {
                        return string.Format(ResHelper.GetString("BasicForm.ErrorNotDecimal"), DecimalPrecision, DecimalScale);
                    }
                }
            }
            else
            {
                return ResHelper.GetString("BasicForm.ErrorNotNumber");
            }

            return null;
        }


        /// <summary>
        /// Validates value for DateTime type.
        /// </summary>
        private string ValidateDateTime()
        {
            if (!ValidationHelper.IsDateTime(mValue, mCulture))
            {
                return ResHelper.GetString("BasicForm.ErrorNotDateTime");
            }
            return null;
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
        /// Validates value for GUID type.
        /// </summary>
        private string ValidateGUID()
        {
            if (!ValidationHelper.IsGuid(mValue))
            {
                return ResHelper.GetString("BasicForm.notguid");
            }
            return null;
        }

        #endregion
    }
}