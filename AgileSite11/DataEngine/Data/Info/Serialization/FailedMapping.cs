using System;
using System.Linq;
using System.Text;

namespace CMS.DataEngine.Serialization
{
    /// <summary>
    /// Provides information of name of field and translation reference that was impossible to map to existing DB objects.
    /// </summary>
    public class FailedMapping
    {
        /// <summary>
        /// Creates a new instance of <see cref="FailedMapping"/>.
        /// </summary>
        /// <param name="fieldName">Name of field the translation to existing DB object failed for.</param>
        /// <param name="translationReference">Reference that failed to translate to existing DB object.</param>
        /// <exception cref="ArgumentException">Thrown when file name is either <see langword="null"/> or empty.</exception>
        /// <exception cref="ArgumentNullException">Thrown when reference was not provided.</exception>
        public FailedMapping(string fieldName, TranslationReference translationReference)
        {
            if (String.IsNullOrEmpty(fieldName))
            {
                throw new ArgumentException("Field name cannot be null nor empty.", fieldName);
            }
            if (translationReference == null)
            {
                throw new ArgumentNullException("translationReference");
            }

            FieldName = fieldName;
            TranslationReference = translationReference;
        }


        /// <summary>
        /// Name of field the translation to existing DB object failed for.
        /// </summary>
        public string FieldName
        {
            get;
            private set;
        }


        /// <summary>
        /// Reference to object that failed to translate to existing DB object.
        /// </summary>
        public TranslationReference TranslationReference
        {
            get;
            private set;
        }


        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        public override string ToString()
        {
            return "FieldName: " + FieldName + ", Reference: " + TranslationReference;
        }
    }
}
