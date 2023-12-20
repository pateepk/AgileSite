using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Routing;

using CMS.Core;
using CMS.FormEngine;
using CMS.Helpers;
using CMS.IO;
using CMS.Membership;
using CMS.OnlineForms;
using CMS.SiteProvider;

using Kentico.Forms.Web.Mvc;
using Kentico.Forms.Web.Mvc.Internal;

[assembly: RegisterFormComponent(FileUploaderComponent.IDENTIFIER, typeof(FileUploaderComponent), "{$kentico.formbuilder.component.fileuploader.name$}", Description = "{$kentico.formbuilder.component.fileuploader.description$}", IconClass = "icon-file", ViewName = FormComponentConstants.AutomaticSystemViewName)]

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Represents a file uploader form component. The component is designed for use in Forms application only.
    /// </summary>
    /// <seealso cref="BizFormComponentContext"/>
    public class FileUploaderComponent : FormComponent<FileUploaderProperties, BizFormUploadFile>
    {
        private readonly IFileUploaderComponentService fileUploaderComponentService;
        private readonly IBizFormFileService bizFormFileService;

        private int formId;
        private string mNewFileIdentifier;


        /// <summary>
        /// Represents the <see cref="FileUploaderComponent"/> identifier.
        /// </summary>
        public const string IDENTIFIER = "Kentico.FileUploader";


        /// <summary>
        /// Gets the site of the form this component is being used for. The site is determined
        /// from the bound context.
        /// </summary>
        /// <seealso cref="BindContext"/>
        /// <seealso cref="BizFormComponentContext"/>
        public SiteInfo FormSite { get; private set; }


        /// <summary>
        /// Gets a value indicating whether to display a view file link after a file has previously been uploaded.
        /// </summary>
        /// <remarks>
        /// The user must be authorized for the resource <see cref="ModuleName.BIZFORM"/>, permission <c>ReadData</c>
        /// to be displayed the link.
        /// </remarks>
        public bool ShowViewFileLink
        {
            get
            {
                return MembershipContext.AuthenticatedUser.IsAuthorizedPerResource(ModuleName.BIZFORM, "ReadData");
            }
        }


        /// <summary>
        /// Gets or sets the identifier of the newly uploaded file.
        /// </summary>
        /// <remarks>File identifier is protected against tampering by the client.</remarks>
        /// <seealso cref="PlainTempFileIdentifier"/>
        [BindableProperty]
        public string TempFileIdentifier
        {
            get
            {
                return fileUploaderComponentService.GetProtectedValue(PlainTempFileIdentifier.ToString() ?? String.Empty, formId, Name, nameof(TempFileIdentifier));
            }
            set
            {
                var stringGuid = fileUploaderComponentService.GetUnprotectedValue(value, formId, Name, nameof(TempFileIdentifier));
                if (!String.IsNullOrEmpty(stringGuid) && Guid.TryParse(stringGuid, out var guid))
                {
                    PlainTempFileIdentifier = guid;
                }
            }
        }


        /// <summary>
        /// Gets or sets the permanent file name of the form file.
        /// </summary>
        /// <seealso cref="PlainSystemFileName"/>
        [BindableProperty]
        public string SystemFileName
        {
            get
            {
                return fileUploaderComponentService.GetProtectedValue(PlainSystemFileName?.ToString() ?? String.Empty, formId, Name, nameof(SystemFileName));
            }
            set
            {
                PlainSystemFileName = fileUploaderComponentService.GetUnprotectedValue(value, formId, Name, nameof(SystemFileName));
            }
        }


        /// <summary>
        /// Gets or sets the original file name of the form file.
        /// </summary>
        /// <seealso cref="PlainOriginalFileName"/>
        [BindableProperty]
        public string OriginalFileName
        {
            get
            {
                return fileUploaderComponentService.GetProtectedValue(PlainOriginalFileName?.ToString() ?? String.Empty, formId, Name, nameof(OriginalFileName));
            }
            set
            {
                PlainOriginalFileName = fileUploaderComponentService.GetUnprotectedValue(value, formId, Name, nameof(OriginalFileName));
            }
        }


        /// <summary>
        /// Gets or sets a value indicating whether to delete or replace the current form file.
        /// </summary>
        [BindableProperty]
        public bool DeleteOrReplaceFile { get; set; }


        /// <summary>
        /// Gets or sets the original name of the newly uploaded file.
        /// </summary>
        [BindableProperty]
        public string TempFileOriginalName { get; set; }


        /// <summary>
        /// Represents the identifier of the newly uploaded file in the unprotected format.
        /// </summary>
        public Guid? PlainTempFileIdentifier { get; set; }


        /// <summary>
        /// Represents the identifier of the permanent file that is already stored in the database and file system.
        /// </summary>
        public string PlainSystemFileName { get; set; }


        /// <summary>
        /// Gets or sets the original file name of the form file.
        /// </summary>
        public string PlainOriginalFileName { get; set; }


        /// <summary>
        /// Gets the maximal size of the file that can be uploaded in kilobytes.
        /// </summary>
        public long MaxAllowedFileSize
        {
            get
            {
                return Service.Resolve<IFormFileUploadLimitService>().MaxAllowedFileSize;
            }
        }


        /// <summary>
        /// Identifier of a new form file used when saving the newly uploaded file.
        /// </summary>
        private string NewFileIdentifier
        {
            get
            {
                return mNewFileIdentifier ?? (mNewFileIdentifier = Guid.NewGuid().ToString());
            }
        }


        /// <summary>
        /// Represents the identifier of the file uploader.
        /// </summary>
        public string FileInputClientId { get; } = "File";


        /// <summary>
        /// Gets the name of the <see cref="FileInputClientId"/> property.
        /// </summary>
        public override string LabelForPropertyName => FileInputClientId;


        /// <summary>
        /// Initializes a new instance of the <see cref="FileUploaderComponent"/> class.
        /// </summary>
        public FileUploaderComponent()
            : this(Service.Resolve<IFileUploaderComponentService>(), Service.Resolve<IBizFormFileService>())
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="FileUploaderComponent"/> class.
        /// </summary>
        internal FileUploaderComponent(IFileUploaderComponentService fileUploaderComponentService, IBizFormFileService bizFormFileService)
        {
            this.fileUploaderComponentService = fileUploaderComponentService;
            this.bizFormFileService = bizFormFileService;
        }


        /// <summary>
        /// Gets the identification string of the form file.
        /// Gets the <see cref="TempFileIdentifier"/>.
        /// </summary>
        public override BizFormUploadFile GetValue()
        {
            if (PlainTempFileIdentifier.HasValue)
            {
                return new BizFormUploadFile()
                {
                    SystemFileName = $"{NewFileIdentifier}{Path.GetExtension(TempFileOriginalName)}",
                    OriginalFileName = TempFileOriginalName
                };
            }
            if (String.IsNullOrEmpty(PlainSystemFileName) || DeleteOrReplaceFile)
            {
                return null;
            }

            return new BizFormUploadFile()
            {
                SystemFileName = PlainSystemFileName,
                OriginalFileName = PlainOriginalFileName
            };
        }


        /// <summary>
        /// Sets the identification string of the form file.
        /// </summary>
        public override void SetValue(BizFormUploadFile value)
        {
            if (String.IsNullOrEmpty(value?.SystemFileName) || String.IsNullOrEmpty(value?.OriginalFileName))
            {
                return;
            }

            PlainSystemFileName = value.SystemFileName;
            PlainOriginalFileName = value.OriginalFileName;
        }


        /// <summary>
        /// Binds contextual information to the form component.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown when <paramref name="context"/> is not an instance of <see cref="BizFormComponentContext"/>.</exception>
        public override void BindContext(FormComponentContext context)
        {
            base.BindContext(context);

            if (context is BizFormComponentContext bizFormContext)
            {
                formId = bizFormContext.FormInfo.FormID;
                FormSite = bizFormContext.FormInfo.Site as SiteInfo;
                bizFormContext.SaveBizFormItem.Before += PromoteTempFile;
            }
            else
            {
                throw new ArgumentException($"The '{typeof(FileUploaderComponent).FullName}' can be used in a biz form only. Context of type '{context?.GetType()?.FullName ?? "(null)"}' was passed.", nameof(context));
            }
        }


        /// <summary>
        /// Validates whether the submitted temp file still exists.
        /// </summary>
        /// <param name="validationContext">The validation context.</param>
        /// <returns>A collection that holds failed-validation information.</returns>
        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var resultValidationErrors = new List<ValidationResult>();
            resultValidationErrors.AddRange(base.Validate(validationContext));

            var tempFilesFolderPath = FormHelper.GetBizFormTempFilesFolderPath();

            if (PlainTempFileIdentifier.HasValue && !bizFormFileService.TempFileExists(PlainTempFileIdentifier.ToString(), tempFilesFolderPath))
            {
                resultValidationErrors.Add(new ValidationResult(ResHelper.GetString("kentico.formbuilder.component.fileuploader.tempfiledeleted")));

                // As the file is not available anymore, we need to reset this component to blank state
                PlainTempFileIdentifier = null;
                TempFileOriginalName = null;
            }

            return resultValidationErrors;
        }


        /// <summary>
        /// Promotes the asynchronously posted temporary file to a permanent form file, if a new file has been posted.
        /// Removes the existing permanent form file based on the <see cref="DeleteOrReplaceFile"/> flag.
        /// </summary>
        private void PromoteTempFile(object sender, SaveBizFormItemEventArgs e)
        {
            var filesFolderPath = FormHelper.GetBizFormFilesFolderPath(FormSite.SiteName);

            if (DeleteOrReplaceFile && !String.IsNullOrEmpty(PlainSystemFileName))
            {
                bizFormFileService.DeleteFile(PlainSystemFileName, filesFolderPath, FormSite.SiteName);

                DeleteOrReplaceFile = false;
                PlainSystemFileName = null;
                PlainOriginalFileName = null;
            }

            if (!PlainTempFileIdentifier.HasValue)
            {
                return;
            }

            var tempFilesFolderPath = FormHelper.GetBizFormTempFilesFolderPath();
            var newFileName = NewFileIdentifier + Path.GetExtension(TempFileOriginalName);

            bizFormFileService.PromoteTempFile(PlainTempFileIdentifier.ToString(), tempFilesFolderPath, newFileName, filesFolderPath, FormSite.SiteName);

            PlainSystemFileName = newFileName;
            PlainOriginalFileName = TempFileOriginalName;
            PlainTempFileIdentifier = null;
            TempFileOriginalName = null;
        }


        /// <summary>
        /// Returns parameters to <see cref="KenticoFormFileUploaderController.PostFile(int, string, string)"/> endpoint for file upload from this form component.
        /// </summary>
        public RouteValueDictionary GetFileUploadParameters()
        {
            return fileUploaderComponentService.CreateUploadEndpointParameters(formId, Name);
        }
    }
}
