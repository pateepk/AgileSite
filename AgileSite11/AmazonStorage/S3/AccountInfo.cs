using System;

using Amazon.S3;

using CMS.Helpers;
using CMS.Base;

namespace CMS.AmazonStorage
{
    /// <summary>
    /// Represents Account of Amazon web services. 
    /// </summary>
    /// <seealso cref="AmazonS3ClientConfiguration"/>
    public class AccountInfo
    {
        #region "Variables"

        private static readonly Lazy<AccountInfo> mCurrent = new Lazy<AccountInfo>(() => new AccountInfo(), true);
        private readonly AmazonS3Client mS3Client;

        #endregion


        #region "Constructors"

        /// <summary>
        /// Initializes a new instance of the <see cref="AccountInfo"/> class using the credentials and bucket name
        /// obtained from application configuration file.
        /// </summary>
        /// <seealso cref="AmazonS3ClientConfiguration"/>
        private AccountInfo()        
        {
            AccessKeyID = ValidationHelper.GetString(SettingsHelper.AppSettings["CMSAmazonAccessKeyID"], string.Empty);
            AccessKey = ValidationHelper.GetString(SettingsHelper.AppSettings["CMSAmazonAccessKey"], string.Empty);
            BucketName = ValidationHelper.GetString(SettingsHelper.AppSettings["CMSAmazonBucketName"], string.Empty);

            if (string.IsNullOrEmpty(BucketName))
            {
                throw new InvalidOperationException("Amazon S3 bucket name could not be found. You must specify it in web.config file by CMSAmazonBucketName application setting key.");
            }

            if (string.IsNullOrEmpty(AccessKey) || string.IsNullOrEmpty(AccessKeyID))
            {
                throw new InvalidOperationException("Amazon S3 access key or access key id could not be found. You must specify it in web.config file by CMSAmazonAccessKey and CMSAmazonAccessKeyID settings keys.");
            }

            mS3Client = new AmazonS3ClientFactory().Create(AccessKeyID, AccessKey, BucketName);
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="AccountInfo"/> class using the credentials and bucket name provided.
        /// </summary>
        /// <param name="accessKeyID">Access key ID.</param>
        /// <param name="accessKey">Access secret key.</param>
        /// <param name="bucketName">Bucket name.</param>
        /// <seealso cref="AmazonS3ClientConfiguration"/>
        public AccountInfo(string accessKeyID, string accessKey, string bucketName)
        {
            if (string.IsNullOrEmpty(bucketName))
            {
                throw new ArgumentException("Bucket name can not be null or empty.", "bucketName");
            }

            if (string.IsNullOrEmpty(accessKey) || string.IsNullOrEmpty(accessKeyID))
            {
                throw new ArgumentException("Access key ID and Access key can not be null or empty.");
            }

            AccessKeyID = accessKeyID;
            AccessKey = accessKey;
            BucketName = bucketName;

            mS3Client = new AmazonS3ClientFactory().Create(AccessKeyID, AccessKey, BucketName);
        }

        #endregion


        #region "Properties"

        /// <summary>
        /// Returns instance of Account Info.
        /// </summary>
        public static AccountInfo Current
        {
            get
            {
                return mCurrent.Value;
            }
        }


        /// <summary>
        /// Sets or gets bucket name.
        /// </summary>
        public string BucketName
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets access key id.
        /// </summary>
        public string AccessKeyID
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets secret access key.
        /// </summary>
        public string AccessKey
        {
            get;
            set;
        }


        /// <summary>
        /// Gets instance of AmazonS3 class (main class providing operations with storage).
        /// </summary>
        public AmazonS3Client S3Client
        {
            get
            {
                return mS3Client;
            }
        }

        #endregion
    }
}
