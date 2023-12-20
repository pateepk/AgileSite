using System;
using System.Collections.Generic;

namespace CMS.Newsletters
{
    /// <summary>
    /// Register for actions extending double opt-in feature.
    /// </summary>
    public sealed class DoubleOptInExtensionDefinitionRegister
    {
        private static readonly Lazy<DoubleOptInExtensionDefinitionRegister> mInstance = new Lazy<DoubleOptInExtensionDefinitionRegister>(() => new DoubleOptInExtensionDefinitionRegister());
        private readonly IList<DoubleOptInExtensionDefinition> mDefinitions = new List<DoubleOptInExtensionDefinition>();

        /// <summary>
        /// Gets the <see cref="DoubleOptInExtensionDefinitionRegister"/> instance.
        /// </summary>
        public static DoubleOptInExtensionDefinitionRegister Instance => mInstance.Value;


        /// <summary>
        /// Gets all registered instances of <see cref="DoubleOptInExtensionDefinition"/>.
        /// </summary>
        public IEnumerable<DoubleOptInExtensionDefinition> Items => mDefinitions;


        /// <summary>
        /// Creates a new instance of <see cref="DoubleOptInExtensionDefinitionRegister"/>.
        /// </summary>
        internal DoubleOptInExtensionDefinitionRegister()
        {
        }


        /// <summary>
        /// Registers given <paramref name="definition"/> to the system.
        /// </summary>
        /// <param name="definition">Instance of <see cref="DoubleOptInExtensionDefinition"/> to be registered.</param>
        /// <exception cref="ArgumentNullException">Thrown when null <paramref name="definition"/> is provided.</exception>
        public void Register(DoubleOptInExtensionDefinition definition)
        {
            if (definition == null)
            {
                throw new ArgumentNullException(nameof(definition));
            }

            mDefinitions.Add(definition);
        }
    }
}
