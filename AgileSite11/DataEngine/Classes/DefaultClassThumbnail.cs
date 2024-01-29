using System;
using System.Linq;
using System.Text;

namespace CMS.DataEngine
{
    /// <summary>
    /// Is able to select default metafile of the class. Default metafile is the one with name starting with "default.".
    /// </summary>
    public class DefaultClassThumbnail
    {
        private const string META_FILE_NAME_STARTS_WITH = "default.";

        private readonly string mObjectType;


        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultClassThumbnail"/> class.
        /// </summary>
        /// <param name="objectType">Code name of the class whose Avatar meta files will be managed</param>
        /// <exception cref="ArgumentNullException"><paramref name="objectType"/> is null</exception>
        public DefaultClassThumbnail(string objectType)
        {
            if (objectType == null)
            {
                throw new ArgumentNullException("objectType");
            }

            mObjectType = objectType;
        }


        /// <summary>
        /// Gets GUID of default avatar located in MetaFile table for processed object info.
        /// </summary>
        /// <returns>GUID of default image, if exists; null otherwise</returns>
        public Guid? GetDefaultClassThumbnailGuid()
        {
            return MetaFileInfoProvider
                .GetMetaFiles()
                .WhereID("MetaFileObjectID", GetClassId())
                .WhereEquals("MetaFileObjectType", "cms.class")
                .WhereEquals("MetaFileGroupName", ObjectAttachmentsCategories.THUMBNAIL)
                .WhereStartsWith("MetaFileName", META_FILE_NAME_STARTS_WITH)
                .Column("MetaFileGUID")
                .AsEnumerable()
                .Select(m => (Guid?)m.MetaFileGUID)
                .FirstOrDefault();
        }


        /// <summary>
        /// Gets class ID of processed object type.
        /// </summary>
        /// <returns>Class ID of processed object type</returns>
        private int GetClassId()
        {
            return DataClassInfoProvider.GetDataClassInfo(mObjectType).ClassID;
        }
    }
}
