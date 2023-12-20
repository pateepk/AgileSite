using System;

using Kentico.Content.Web.Mvc;

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Registers a section to Form builder.
    /// </summary>
    public sealed class RegisterFormSectionAttribute : RegisterComponentAttribute
    {
        /// <summary>
        /// Description of the registered section.
        /// </summary>
        public string Description { get; set; }


        /// <summary>
        /// Font-icon CSS class of the registered section.
        /// </summary>
        public string IconClass { get; set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="RegisterFormSectionAttribute"/> class.
        /// </summary>
        /// <param name="identifier">Unique identifier of the section.</param>
        /// <param name="sectionType">Type of the section controller to register.</param>
        /// <param name="name">Name of the section.</param>
        /// <remarks>
        /// Make sure to provide unique identifier for the section definition from the start. 
        /// This identifier is used within the page configuration and any further change can lead to incorrect configuration load.
        /// Consider specifying identifier in format 'CompanyName.ModuleName.SectionName', e.g. 'Kentico.Content.SingleColumn'.
        /// </remarks>
        public RegisterFormSectionAttribute(string identifier, Type sectionType, string name)
            : base(identifier, sectionType, name)
        {
        }


        /// <summary>
        /// Registers the section definition during application pre-initialization.
        /// </summary>
        public override void PreInit()
        {
            ComponentDefinitionStore<SectionDefinition>.Instance.Add(
                new SectionDefinition(Identifier, MarkedType, Name)
                {
                    Description = Description,
                    IconClass = IconClass
                }
            );
        }
    }
}
