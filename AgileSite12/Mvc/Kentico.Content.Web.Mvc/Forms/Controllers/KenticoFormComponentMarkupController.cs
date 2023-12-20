using System;
using System.Web.Mvc;

using CMS.Core;
using CMS.DataEngine;
using CMS.OnlineForms;

using Kentico.Forms.Web.Attributes.Mvc;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Kentico.Forms.Web.Mvc.Internal
{
    /// <summary>
    /// Provides endpoint for retrieving the Form builder component markup.
    /// </summary>
    /// <seealso cref="RouteCollectionExtensions.MapFormBuilderRoutes"/>
    /// <exclude />
    [AuthorizeFormVirtualContext(PredefinedObjectType.BIZFORM, "EditForm")]
    public sealed class KenticoFormComponentMarkupController : Controller
    {
        private readonly IFormComponentDefinitionProvider formComponentDefinitionProvider;
        private readonly IFormComponentActivator formComponentActivator;


        /// <summary>
        /// Initializes a new instance of the <see cref="FormComponentDefinitionProvider"/> class.
        /// </summary>
        public KenticoFormComponentMarkupController()
        {
            formComponentDefinitionProvider = Service.Resolve<IFormComponentDefinitionProvider>();
            formComponentActivator = Service.Resolve<IFormComponentActivator>();
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="FormComponentDefinitionProvider"/> class using the specified form component definition provider.
        /// </summary>
        /// <param name="formComponentDefinitionProvider">Provider for registered form components retrieval.</param>
        /// <param name="formComponentActivator">Activator for form components.</param>
        internal KenticoFormComponentMarkupController(IFormComponentDefinitionProvider formComponentDefinitionProvider, IFormComponentActivator formComponentActivator)
        {
            this.formComponentDefinitionProvider = formComponentDefinitionProvider ?? throw new ArgumentNullException(nameof(formComponentDefinitionProvider));
            this.formComponentActivator = formComponentActivator ?? throw new ArgumentNullException(nameof(formComponentActivator));
        }


#pragma warning disable SCS0016 // Controller method is vulnerable to CSRF

        /// <summary>
        /// Action for rendering a single Form builder editor row (label and form component).
        /// The post request data are a JSON containing 'formIdentifier' and 'postedData'. The 'postedData' represent form component properties.
        /// </summary>
        /// <param name="identifier">Identifier of the form component to be rendered.</param>
        /// <param name="formId">Identifier of biz form the editor row is rendered for.</param>
        [HttpPost]
        [EditorRowExceptionFilter]
        public ActionResult EditorRow(string identifier, int formId)
        {
            var formInfo = BizFormInfoProvider.GetBizFormInfo(formId);
            if (formInfo == null)
            {
                throw new ArgumentException($"Form with ID '{formId}' does not exist.", nameof(formId));
            }

            var formComponentDefinition = formComponentDefinitionProvider.Get(identifier);

            string requestJson = StreamUtils.ReadStreamToEnd(Request.InputStream, true);
            var editorRowRequestPostData = JsonConvert.DeserializeObject<EditorRowRequestPostData>(requestJson, FormBuilderConfigurationSerializer.GetSettings());

            var properties = GetFormComponentProperties(formComponentDefinition, editorRowRequestPostData.PostedData);

            var formComponentModel = formComponentActivator.CreateFormComponent(formComponentDefinition, properties,
                new BizFormComponentContext { FormInfo = formInfo, FormIsSubmittable = false });
            formComponentModel.Name = $"component-{Guid.NewGuid()}";

            var defaultValue = properties.GetDefaultValue();
            if (defaultValue != null)
            {
                formComponentModel.SetObjectValue(properties.GetDefaultValue());
            }

            ViewData.AddFormFieldRenderingConfiguration(SystemRenderingConfigurations.EditorField);

            return PartialView("~/Views/Shared/Kentico/FormBuilder/_FormField.cshtml", formComponentModel);
        }

#pragma warning restore SCS0016 // Controller method is vulnerable to CSRF


        private FormComponentProperties GetFormComponentProperties(FormComponentDefinition definition, JToken jFormComponentProperties)
        {
            return jFormComponentProperties.ToObject(definition.FormComponentPropertiesType, JsonSerializer.Create(FormBuilderConfigurationSerializer.GetSettings())) as FormComponentProperties;
        }


        /// <summary>
        /// Represents post data of a request for <see cref="EditorRow"/>.
        /// </summary>
        public class EditorRowRequestPostData
        {
            /// <summary>
            /// Identifier of biz form.
            /// </summary>
            public int FormIdentifier
            {
                get;
                set;
            }


            /// <summary>
            /// Object representing <see cref="FormComponentProperties"/>.
            /// </summary>
            public JToken PostedData
            {
                get;
                set;
            }
        }
    }
}
