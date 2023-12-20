using System;

using Kentico.Content.Web.Mvc;

namespace Kentico.PageBuilder.Web.Mvc
{
    /// <summary>
    /// Registers a section definition to be used within Page builder feature.
    /// </summary>
    public sealed class RegisterSectionAttribute : RegisterComponentAttribute
    {
        private readonly Type propertiesType;
        private readonly string customViewName;


        /// <summary>
        /// Creates an instance of the <see cref="RegisterSectionAttribute"/> class.
        /// </summary>
        /// <param name="identifier">Unique identifier of the section definition.</param>
        /// <param name="controllerType">Type of the section controller to register.</param>
        /// <param name="name">Name of the registered section.</param>
        /// <remarks>
        /// Make sure to provide unique identifier for the section definition from the start. 
        /// This identifier is used within the page configuration and any further change can lead to incorrect configuration load.
        /// Consider specifying identifier in format 'CompanyName.ModuleName.SectionName', e.g. 'Kentico.Content.SingleColumn'.
        /// </remarks>
        public RegisterSectionAttribute(string identifier, Type controllerType, string name)
            : base(identifier, controllerType, name)
        {
        }


        /// <summary>
        /// Creates an instance of the <see cref="RegisterSectionAttribute"/> class.
        /// </summary>
        /// <param name="identifier">Unique identifier of the section definition.</param>
        /// <param name="name">Name of the registered section.</param>
        /// <param name="propertiesType">Type of the registered section properties model. The type needs to implement the <see cref="ISectionProperties"/> interface.</param>
        /// <param name="customViewName">Custom view name for the registered section. If not specified, the default system view name in format '~/Shared/Sections/_{identifier}' is used.</param>
        /// <remarks>
        /// Make sure to provide unique identifier for the section definition from the start. 
        /// This identifier is used within the page configuration and any further change can lead to incorrect configuration load.
        /// Consider specifying identifier in format 'CompanyName.ModuleName.SectionName', e.g. 'Kentico.Content.SingleColumn'.
        /// 
        /// Use this attribute when simple section is being developed and no custom controller is needed. 
        /// </remarks>
        public RegisterSectionAttribute(string identifier, string name, Type propertiesType = null, string customViewName = null)
            : base(identifier, null, name)
        {
            this.propertiesType = propertiesType;
            this.customViewName = customViewName;
        }


        /// <summary>
        /// Registers the section definition during application pre-initialization.
        /// </summary>
        public override void PreInit()
        {
            ComponentDefinitionStore<SectionDefinition>.Instance.Add(new SectionDefinition(Identifier, MarkedType, Name, Description, IconClass, propertiesType, customViewName));
        }
    }
}
