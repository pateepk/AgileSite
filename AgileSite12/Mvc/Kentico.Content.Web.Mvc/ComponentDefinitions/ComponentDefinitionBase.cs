using System;

using CMS.Helpers;

namespace Kentico.Content.Web.Mvc
{
    /// <summary>
    /// Base class for component definition.
    /// </summary>
    public abstract class ComponentDefinitionBase
    {
        /// <summary>
        /// Unique identifier of the component definition.
        /// </summary> 
        public string Identifier { get; internal set; }


        /// <summary>
        /// Name of the registered component.
        /// </summary> 
        public string Name { get; internal set; }


        /// <summary>
        /// Creates an instance of the <see cref="ComponentDefinitionBase"/> class.
        /// </summary>
        /// <param name="identifier">Unique identifier of the component definition.</param>
        /// <param name="name">Name of the registered component.</param>
        protected ComponentDefinitionBase(string identifier, string name)
        {
            ValidateIdentifier(identifier);
            ValidateName(name);

            Identifier = identifier;
            Name = name;
        }


        /// <summary>
        /// Creates an empty instance of the <see cref="ComponentDefinitionBase"/> class.
        /// </summary>
        protected ComponentDefinitionBase()
        {
        }


        private void ValidateIdentifier(string identifier)
        {
            if (string.IsNullOrEmpty(identifier))
            {
                throw new ArgumentException("The component definition identifier cannot be empty.", nameof(identifier));
            }

            if (!ValidationHelper.IsCodeName(identifier))
            {
                throw new ArgumentException("The component definition identifier contains invalid character.", nameof(identifier));
            }
        }


        private void ValidateName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("The component name cannot be empty.", nameof(name));
            }
        }
    }
}
