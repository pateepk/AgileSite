using System;

using CMS.Helpers;

namespace CMS.OnlineMarketing
{
    /// <summary>
    /// Describes configuration for A/B test conversion type.
    /// </summary>
    public sealed class ABTestConversionDefinition
    {
        /// <summary>
        /// Represents unique code name of conversion type.
        /// </summary>
        public string ConversionName
        {
            get;
        }


        /// <summary>
        /// Represents display name of conversion type.
        /// </summary>
        /// <remarks>
        /// String for localization can be used.
        /// </remarks>
        public string ConversionDisplayName
        {
            get;
        }


        /// <summary>
        /// A placeholder text for the conversion type default value field.
        /// </summary>
        /// <remarks>
        /// String for localization can be used.
        /// </remarks>
        public string DefaultValuePlaceholderText
        {
            get;
            set;
        }


        /// <summary>
        /// Definition of the form control used to pick the object related to conversion type.
        /// </summary>
        public ABTestFormControlDefinition FormControlDefinition
        {
            get;
        }


        /// <summary>
        /// Indicates whether the conversion is a system conversion.
        /// </summary>
        internal bool IsSystem
        {
            get;
        }


        /// <summary>
        /// Initializes a new instance of <see cref="ABTestConversionDefinition"/>.
        /// </summary>
        /// <param name="conversionName">Unique code name of conversion type.</param>
        /// <param name="conversionDisplayName">Display name of conversion type.</param>
        /// <exception cref="ArgumentException"><paramref name="conversionName"/> or <paramref name="conversionDisplayName"/> is null or empty, or <paramref name="conversionName"/> is not a valid code name (<see cref="ValidationHelper.IsCodeName"/>).</exception>
        public ABTestConversionDefinition(string conversionName, string conversionDisplayName)
            : this(conversionName, conversionDisplayName, false)
        {
        }


        /// <summary>
        /// Initializes a new instance of <see cref="ABTestConversionDefinition"/>.
        /// </summary>
        /// <param name="conversionName">Unique code name of conversion type.</param>
        /// <param name="conversionDisplayName">Display name of conversion type.</param>
        /// <param name="formControlDefinition">Form control definition used to pick the object related to conversion type.</param>
        /// <exception cref="ArgumentException"><paramref name="conversionName"/> or <paramref name="conversionDisplayName"/> is null or empty, or <paramref name="conversionName"/> is not a valid code name (<see cref="ValidationHelper.IsCodeName"/>).</exception>
        /// <exception cref="ArgumentNullException"><paramref name="formControlDefinition"/> is null.</exception>
        public ABTestConversionDefinition(string conversionName, string conversionDisplayName, ABTestFormControlDefinition formControlDefinition)
            : this(conversionName, conversionDisplayName, formControlDefinition, false)
        {
        }


        /// <summary>
        /// Initializes a new instance of <see cref="ABTestConversionDefinition"/>.
        /// </summary>
        /// <param name="conversionName">Unique code name of conversion type.</param>
        /// <param name="conversionDisplayName">Display name of conversion type.</param>
        /// <param name="isSystem">Determines whether the conversion should be registered as a system conversion.</param>
        /// <param name="defaultValuePlaceholderText">Placeholder text for the conversion type default value field.</param>
        /// <exception cref="ArgumentException"><paramref name="conversionName"/> or <paramref name="conversionDisplayName"/> is null or empty, or <paramref name="conversionName"/> is not a valid code name (<see cref="ValidationHelper.IsCodeName"/>).</exception>
        internal ABTestConversionDefinition(string conversionName, string conversionDisplayName, bool isSystem, string defaultValuePlaceholderText = null)
        {
            if (String.IsNullOrEmpty(conversionName))
            {
                throw new ArgumentException("Conversion name cannot be empty.", nameof(conversionName));
            }

            if (String.IsNullOrEmpty(conversionDisplayName))
            {
                throw new ArgumentException("Conversion display name cannot be empty.", nameof(conversionDisplayName));
            }

            if (!ValidationHelper.IsCodeName(conversionName))
            {
                throw new ArgumentException($"'{conversionName}' is not a valid code name for conversion type.", nameof(conversionName));
            }

            ConversionName = conversionName;
            ConversionDisplayName = conversionDisplayName;
            IsSystem = isSystem;
            DefaultValuePlaceholderText = defaultValuePlaceholderText;
        }


        /// <summary>
        /// Initializes a new instance of <see cref="ABTestConversionDefinition"/>.
        /// </summary>
        /// <param name="conversionName">Unique code name of conversion type.</param>
        /// <param name="conversionDisplayName">Display name of conversion type.</param>
        /// <param name="formControlDefinition">Form control definition used to pick the object related to conversion type.</param>
        /// <param name="isSystem">Determines whether the conversion should be registered as a system conversion.</param>
        /// <param name="defaultValuePlaceholderText">Placeholder text for the conversion type default value field.</param>
        /// <exception cref="ArgumentException"><paramref name="conversionName"/> or <paramref name="conversionDisplayName"/> is null or empty, or <paramref name="conversionName"/> is not a valid code name (<see cref="ValidationHelper.IsCodeName"/>).</exception>
        /// <exception cref="ArgumentNullException"><paramref name="formControlDefinition"/> is null.</exception>
        internal ABTestConversionDefinition(string conversionName, string conversionDisplayName, ABTestFormControlDefinition formControlDefinition, bool isSystem, string defaultValuePlaceholderText = null)
            : this(conversionName, conversionDisplayName, isSystem, defaultValuePlaceholderText)
        {
            FormControlDefinition = formControlDefinition ?? throw new ArgumentNullException( nameof(formControlDefinition));
        }
    }
}
