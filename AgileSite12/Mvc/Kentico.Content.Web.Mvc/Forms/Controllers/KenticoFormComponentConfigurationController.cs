using System;
using System.ComponentModel;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using System.Web.Http.Description;

using CMS.Core;
using CMS.DataEngine;
using CMS.FormEngine;
using CMS.Helpers;
using CMS.OnlineForms;

using Kentico.Forms.Web.Attributes.Http;

using Newtonsoft.Json;

namespace Kentico.Forms.Web.Mvc.Internal
{
    /// <summary>
    /// Provides endpoint for retrieving default form component configuration.
    /// </summary>
    /// <seealso cref="RouteCollectionExtensions.MapFormBuilderRoutes"/>
    /// <exclude />
    [EditorBrowsable(EditorBrowsableState.Never)]
    [ApiExplorerSettings(IgnoreApi = true)]
    [AuthorizeFormVirtualContext(PredefinedObjectType.BIZFORM, "EditForm")]
    [FormBuilderApiExceptionFilter]
    public sealed class KenticoFormComponentConfigurationController : ApiController
    {
        private readonly IFormComponentDefinitionProvider formComponentDefinitionProvider;
        private readonly IFormComponentActivator formComponentActivator;


        /// <summary>
        /// Initializes a new instance of the <see cref="KenticoFormComponentConfigurationController"/> class.
        /// </summary>
        public KenticoFormComponentConfigurationController()
        {
            formComponentDefinitionProvider = Service.Resolve<IFormComponentDefinitionProvider>();
            formComponentActivator = Service.Resolve<IFormComponentActivator>();
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="KenticoFormComponentConfigurationController"/> class.
        /// </summary>
        /// <param name="formComponentDefinitionProvider">Provider for registered form components retrieval.</param>
        /// <param name="formComponentActivator">Activator for form components.</param>
        internal KenticoFormComponentConfigurationController(IFormComponentDefinitionProvider formComponentDefinitionProvider, IFormComponentActivator formComponentActivator)
        {
            this.formComponentDefinitionProvider = formComponentDefinitionProvider ?? throw new ArgumentNullException(nameof(formComponentDefinitionProvider));
            this.formComponentActivator = formComponentActivator ?? throw new ArgumentNullException(nameof(formComponentActivator));
        }


        /// <summary>
        /// Gets default properties for a form component specified by <paramref name="identifier"/>.
        /// </summary>
        /// <param name="identifier">Widget type identifier.</param>
        /// <param name="formId">Identifier of a BizFormInfo where form component is used.</param>
        [HttpGet]
        public HttpResponseMessage GetDefaultProperties(string identifier, int formId)
        {
            var formComponentDefinition = formComponentDefinitionProvider.Get(identifier);
            var formComponentProperties = formComponentActivator.CreateDefaultProperties(formComponentDefinition);
            formComponentProperties.Name = EnsureUniqueName(formId, IdentifierUtils.GetIdentifier(formComponentDefinition.Identifier));

            var formComponentPropertiesJson = JsonConvert.SerializeObject(formComponentProperties, typeof(FormComponentProperties), FormBuilderConfigurationSerializer.GetSettings());

            var response = Request.CreateResponse(HttpStatusCode.OK, formComponentProperties);
            response.Content = new StringContent(formComponentPropertiesJson, Encoding.UTF8, "application/json");

            return response;
        }


        /// <summary>
        /// Return <paramref name="name"/> appended with number suffixes if necessary, so it is unique among field names of <see cref="BizFormInfo"/> given by <paramref name="formId"/> and doesn't exceed <see cref="FormComponentDefinition.IDENTIFIER_MAX_LENGTH"/> length.
        /// </summary>
        private string EnsureUniqueName(int formId, string name)
        {
            var formInfo = BizFormInfoProvider.GetBizFormInfo(formId).Form;

            var exceededLength = 0;
            while (exceededLength < name.Length)
            {
                var candidateName = FormInfoHelper.GetUniqueFieldName(formInfo, name);

                exceededLength = candidateName.Length - FormComponentDefinition.IDENTIFIER_MAX_LENGTH;
                if (exceededLength <= 0)
                {
                    return candidateName;
                }

                name = TextHelper.LimitLength(name, name.Length - exceededLength, String.Empty);
            }

            return name;
        }
    }
}
