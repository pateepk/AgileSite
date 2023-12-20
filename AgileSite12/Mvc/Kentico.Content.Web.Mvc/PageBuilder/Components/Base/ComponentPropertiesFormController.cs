using System;
using System.Web.Mvc;

using CMS.EventLog;

using Kentico.Content.Web.Mvc;
using Kentico.Forms.Web.Mvc;

namespace Kentico.PageBuilder.Web.Mvc.Internal
{
    /// <summary>
    /// Provides base class for retrieving and validating the component properties within form.
    /// </summary>
    public abstract class ComponentPropertiesFormController : Controller
    {
        private const string FORM_COMPONENT_FIELD_PREFIX = "kentico-form";

        /// <summary>
        /// Component view path
        /// </summary>
        internal abstract string ViewPath { get; }

        private readonly IEditablePropertiesCollector editablePropertiesCollector;
        private readonly IComponentPropertiesSerializer propertiesSerializer;
        private readonly IAnnotatedPropertiesSerializer annotatedPropertiesSerializer;
        private IPageBuilderPostDataRetriever<ComponentPropertiesPostData> mPostDataRetriever;
        private readonly IEditablePropertiesModelBinder editablePropertiesModelBinder;


        internal IPageBuilderPostDataRetriever<ComponentPropertiesPostData> PostDataRetriever
        {
            private get
            {
                if (mPostDataRetriever == null)
                {
                    var securityChecker = new PageSecurityChecker(new VirtualContextPageRetriever());
                    mPostDataRetriever = new PageBuilderPostDataRetriever<ComponentPropertiesPostData>(HttpContext, securityChecker);
                }

                return mPostDataRetriever;
            }
            set
            {
                mPostDataRetriever = value;
            }
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="ComponentPropertiesFormController" /> class.
        /// </summary>
        /// <param name="editablePropertiesCollector">Collects editable properties from a model.</param>
        /// <param name="propertiesSerializer">Serializer of the component properties.</param>
        /// <param name="annotatedPropertiesSerializer">Serializer of the annotated object properties.</param>
        /// <param name="editablePropertiesModelBinder">Binder responsible for binding form data to a model.</param>
        internal ComponentPropertiesFormController(IEditablePropertiesCollector editablePropertiesCollector, IComponentPropertiesSerializer propertiesSerializer, IAnnotatedPropertiesSerializer annotatedPropertiesSerializer,
                                            IEditablePropertiesModelBinder editablePropertiesModelBinder)
        {
            this.editablePropertiesCollector = editablePropertiesCollector ?? throw new ArgumentNullException(nameof(editablePropertiesCollector));
            this.propertiesSerializer = propertiesSerializer ?? throw new ArgumentNullException(nameof(propertiesSerializer));
            this.annotatedPropertiesSerializer = annotatedPropertiesSerializer ?? throw new ArgumentNullException(nameof(annotatedPropertiesSerializer));
            this.editablePropertiesModelBinder = editablePropertiesModelBinder ?? throw new ArgumentNullException(nameof(editablePropertiesModelBinder));
        }


#pragma warning disable SCS0016 // Controller method is vulnerable to CSRF

        /// <summary>
        /// Returns a markup of the component properties form.
        /// </summary>
        /// <param name="typeIdentifier">Component type identifier.</param>
        [HttpPost]
        public ActionResult Index(string typeIdentifier)
        {
            try
            {
                var propertiesJson = PostDataRetriever.Retrieve().Properties;
                var componentDefinition = GetComponentDefinition(typeIdentifier);
                if (componentDefinition == null)
                {
                    throw new InvalidOperationException($"Missing '{typeIdentifier}' component definition.");
                }

                var properties = propertiesSerializer.Deserialize(propertiesJson, componentDefinition.PropertiesType);
                var viewModel = GetPropertiesViewModel(properties);

                ViewData.TemplateInfo.HtmlFieldPrefix = FORM_COMPONENT_FIELD_PREFIX;

                return PartialView(ViewPath, viewModel);
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException(PageBuilderConstants.EVENT_LOG_SOURCE, "Index", ex);
                throw;
            }
        }

#pragma warning restore SCS0016 // Controller method is vulnerable to CSRF


#pragma warning disable SCS0016 // Controller method is vulnerable to CSRF

        /// <summary>
        /// Validates the component properties form.
        /// </summary>
        /// <param name="typeIdentifier">The component type identifier.</param>
        [HttpPost]
        public ActionResult Validate(string typeIdentifier)
        {
            try
            {
                FormCollection form = new FormCollection(ControllerContext.HttpContext.Request.Unvalidated.Form);
                var properties = GetEmptyProperties(typeIdentifier);
                editablePropertiesModelBinder.BindModel(ControllerContext, new PageBuilderFormComponentContext(), properties, form, FORM_COMPONENT_FIELD_PREFIX);

                if (!ModelState.IsValid)
                {
                    ViewData.TemplateInfo.HtmlFieldPrefix = FORM_COMPONENT_FIELD_PREFIX;
                    var viewModel = GetPropertiesViewModel(properties);

                    return PartialView(ViewPath, viewModel);
                }

                var propertiesJson = annotatedPropertiesSerializer.Serialize(properties);

                return Content(propertiesJson, "application/json");
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException(FormBuilderConstants.EVENT_LOG_SOURCE, "Validate", ex);
                throw;
            }
        }

#pragma warning restore SCS0016 // Controller method is vulnerable to CSRF


        private GeneratedFormViewModel GetPropertiesViewModel(IComponentProperties properties)
        {
            return new GeneratedFormViewModel
            {
                FormComponents = editablePropertiesCollector.GetFormComponents(properties, new PageBuilderFormComponentContext()),
            };
        }


        private IComponentProperties GetEmptyProperties(string typeIdentifier)
        {
            var componentDefinition = GetComponentDefinition(typeIdentifier);

            return new ComponentDefaultPropertiesProvider().Get<IComponentProperties>(componentDefinition?.PropertiesType);
        }


        /// <summary>
        /// Gets component definition based on identifier.
        /// </summary>
        /// <param name="typeIdentifier">Type identifier.</param>
        internal abstract IPropertiesComponentDefinition GetComponentDefinition(string typeIdentifier);
    }
}
