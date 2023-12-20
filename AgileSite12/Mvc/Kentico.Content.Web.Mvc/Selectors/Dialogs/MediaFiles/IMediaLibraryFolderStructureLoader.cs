namespace Kentico.Components.Web.Mvc.Dialogs
{
    /// <summary>
    /// Loads folder structure of a media library.
    /// </summary>
    internal interface IMediaLibraryFolderStructureLoader
    {
        /// <summary>
        /// Loads folder structure of a media library.
        /// </summary>
        /// <param name="libraryName">Media library name.</param>
        MediaLibraryFolderNode Load(string libraryName);
    }
}