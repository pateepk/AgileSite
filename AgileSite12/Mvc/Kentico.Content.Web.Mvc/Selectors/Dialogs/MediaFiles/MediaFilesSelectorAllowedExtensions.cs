using System;
using System.Collections.Generic;
using System.Linq;

using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.MediaLibrary;

namespace Kentico.Components.Web.Mvc.Dialogs
{
    /// <summary>
    /// Handles the allowed extensions for media files selector.
    /// </summary>
    internal class MediaFilesSelectorAllowedExtensions
    {
        private readonly HashSet<string> allowedExtensions = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
        private readonly ISiteService siteService;


        /// <summary>
        /// Creates an instance of <see cref="MediaFilesSelectorAllowedExtensions"/> class.
        /// </summary>
        /// <param name="allowedExtensions">Allowed extensions separated by semicolon.</param>
        public MediaFilesSelectorAllowedExtensions(string allowedExtensions) : 
            this(allowedExtensions, Service.Resolve<ISiteService>())
        {
            ParseAllowedExtensions(allowedExtensions);
        }


        /// <summary>
        /// Creates an instance of <see cref="MediaFilesSelectorAllowedExtensions"/> class.
        /// </summary>
        /// <param name="allowedExtensions">Allowed extensions separated by semicolon.</param>
        /// <param name="siteService">Site service.</param>
        internal MediaFilesSelectorAllowedExtensions(string allowedExtensions, ISiteService siteService)
        {
            this.siteService = siteService;
            ParseAllowedExtensions(allowedExtensions);
        }


        private void ParseAllowedExtensions(string allowedExtensions)
        {
            var extensions = string.IsNullOrEmpty(allowedExtensions) ? MediaLibraryHelper.GetAllowedExtensions(siteService.CurrentSite.SiteName) : allowedExtensions;
            var unifiedExtensions = extensions.Split(';').Select(GetUnifiedExtension);

            foreach (var extension in unifiedExtensions)
            {
                this.allowedExtensions.Add(extension);
            }
        }


        /// <summary>
        /// Gets allowed extensions.
        /// </summary>
        public IEnumerable<string> Get()
        {
            return allowedExtensions;
        }


        /// <summary>
        /// Validates extension against the allowed extensions.
        /// </summary>
        /// <param name="extension">File extension.</param>
        public bool Validate(string extension)
        {
            return allowedExtensions.Contains(GetUnifiedExtension(extension));
        }


        /// <summary>
        /// Gets where condition to retrieve only media files with allowed extensions.
        /// </summary>
        public IWhereCondition GetWhereCondition()
        {
            var where = new WhereCondition();

            if (allowedExtensions.Count > 0)
            {
                where = where.WhereIn("FileExtension", allowedExtensions);
            }

            return where;
        }


        private string GetUnifiedExtension(string extension)
        {
            return string.IsNullOrEmpty(extension) ? extension : $".{extension.TrimStart('.')}";
        }
    }
}
