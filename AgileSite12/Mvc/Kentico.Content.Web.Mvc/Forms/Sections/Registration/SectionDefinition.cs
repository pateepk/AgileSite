using System;

using Kentico.Content.Web.Mvc;

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Definition of registered section for Form builder.
    /// </summary>
    public sealed class SectionDefinition : ComponentDefinitionBase, IFormBuilderDefinition
    {
        private const string CONTROLLER_NAME_SUFFIX = "Controller";


        /// <summary>
        /// Gets the type of the section.
        /// </summary>
        public Type SectionType { get; }


        /// <summary>
        /// Name of the registered section controller - this property is based on <see cref="SectionType"/>.Name property without "Controller" suffix.
        /// </summary>
        internal string ControllerName { get; set; }


        /// <summary>
        /// Description of the registered section.
        /// </summary> 
        public string Description { get; internal set; }


        /// <summary>
        /// Icon CSS class of the registered section.
        /// </summary> 
        public string IconClass { get; internal set; }


        Type IFormBuilderDefinition.DefinitionType => SectionType;


        /// <summary>
        /// Creates an instance of the <see cref="SectionDefinition"/> class.
        /// </summary>
        /// <param name="identifier">Unique identifier of the section definition.</param>
        /// <param name="sectionType">Type of the registered section controller.</param>
        /// <param name="name">Name of the registered section.</param>
        public SectionDefinition(string identifier, Type sectionType, string name)
            : base(identifier, name)
        {
            ValidateSectionType(sectionType);

            SectionType = sectionType;
            ControllerName = sectionType.Name.Substring(0, sectionType.Name.Length - CONTROLLER_NAME_SUFFIX.Length);
        }


        /// <summary>
        /// Creates an empty instance o the <see cref="SectionDefinition"/> class. 
        /// </summary>
        internal SectionDefinition()
        {
        }


        private void ValidateSectionType(Type controllerType)
        {
            if (controllerType == null)
            {
                throw new ArgumentNullException(nameof(controllerType), "The section type must be specified.");
            }

            if (controllerType.IsAbstract)
            {
                throw new ArgumentException($"Implementation of the {controllerType.FullName} section controller cannot be abstract.", nameof(controllerType));
            }

            if (!controllerType.IsPublic)
            {
                throw new ArgumentException($"Implementation of the {controllerType.FullName} section controller must be public.", nameof(controllerType));
            }
        }
    }
}
