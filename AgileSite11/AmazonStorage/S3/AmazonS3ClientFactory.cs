using System;

using Amazon;
using Amazon.S3;
using Amazon.S3.Model;

namespace CMS.AmazonStorage
{
    /// <summary>
    /// Class that encapsulates creation of the <see cref="AmazonS3Client"/> instance.
    /// </summary>
    /// <seealso cref="AmazonS3ClientConfiguration"/>
    internal class AmazonS3ClientFactory
    {
        // Constant returned by Amazon S3 SDK when current S3 SDK does not recognize region endpoint.
        private const string NOT_RECOGNIZED_REGION_ENDPOINT = "Unknown";


        /// <summary>
        /// Creates <see cref="AmazonS3Client"/> instance that can access Amazon S3 via REST interface.
        /// </summary>
        /// <param name="accessKeyID">It is alphanumeric text string that uniquely identifies the user who owns the account.</param>
        /// <param name="accessKey">Password to the Amazon S3 storage.</param>
        /// <param name="bucketName">Name of the bucket in Amazon S3 storage.</param>
        /// <returns>Returns <see cref="AmazonS3Client"/> instance that can access Amazon S3 via REST interface.</returns>
        /// <seealso cref="AmazonS3ClientConfiguration"/>
        public AmazonS3Client Create(string accessKeyID, string accessKey, string bucketName)
        {
            if (String.IsNullOrWhiteSpace(accessKeyID))
            {
                throw new ArgumentException("accessKeyID is null or empty", "accessKeyID");
            }

            if (String.IsNullOrWhiteSpace(accessKey))
            {
                throw new ArgumentException("accessKey is null or empty", "accessKey");
            }

            if (String.IsNullOrWhiteSpace(bucketName))
            {
                throw new ArgumentException("bucketName is null or empty", "bucketName");
            }

            return CreateInternal(accessKeyID, accessKey, bucketName);
        }


        /// <summary>
        /// Creates <see cref="AmazonS3Client"/> instance that can access Amazon S3 via REST interface.
        /// </summary>
        /// <param name="accessKeyID">It is alphanumeric text string that uniquely identifies the user who owns the account.</param>
        /// <param name="accessKey">Password to the Amazon S3 storage.</param>
        /// <param name="bucketName">Name of the bucket in Amazon S3 storage.</param>
        /// <returns>Returns <see cref="AmazonS3Client"/> instance that can access Amazon S3 via REST interface.</returns>
        private AmazonS3Client CreateInternal(string accessKeyID, string accessKey, string bucketName)
        {
            var s3Config = new AmazonS3Config
            {
                ServiceURL = AmazonS3ClientConfiguration.RestApiEndPointUrl
            };

            var client = new AmazonS3Client(accessKeyID, accessKey, s3Config);

            GetBucketLocationResponse locationResponse = null;
            try
            {
                // Try to get bucket region from generic service
                locationResponse = client.GetBucketLocation(bucketName);
            }
            catch (AmazonS3Exception e)
            {
                throw new InvalidOperationException("This exception is typically thrown when given accessKeyID and accessKey is incorrect or bucket with given bucketName does not exist.", e);
            }

            RegionEndpoint regionEndpoint = GetRegionEndpoint(locationResponse.Location);

            if (regionEndpoint.SystemName.Equals(NOT_RECOGNIZED_REGION_ENDPOINT, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("The specified Amazon S3 bucket does not exist or is located in region that is currently not supported.");
            }

            // Set region
            s3Config.RegionEndpoint = regionEndpoint;

            return client;
        }


        /// <summary>
        /// Gets existing <see cref="RegionEndpoint"/> from <paramref name="location"/>.
        /// </summary>
        /// <param name="location">String value returned from Amazon S3 service that defines bucket location.</param>
        internal RegionEndpoint GetRegionEndpoint(string location)
        {
            RegionEndpoint regionEndpoint = null;

            // If returned location is empty then region endpoint is US East (Virginia)
            if (String.IsNullOrWhiteSpace(location))
            {
                return RegionEndpoint.USEast1;
            }
            else
            {
                // Check if region is supported
                regionEndpoint = RegionEndpoint.GetBySystemName(location);
            }

            // If the location constraint is returned as EU but without further details
            // it means that it's data center in Ireland according to the AWS documentation
            if (regionEndpoint.SystemName.Equals("EU", StringComparison.OrdinalIgnoreCase) &&
                regionEndpoint.DisplayName.Equals(NOT_RECOGNIZED_REGION_ENDPOINT, StringComparison.OrdinalIgnoreCase))
            {
                regionEndpoint = RegionEndpoint.EUWest1;
            }

            return regionEndpoint;
        }
    }
}
