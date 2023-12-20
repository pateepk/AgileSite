namespace CMS.Base
{
    /// <summary>
    /// Represents an error resulting from a validation.
    /// </summary>
    /// <seealso cref="IValidator"/>
    public interface IValidationError
    {
        /// <summary>
        /// Gets a key which can be used to retrieve a localized error message.
        /// </summary>
        string MessageKey
        {
            get;
        }


        /// <summary>
        /// Gets an array of parameters to be substituted to localized error message.
        /// Returns an empty array when no parameters are available.
        /// </summary>
        object[] MessageParameters
        {
            get;
        }
    }
}
