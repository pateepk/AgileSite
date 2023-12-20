using System;
using System.Collections.Generic;

using CMS.Core;

namespace Kentico.Content.Web.Mvc
{
    /// <summary>
    ///  Stores component definitions.
    /// </summary>
    /// <typeparam name="TDefinition">Type of the component definition.</typeparam>
    internal sealed class ComponentDefinitionStore<TDefinition>
        where TDefinition : ComponentDefinitionBase
    {
        private ComponentRegister<string, TDefinition> components;


        private static readonly Lazy<ComponentDefinitionStore<TDefinition>> mInstance = new Lazy<ComponentDefinitionStore<TDefinition>>(() => new ComponentDefinitionStore<TDefinition>());


        /// <summary>
        /// Gets current instance of the <see cref="ComponentDefinitionStore{TDefinition}"/> class.
        /// </summary>
        public static ComponentDefinitionStore<TDefinition> Instance => mInstance.Value;


        static ComponentDefinitionStore()
        {
            TypeManager.RegisterGenericType(typeof(ComponentDefinitionStore<TDefinition>));
        }


        /// <summary>
        /// Creates an instance of the <see cref="ComponentDefinitionStore{TDefinition}"/> class.
        /// </summary>
        internal ComponentDefinitionStore()
        {
            InitializeComponentsRegister();
        }


        /// <summary>
        /// Gets all registered component definitions.
        /// </summary>
        public IEnumerable<TDefinition> GetAll()
        {
            return components.GetAll();
        }


        /// <summary>
        /// Gets a component definition by its <see cref="ComponentDefinitionBase.Identifier"/>.
        /// </summary>
        /// <param name="identifier">Identifier of the component definition to retrieve.</param>
        /// <returns>Returns component definition with given identifier, or null when not found.</returns>
        public TDefinition Get(string identifier)
        {
            if (components.TryGetValue(identifier, out var definition))
            {
                return definition;
            }
            return default(TDefinition);
        }


        /// <summary>
        /// Adds a component definition to a appropriate store.
        /// </summary>
        /// <param name="registeredDefinition">Component definition to register.</param>
        /// <exception cref="ArgumentNullException"> Is thrown when <paramref name="registeredDefinition"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException"> Is thrown when the store already contains component with the same identifier as <paramref name="registeredDefinition"/>.</exception>

        public void Add(TDefinition registeredDefinition)
        {
            if (registeredDefinition == null)
            {
                throw new ArgumentNullException(nameof(registeredDefinition));
            }

           
            if (!components.TryAddUnsafe(registeredDefinition.Identifier, registeredDefinition))
            {
                throw new ArgumentException($"Component with identifier '{registeredDefinition.Identifier}' cannot be registered because another component with such identifier is already present.");
            }
        }


        /// <summary>
        /// Clears collection of registered components.
        /// </summary>
        internal void Clear()
        {
            InitializeComponentsRegister();
        }


        private void InitializeComponentsRegister()
        {
            components = new ComponentRegister<string, TDefinition>(StringComparer.InvariantCultureIgnoreCase);
        }
    }
}
