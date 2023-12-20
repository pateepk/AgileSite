using System;
using System.ComponentModel;
using System.Linq;
using System.Web.Http.Description;

using CMS.Core;
using CMS.DataEngine;

using Kentico.Content.Web.Mvc.Attributes;
using Kentico.Forms.Web.Mvc;
using Kentico.PageBuilder.Web.Mvc.Internal;

namespace Kentico.PageBuilder.Web.Mvc.PageTemplates.Internal
{
    /// <summary>
    /// Provides endpoints for retrieving and validating the page template properties within form.
    /// </summary>
    /// <exclude />
    [EditorBrowsable(EditorBrowsableState.Never)]
    [ApiExplorerSettings(IgnoreApi = true)]
    [CheckPagePermissions(PermissionsEnum.Modify)]
    public sealed class KenticoPageTemplatePropertiesFormController : ComponentPropertiesFormController
    {
        private readonly IComponentDefinitionProvider<PageTemplateDefinition> pageTemplateDefinitionProvider;


        /// <summary>
        /// Component view path.
        /// </summary>
        internal override string ViewPath => "~/Views/Shared/Kentico/PageBuilder/_PageTemplatePropertiesForm.cshtml";


        /// <summary>
        /// Initializes a new instance of the <see cref="KenticoPageTemplatePropertiesFormController"/> class.
        /// </summary>
        public KenticoPageTemplatePropertiesFormController()
            : this(Service.Resolve<IEditablePropertiesCollector>(), new ComponentPropertiesSerializer(), new AnnotatedPropertiesSerializer(),
                   Service.Resolve<IEditablePropertiesModelBinder>(), new ComponentDefinitionProvider<PageTemplateDefinition>())
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="KenticoPageTemplatePropertiesFormController" /> class.
        /// </summary>
        /// <param name="editablePropertiesCollector">Collects editable properties from a model.</param>
        /// <param name="propertiesSerializer">Serializer of the page template properties.</param>
        /// <param name="annotatedPropertiesSerializer">Serializer of the annotated object properties.</param>
        /// <param name="editablePropertiesModelBinder">Binder responsible for binding form data to a model.</param>
        /// <param name="pageTemplateDefinitionProvider">Provider to retrieve page template definitions.</param>
        internal KenticoPageTemplatePropertiesFormController(IEditablePropertiesCollector editablePropertiesCollector, IComponentPropertiesSerializer propertiesSerializer, IAnnotatedPropertiesSerializer annotatedPropertiesSerializer,
                                            IEditablePropertiesModelBinder editablePropertiesModelBinder, IComponentDefinitionProvider<PageTemplateDefinition> pageTemplateDefinitionProvider) :
            base(editablePropertiesCollector, propertiesSerializer, annotatedPropertiesSerializer, editablePropertiesModelBinder)
        {
            this.pageTemplateDefinitionProvider = pageTemplateDefinitionProvider ?? throw new ArgumentNullException(nameof(pageTemplateDefinitionProvider));
        }


        /// <summary>
        /// Gets component definition based on identifier.
        /// </summary>
        /// <param name="typeIdentifier">Type identifier.</param>
        internal override IPropertiesComponentDefinition GetComponentDefinition(string typeIdentifier)
        {
            return pageTemplateDefinitionProvider
                          .GetAll()
                          .First(w => w.Identifier.Equals(typeIdentifier, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}
