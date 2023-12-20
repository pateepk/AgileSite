using System;
using System.Collections.Generic;
using CMS.DataEngine;

using Kentico.Forms.Web.Mvc;

namespace Kentico.Components.Web.Mvc.FormComponents
{
    /// <summary>
    /// Properties for media files selector.
    /// </summary>
    public class MediaFilesSelectorProperties : FormComponentProperties<IList<MediaFilesSelectorItem>>
    {
        /// <summary>
        /// Default value.
        /// </summary>
        public override IList<MediaFilesSelectorItem> DefaultValue { get; set; }


        /// <summary>
        /// Media library name.
        /// </summary>
        public string LibraryName { get; set; }


        /// <summary>
        /// Limit of maximum number of files allowed to be selected.
        /// </summary>
        /// <remarks>
        /// Following values can be used to limit the maximum number of files:
        /// 0 - unlimited
        /// 1 - single file selection
        /// n - n-files selection
        /// </remarks>
        public int MaxFilesLimit { get; set; } = 1;


        /// <summary>
        /// Semicolon separated list of allowed file extensions.
        /// </summary>
        /// <remarks>
        /// If provided, the list of extensions needs to be a subset of allowed extensions specified in 'Media file allowed extensions' site settings key value.
        /// If the property value not provided, 'Media file allowed extensions' site settings key value is used.
        /// </remarks>
        public string AllowedExtensions { get; set; }


        /// <summary>
        /// Creates an instace of the <see cref="MediaFilesSelectorProperties"/> class.
        /// </summary>
        public MediaFilesSelectorProperties()
            : base(FieldDataType.Unknown)
        {
        }
    }
}