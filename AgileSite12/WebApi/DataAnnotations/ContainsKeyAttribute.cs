using System;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CMS.WebApi
{
    /// <summary>
    /// Validates if dictionary contains specific key.
    /// </summary>
    /// <remarks>
    /// This API supports the framework infrastructure and is not intended to be used directly from your code.
    /// </remarks>
    /// <exclude />
    [EditorBrowsable(EditorBrowsableState.Never)]
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
    public class ContainsKeyAttribute : ValidationAttribute
    {
        private readonly object mKey;


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="key">key which should be present in dictionary.</param>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is null</exception>
        public ContainsKeyAttribute(object key)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }

            mKey = key;
        }


        /// <summary>
        /// Checks if object is valid.
        /// </summary>
        /// <param name="value">Object to validate.</param>
        /// <param name="validationContext">Validation context.</param>
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var dict = value as IDictionary;
            if (dict == null)
            {
                return new ValidationResult("Value is not instance of IDictionary");
            }

            return dict.Contains(mKey) ? ValidationResult.Success : new ValidationResult("Dictionary does not contain key " + mKey);
        }
    }
}