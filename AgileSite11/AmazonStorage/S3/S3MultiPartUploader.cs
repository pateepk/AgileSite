using System;
using System.Collections.Generic;

using CMS.EventLog;

using Amazon.S3;
using Amazon.S3.Model;

using SystemIO = System.IO;

namespace CMS.AmazonStorage
{
    /// <summary>
    /// Class for uploading large files to Amazon S3 storage.
    /// </summary>
    internal class S3MultiPartUploader
    {
        #region "Variables"

        private readonly AmazonS3Client mS3Client;
        private readonly long mMinimalPartSize;
        private readonly long mMaximalPartSize;

        #endregion


        #region "Constructor"

        /// <summary>
        /// Creates utility for uploading large files in smaller parts to Amazon S3 storage.
        /// </summary>
        /// <param name="s3Client">Client for accessing the Amazon S3 storage.</param>
        /// <param name="minimalPartSize">Minimal size of the part sent in one request to Amazon S3 storage.</param>
        /// <param name="maximalPartSize">Maximal possible size of the part sent in one request to Amazon S3 storage.</param>
        internal S3MultiPartUploader(AmazonS3Client s3Client, long minimalPartSize, long maximalPartSize)
        {
            if (s3Client == null)
            {
                throw new ArgumentNullException("s3Client");
            }

            if (minimalPartSize < 1)
            {
                throw new ArgumentOutOfRangeException("minimalPartSize", "minimalPartSize cannot be smaller than 1.");
            }


            if (maximalPartSize <= minimalPartSize)
            {
                throw new ArgumentOutOfRangeException("maximalPartSize", "maximalPartSize cannot be smaller than minimalPartSize.");
            }

            mS3Client = s3Client;
            mMinimalPartSize = minimalPartSize;
            mMaximalPartSize = maximalPartSize;
        }

        #endregion


        #region "Internal methods"

        /// <summary>
        /// Inits multipart upload with given <paramref name="key"/> inside <paramref name="bucket"/> with Amazon S3 storage.
        /// </summary>
        /// <param name="key">Unique identifier for an object within a bucket.</param>
        /// <param name="bucket">Existing Amazon S3 bucket.</param>
        /// <returns>
        /// Upload ID, unique identifier for one multipart upload to Amazon S3 storage.
        /// Returned upload ID is needed for each subsequent multipart upload operation.
        /// </returns>
        public string InitMultiPartUpload(string key, string bucket)
        {
            if (String.IsNullOrEmpty(key) || String.IsNullOrEmpty(bucket))
            {
                throw new ArgumentException("key and bucket cannot be empty.");
            }

            var initiateMultipartRequest = new InitiateMultipartUploadRequest
            {
                Key = key,
                BucketName = bucket,
                CannedACL = AmazonHelper.PublicAccess ? S3CannedACL.PublicRead : S3CannedACL.NoACL
            };

            return mS3Client.InitiateMultipartUpload(initiateMultipartRequest).UploadId;
        }


        /// <summary>
        /// Uploads one part of a file to Amazon S3 storage.
        /// </summary>
        /// <remarks>
        /// If some of the Amazon S3 policies about multipart upload is violated then exception will be thrown.
        /// For example, if part smaller than 5 MB is uploaded and given part is not the last part of the multipart upload process.
        /// </remarks>
        /// <param name="uploadId">Unique identifier for one multipart upload. Can be obtained by <see cref="InitMultiPartUpload(string, string)"/> method.</param>
        /// <param name="key">Unique identifier for an object within a bucket.</param>
        /// <param name="bucket">Existing Amazon S3 bucket.</param>
        /// <param name="partNumber">Number that defines position of the data obtained by the stream in the whole multipart upload process.</param>
        /// <param name="stream">Stream with data to upload. Supplied stream has always it's position set to origin.</param>
        /// <returns>Unique identifier of the uploaded part.</returns>
        public string UploadPartFromStream(string uploadId, string key, string bucket, int partNumber, SystemIO.Stream stream)
        {
            if (stream.Length > mMaximalPartSize)
            {
                throw new ArgumentException("stream's length exceeds maximal part size ("+ mMaximalPartSize +"B) that can be uploaded to Amazon S3 storage.");
            }

            var req = CreateUploadPartRequest(key, bucket, uploadId);

            stream.Seek(0, SystemIO.SeekOrigin.Begin);
            req.PartSize = stream.Length;
            req.InputStream = stream;
            req.PartNumber = partNumber;
            
            return mS3Client.UploadPart(req).ETag;
        }


        /// <summary>
        /// Uploads one large file to Amazon S3 storage in smaller parts.
        /// </summary>
        /// <param name="key">Unique identifier for an object within a bucket.</param>
        /// <param name="bucket">Existing Amazon S3 bucket.</param>
        /// <param name="stream">Stream with data to upload. Supplied stream has always it's position set to origin.</param>
        /// <returns>
        /// Response containing metadata of uploaded file.
        /// </returns>
        public CompleteMultipartUploadResponse UploadFromStream(string key, string bucket, SystemIO.Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            return MultiPartUploadFromStream(key, bucket, stream);
        }


        /// <summary>
        /// Uploads large file from local file system to Amazon S3 storage in smaller parts.
        /// </summary>
        /// <param name="key">Unique identifier for an object within a bucket.</param>
        /// <param name="bucket">Existing Amazon S3 bucket.</param>
        /// <param name="filePath">Path to a local file.</param>
        /// <returns>
        /// Response contains metadata of uploaded file like ETag.
        /// </returns>
        public CompleteMultipartUploadResponse UploadFromFilePath(string key, string bucket, string filePath)
        {
            if (String.IsNullOrEmpty(filePath))
            {
                throw new ArgumentException("filePath cannot be null or empty.");
            }

            using (SystemIO.FileStream fs = new SystemIO.FileStream(filePath, SystemIO.FileMode.Open))
            {
                return UploadFromStream(key, bucket, fs);
            }
        }


        /// <summary>
        /// Completes multiple part upload process.
        /// Sends final request to Amazon S3 to merge all parts already sent to storage.
        /// </summary>
        /// <param name="key">Unique identifier for an object within a bucket.</param>
        /// <param name="bucket">Existing Amazon S3 bucket.</param>
        /// <param name="uploadId">Unique identifier for one multipart upload. Can be obtained by <see cref="InitMultiPartUpload(string, string)"/> method.</param>
        /// <param name="uploadedPartResponses">List of responses from Amazon S3 received after uploading each part.</param>
        /// <returns>Response from Amazon S3 storage after finishing the multipart upload.</returns>
        public CompleteMultipartUploadResponse CompleteMultiPartUploadProcess(string key, string bucket, string uploadId, IEnumerable<UploadPartResponse> uploadedPartResponses)
        {
            var completeRequest = new CompleteMultipartUploadRequest
            {
                Key = key,
                BucketName = bucket,
                UploadId = uploadId,
            };
            completeRequest.AddPartETags(uploadedPartResponses);

            var completeResponse = mS3Client.CompleteMultipartUpload(completeRequest);
            return completeResponse;
        }


        /// <summary>
        /// Aborts multipart upload, so Amazon S3 storage can delete uploaded parts.
        /// </summary>
        /// <param name="key">Unique identifier for an object within a bucket.</param>
        /// <param name="bucket">Existing Amazon S3 bucket.</param>
        /// <param name="uploadId">Unique identifier for one multipart upload. Can be obtained by <see cref="InitMultiPartUpload(string, string)"/> method.</param>
        public void AbortMultiPartUpload(string key, string bucket, string uploadId)
        {
            AbortMultipartUploadRequest abortMPURequest = new AbortMultipartUploadRequest
            {
                BucketName = bucket,
                Key = key,
                UploadId = uploadId
            };
            mS3Client.AbortMultipartUpload(abortMPURequest);
        }

        #endregion


        #region "Private Methods"

        /// <summary>
        /// Uploads one large file stored in <paramref name="stream"/> to Amazon S3 storage in smaller parts.
        /// </summary>
        /// <param name="key">Unique identifier for an object within a bucket.</param>
        /// <param name="bucket">Existing Amazon S3 bucket.</param>
        /// <param name="stream">Stream with data to upload. Supplied stream has always it's position set to origin.</param>
        /// <returns>
        /// Response from Amazon S3 storage after finishing the multipart upload.
        /// Response contains metadata of the uploaded file.
        /// </returns>
        private CompleteMultipartUploadResponse MultiPartUploadFromStream(string key, string bucket, SystemIO.Stream stream)
        {
            List<UploadPartResponse> partResponses = new List<UploadPartResponse>();
            
            var uploadId = InitMultiPartUpload(key, bucket);

            var request = CreateUploadPartRequest(key, bucket, uploadId);
            stream.Seek(0, SystemIO.SeekOrigin.Begin);

            var uploadStream = stream;
            try
            {
                for (request.PartNumber = 1; request.FilePosition < uploadStream.Length; request.PartNumber++)
                {
                    // Stream has to be re-assigned to the request because internally
                    // in the request is wrapped by special stream with reduced length
                    request.InputStream = uploadStream;

                    partResponses.Add(mS3Client.UploadPart(request));
                    request.FilePosition += request.PartSize;
                }

                return CompleteMultiPartUploadProcess(request.Key, request.BucketName, request.UploadId, partResponses);
            }
            catch (AmazonS3Exception e)
            {
                EventLogProvider.LogException("AmazonStorage", "MULTIPARTUPLOAD", e);
                AbortMultiPartUpload(request.Key, request.BucketName, request.UploadId);

                throw;
            }
        }


        /// <summary>
        /// Creates first request used for multipart upload.
        /// </summary>
        /// <param name="key">Unique identifier for an object within a bucket.</param>
        /// <param name="bucket">Existing Amazon S3 bucket.</param>
        /// <param name="uploadId">Unique identifier for one multipart upload. Can be obtained by <see cref="InitMultiPartUpload(string, string)"/> method.</param>
        /// <returns>Returns first request used for multipart upload.</returns>
        private UploadPartRequest CreateUploadPartRequest(string key, string bucket, string uploadId)
        {
            if (String.IsNullOrEmpty(key) || String.IsNullOrEmpty(bucket) || String.IsNullOrEmpty(uploadId))
            {
                throw new ArgumentException("key, bucket and uploadId cannot be empty.");
            }

            return new UploadPartRequest
            {
                Key = key,
                BucketName = bucket,
                PartSize = mMinimalPartSize,
                FilePosition = 0,
                UploadId = uploadId,
                PartNumber = 1
            };
        }

        #endregion
    }
}
