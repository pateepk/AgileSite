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
    /// Provides endpoints for retrieving and validating the widget properties within form.
    /// </summary>
    /// <exclude />
    [EditorBrowsable(EditorBrowsableState.Never)]
    [ApiExplorerSettings(IgnoreApi = true)]
    [CheckPagePermissions(PermissionsEnum.Modify)]
    public sealed class KenticoWidgetPropertiesFormController : ComponentPropertiesFormController
    {
        private readonly IComponentDefinitionProvider<WidgetDefinition> widgetDefinitionProvider;


        /// <summary>
        /// Component view path
        /// </summary>
        internal override string ViewPath => "~/Views/Shared/Kentico/PageBuilder/_WidgetPropertiesForm.cshtml";


        /// <summary>
        /// Initializes a new instance of the <see cref="KenticoWidgetPropertiesFormController"/> class.
        /// </summary>
        public KenticoWidgetPropertiesFormController()
            : this(Service.Resolve<IEditablePropertiesCollector>(), new ComponentPropertiesSerializer(), new AnnotatedPropertiesSerializer(),
                   Service.Resolve<IEditablePropertiesModelBinder>(), new ComponentDefinitionProvider<WidgetDefinition>())
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="KenticoWidgetPropertiesFormController" /> class.
        /// </summary>
        /// <param name="editablePropertiesCollector">Collects editable properties from a model.</param>
        /// <param name="propertiesSerializer">Serializer of the widget properties.</param>
        /// <param name="annotatedPropertiesSerializer">Serializer of the annotated object properties.</param>
        /// <param name="editablePropertiesModelBinder">Binder responsible for binding form data to a model.</param>
        /// <param name="widgetDefinitionProvider">Provider to retrieve widget definitions.</param>
        internal KenticoWidgetPropertiesFormController(IEditablePropertiesCollector editablePropertiesCollector, IComponentPropertiesSerializer propertiesSerializer, IAnnotatedPropertiesSerializer annotatedPropertiesSerializer,
                                            IEditablePropertiesModelBinder editablePropertiesModelBinder, IComponentDefinitionProvider<WidgetDefinition> widgetDefinitionProvider) :
            base(editablePropertiesCollector, propertiesSerializer, annotatedPropertiesSerializer, editablePropertiesModelBinder)
        {
            this.widgetDefinitionProvider = widgetDefinitionProvider ?? throw new ArgumentNullException(nameof(widgetDefinitionProvider));
        }


        /// <summary>
        /// Gets component definition based on identifier.
        /// </summary>
        /// <param name="typeIdentifier">Type identifier.</param>
        internal override IPropertiesComponentDefinition GetComponentDefinition(string typeIdentifier)
        {
            return widgetDefinitionProvider
                          .GetAll()
                          .First(w => w.Identifier.Equals(typeIdentifier, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}
