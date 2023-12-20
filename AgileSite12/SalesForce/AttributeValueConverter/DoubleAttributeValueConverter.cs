using System;
using System.Collections.Generic;
using System.Globalization;

using CMS.DataEngine;
using CMS.FormEngine;
using CMS.Helpers;

namespace CMS.SalesForce
{
    /// <summary>
    /// Provides an implementation of the Double value converter.
    /// </summary>
    public sealed class DoubleAttributeValueConverter : AttributeValueConverterBase
    {
        #region "Constructors"

        /// <summary>
        /// Initializes a new instance of the Double value converter using the specified attribute model and field info.
        /// </summary>
        /// <param name="attributeModel">The entity attribute model to convert value to.</param>
        /// <param name="fieldInfo">The form field info to convert value from.</param>
        public DoubleAttributeValueConverter(EntityAttributeModel attributeModel, FormFieldInfo fieldInfo)
            : base(attributeModel, fieldInfo)
        {

        }

        #endregion

        #region "Public methods"

        /// <summary>
        /// Assigns the value from the specified CMS object field to the specified SalesForce entity attribute.
        /// </summary>
        /// <param name="entity">A SalesForce entity.</param>
        /// <param name="baseInfo">A CMS object.</param>
        public override void Convert(Entity entity, BaseInfo baseInfo)
        {
            switch (mFieldInfo.DataType)
            {
                case FieldDataType.Integer:
                case FieldDataType.LongInteger:
                case FieldDataType.Double:
                case FieldDataType.Decimal:
                    ConvertNumber(entity, baseInfo);
                    break;

                default:
                    throw new Exception(GetUnsupportedFieldTypeMessage(mFieldInfo.DataType));
            }
        }

        /// <summary>
        /// Creates a list of compatibility problems that might occur during the value conversion, and returns it.
        /// </summary>
        /// <returns>A list of compatibility problems that might occur during the value conversion.</returns>
        public override List<string> GetCompatibilityWarnings()
        {
            List<string> warnings = base.GetCompatibilityWarnings();

            double minValue = GetMinValue();
            double maxValue = GetMaxValue();

            if (!CheckPrecision(minValue) || !CheckPrecision(maxValue))
            {
                warnings.Add(ResHelper.GetString("sf.attributecompatibility.lowprecision"));
            }

            return warnings;
        }

        #endregion

        #region "Private methods"

        private void ConvertNumber(Entity entity, BaseInfo baseInfo)
        {
            object value = baseInfo.GetValue(mFieldInfo.Name);
            if (value != null || mAttributeModel.IsNullable)
            {
                entity[mAttributeModel.Name] = value;
            }
        }

        private double GetMinValue()
        {
            double[] range = GetDefaultRange();
            string minValue = mFieldInfo.MinValue;

            if (!String.IsNullOrEmpty(minValue))
            {
                double value = 0;
                if (Double.TryParse(minValue, out value))
                {
                    if (range[0] < value && value < range[1])
                    {
                        range[0] = value;
                    }
                }
            }

            return range[0];
        }

        private double GetMaxValue()
        {
            double[] range = GetDefaultRange();
            string maxValue = mFieldInfo.MaxValue;

            if (!String.IsNullOrEmpty(maxValue))
            {
                double value = 0;
                if (Double.TryParse(maxValue, out value))
                {
                    if (range[0] < value && value < range[1])
                    {
                        range[1] = value;
                    }
                }
            }

            return range[1];
        }

        private double[] GetDefaultRange()
        {
            switch (mFieldInfo.DataType)
            {
                case FieldDataType.Double:
                    return new[] { Double.MinValue, Double.MaxValue };

                case FieldDataType.LongInteger:
                    return new double[] { Int64.MinValue, Int64.MaxValue };

                case FieldDataType.Integer:
                    return new double[] { Int32.MinValue, Int32.MaxValue };

                case FieldDataType.Decimal:
                    return new double[] { (double)Decimal.MinValue, (double)Decimal.MaxValue };

                default:
                    throw new Exception(GetUnsupportedFieldTypeMessage(mFieldInfo.DataType));
            }
        }

        private bool CheckPrecision(double value)
        {
            string representation = Math.Floor(Math.Abs(value)).ToString("F0", System.Globalization.CultureInfo.InvariantCulture);

            return representation.Length <= mAttributeModel.Precision - mAttributeModel.Scale;
        }

        #endregion
    }
}