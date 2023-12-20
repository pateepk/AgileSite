using System;
using System.Web;

using CMS;
using CMS.Base;
using CMS.Core;
using CMS.Helpers;
using CMS.IO;
using CMS.OnlineForms;

[assembly: RegisterImplementation(typeof(IBizFormFileService), typeof(BizFormFileService), Priority = RegistrationPriority.SystemDefault, Lifestyle = Lifestyle.Singleton)]

namespace CMS.OnlineForms
{
    /// <summary>
    /// Contains uploaded file manipulation methods for forms.
    /// </summary>
    public class BizFormFileService : IBizFormFileService
    {
        internal const string TEMP_FILE_EXTENSION = ".tmp";
        

        /// <summary>
        /// Gets temporary file name for an identifier.
        /// </summary>
        /// <param name="tempFileIdentifier">Identifier of the temporary file to derive file name from.</param>
        /// <returns>Returns the temporary file name based on <paramref name="tempFileIdentifier"/>.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="tempFileIdentifier"/> is null or an empty string.</exception>
        public string GetTempFileName(string tempFileIdentifier)
        {
            if (String.IsNullOrEmpty(tempFileIdentifier))
            {
                throw new ArgumentException("Temporary file identifier must be specified.", nameof(tempFileIdentifier));
            }

            return tempFileIdentifier + TEMP_FILE_EXTENSION;
        }


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
        public void SaveUploadedFile(HttpPostedFileBase postedFile, string fileName, string filesFolderPath, int width, int height, int maxSideSize, string siteName)
        {
            if (postedFile == null)
            {
                throw new ArgumentNullException(nameof(postedFile));
            }
            if (String.IsNullOrEmpty(fileName))
            {
                throw new ArgumentException("File name must be specified.", nameof(fileName));
            }
            if (String.IsNullOrEmpty(filesFolderPath))
            {
                throw new ArgumentException("Files folder path must be specified.", nameof(filesFolderPath));
            }

            string originalFileExtension = Path.GetExtension(postedFile.FileName);

            SaveUploadedFileCore(postedFile, fileName, filesFolderPath, originalFileExtension, width, height, maxSideSize, siteName, false);
        }


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
        public void SaveUploadedFileAsTempFile(HttpPostedFileBase postedFile, string tempFileIdentifier, string tempFilesFolderPath, int width, int height, int maxSideSize)
        {
            if (postedFile == null)
            {
                throw new ArgumentNullException(nameof(postedFile));
            }
            if (String.IsNullOrEmpty(tempFileIdentifier))
            {
                throw new ArgumentException("Temporary file identifier must be specified.", nameof(tempFileIdentifier));
            }
            if (String.IsNullOrEmpty(tempFilesFolderPath))
            {
                throw new ArgumentException("Temporary files folder path must be specified.", nameof(tempFilesFolderPath));
            }

            var tempFileName = GetTempFileName(tempFileIdentifier);

            var originalFileExtension = Path.GetExtension(postedFile.FileName);

            SaveUploadedFileCore(postedFile, tempFileName, tempFilesFolderPath, originalFileExtension, width, height, maxSideSize, null, true);
        }


        /// <summary>
        /// Saves uploaded file to the file system. The file can be optionally resized if it is an image file.
        /// The type of the file is determined from the <paramref name="originalFileExtension"/>.
        /// </summary>
        /// <param name="postedFile">Posted file from a client.</param>
        /// <param name="fileName">File name to be used for <paramref name="postedFile"/>.</param>
        /// <param name="filesFolderPath">Folder path where uploaded form files are being saved.</param>
        /// <param name="originalFileExtension">Original file extension to determine whether file is an image file.</param>
        /// <param name="width">Required image width to resize the <paramref name="postedFile"/> to if the file is an image file, or 0.</param>
        /// <param name="height">Required image height to resize the <paramref name="postedFile"/> to if the file is an image file, or 0.</param>
        /// <param name="maxSideSize">Required image max side size to resize the <paramref name="postedFile"/> to if the file is an image file, or 0.</param>
        /// <param name="siteName">Site name the corresponding form belongs to.</param>
        /// <param name="isTempFile">A value indicating whether file represents a temporary file.</param>
        private void SaveUploadedFileCore(HttpPostedFileBase postedFile, string fileName, string filesFolderPath, string originalFileExtension, int width, int height, int maxSideSize, string siteName, bool isTempFile)
        {
            // Get file size and path
            var filePath = Path.Combine(filesFolderPath, fileName);

            // Ensure disk path
            DirectoryHelper.EnsureDiskPath(filePath, SystemContext.WebApplicationPhysicalPath);

            using (var stream = postedFile.InputStream)
            {
                byte[] imageBinary = null;

                // Images -> save as binary
                if (ImageHelper.IsImage(originalFileExtension))
                {
                    imageBinary = new byte[postedFile.ContentLength];
                    stream.Read(imageBinary, 0, postedFile.ContentLength);

                    // Resize image if required
                    imageBinary = ImageHelper.GetResizedImageData(imageBinary, originalFileExtension, width, height, maxSideSize);

                    StorageHelper.SaveFileToDisk(filePath, imageBinary);
                }
                // Other files -> save as stream
                else
                {
                    StorageHelper.SaveFileToDisk(filePath, stream, false);
                }

                BinaryData fileBinaryData = imageBinary?.Length > 0 ? new BinaryData(imageBinary) : stream;
                LogUpdateBizFormFileTask(fileName, filePath, isTempFile, fileBinaryData, siteName);
            }
        }


        private void LogUpdateBizFormFileTask(string fileName, string filePath, bool isTempFile, BinaryData fileBinaryData, string siteName)
        {
            if (WebFarmHelper.WebFarmEnabled)
            {
                WebFarmHelper.CreateIOTask(new UpdateBizFormFileWebFarmTask
                {
                    SiteName = siteName,
                    FileName = fileName,
                    IsTempFile = isTempFile,
                    TaskBinaryData = fileBinaryData,
                    TaskFilePath = filePath,
                });
            }
        }


        /// <summary>
        /// Saves temporary file previously saved via the <see cref="SaveUploadedFileAsTempFile"/> method as a parmenent form file.
        /// </summary>
        /// <param name="tempFileIdentifier">Identifier of the temporary file to derive file name from.</param>
        /// <param name="tempFilesFolderPath">Folder path where temporary files are being saved.</param>
        /// <param name="fileName">File name to be used for the permanent file.</param>
        /// <param name="filesFolderPath">Folder path where uploaded form files are being saved.</param>
        /// <param name="siteName">Site name the corresponding form belongs to.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="tempFileIdentifier"/>, <paramref name="tempFilesFolderPath"/>, <paramref name="fileName"/> or <paramref name="filesFolderPath"/> is null or an empty string.</exception>
        public void PromoteTempFile(string tempFileIdentifier, string tempFilesFolderPath, string fileName, string filesFolderPath, string siteName)
        {
            if (String.IsNullOrEmpty(tempFileIdentifier))
            {
                throw new ArgumentException("Temporary file identifier must be specified.", nameof(tempFileIdentifier));
            }
            if (String.IsNullOrEmpty(tempFilesFolderPath))
            {
                throw new ArgumentException("Temporary files folder path must be specified.", nameof(tempFilesFolderPath));
            }
            if (String.IsNullOrEmpty(fileName))
            {
                throw new ArgumentException("File name must be specified.", nameof(fileName));
            }
            if (String.IsNullOrEmpty(filesFolderPath))
            {
                throw new ArgumentException("Files folder path must be specified.", nameof(filesFolderPath));
            }

            var tempFileName = GetTempFileName(tempFileIdentifier);

            var tempFilePath = Path.Combine(tempFilesFolderPath, tempFileName);
            var filePath = Path.Combine(filesFolderPath, fileName);

            DirectoryHelper.EnsureDiskPath(filePath, SystemContext.WebApplicationPhysicalPath);

            File.Move(tempFilePath, filePath);

            LogPromoteBizFormTempFileTask(tempFileName, fileName, filePath, siteName);
        }


        private void LogPromoteBizFormTempFileTask(string tempFileName, string fileName, string filePath, string siteName)
        {
            if (WebFarmHelper.WebFarmEnabled)
            {
                WebFarmHelper.CreateIOTask(new PromoteBizFormTempFileWebFarmTask
                {
                    SiteName = siteName,
                    FileName = fileName,
                    TempFileName = tempFileName,
                    TaskFilePath = filePath
                });
            }
        }


        /// <summary>
        /// Deletes file previously saved via the <see cref="SaveUploadedFile"/> or <see cref="PromoteTempFile"/> method.
        /// </summary>
        /// <param name="fileName">File name to be used for the file.</param>
        /// <param name="filesFolderPath">Folder path where uploaded form files are being saved.</param>
        /// <param name="siteName">Site name the corresponding form belongs to.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="fileName"/> or <paramref name="filesFolderPath"/> is null or an empty string.</exception>
        public void DeleteFile(string fileName, string filesFolderPath, string siteName)
        {
            if (String.IsNullOrEmpty(fileName))
            {
                throw new ArgumentException("File name must be specified.", nameof(fileName));
            }
            if (String.IsNullOrEmpty(filesFolderPath))
            {
                throw new ArgumentException("Files folder path must be specified.", nameof(filesFolderPath));
            }

            var filePath = Path.Combine(filesFolderPath, fileName);

            File.Delete(filePath);

            LogDeleteBizFormFileTask(fileName, filesFolderPath, false, siteName);
        }


        /// <summary>
        /// Deletes temporary file previously saved via the <see cref="SaveUploadedFileAsTempFile"/> method which was not promoted to a permanent file.
        /// </summary>
        /// <param name="tempFileIdentifier">Identifier of the temporary file to derive file name from.</param>
        /// <param name="tempFilesFolderPath">Folder path where temporary files are being saved.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="tempFileIdentifier"/> or <paramref name="tempFilesFolderPath"/> is null or an empty string.</exception>
        public void DeleteTempFile(string tempFileIdentifier, string tempFilesFolderPath)
        {
            if (String.IsNullOrEmpty(tempFileIdentifier))
            {
                throw new ArgumentException("Temporary file identifier must be specified.", nameof(tempFileIdentifier));
            }
            if (String.IsNullOrEmpty(tempFilesFolderPath))
            {
                throw new ArgumentException("Temporary files folder path must be specified.", nameof(tempFilesFolderPath));
            }

            var tempFileName = GetTempFileName(tempFileIdentifier);

            var tempFilePath = Path.Combine(tempFilesFolderPath, tempFileName);

            File.Delete(tempFilePath);

            LogDeleteBizFormFileTask(tempFileName, tempFilePath, true, null);
        }


        private void LogDeleteBizFormFileTask(string fileName, string filePath, bool isTempFile, string siteName)
        {
            if (WebFarmHelper.WebFarmEnabled)
            {
                WebFarmHelper.CreateIOTask(new DeleteBizFormFileWebFarmTask
                {
                    SiteName = siteName,
                    FileName = fileName,
                    IsTempFile = isTempFile,
                    TaskFilePath = filePath
                });
            }
        }


        /// <summary>
        /// Checks whether file previously saved via the <see cref="SaveUploadedFileAsTempFile(HttpPostedFileBase, string, string, int, int, int)"/> still exists.
        /// </summary>
        /// <param name="tempFileIdentifier">Identifier of the temporary file to derive file name from.</param>
        /// <param name="tempFilesFolderPath">Folder path where temporary files are being saved.</param>
        /// <returns>True if the file exists, otherwise false.</returns>
        public bool TempFileExists(string tempFileIdentifier, string tempFilesFolderPath)
        {
            var tempFileName = GetTempFileName(tempFileIdentifier);
            var tempFilePath = Path.Combine(tempFilesFolderPath, tempFileName);

            return File.Exists(tempFilePath);
        }
    }
}
