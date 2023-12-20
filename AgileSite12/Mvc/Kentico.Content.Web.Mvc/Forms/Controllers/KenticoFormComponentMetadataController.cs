using System;
using System.ComponentModel;
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
    /// Provides endpoints for retrieving the Form builder component metadata.
    /// </summary>
    /// <exclude />
    [EditorBrowsable(EditorBrowsableState.Never)]
    [ApiExplorerSettings(IgnoreApi = true)]
    [UseFormBuilderJsonSerializerSettings]
    [AuthorizeFormVirtualContext(PredefinedObjectType.BIZFORM, "EditForm")]
    [FormBuilderApiExceptionFilter]
    public sealed class KenticoFormComponentMetadataController : ApiController
    {
        private readonly IFormComponentsMetadataBuilder metadataBuilder;


        /// <summary>
        /// Initializes a new instance of the <see cref="KenticoFormComponentMetadataController"/> class.
        /// </summary>
        public KenticoFormComponentMetadataController()
        {
            metadataBuilder = Service.Resolve<IFormComponentsMetadataBuilder>();
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="KenticoFormComponentMetadataController"/> class.
        /// </summary>
        /// <param name="metadataBuilder">Builder for form components metadata.</param>
        internal KenticoFormComponentMetadataController(IFormComponentsMetadataBuilder metadataBuilder)
        {
            this.metadataBuilder = metadataBuilder;
        }


        /// <summary>
        /// Gets metadata for all registered form components.
        /// </summary>
        /// <param name="formId">Identifier of a BizFormInfo where form components are used.</param>
        [HttpGet]
        [ResponseType(typeof(FormComponentsMetadata))]
        public HttpResponseMessage GetAll(int formId)
        {
            return Request.CreateResponse(HttpStatusCode.OK, metadataBuilder.GetAll(formId));

        }
    }
}
