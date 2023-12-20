using System;
using System.ComponentModel;
using System.Linq;
using System.Web.Http.Description;
using System.Web.Mvc;

using CMS.Core;
using CMS.DataEngine;
using CMS.EventLog;

using Kentico.Content.Web.Mvc;
using Kentico.Content.Web.Mvc.Attributes;
using Kentico.Forms.Web.Mvc;

namespace Kentico.PageBuilder.Web.Mvc.Personalization.Internal
{
    /// <summary>
    /// Provides endpoints for retrieving and validating the condition type parameters within form.
    /// </summary>
    /// <exclude />
    [EditorBrowsable(EditorBrowsableState.Never)]
    [ApiExplorerSettings(IgnoreApi = true)]
    [CheckPagePermissions(PermissionsEnum.Modify)]
    public sealed class KenticoConditionTypeParametersFormController : Controller
    {
        private const string FORM_COMPONENT_FIELD_PREFIX = "kentico-form";
        internal const string VIEW_PATH = "~/Views/Shared/Kentico/PageBuilder/_ConditionTypeParametersForm.cshtml";

        private readonly IEditablePropertiesCollector editablePropertiesCollector;
        private readonly IConditionTypeParametersSerializer parametersSerializer;
        private IPageBuilderPostDataRetriever<PageBuilderPostData> mPostDataRetriever;
        private readonly IEditablePropertiesModelBinder editablePropertiesModelBinder;
        private readonly IComponentDefinitionProvider<ConditionTypeDefinition> conditionTypeDefinitionProvider;


        internal IPageBuilderPostDataRetriever<PageBuilderPostData> PostDataRetriever
        {
            private get
            {
                if (mPostDataRetriever == null)
                {
                    var securityChecker = new PageSecurityChecker(new VirtualContextPageRetriever());
                    mPostDataRetriever = new PageBuilderPostDataRetriever<PageBuilderPostData>(HttpContext, securityChecker);
                }

                return mPostDataRetriever;
            }
            set
            {
                mPostDataRetriever = value;
            }
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="KenticoConditionTypeParametersFormController"/> class.
        /// </summary>
        public KenticoConditionTypeParametersFormController()
            : this(Service.Resolve<IEditablePropertiesCollector>(), new ConditionTypeParametersSerializer(),
                   Service.Resolve<IEditablePropertiesModelBinder>(), new ComponentDefinitionProvider<ConditionTypeDefinition>())
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="KenticoConditionTypeParametersFormController" /> class.
        /// </summary>
        /// <param name="editablePropertiesCollector">Collects editable properties from a model.</param>
        /// <param name="parametersSerializer">Serializer of the condition type parameters.</param>
        /// <param name="editablePropertiesModelBinder">Binder responsible for binding form data to a model.</param>
        /// <param name="conditionTypeDefinitionProvider">Provider to retrieve condition type definitions.</param>
        internal KenticoConditionTypeParametersFormController(IEditablePropertiesCollector editablePropertiesCollector, IConditionTypeParametersSerializer parametersSerializer,
                                            IEditablePropertiesModelBinder editablePropertiesModelBinder, IComponentDefinitionProvider<ConditionTypeDefinition> conditionTypeDefinitionProvider)
        {
            this.editablePropertiesCollector = editablePropertiesCollector ?? throw new ArgumentNullException(nameof(editablePropertiesCollector));
            this.parametersSerializer = parametersSerializer ?? throw new ArgumentNullException(nameof(parametersSerializer));
            this.editablePropertiesModelBinder = editablePropertiesModelBinder ?? throw new ArgumentNullException(nameof(editablePropertiesModelBinder));
            this.conditionTypeDefinitionProvider = conditionTypeDefinitionProvider ?? throw new ArgumentNullException(nameof(conditionTypeDefinitionProvider));
        }


#pragma warning disable SCS0016 // Controller method is vulnerable to CSRF

        /// <summary>
        /// Returns a markup of the condition type parameters form.
        /// </summary>
        /// <param name="typeIdentifier">Condition type identifier.</param>
        [HttpPost]
        public ActionResult Index(string typeIdentifier)
        {
            try
            {
                var parametersJson = PostDataRetriever.Retrieve().Data;
                var conditionTypeDefinition = GetConditionTypeDefinition(typeIdentifier);
                if (conditionTypeDefinition == null)
                {
                    throw new InvalidOperationException($"Missing '{typeIdentifier}' condition type definition.");
                }

                var parameters = parametersSerializer.Deserialize(parametersJson, conditionTypeDefinition.Type);
                var viewModel = GetConditionTypeViewModel(parameters);

                ViewData.TemplateInfo.HtmlFieldPrefix = FORM_COMPONENT_FIELD_PREFIX;

                return PartialView(VIEW_PATH, viewModel);
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
        /// Validates the condition type parameters form.
        /// </summary>
        /// <param name="typeIdentifier">The widget type identifier.</param>
        [HttpPost]
        public ActionResult Validate(string typeIdentifier)
        {
            try
            {
                FormCollection form = new FormCollection(ControllerContext.HttpContext.Request.Unvalidated.Form);
                var conditionType = GetEmptyConditionType(typeIdentifier);
                editablePropertiesModelBinder.BindModel(ControllerContext, new PageBuilderFormComponentContext(), conditionType, form, FORM_COMPONENT_FIELD_PREFIX);

                if (!ModelState.IsValid)
                {
                    ViewData.TemplateInfo.HtmlFieldPrefix = FORM_COMPONENT_FIELD_PREFIX;
                    var viewModel = GetConditionTypeViewModel(conditionType);

                    return PartialView(VIEW_PATH, viewModel);
                }

                return new JsonCamelCaseResult { Data = conditionType };
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException(FormBuilderConstants.EVENT_LOG_SOURCE, "Validate", ex);
                throw;
            }
        }

#pragma warning restore SCS0016 // Controller method is vulnerable to CSRF


        private GeneratedFormViewModel GetConditionTypeViewModel(IConditionType conditionType)
        {
            return new GeneratedFormViewModel
            {
                FormComponents = editablePropertiesCollector.GetFormComponents(conditionType, new PageBuilderFormComponentContext()),
            };
        }


        private IConditionType GetEmptyConditionType(string typeIdentifier)
        {
            var conditionTypeDefinition = GetConditionTypeDefinition(typeIdentifier);

            return Activator.CreateInstance(conditionTypeDefinition.Type) as IConditionType;
        }


        private ConditionTypeDefinition GetConditionTypeDefinition(string typeIdentifier)
        {
            return conditionTypeDefinitionProvider
                          .GetAll()
                          .First(w => w.Identifier.Equals(typeIdentifier, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}
