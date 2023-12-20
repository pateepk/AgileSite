using System;

using CMS;

using Kentico.Content.Web.Mvc;

namespace Kentico.PageBuilder.Web.Mvc.PageTemplates
{
    /// <summary>
    /// Registers a page template definition to be used within the system.
    /// </summary>
    public sealed class RegisterPageTemplateAttribute : RegisterComponentAttribute, IPreInitAttribute
    {
        private readonly Type propertiesType;
        private readonly string customViewName;


        /// <summary>
        /// Creates an instance of the <see cref="RegisterPageTemplateAttribute"/> class.
        /// </summary>
        /// <param name="identifier">Unique identifier of the page template definition.</param>
        /// <param name="controllerType">Type of the page template controller to register.</param>
        /// <param name="name">Name of the registered page template.</param>
        /// <remarks>
        /// Make sure to provide unique identifier for the page template definition from the start. 
        /// This identifier is used within the page configuration and any further change can lead to incorrect configuration load.
        /// Consider specifying identifier in format 'CompanyName.ModuleName.PageTemplateName', e.g. 'Kentico.Content.LandingPageTemplate'.
        /// </remarks>
        public RegisterPageTemplateAttribute(string identifier, Type controllerType, string name)
            :base(identifier, controllerType, name)
        {
        }


        /// <summary>
        /// Creates an instance of the <see cref="RegisterPageTemplateAttribute"/> class.
        /// </summary>
        /// <param name="identifier">Unique identifier of the page template definition.</param>
        /// <param name="name">Name of the registered page template.</param>
        /// <param name="propertiesType">Type of the registered page template properties model. The type needs to implement the <see cref="IPageTemplateProperties"/> interface.</param>
        /// <param name="customViewName">Custom view name for the registered page template. If not specified, the default system view name in format '~/Shared/PageTemplates/_{identifier}' is used.</param>
        /// <remarks>
        /// Make sure to provide unique identifier for the page template definition from the start. 
        /// This identifier is used within the page configuration and any further change can lead to incorrect configuration load.
        /// Consider specifying identifier in format 'CompanyName.ModuleName.PageTemplateName', e.g. 'Kentico.Content.LandingPageTemplate'.
        /// 
        /// Use this attribute when simple page template is being developed and no custom controller is needed. 
        /// </remarks>
        public RegisterPageTemplateAttribute(string identifier, string name, Type propertiesType = null, string customViewName = null)
            : base(identifier, null, name)
        {
            this.propertiesType = propertiesType;
            this.customViewName = customViewName;
        }


        /// <summary>
        /// Registers the page template definition during application pre-initialization.
        /// </summary>
        public override void PreInit()
        {
            ComponentDefinitionStore<PageTemplateDefinition>.Instance.Add(new PageTemplateDefinition(Identifier, MarkedType, Name, Description, IconClass, propertiesType, customViewName));
        }
    }
}
