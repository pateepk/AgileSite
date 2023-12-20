using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using CMS.Base;
using CMS.Core;
using CMS.Helpers;

using Kentico.Components.Web.Mvc.Dialogs;
using Kentico.Components.Web.Mvc.FormComponents;
using Kentico.Forms.Web.Mvc;
using Kentico.PageBuilder.Web.Mvc;

using Newtonsoft.Json;

[assembly: RegisterFormComponent(MediaFilesSelector.IDENTIFIER, typeof(MediaFilesSelector), "{$kentico.components.mediafileselector.name$}", ViewName = "~/Views/Shared/Kentico/Selectors/FormComponents/_MediaFilesSelector.cshtml", IsAvailableInFormBuilderEditor = false)]

namespace Kentico.Components.Web.Mvc.FormComponents
{
    /// <summary>
    /// Represents media files selector.
    /// </summary>
    public class MediaFilesSelector : FormComponent<MediaFilesSelectorProperties, IList<MediaFilesSelectorItem>>
    {
        private readonly ISiteService siteService = Service.Resolve<ISiteService>();
        private string mValue;

        /// <summary>
        /// Represents the <see cref="MediaFilesSelector"/> identifier.
        /// </summary>
        public const string IDENTIFIER = "Kentico.MediaFilesSelector";

        private IEnumerable<MediaLibraryFileItem> mSelectedFiles;


        /// <summary>
        /// Binds contextual information to the form component.
        /// </summary>
        /// <param name="context">Form component context.</param>
        public override void BindContext(FormComponentContext context)
        {
            base.BindContext(context);

            if (!(context is PageBuilderFormComponentContext))
            {
                throw new NotSupportedException("Media file selector is only available in page builder.");
            }
        }


        private IEnumerable<MediaLibraryFileItem> SelectedFiles
        {
            get
            {
                if (mSelectedFiles == null)
                {
                    var items = !string.IsNullOrEmpty(Value) ? JsonConvert.DeserializeObject<IList<MediaFilesSelectorItem>>(Value) : Enumerable.Empty<MediaFilesSelectorItem>();
                    mSelectedFiles = new MediaLibraryDataLoader().LoadFiles(items.Select(item => item.FileGuid), new UrlHelper(HttpContext.Current.Request.RequestContext)).ToArray();
                }

                return mSelectedFiles;
            }
        }


        private MediaFilesSelectorAllowedExtensions AllowedExtensionsProvider => new MediaFilesSelectorAllowedExtensions(Properties.AllowedExtensions);


        /// <summary>
        /// Gets serialized model of selected files.
        /// </summary>
        /// <returns></returns>
        public string FilesData => JsonConvert.SerializeObject(GetValidatedFiles());


        /// <summary>
        /// Gets allowed extensions for the selector.
        /// </summary>
        /// <remarks>
        /// If the <see cref="MediaFilesSelectorProperties.AllowedExtensions"/> property value is not provided, 'Media file allowed extensions' site settings key value is used.
        /// </remarks>
        public string AllowedExtensions => string.Join(";", AllowedExtensionsProvider.Get());


        /// <summary>
        /// Represents the value in the resulting HTML.
        /// </summary>
        [BindableProperty]
        public string Value
        {
            get
            {
                return mValue;
            }
            set
            {
                mSelectedFiles = null;
                mValue = value;
            }
        }


        /// <summary>
        /// Returns collection of selected media files.
        /// </summary>
        public override IList<MediaFilesSelectorItem> GetValue()
        {
            if (string.IsNullOrEmpty(Value))
            {
                return null;
            }

            return JsonConvert.DeserializeObject<IList<MediaFilesSelectorItem>>(Value);
        }


        /// <summary>
        /// Sets selected media files to the selector. 
        /// </summary>
        public override void SetValue(IList<MediaFilesSelectorItem> value)
        {
            Value = value != null ? JsonConvert.SerializeObject(value) : null;
        }


        /// <summary>
        /// Validates whether selected media file is valid.
        /// </summary>        
        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var result = base.Validate(validationContext).ToList();

            var files = GetValidatedFiles();
            if (files.Any(file => !file.IsValid))
            {
                result.Add(new ValidationResult(ResHelper.GetString("kentico.components.mediafileselector.invalidvalue")));
            }

            if (Properties.MaxFilesLimit > 0 && files.Count > Properties.MaxFilesLimit)
            {
                result.Add(new ValidationResult(ResHelper.GetString("kentico.components.mediafileselector.maxFilesLimitExceeded")));
            }

            return result;
        }


        private IList<MediaLibraryFileItem> GetValidatedFiles()
        {
            return SelectedFiles.Select(file =>
            {
                file.IsValid = IsValid(file);
                return file;
            }).ToList();
        }


        private bool IsValid(MediaLibraryFileItem file)
        {
            if (!file.IsValid)
            {
                return false;
            }

            if (!IsAllowedFileType(file))
            {
                return false;
            }

            if (!IsFromCorrectMediaLibrary(file))
            {
                return false;
            }

            return true;
        }


        private bool IsAllowedFileType(MediaLibraryFileItem file)
        {
            return AllowedExtensionsProvider.Validate(file.Extension);
        }


        private bool IsFromCorrectMediaLibrary(MediaLibraryFileItem file)
        {
            if (string.IsNullOrEmpty(Properties.LibraryName))
            {
                return string.Equals(file.SiteName, siteService.CurrentSite.SiteName, StringComparison.InvariantCultureIgnoreCase);
            }

            return string.Equals(Properties.LibraryName, file.LibraryName, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
