namespace CMS.FormEngine
{
    /// <summary>
    /// DataType integrity validation result types
    /// </summary>
    public enum DataTypeIntegrityValidationResultType
    {
        /// <summary>
        /// Validation passed
        /// </summary>
        NoError,

        /// <summary>
        /// Input value cannot be converted to required type
        /// </summary>
        TypeError,

        /// <summary>
        /// String length exceeds database allowed storage size
        /// </summary>
        MaxLengthError,

        /// <summary>
        /// Decimal precision is not valid
        /// </summary>
        PrecisionError,
    }
}
