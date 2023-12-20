using System;
using System.Net;
using System.Web;
using System.Web.Mvc;

using CMS.Core;
using CMS.EventLog;
using CMS.FormEngine;
using CMS.Helpers;
using CMS.OnlineForms;
using CMS.SiteProvider;

namespace Kentico.Forms.Web.Mvc.Internal
{
    /// <summary>
    /// Provides endpoint for a form file uploader.
    /// </summary>
    public sealed class KenticoFormFileUploaderController : Controller
    {
        private readonly IFileUploaderComponentService fileUploaderComponentService;
        private readonly IBizFormFileService bizFormFileService;
        private readonly IEventLogService eventLogService;


        /// <summary>
        /// Initializes a new instance of the <see cref="KenticoFormFileUploaderController"/> class.
        /// </summary>
        public KenticoFormFileUploaderController()
            : this(Service.Resolve<IFileUploaderComponentService>(),
                  Service.Resolve<IBizFormFileService>(),
                  Service.Resolve<IEventLogService>())
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="KenticoFormFileUploaderController"/> class.
        /// </summary>
        internal KenticoFormFileUploaderController(
            IFileUploaderComponentService fileUploaderComponentService,
            IBizFormFileService bizFormFileService,
            IEventLogService eventLogService)
        {
            this.fileUploaderComponentService = fileUploaderComponentService;
            this.bizFormFileService = bizFormFileService;
            this.eventLogService = eventLogService;
        }


        /// <summary>
        /// Posts the uploaded file. 
        /// </summary>
        [HttpPost]
#pragma warning disable SCS0016 // Controller method is vulnerable to CSRF
        public ActionResult PostFile(int formId, string fieldName, string hash)
#pragma warning restore SCS0016 // Controller method is vulnerable to CSRF
        {
            if (Request.Files.Count != 1)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            if (!fileUploaderComponentService.IsHashValid(formId, fieldName, hash))
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }

            var formInfo = BizFormInfoProvider.GetBizFormInfo(formId);
            if (formInfo == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);
            }

            var file = Request.Files[0];
            if (!fileUploaderComponentService.IsExtensionValid(file.FileName, formInfo.FormSiteID, out string errorMessage))
            {
                return CreateErrorResult(errorMessage);
            }

            var tempFilesFolderPath = FormHelper.GetBizFormTempFilesFolderPath();
            if (!fileUploaderComponentService.CanUploadTempFile(tempFilesFolderPath, formInfo.FormSiteID, out errorMessage))
            {
                LogCannotUploadTempFileError(tempFilesFolderPath);
                return CreateErrorResult(errorMessage);
            }

            var tempFileIdentifier = Guid.NewGuid().ToString();

            SaveUploadedTempFile(file, tempFileIdentifier, tempFilesFolderPath);

            return CreateSuccessResult(tempFileIdentifier, formId, fieldName);
        }


        /// <summary>
        /// Deletes the uploaded temp file.
        /// </summary>
        /// <returns></returns>
        [HttpDelete]
#pragma warning disable SCS0016 // Controller method is vulnerable to CSRF
        public ActionResult DeleteTempFile(int formId, string fieldName, string tempFileIdentifier, string hash)
#pragma warning restore SCS0016 // Controller method is vulnerable to CSRF
        {
            if (formId < 1 || String.IsNullOrEmpty(tempFileIdentifier))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            
            if (!fileUploaderComponentService.IsHashValid(formId, fieldName, hash))
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }

            var formInfo = BizFormInfoProvider.GetBizFormInfo(formId);
            if (formInfo == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);
            }

            var tempFilesFolderPath = FormHelper.GetBizFormTempFilesFolderPath();

            var unprotectedFileId = fileUploaderComponentService.GetUnprotectedValue(tempFileIdentifier, formId, fieldName, nameof(FileUploaderComponent.TempFileIdentifier));

            bizFormFileService.DeleteTempFile(unprotectedFileId, tempFilesFolderPath);

            return new EmptyResult();
        }


        private void SaveUploadedTempFile(HttpPostedFileBase file, string tempFileIdentifier, string tempFilesFolderPath)
        {
            bizFormFileService.SaveUploadedFileAsTempFile(file, tempFileIdentifier, tempFilesFolderPath, 0, 0, 0);
        }


        private JsonResult CreateErrorResult(string errorMessage)
        {
            return new JsonResult { Data = new { errorMessage } };
        }


        private JsonResult CreateSuccessResult(string fileIdentifier, int formId, string fieldName)
        {
            return new JsonResult { Data = new { fileIdentifier = fileUploaderComponentService.GetProtectedValue(fileIdentifier, formId, fieldName, nameof(FileUploaderComponent.TempFileIdentifier)) } };
        }


        /// <summary>
        /// Logs error into EventLog when file could not be uploaded due to quota limitation
        /// </summary>
        private void LogCannotUploadTempFileError(string tempFilesFolderPath)
        {
            // Note: I will have to use exception here because currently IEventLogService
            // does not contain LogEvent method with LoggingPolicy parameter.
            // In V13 we should expose such possibility and once that is done, the code below
            // should be changed to directly log custom event without wrapping it into exception.

            var errorMessage = ResHelper.GetStringFormat("formfileuploader.temporaryunavailable.eventlog", tempFilesFolderPath);

            var ex = new BizFormException(errorMessage);

            var loggingPolicy = new LoggingPolicy(TimeSpan.FromMinutes(15));

            eventLogService.LogException(FileUploaderComponent.IDENTIFIER, "QUOTALIMIT", ex, loggingPolicy);
        }
    }
}