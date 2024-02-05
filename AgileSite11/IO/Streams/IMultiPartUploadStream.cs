using System.Collections.Generic;
using System.IO;

namespace CMS.IO
{
    /// <summary>
    /// Interface extending <see cref="Stream"/>'s functionality to enable upload of large files in smaller parts to external storage.
    /// </summary>
    public interface IMultiPartUploadStream
    {
        /// <summary>
        /// Returns minimal size in bytes of a part that can be sent to external storage.
        /// </summary>
        long MinimalPartSize { get; }


        /// <summary>
        /// Returns maximal size in bytes of a part that can be sent to external storage.
        /// </summary>
        long MaximalPartSize { get; }


        /// <summary>
        /// Inits multipart upload with external storage.
        /// </summary>
        /// <returns>
        /// Unique identifier for multipart upload process to external storage.
        /// Returned unique identifier may be needed for each subsequent multipart upload operation.
        /// </returns>
        string InitMultiPartUpload();


        /// <summary>
        /// Uploads stream content to external storage.
        /// </summary>
        /// <param name="uploadSessionId">
        /// Unique identifier for multipart upload process to external storage.
        /// Is obtained by <see cref="InitMultiPartUpload()"/>.
        /// </param>
        /// <param name="nextPartNumber">Number that defines position of the data obtained by the stream in the whole multipart upload process.</param>
        /// <returns>Unique identifiers of currently uploaded parts to external storage.</returns>
        IEnumerable<string> UploadStreamContentAsMultiPart(string uploadSessionId, int nextPartNumber);


        /// <summary>
        /// Completes multipart upload process.
        /// </summary>
        /// <remarks>Stream still needs to be disposed.</remarks>
        /// <param name="uploadSessionId">
        /// Unique identifier for multipart upload process to external storage.
        /// Is obtained by <see cref="InitMultiPartUpload()"/>.
        /// </param>
        /// <param name="partIdentifiers">All unique identifiers of uploaded parts acquired by <see cref="UploadStreamContentAsMultiPart(string, int)"/> method.</param>
        /// <returns>ETag of the uploaded file.</returns>
        string CompleteMultiPartUploadProcess(string uploadSessionId, IEnumerable<string> partIdentifiers);


        /// <summary>
        /// Aborts multipart upload to external storage and should remove all resources already uploaded to external storage if necessary.
        /// </summary>
        /// <param name="uploadSessionId">
        /// Unique identifier for multipart upload process to external storage.
        /// Is obtained by <see cref="InitMultiPartUpload()"/>.
        /// </param>
        void AbortMultiPartUpload(string uploadSessionId);
    }
}
