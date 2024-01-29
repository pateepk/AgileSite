namespace CMS.DataEngine
{
    /// <summary>
    /// Web farm task types for DataEngine module
    /// </summary>
    public class DataTaskType
    {
        /// <summary>
        /// MetaFile update.
        /// </summary>
        public const string UpdateMetaFile = "UPDATEMETAFILE";

        /// <summary>
        /// MetaFile delete.
        /// </summary>
        public const string DeleteMetaFile = "DELETEMETAFILE";

        /// <summary>
        /// Process custom object task
        /// </summary>
        public const string ProcessObject = "PROCESSOBJECT";

        /// <summary>
        /// Invalidation of single object.
        /// </summary>
        public const string InvalidateObject = "INVALIDATEOBJECT";

        /// <summary>
        /// Invalidation of direct children objects.
        /// </summary>
        public const string InvalidateChildren = "INVALIDATECHILDREN";

        /// <summary>
        /// Invalidation of all objects of specific type.
        /// </summary>
        public const string InvalidateAllObjects = "INVALIDATEALLOBJECTS";

        /// <summary>
        /// Provider dictionary command.
        /// </summary>
        public const string DictionaryCommand = "DICTIONARYCOMMAND";

        /// <summary>
        /// Clears the settings
        /// </summary>
        public const string ClearSettings = "CLEARSETTINGS";

        /// <summary>
        /// Clears resolved class names
        /// </summary>
        public const string ClearResolvedClassNames = "CLEARRESOLVEDCLASSNAMES";

        /// <summary>
        /// Removes read only object
        /// </summary>
        public const string RemoveReadOnlyObject = "REMOVEREADONLYOBJECT";

        /// <summary>
        /// Clears read only objects
        /// </summary>
        public const string ClearReadOnlyObjects = "CLEARREADONLYOBJECTS";

        /// <summary>
        /// Removes class structure info
        /// </summary>
        public const string RemoveClassStructureInfo = "REMOVECLASSSTRUCTUREINFO";

        /// <summary>
        /// Clears class structure infos
        /// </summary>
        public const string ClearClassStructureInfos = "CLEARCLASSSTRUCTUREINFOS";
        
        /// <summary>
        /// Clear all the system hashtables.
        /// </summary>
        public const string ClearHashtables = "CLEARHASHTABLES";
    }
}
