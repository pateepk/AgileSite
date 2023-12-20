using System;
using System.Collections.Generic;

namespace CMS.OnlineMarketing
{
    /// <summary>
    /// Register for conversion types used in A/B testing feature.
    /// </summary>
    public sealed class ABTestConversionDefinitionRegister
    {
        private static readonly Lazy<ABTestConversionDefinitionRegister> mInstance = new Lazy<ABTestConversionDefinitionRegister>(() => new ABTestConversionDefinitionRegister());
        private readonly IDictionary<string, ABTestConversionDefinition> mConfigurations = new Dictionary<string, ABTestConversionDefinition>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Gets the <see cref="ABTestConversionDefinitionRegister"/> instance.
        /// </summary>
        public static ABTestConversionDefinitionRegister Instance => mInstance.Value;


        /// <summary>
        /// Gets all registered instances of <see cref="ABTestConversionDefinition"/>.
        /// </summary>
        public IEnumerable<ABTestConversionDefinition> Items => mConfigurations.Values;


        /// <summary>
        /// Creates a new instance of <see cref="ABTestConversionDefinitionRegister"/>.
        /// </summary>
        internal ABTestConversionDefinitionRegister()
        {
        }


        /// <summary>
        /// Registers given <paramref name="definition"/> to the system.
        /// </summary>
        /// <param name="definition">Instance of <see cref="ABTestConversionDefinition"/> to be registered.</param>
        /// <exception cref="ArgumentNullException">Thrown when null <paramref name="definition"/> is provided.</exception>
        /// <exception cref="InvalidOperationException">Thrown when <paramref name="definition"/> with already registered <see cref="ABTestConversionDefinition.ConversionName"/> is provided.</exception>
        public void Register(ABTestConversionDefinition definition)
        {
            if (definition == null)
            {
                throw new ArgumentNullException(nameof(definition));
            }

            // Add the configuration to the collection if it's not registered yet, update if the new configuration is a custom one
            if (!mConfigurations.ContainsKey(definition.ConversionName))
            {
                mConfigurations.Add(definition.ConversionName, definition);
            }
            else if (!definition.IsSystem)
            {
                mConfigurations[definition.ConversionName] = definition;
            }
        }


        /// <summary>
        /// Gets registered configuration retrieved by given <paramref name="conversionName"/>.
        /// </summary>
        /// <param name="conversionName">Conversion type name.</param>
        /// <returns>Configuration instance or null.</returns>
        /// <exception cref="ArgumentException">Thrown when null or empty <paramref name="conversionName"/> is provided.</exception>
        public ABTestConversionDefinition Get(string conversionName)
        {
            if (String.IsNullOrEmpty(conversionName))
            {
                throw new ArgumentException(nameof(conversionName));
            }

            if (mConfigurations.ContainsKey(conversionName))
            {
                return mConfigurations[conversionName];
            }

            return null;
        }
    }
}
