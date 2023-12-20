using System;
using System.Collections.Generic;
using System.Linq;

using CMS.Helpers;

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Builds form components metadata.
    /// </summary>
    internal sealed class FormComponentsMetadataBuilder : IFormComponentsMetadataBuilder
    {
        private readonly IFormComponentDefinitionProvider formComponentProvider;
        private readonly IFormComponentMarkupUrlRetriever markupUrlRetriever;
        private readonly IFormComponentDefaultPropertiesUrlRetriever defaultPropertiesUrlRetriever;
        private readonly ISectionDefinitionProvider sectionProvider;
        private readonly ISectionMarkupUrlRetriever sectionMarkupUrlRetriever;


        /// <summary>
        /// Initializes a new instance of the <see cref="FormComponentsMetadataBuilder"/> class.
        /// </summary>
        /// <param name="formComponentProvider">Provider for registered form components retrieval.</param>
        /// <param name="markupUrlRetriever">Retriever for form component markup URL.</param>
        /// <param name="defaultPropertiesUrlRetriever">Retriever for form component default properties URL.</param>
        /// <param name="sectionProvider">Provider for registered sections retrieval.</param>
        /// <param name="sectionMarkupUrlRetriever">Retriever for default section markup URL.</param>
        public FormComponentsMetadataBuilder(IFormComponentDefinitionProvider formComponentProvider, IFormComponentMarkupUrlRetriever markupUrlRetriever,
            IFormComponentDefaultPropertiesUrlRetriever defaultPropertiesUrlRetriever,
            ISectionDefinitionProvider sectionProvider, ISectionMarkupUrlRetriever sectionMarkupUrlRetriever)
        {
            this.formComponentProvider = formComponentProvider ?? throw new ArgumentNullException(nameof(formComponentProvider));
            this.markupUrlRetriever = markupUrlRetriever ?? throw new ArgumentNullException(nameof(markupUrlRetriever));
            this.defaultPropertiesUrlRetriever = defaultPropertiesUrlRetriever ?? throw new ArgumentNullException(nameof(defaultPropertiesUrlRetriever));
            this.sectionProvider = sectionProvider ?? throw new ArgumentNullException(nameof(sectionProvider));
            this.sectionMarkupUrlRetriever = sectionMarkupUrlRetriever ?? throw new ArgumentNullException(nameof(sectionMarkupUrlRetriever));
        }


        /// <summary>
        /// Gets metadata of all registered form components.
        /// </summary>
        /// <param name="formId">Id of a BizFormInfo where form components will be used.</param>
        public FormComponentsMetadata GetAll(int formId)
        {
            var registeredFormComponents = formComponentProvider.GetAll();
            var registeredSections = sectionProvider.GetAll();

            return CreateFormComponentsMetadata(registeredFormComponents, registeredSections, formId);
        }


        private FormComponentsMetadata CreateFormComponentsMetadata(IEnumerable<FormComponentDefinition> formComponents, IEnumerable<SectionDefinition> sections, int formId)
        {
            return new FormComponentsMetadata
            {
                FormComponents = formComponents.Select(c => CreateFormComponentMetadata(c, formId)).OrderBy(m => m.Name).ToList(),
                Sections = sections.Select(CreateSectionMetadata).OrderBy(m => m.Name).ToList()
            };
        }


        private FormComponentMetadata CreateFormComponentMetadata(FormComponentDefinition formComponent, int formId)
        {
            return new FormComponentMetadata
            {
                Identifier = formComponent.Identifier,
                MarkupUrl = markupUrlRetriever.GetUrl(formComponent, formId),
                DefaultPropertiesUrl = defaultPropertiesUrlRetriever.GetUrl(formComponent, formId),
                Name = ResHelper.LocalizeString(formComponent.Name),
                Description = ResHelper.LocalizeString(formComponent.Description),
                IconClass = formComponent.IconClass,
                ValueType = formComponent.GetNonNullableValueType().FullName.ToLowerInvariant()
            };
        }


        private SectionMetadata CreateSectionMetadata(SectionDefinition section)
        {
            return new SectionMetadata
            {
                TypeIdentifier = section.Identifier,
                MarkupUrl = sectionMarkupUrlRetriever.GetUrl(section),
                Name = ResHelper.LocalizeString(section.Name),
                Description = ResHelper.LocalizeString(section.Description),
                IconClass = section.IconClass
            };
        }
    }
}
