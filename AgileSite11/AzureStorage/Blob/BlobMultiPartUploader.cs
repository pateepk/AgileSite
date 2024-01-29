using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CMS.AzureStorage
{
    /// <summary>
    /// Class for uploading large files to Azure blob storage.
    /// </summary>
    internal class BlobMultiPartUploader
    {
        #region "Variables"

        private readonly BlobInfo mBlobInfo;
        private readonly int mMaximalPartSize;

        #endregion


        #region "Constructor"

        /// <summary>
        /// Creates object for uploading large files in smaller parts to Azure blob storage.
        /// </summary>
        /// <param name="blobInfo">Class representing azure blob unit.</param>
        /// <param name="maximalPartSize">Maximal size of the part sent in one request to Azure blob storage.</param>
        public BlobMultiPartUploader(BlobInfo blobInfo, int maximalPartSize)
        {
            mBlobInfo = blobInfo;
            mMaximalPartSize = maximalPartSize;
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Inits multipart upload to Azure blob storage.
        /// </summary>
        /// <returns>
        /// Upload ID, unique identifier for one multipart upload to Azure blob storage.
        /// Returned upload ID is needed for each subsequent multipart upload operation.
        /// </returns>
        public string InitMultiPartUpload()
        {
            return Guid.NewGuid().ToString();
        }


        /// <summary>
        /// Uploads data inside a stream in multiple parts to Azure blob storage.
        /// </summary>
        /// <param name="uploadSessionId">Unique identifier for one multipart upload. Can be obtained by <see cref="InitMultiPartUpload()"/> method.</param>
        /// <param name="nextPartNumber">Number that defines position of the data obtained by the stream in the whole multipart upload process.</param>
        /// <param name="stream">Stream with data to upload. Supplied stream has always it's position set to origin.</param>
        /// <returns>Unique identifiers of the uploaded parts.</returns>
        public IEnumerable<string> MultiPartUploadFromStream(string uploadSessionId, int nextPartNumber, Stream stream)
        {
            List<string> blockIds = new List<string>();
            stream.Seek(0, SeekOrigin.Begin);

            if (stream.Length <= mMaximalPartSize)
            {
                string blockId = GetBlockId(uploadSessionId, nextPartNumber);
                mBlobInfo.Blob.PutBlock(blockId, stream, null);
                blockIds.Add(blockId);
            }
            else
            {
                long bytesLeft = stream.Length;
                byte[] bytes = new byte[mMaximalPartSize];

                using (var ms = new MemoryStream(mMaximalPartSize))
                {
                    while (bytesLeft > 0)
                    {
                        string blockId = GetBlockId(uploadSessionId, nextPartNumber);
                        int bytesToRead = bytesLeft >= mMaximalPartSize ? mMaximalPartSize : (int)bytesLeft;

                        // Read data from stream to byte array
                        ReadDataFromStreamToByteArray(stream, bytes, bytesToRead);

                        // Write data to memory stream
                        ms.SetLength(bytesToRead);
                        ms.Seek(0, SeekOrigin.Begin);
                        ms.Write(bytes, 0, bytesToRead);

                        // MS position has to be set to beginning
                        ms.Seek(0, SeekOrigin.Begin);
                        mBlobInfo.Blob.PutBlock(blockId, ms, null);
                        blockIds.Add(blockId);

                        bytesLeft -= bytesToRead;
                        ++nextPartNumber;
                    }
                }
            }

            BlobInfoProvider.RemoveRequestCache(mBlobInfo);

            return blockIds;
        }


        /// <summary>
        /// Completes multiple part upload process.
        /// Sends final request to Azure blob storage to merge all parts already sent.
        /// </summary>
        /// <param name="uploadSessionId">Unique identifier for one multipart upload. Can be obtained by <see cref="InitMultiPartUpload()"/> method.</param>
        /// <param name="partIdentifiers">Identifiers of the parts already sent to Azure blob storage, obtained by calling <see cref="MultiPartUploadFromStream(string, int, Stream)"/></param>
        /// <returns>
        /// ETag of the uploaded file.
        /// </returns>
        public string CompleteMultiPartUploadProcess(string uploadSessionId, IEnumerable<string> partIdentifiers)
        {
            mBlobInfo.Blob.PutBlockList(partIdentifiers);
            BlobInfoProvider.RemoveRequestCache(mBlobInfo);

            return mBlobInfo.ETag;
        }


        /// <summary>
        /// Aborts multipart upload.
        /// </summary>
        /// <param name="uploadSessionId">Unique identifier for one multipart upload. Can be obtained by <see cref="InitMultiPartUpload()"/> method.</param>
        public void AbortMultiPartUpload(string uploadSessionId)
        {
            // Parts already uploaded to azure storage are deleted automatically after 7 days
            BlobInfoProvider.RemoveRequestCache(mBlobInfo);
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Creates block id for given uploadSessionId and part number.
        /// For one blob all the block ids need to have the same length.
        /// </summary>
        /// <param name="uploadSessionId">Unique identifier for one multipart upload. Can be obtained by <see cref="InitMultiPartUpload()"/> method.</param>
        /// <param name="nextPartNumber">Number that defines position of the data obtained by the stream in the whole multipart upload process.</param>
        /// <returns>A Base64-encoded string that identifies the block.</returns>
        private static string GetBlockId(string uploadSessionId, int nextPartNumber)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(uploadSessionId + nextPartNumber.ToString().PadLeft(7, '0')));
        }


        /// <summary>
        /// Reads at most <paramref name="bytesToRead"/> data from <paramref name="stream"/> to <paramref name="bytes"/>.
        /// </summary>
        /// <param name="stream">Stream to read from.</param>
        /// <param name="bytes">Byte array to save data from <paramref name="stream"/>.</param>
        /// <param name="bytesToRead">Number of bytes to read from <paramref name="stream"/>.</param>
        private void ReadDataFromStreamToByteArray(Stream stream, byte[] bytes, int bytesToRead)
        {
            int bytesCurrentlyRead;
            int numberOfAlreadyReadBytes = 0;
            while ((bytesCurrentlyRead = stream.Read(bytes, numberOfAlreadyReadBytes, bytesToRead)) > 0)
            {
                numberOfAlreadyReadBytes += bytesCurrentlyRead;
                bytesToRead -= numberOfAlreadyReadBytes;
            }
        }

        #endregion
    }
}
