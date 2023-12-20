using System.Collections.Generic;
using System.Linq;

using CMS.DataEngine;


namespace CMS.ContinuousIntegration.Internal
{
    /// <summary>
    /// Class providing <see cref="FileMetadataInfo"/> management.
    /// </summary>
    public class FileMetadataInfoProvider : AbstractInfoProvider<FileMetadataInfo, FileMetadataInfoProvider>
    {
        #region "Constructors"

        /// <summary>
        /// Creates new instance of the <see cref="FileMetadataInfoProvider"/>
        /// </summary>
        public FileMetadataInfoProvider()
            : base(FileMetadataInfo.TYPEINFO)
        {
        }

        #endregion


        #region "Public methods - Basic"

        /// <summary>
        /// Returns a query for all the <see cref="FileMetadataInfo"/> objects.
        /// </summary>
        public static ObjectQuery<FileMetadataInfo> GetFileMetadataInfos()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns <see cref="FileMetadataInfo"/> with specified ID.
        /// </summary>
        /// <param name="id"><see cref="FileMetadataInfo"/> ID</param>
        public static FileMetadataInfo GetFileMetadataInfo(int id)
        {
            return ProviderObject.GetInfoById(id);
        }


        /// <summary>
        /// Returns <see cref="FileMetadataInfo"/> with specified location.
        /// </summary>
        /// <param name="location"><see cref="FileMetadataInfo"/> location</param>
        public static FileMetadataInfo GetFileMetadataInfo(string location)
        {
            return ProviderObject.GetFileMetadataInfoInternal(location);
        }


        /// <summary>
        /// Sets (updates or inserts) specified <see cref="FileMetadataInfo"/>.
        /// </summary>
        /// <param name="infoObj"><see cref="FileMetadataInfo"/> to be set</param>
        public static void SetFileMetadataInfo(FileMetadataInfo infoObj)
        {
            ProviderObject.SetInfo(infoObj);
        }


        /// <summary>
        /// Bulk inserts the given list of <see cref="FileMetadataInfo"/> objects
        /// </summary>
        /// <param name="objects">List of <see cref="FileMetadataInfo"/> objects</param>
        public static void BulkInsertFileMetadataInfos(IEnumerable<FileMetadataInfo> objects)
        {
            ProviderObject.BulkInsertFileMetadataInfosInternal(objects);
        }


        /// <summary>
        /// Deletes specified <see cref="FileMetadataInfo"/>.
        /// </summary>
        /// <param name="infoObj"><see cref="FileMetadataInfo"/> to be deleted</param>
        public static void DeleteFileMetadataInfo(FileMetadataInfo infoObj)
        {
            ProviderObject.DeleteInfo(infoObj);
        }


        /// <summary>
        /// Deletes all <see cref="FileMetadataInfo"/> objects.
        /// </summary>
        public static void DeleteAllFileMetadataInfos()
        {
            ProviderObject.DeleteAllFileMetadataInfosInternal();
        }


        /// <summary>
        /// Deletes <see cref="FileMetadataInfo"/> with specified ID.
        /// </summary>
        /// <param name="id"><see cref="FileMetadataInfo"/> ID</param>
        public static void DeleteFileMetadataInfo(int id)
        {
            FileMetadataInfo infoObj = GetFileMetadataInfo(id);
            DeleteFileMetadataInfo(infoObj);
        }

        #endregion


        #region "Internal methods - Basic"

        /// <summary>
        /// Returns <see cref="FileMetadataInfo"/> with specified location.
        /// </summary>
        /// <param name="location"><see cref="FileMetadataInfo"/> location</param>
        protected virtual FileMetadataInfo GetFileMetadataInfoInternal(string location)
        {
            return GetFileMetadataInfos().WhereEquals("FileLocation", location).TopN(1).FirstOrDefault();
        }


        /// <summary>
        /// Bulk inserts the given list of <see cref="FileMetadataInfo"/> objects.
        /// </summary>
        /// <param name="objects">List of <see cref="FileMetadataInfo"/> objects</param>
        protected virtual void BulkInsertFileMetadataInfosInternal(IEnumerable<FileMetadataInfo> objects)
        {
            ProviderObject.BulkInsertInfos(objects);
        }


        /// <summary>
        /// Deletes all <see cref="FileMetadataInfo"/> objects.
        /// </summary>
        protected virtual void DeleteAllFileMetadataInfosInternal()
        {
            ConnectionHelper.ExecuteNonQuery("ci.filemetadata.deleteall");
        }

        #endregion
    }
}