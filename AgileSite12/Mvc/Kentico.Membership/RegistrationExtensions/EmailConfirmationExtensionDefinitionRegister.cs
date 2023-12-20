using System;
using System.Collections.Generic;

namespace Kentico.Membership
{
    /// <summary>
    /// Register for actions extending registration email confirmation feature.
    /// </summary>
    public sealed class EmailConfirmationExtensionDefinitionRegister
    {
        private static readonly Lazy<EmailConfirmationExtensionDefinitionRegister> mInstance = new Lazy<EmailConfirmationExtensionDefinitionRegister>(() => new EmailConfirmationExtensionDefinitionRegister());
        private readonly IList<EmailConfirmationExtensionDefinition> mDefinitions = new List<EmailConfirmationExtensionDefinition>();

        /// <summary>
        /// Gets the <see cref="EmailConfirmationExtensionDefinitionRegister"/> instance.
        /// </summary>
        public static EmailConfirmationExtensionDefinitionRegister Instance => mInstance.Value;


        /// <summary>
        /// Gets all registered instances of <see cref="EmailConfirmationExtensionDefinition"/>.
        /// </summary>
        public IEnumerable<EmailConfirmationExtensionDefinition> Items => mDefinitions;


        /// <summary>
        /// Creates a new instance of <see cref="EmailConfirmationExtensionDefinitionRegister"/>.
        /// </summary>
        internal EmailConfirmationExtensionDefinitionRegister()
        {
        }


        /// <summary>
        /// Registers given <paramref name="definition"/> to the system.
        /// </summary>
        /// <param name="definition">Instance of <see cref="EmailConfirmationExtensionDefinition"/> to be registered.</param>
        /// <exception cref="ArgumentNullException">Thrown when null <paramref name="definition"/> is provided.</exception>
        public void Register(EmailConfirmationExtensionDefinition definition)
        {
            if (definition == null)
            {
                throw new ArgumentNullException(nameof(definition));
            }

            mDefinitions.Add(definition);
        }
    }
}
