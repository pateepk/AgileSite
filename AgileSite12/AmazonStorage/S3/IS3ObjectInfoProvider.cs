using System.Collections.Generic;
using System.IO;

namespace CMS.AmazonStorage
{
    /// <summary>
    /// Interface of the S3ObjectInfoProvider
    /// </summary>
    public interface IS3ObjectInfoProvider
    {
        #region "Provider methods"

        /// <summary>
        /// Returns list with objects from given bucket and under given path. 
        /// </summary>        
        /// <param name="path">Path.</param>
        /// <param name="type">Specifies which objects are returned (files, directories, both).</param>
        /// <param name="useFlatListing">Whether flat listing is used (all files from all subdirectories all in the result).</param>
        /// <param name="lower">Specifies whether path should be lowered inside method.</param>
        /// <param name="useCache">Indicates if results should be primary taken from cache to get better performance</param>
        /// <remarks>
        /// In order to allow to distinguish between files and directories, directories are listed with a trailing backslash.
        /// </remarks>
        List<string> GetObjectsList(string path, ObjectTypeEnum type, bool useFlatListing = false, bool lower = true, bool useCache = true);


        /// <summary>
        /// Returns whether object exists.
        /// </summary>
        /// <param name="obj">Object info.</param>
        bool ObjectExists(IS3ObjectInfo obj);


        /// <summary>
        /// Returns object content as a CMS.IO.Stream.
        /// </summary>
        /// <param name="obj">Object info.</param>
        /// <param name="fileMode">File mode.</param>
        /// <param name="fileAccess">File access.</param>
        /// <param name="fileShare">Sharing permissions.</param>
        /// <param name="bufferSize">Buffer size.</param>
        Stream GetObjectContent(IS3ObjectInfo obj, FileMode fileMode = FileMode.Open, FileAccess fileAccess = FileAccess.Read, FileShare fileShare = FileShare.Read, int bufferSize = 0x1000);


        /// <summary>
        /// Puts file to Amazon S3 storage.
        /// </summary>
        /// <param name="obj">Object info.</param>
        /// <param name="pathToSource">Path to source file.</param>
        void PutFileToObject(IS3ObjectInfo obj, string pathToSource);


        /// <summary>
        /// Puts data from stream to Amazon S3 storage.
        /// </summary>
        /// <param name="obj">Object info.</param>
        /// <param name="stream">Stream to upload.</param>
        void PutDataFromStreamToObject(IS3ObjectInfo obj, Stream stream);


        /// <summary>
        /// Puts text to Amazon S3 storage object.
        /// </summary>
        /// <param name="obj">Object info.</param>
        /// <param name="content">Content to add.</param>
        void PutTextToObject(IS3ObjectInfo obj, string content);


        /// <summary>
        /// Appends text to Amazon S3 storage object.
        /// </summary>
        /// <param name="obj">Object info.</param>
        /// <param name="content">Content to append.</param>
        void AppendTextToObject(IS3ObjectInfo obj, string content);


        /// <summary>
        /// Deletes object from Amazon S3 storage.
        /// </summary>
        /// <param name="obj">Object info.</param>
        void DeleteObject(IS3ObjectInfo obj);


        /// <summary>
        /// Copies object to another.
        /// </summary>
        /// <param name="sourceObject">Source object info.</param>
        /// <param name="destObject">Destination object info.</param>
        void CopyObjects(IS3ObjectInfo sourceObject, IS3ObjectInfo destObject);


        /// <summary>
        /// Creates empty object.
        /// </summary>
        /// <param name="obj">Object info.</param>
        void CreateEmptyObject(IS3ObjectInfo obj);

        #endregion


        #region "Factory methods"

        /// <summary>
        /// Returns new instance of S3ObjectInfo.
        /// </summary>
        /// <param name="path">Path with file name.</param>        
        IS3ObjectInfo GetInfo(string path);


        /// <summary>
        /// Initializes new instance of S3 object info with specified bucket name.
        /// </summary>        
        /// <param name="path">Path with file name.</param>
        /// <param name="key">Specifies that given path is already object key.</param>
        IS3ObjectInfo GetInfo(string path, bool key);

        #endregion
    }
}
