using System;
using System.ComponentModel;
using System.Linq;
using System.Web.Http.Description;

using CMS.Core;
using CMS.DataEngine;

using Kentico.Content.Web.Mvc.Attributes;
using Kentico.Forms.Web.Mvc;

namespace Kentico.PageBuilder.Web.Mvc.Internal
{
    /// <summary>
    /// Provides endpoints for retrieving and validating the section properties within form.
    /// </summary>
    /// <exclude />
    [EditorBrowsable(EditorBrowsableState.Never)]
    [ApiExplorerSettings(IgnoreApi = true)]
    [CheckPagePermissions(PermissionsEnum.Modify)]
    public sealed class KenticoSectionPropertiesFormController : ComponentPropertiesFormController
    {
        private readonly IComponentDefinitionProvider<SectionDefinition> sectionDefinitionProvider;


        /// <summary>
        /// Component view path
        /// </summary>
        internal override string ViewPath => "~/Views/Shared/Kentico/PageBuilder/_SectionPropertiesForm.cshtml";


        /// <summary>
        /// Initializes a new instance of the <see cref="KenticoSectionPropertiesFormController"/> class.
        /// </summary>
        public KenticoSectionPropertiesFormController()
            : this(Service.Resolve<IEditablePropertiesCollector>(), new ComponentPropertiesSerializer(), new AnnotatedPropertiesSerializer(),
                   Service.Resolve<IEditablePropertiesModelBinder>(), new ComponentDefinitionProvider<SectionDefinition>())
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="KenticoSectionPropertiesFormController" /> class.
        /// </summary>
        /// <param name="editablePropertiesCollector">Collects editable properties from a model.</param>
        /// <param name="propertiesSerializer">Serializer of the section properties.</param>
        /// <param name="annotatedPropertiesSerializer">Serializer of the annotated object properties.</param>
        /// <param name="editablePropertiesModelBinder">Binder responsible for binding form data to a model.</param>
        /// <param name="sectionDefinitionProvider">Provider to retrieve section definitions.</param>
        internal KenticoSectionPropertiesFormController(IEditablePropertiesCollector editablePropertiesCollector, IComponentPropertiesSerializer propertiesSerializer, IAnnotatedPropertiesSerializer annotatedPropertiesSerializer,
                                            IEditablePropertiesModelBinder editablePropertiesModelBinder, IComponentDefinitionProvider<SectionDefinition> sectionDefinitionProvider) :
            base(editablePropertiesCollector, propertiesSerializer, annotatedPropertiesSerializer, editablePropertiesModelBinder)
        {
            this.sectionDefinitionProvider = sectionDefinitionProvider ?? throw new ArgumentNullException(nameof(sectionDefinitionProvider));
        }


        /// <summary>
        /// Gets component definition based on identifier.
        /// </summary>
        /// <param name="typeIdentifier">Type identifier.</param>
        internal override IPropertiesComponentDefinition GetComponentDefinition(string typeIdentifier)
        {
            return sectionDefinitionProvider
                          .GetAll()
                          .First(w => w.Identifier.Equals(typeIdentifier, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}
