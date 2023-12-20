using System;

namespace CMS.FormEngine
{
    /// <summary>
    /// DataType integrity validation result object
    /// </summary>
    public class DataTypeIntegrityValidationResult
    {
        /// <summary>
        /// DataType integrity validation result type
        /// </summary>
        public DataTypeIntegrityValidationResultType ResultType { get; }


        /// <summary>
        /// Error message if validation failed
        /// </summary>
        public string ErrorMessage { get; }


        /// <summary>
        /// Represents the success of the validation,
        /// (<see langword="true"/> if validation was successful; otherwise, <see langword="false"/>).
        /// </summary>
        /// <remarks><seealso cref="DataTypeIntegrityValidationResultType.NoError"/></remarks>
        public bool Success
        {
            get
            {
                return ResultType == DataTypeIntegrityValidationResultType.NoError;
            }
        }


        /// <summary>
        /// Initializes a new instance of <see cref="DataTypeIntegrityValidationResult"/>
        /// class with a successful validation.
        /// </summary>
        public DataTypeIntegrityValidationResult()
        {
            ResultType = DataTypeIntegrityValidationResultType.NoError;
            ErrorMessage = null;
        }


        /// <summary>
        /// Initializes a new instance of <see cref="DataTypeIntegrityValidationResult"/>
        /// class by using a validation result type and an error message.
        /// </summary>
        public DataTypeIntegrityValidationResult(
            DataTypeIntegrityValidationResultType resultType, string errorMessage)
        {
            if ((resultType == DataTypeIntegrityValidationResultType.NoError) && !String.IsNullOrEmpty(errorMessage))
            {
                throw new ArgumentException("Error message should be empty for non-error result type.", nameof(errorMessage));
            }

            if ((resultType != DataTypeIntegrityValidationResultType.NoError) && String.IsNullOrEmpty(errorMessage))
            {
                throw new ArgumentException("Error message should be provided when result type is an error.", nameof(errorMessage));
            }

            ResultType = resultType;
            ErrorMessage = errorMessage;
        }


        /// <summary>
        /// Initializes a new instance of <see cref="DataTypeIntegrityValidationResult"/>
        /// class by using <see cref="DataTypeIntegrityValidationResult"/> object.
        /// </summary>
        public DataTypeIntegrityValidationResult(
            DataTypeIntegrityValidationResult validationResult)
            : this(validationResult.ResultType, validationResult.ErrorMessage)
        {
        }
    }
}
