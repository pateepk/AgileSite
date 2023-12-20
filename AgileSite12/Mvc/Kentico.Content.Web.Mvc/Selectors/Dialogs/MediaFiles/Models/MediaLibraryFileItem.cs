using System;

using Newtonsoft.Json;

namespace Kentico.Components.Web.Mvc.Dialogs
{
    /// <summary>
    /// Represents an item for media files selector files grid.
    /// </summary>
    internal class MediaLibraryFileItem
    {
        [JsonProperty("name")]
        public string Name { get; set; }


        [JsonProperty("extension")]
        public string Extension { get; set; }


        [JsonProperty("fileGuid")]
        public Guid FileGUID { get; set; }


        [JsonProperty("url")]
        public string Url { get; set; }


        [JsonProperty("thumbnailUrls")]
        public MediaLibraryFileThumbnails ThumbnailUrls { get; set; }


        [JsonProperty("mimeType")]
        public string MimeType { get; set; }


        [JsonProperty("size")]
        public long Size { get; set; }


        [JsonProperty("title")]
        public string Title { get; set; }


        [JsonProperty("description")]
        public string Description { get; set; }


        [JsonProperty("folderPath")]
        public string FolderPath { get; set; }


        [JsonProperty("libraryName")]
        public string LibraryName { get; set; }


        [JsonProperty("siteName")]
        public string SiteName { get; set; }


        [JsonProperty("isValid")]
        public bool IsValid { get; set; } = true;
    }


    /// <summary>
    /// Represents predefined thumbnails for a media file.
    /// </summary>
    internal class MediaLibraryFileThumbnails
    {
        [JsonProperty("small")]
        public string Small { get; set; }


        [JsonProperty("medium")]
        public string Medium { get; set; }


        [JsonProperty("large")]
        public string Large { get; set; }
    }
}