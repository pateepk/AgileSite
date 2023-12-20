using CMS.DataEngine;
using CMS.FormEngine;

namespace Kentico.Forms.Web.Mvc
{
    internal static class FormComponentDataTypeHelper
    {
        /// <summary>
        /// Returns size explicitly set in <paramref name="size"/>, or default size of given <paramref name="dataType"/>.
        /// </summary>
        public static int EnsureSize(string dataType, int size)
        {
            if (size > 0)
            {
                return size;
            }

            return DataTypeManager.GetDataType(TypeEnum.Field, dataType)?.DefaultSize ?? size;
        }


        /// <summary>
        /// Returns precision explicitly set in <paramref name="precision"/>, or default precision of given <paramref name="dataType"/>.
        /// </summary>
        public static int EnsurePrecision(string dataType, int precision)
        {
            if (precision > 0)
            {
                return precision;
            }

            return DataTypeManager.GetDataType(TypeEnum.Field, dataType)?.DefaultPrecision ?? precision;
        }


        /// <summary>
        /// Validates whether value of given <paramref name="component"/> meets constraints of it's data type.
        /// </summary>
        /// <param name="component">Form component to validate.</param>
        /// <returns>Possible validation error.</returns>
        public static DataTypeIntegrityValidationResult GetValueDataTypeValidationResult(FormComponent component)
        {
            var dataTypeIntegrity = new DataTypeIntegrity(component.GetObjectValue(), component.BaseProperties.DataType);

            // When in biz form component context then perform SQL validation on size and percision.
            // Because only in biz form context values are stored in DB as columns.
            var performDataTypeSizeValidation = component.GetBizFormComponentContext() != null;
            if (performDataTypeSizeValidation)
            {
                dataTypeIntegrity.TextLength = component.BaseProperties.Size;
                dataTypeIntegrity.DecimalPrecision = component.BaseProperties.Size;
                dataTypeIntegrity.DecimalScale = component.BaseProperties.Precision;
            }

            return dataTypeIntegrity.GetValidationResult();
        }
    }
}
