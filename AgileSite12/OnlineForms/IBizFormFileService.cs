using System;
using System.Web;

namespace CMS.OnlineForms
{
    /// <summary>
    /// Defines manipulation methods for a file uploaded via forms.
    /// </summary>
    public interface IBizFormFileService
    {
        /// <summary>
        /// Gets temporary file name for an identifier.
        /// </summary>
        /// <param name="tempFileIdentifier">Identifier of the temporary file to derive file name from.</param>
        /// <returns>Returns the temporary file name based on <paramref name="tempFileIdentifier"/>.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="tempFileIdentifier"/> is null or an empty string.</exception>
        string GetTempFileName(string tempFileIdentifier);


        /// <summary>
        /// Saves uploded form file to the file system. The file can be optionally resized if it is an image file.
        /// The type of the file is determined from the <paramref name="postedFile"/>'s <see cref="HttpPostedFileBase.FileName"/>.
        /// </summary>
        /// <param name="postedFile">Posted file from a client.</param>
        /// <param name="fileName">File name to be used for <paramref name="postedFile"/>.</param>
        /// <param name="filesFolderPath">Folder path where uploaded form files are being saved.</param>
        /// <param name="width">Required image width to resize the <paramref name="postedFile"/> to if the file is an image file, or 0.</param>
        /// <param name="height">Required image height to resize the <paramref name="postedFile"/> to if the file is an image file, or 0.</param>
        /// <param name="maxSideSize">Required image max side size to resize the <paramref name="postedFile"/> to if the file is an image file, or 0.</param>
        /// <param name="siteName">Site name the corresponding form belongs to.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="postedFile"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="fileName"/> or <paramref name="filesFolderPath"/> is null or an empty string.</exception>
        void SaveUploadedFile(HttpPostedFileBase postedFile, string fileName, string filesFolderPath, int width, int height, int maxSideSize, string siteName);
        

        /// <summary>
        /// Saves uploaded form file as a temporary file to the file system. The file can be optionally resized if it is an image file.
        /// The type of the file is determined from the <paramref name="postedFile"/>'s <see cref="HttpPostedFileBase.FileName"/>.
        /// </summary>
        /// <param name="postedFile">Posted file from a client.</param>
        /// <param name="tempFileIdentifier">Identifier of the temporary file to derive file name from.</param>
        /// <param name="tempFilesFolderPath">Folder path where temporary form files are being saved.</param>
        /// <param name="width">Required image width to resize the <paramref name="postedFile"/> to if the file is an image file, or 0.</param>
        /// <param name="height">Required image height to resize the <paramref name="postedFile"/> to if the file is an image file, or 0.</param>
        /// <param name="maxSideSize">Required image max side size to resize the <paramref name="postedFile"/> to if the file is an image file, or 0.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="postedFile"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="tempFileIdentifier"/> or <paramref name="tempFilesFolderPath"/> is null or an empty string.</exception>
        void SaveUploadedFileAsTempFile(HttpPostedFileBase postedFile, string tempFileIdentifier, string tempFilesFolderPath, int width, int height, int maxSideSize);
        

        /// <summary>
        /// Saves temporary file previously saved via the <see cref="SaveUploadedFileAsTempFile"/> method as a parmenent form file.
        /// </summary>
        /// <param name="tempFileIdentifier">Identifier of the temporary file to derive file name from.</param>
        /// <param name="tempFilesFolderPath">Folder path where temporary files are being saved.</param>
        /// <param name="fileName">File name to be used for temporary file.</param>
        /// <param name="filesFolderPath">Folder path where uploaded form files are being saved.</param>
        /// <param name="siteName">Site name the corresponding form belongs to.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="tempFileIdentifier"/>, <paramref name="tempFilesFolderPath"/>, <paramref name="fileName"/> or <paramref name="filesFolderPath"/> is null or an empty string.</exception>
        void PromoteTempFile(string tempFileIdentifier, string tempFilesFolderPath, string fileName, string filesFolderPath, string siteName);


        /// <summary>
        /// Deletes temporary file previously saved via the <see cref="SaveUploadedFileAsTempFile"/> method which was not promoted to a permanent file.
        /// </summary>
        /// <param name="tempFileIdentifier">Identifier of the temporary file to derive file name from.</param>
        /// <param name="tempFilesFolderPath">Folder path where temporary files are being saved.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="tempFileIdentifier"/> or <paramref name="tempFilesFolderPath"/> is null or an empty string.</exception>
        void DeleteTempFile(string tempFileIdentifier, string tempFilesFolderPath);


        /// <summary>
        /// Deletes file previously saved via the <see cref="SaveUploadedFile"/> or <see cref="PromoteTempFile"/> method.
        /// </summary>
        /// <param name="fileName">File name to be used for the file.</param>
        /// <param name="filesFolderPath">Folder path where uploaded form files are being saved.</param>
        /// <param name="siteName">Site name the corresponding form belongs to.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="fileName"/> or <paramref name="filesFolderPath"/> is null or an empty string.</exception>
        void DeleteFile(string fileName, string filesFolderPath, string siteName);


        /// <summary>
        /// Checks whether file previously saved via the <see cref="SaveUploadedFileAsTempFile(HttpPostedFileBase, string, string, int, int, int)"/> still exists.
        /// </summary>
        /// <param name="tempFileIdentifier">Identifier of the temporary file to derive file name from.</param>
        /// <param name="tempFilesFolderPath">Folder path where temporary files are being saved.</param>
        /// <returns>True if the file exists, otherwise false.</returns>
        bool TempFileExists(string tempFileIdentifier, string tempFilesFolderPath);
    }
}