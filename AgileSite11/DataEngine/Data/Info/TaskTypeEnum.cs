using CMS.Helpers;
using CMS.Base;

namespace CMS.DataEngine
{
    /// <summary>
    /// Synchronization task types.
    /// </summary>
    public enum TaskTypeEnum
    {
        /// <summary>
        /// All task types (used within integration module)
        /// </summary>
        [EnumOrder(1)]
        [EnumDefaultValue]
        [EnumCategory("General")]
        All = -1,

        /// <summary>
        /// Unkown task type.
        /// </summary>
        [EnumOrder(21)]
        Unknown = 0,

        /// <summary>
        /// Update document task.
        /// </summary>
        [EnumOrder(22)]
        [EnumCategory("ContentStaging")]
        UpdateDocument = 1,

        /// <summary>
        /// Publish document task.
        /// </summary>
        [EnumOrder(16)]
        [EnumCategory("ContentStaging")]
        PublishDocument = 2,

        /// <summary>
        /// Delete document task.
        /// </summary>
        [EnumOrder(10)]
        [EnumCategory("ContentStaging")]
        DeleteDocument = 3,

        /// <summary>
        /// Delete all culture versions of specified document task.
        /// </summary>
        [EnumOrder(9)]
        [EnumCategory("ContentStaging")]
        DeleteAllCultures = 4,

        /// <summary>
        /// Moves document task.
        /// </summary>
        [EnumOrder(13)]
        [EnumCategory("ContentStaging")]
        MoveDocument = 5,

        /// <summary>
        /// Archive document task.
        /// </summary>
        [EnumOrder(3)]
        [EnumCategory("ContentStaging")]
        ArchiveDocument = 6,

        /// <summary>
        /// Update object task.
        /// </summary>
        [EnumOrder(23)]
        [EnumCategory("DataStaging")]
        UpdateObject = 7,

        /// <summary>
        /// Delete object task.
        /// </summary>
        [EnumOrder(12)]
        [EnumCategory("DataStaging")]
        DeleteObject = 8,

        /// <summary>
        /// Archive document task.
        /// </summary>
        [EnumOrder(17)]
        [EnumCategory("ContentStaging")]
        RejectDocument = 9,

        /// <summary>
        /// Creates object task.
        /// </summary>
        [EnumOrder(8)]
        [EnumCategory("DataStaging")]
        CreateObject = 10,

        /// <summary>
        /// Creates document task.
        /// </summary>
        [EnumOrder(6)]
        [EnumCategory("ContentStaging")]
        CreateDocument = 11,

        /// <summary>
        /// Creates media library folder.
        /// </summary>
        [EnumOrder(7)]
        [EnumCategory("ObjectStaging")]
        CreateMediaFolder = 12,

        /// <summary>
        /// Copy media library folder.
        /// </summary>
        [EnumOrder(5)]
        [EnumCategory("ObjectStaging")]
        CopyMediaFolder = 13,

        /// <summary>
        /// Moves media library folder.
        /// </summary>
        [EnumOrder(14)]
        [EnumCategory("ObjectStaging")]
        MoveMediaFolder = 14,

        /// <summary>
        /// Delete media library folder.
        /// </summary>
        [EnumOrder(10)]
        [EnumCategory("ObjectStaging")]
        DeleteMediaFolder = 15,

        /// <summary>
        /// Delete media library root folder.
        /// </summary>
        [EnumOrder(11)]
        [EnumCategory("ObjectStaging")]
        DeleteMediaRootFolder = 16,

        /// <summary>
        /// Rename media library folder.
        /// </summary>
        [EnumOrder(19)]
        [EnumCategory("ObjectStaging")]
        RenameMediaFolder = 17,

        /// <summary>
        /// Add object to site.
        /// </summary>
        [EnumOrder(2)]
        [EnumCategory("ObjectStaging")]
        AddToSite = 18,

        /// <summary>
        /// Remove object from site.
        /// </summary>
        [EnumOrder(18)]
        [EnumCategory("ObjectStaging")]
        RemoveFromSite = 19,

        /// <summary>
        /// Break ACL inheritance.
        /// </summary>
        [EnumOrder(4)]
        [EnumCategory("ContentStaging")]
        BreakACLInheritance = 20,

        /// <summary>
        /// Restore ACL inheritance.
        /// </summary>
        [EnumOrder(20)]
        [EnumCategory("ContentStaging")]
        RestoreACLInheritance = 21
    }
}