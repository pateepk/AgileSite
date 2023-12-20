using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;

using CMS.Core;
using CMS.DataEngine;

using Kentico.Forms.Web.Attributes.Http;

namespace Kentico.Forms.Web.Mvc.Internal
{
    /// <summary>
    /// Provides endpoints for retrieving the validation rule metadata.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [ApiExplorerSettings(IgnoreApi = true)]
    [UseFormBuilderJsonSerializerSettings]
    [AuthorizeFormVirtualContext(PredefinedObjectType.BIZFORM, "EditForm")]
    [FormBuilderApiExceptionFilter]
    public class KenticoValidationRuleMetadataController : ApiController
    {
        private readonly IValidationRulesMetadataBuilder validationRulesMetadataBuilder;


        /// <summary>
        /// Initializes a new instance of the <see cref="KenticoValidationRuleMetadataController"/> class.
        /// </summary>
        public KenticoValidationRuleMetadataController()
            : this(Service.Resolve<IValidationRulesMetadataBuilder>())
        { }



        /// <summary>
        /// Initializes a new instance of the <see cref="IValidationRuleDefinitionProvider"/> class.
        /// </summary>
        /// <param name="validationRulesMetadataBuilder">Builder for validation rules metadata.</param>
        internal KenticoValidationRuleMetadataController(IValidationRulesMetadataBuilder validationRulesMetadataBuilder)
        {
            this.validationRulesMetadataBuilder = validationRulesMetadataBuilder;
        }


        /// <summary>
        /// Gets metadata for all registered validation rules grouped by their type and alphabetically ordered.
        /// </summary>
        [HttpGet]
        [ResponseType(typeof(Dictionary<string, List<ValidationRuleMetadata>>))]
        public HttpResponseMessage GetAll()
        {
            var validationRules = validationRulesMetadataBuilder.GetAll();
            var groupedValidationRules = validationRules.GroupBy(r => r.ValidatedDataType);

            var orderedValidationRules = groupedValidationRules
                .ToDictionary(d => d.Key, d => d.AsEnumerable().OrderBy(r => r.Name).ToList());

            return Request.CreateResponse(HttpStatusCode.OK, orderedValidationRules);
        }
    }
}
