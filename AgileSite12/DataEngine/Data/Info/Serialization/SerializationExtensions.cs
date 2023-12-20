using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace CMS.DataEngine.Serialization
{
    /// <summary>
    /// Extension methods for serialization and deserialization of <see cref="CMS.DataEngine.BaseInfo"/>.
    /// </summary>
    public static class SerializationExtensions
    {
        #region "Methods"

        /// <summary>
        /// Returns object data serialized into the XML element.
        /// </summary>
        /// <param name="infoObject">Info object to serialize</param>
        /// <exception cref="ArgumentNullException"><paramref name="infoObject"/> is null.</exception>
        public static XmlElement Serialize(this BaseInfo infoObject)
        {
            return Serialize(infoObject, null);
        }


        /// <summary>
        /// Returns object data serialized into the XML element.
        /// </summary>
        /// <param name="infoObject">Info object to serialize</param>
        /// <param name="translationHelper">Custom helper <see cref="CMS.DataEngine.TranslationHelper"/> providing objects translation interface. Default helper is used if null</param>
        /// <param name="settings">Specific serialization settings that can override those from type info.</param>
        /// <exception cref="ArgumentNullException"><paramref name="infoObject"/> is null.</exception>
        internal static XmlElement Serialize(this BaseInfo infoObject, TranslationHelper translationHelper, SerializationSettings settings = null)
        {
            if (infoObject == null)
            {
                throw new ArgumentNullException(nameof(infoObject));
            }

            var serializer = new InfoSerializer();

            if (translationHelper != null)
            {
                serializer.TranslationHelper = translationHelper;
            }

            return serializer.SerializeObject(infoObject, settings);
        }


        /// <summary>
        /// Returns the XML element deserialized as a <see cref="CMS.DataEngine.BaseInfo"/> object.
        /// </summary>
        /// <param name="element">XML element containing all the object data</param>
        /// <exception cref="ArgumentNullException"><paramref name="element"/> is null.</exception>
        public static DeserializationResult Deserialize(this XmlElement element)
        {
            return Deserialize(element, null);
        }


        /// <summary>
        /// Returns the XML element deserialized as a <see cref="CMS.DataEngine.BaseInfo"/> object.
        /// </summary>
        /// <param name="element">XML element containing all the object data</param>
        /// <param name="translationHelper">Custom helper <see cref="CMS.DataEngine.TranslationHelper"/> providing objects translation interface. Default helper is used if null</param>
        /// <exception cref="ArgumentNullException"><paramref name="element"/> is null.</exception>
        internal static DeserializationResult Deserialize(this XmlElement element, TranslationHelper translationHelper)
        {
            if (element == null)
            {
                throw new ArgumentNullException(nameof(element));
            }

            var deserializer = new InfoDeserializer();

            if (translationHelper != null)
            {
                deserializer.TranslationHelper = translationHelper;
            }

            return deserializer.DeserializeObject(element);
        }


        /// <summary>
        /// Adds new failed mapping into provided collection.
        /// </summary>
        /// <param name="failedMappings">Collection of failed mappings (see <see cref="DeserializationResult"/>).</param>
        /// <param name="fieldName">Name of field that failed to translate reference.</param>
        /// <param name="translationReference">Translation reference that failed to translate into and ID.</param>
        internal static void Add(this ICollection<FailedMapping> failedMappings, string fieldName, TranslationReference translationReference)
        {
            failedMappings.Add(new FailedMapping(fieldName, translationReference));
        }


        /// <summary>
        /// Selects only such <see cref="FailedMapping"/>s which <see cref="FailedMapping.FieldName"/>
        /// is same as <paramref name="fieldName"/> from the <paramref name="failedMappings"/> (comparison is case insensitive).
        /// </summary>
        /// <param name="failedMappings">Collection of failed mappings (see <see cref="DeserializationResult"/>).</param>
        /// <param name="fieldName">Name of field that which failed <see cref="TranslationReference"/>s should be selected.</param>
        /// <remarks><paramref name="fieldName"/> comparison is case insensitive.</remarks>
        public static IEnumerable<FailedMapping> SelectField(this ICollection<FailedMapping> failedMappings, string fieldName)
        {
            return failedMappings.Where(failedMapping => failedMapping.FieldName.Equals(fieldName, StringComparison.InvariantCultureIgnoreCase));
        }


        /// <summary>
        /// Returns <see langword="true"/> if provided <paramref name="fieldName"/> has at least one reference
        /// in the <paramref name="failedMappings"/> collection.
        /// </summary>
        /// <param name="failedMappings">Collection of failed mappings (see <see cref="DeserializationResult"/>).</param>
        /// <param name="fieldName">Name of field that might be present in the collection</param>
        /// <remarks><paramref name="fieldName"/> comparison is case insensitive.</remarks>
        public static bool ContainsField(this ICollection<FailedMapping> failedMappings, string fieldName)
        {
            return failedMappings.SelectField(fieldName).Any();
        }

        #endregion
    }
}