using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;

using CMS.Core;
using CMS.DataEngine;
using CMS.EventLog;
using CMS.OnlineForms;
using CMS.SiteProvider;

using Kentico.Builder.Web.Mvc;
using Kentico.Forms.Web.Attributes.Http;

namespace Kentico.Forms.Web.Mvc.Internal
{
    /// <summary>
    /// Provides endpoints for retrieving and storing the Form builder components configuration.
    /// </summary>
    [AuthorizeFormVirtualContext(PredefinedObjectType.BIZFORM, "EditForm")]
    [UseFormBuilderJsonSerializerSettings]
    [FormBuilderApiExceptionFilter]
    public class KenticoFormBuilderConfigurationController : ApiController
    {
        private readonly IFormBuilderConfigurationManager formBuilderConfigurationManager;


        /// <summary>
        /// Initializes a new instance of the <see cref="KenticoFormBuilderConfigurationController"/> class.
        /// </summary>
        public KenticoFormBuilderConfigurationController()
        {
            formBuilderConfigurationManager = Service.Resolve<IFormBuilderConfigurationManager>();
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="KenticoFormBuilderConfigurationController"/> class.
        /// </summary>
        /// <param name="formBuilderConfigurationManager">Manager to be used for configuration load and store.</param>
        internal KenticoFormBuilderConfigurationController(IFormBuilderConfigurationManager formBuilderConfigurationManager)
        {
            this.formBuilderConfigurationManager = formBuilderConfigurationManager ?? throw new ArgumentNullException(nameof(formBuilderConfigurationManager));
        }


        /// <summary>
        /// Gets configuration of a form edited by Form builder.
        /// </summary>
        /// <param name="formId">ID of the form being edited.</param>
        [HttpGet]
        [ResponseType(typeof(FormBuilderConfiguration))]
        public HttpResponseMessage Get(int formId)
        {
            try
            {
                BizFormInfo bizFormInfo = BizFormInfoProvider.GetBizFormInfo(formId);
                if (bizFormInfo == null || (bizFormInfo.FormSiteID != SiteContext.CurrentSiteID))
                {
                    throw new InvalidOperationException($"ID {formId} is not a valid identifier of a form on the current site.");
                }

                var configuration = formBuilderConfigurationManager.Load(bizFormInfo);
                var exceptions = configuration.GetConfigurationExceptions();

                if (exceptions.Any())
                {
                    var errorMessage = $"Form '{bizFormInfo.FormName}' contains invalid form component configurations. Following exceptions occured during their processing: {Environment.NewLine}{Environment.NewLine}"
                                     + String.Join(Environment.NewLine + Environment.NewLine, exceptions.ToList());

                    EventLogProvider.LogEvent(EventType.ERROR, FormBuilderConstants.EVENT_LOG_SOURCE, "GetConfiguration", errorMessage);
                }

                return Request.CreateResponse(HttpStatusCode.OK, configuration);
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException(FormBuilderConstants.EVENT_LOG_SOURCE, "GetConfiguration", ex);
                throw;
            }
        }


        /// <summary>
        /// Sets configuration of a form edited by Form builder.
        /// </summary>
        [HttpPost]
        public HttpResponseMessage Set()
        {
            if (!Int32.TryParse(Request.Headers.GetValues(BuilderConstants.INSTANCE_HEADER_NAME).FirstOrDefault(), out var formId))
            {
                throw new InvalidOperationException("Missing editing instance identifier in headers.");
            }

            BizFormInfo bizFormInfo = BizFormInfoProvider.GetBizFormInfo(formId);
            if (bizFormInfo == null || (bizFormInfo.FormSiteID != SiteContext.CurrentSiteID))
            {
                throw new InvalidOperationException($"ID {formId} is not a valid identifier of a form on the current site.");
            }

            var configurationJson = Request.Content.ReadAsStringAsync().Result;
            
            formBuilderConfigurationManager.Store(bizFormInfo, configurationJson);

            return Request.CreateResponse(HttpStatusCode.NoContent);
        }
    }
}
