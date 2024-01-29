namespace CMS.DataEngine
{
    /// <summary>
    /// Determines whether objects of child types are included into the export/staging data of parent objects.
    /// </summary>
    /// <remarks>
    /// Use for the values of the <see cref="SynchronizationSettings"/>.<see cref="SynchronizationSettings.IncludeToSynchronizationParentDataSet"/> and <see cref="ImportExportSettings"/>.<see cref="ImportExportSettings.IncludeToExportParentDataSet"/> properties.
    /// </remarks>
    public enum IncludeToParentEnum : byte
    {
        /// <summary>
        /// Objects of the child type are not included in the data of parent objects.
        /// </summary>
        None = 0,

        /// <summary>        
        /// The data of parent objects includes all existing child objects of the given type.
        /// Import/staging adds new child objects or updates existing ones. Deletes child objects that exist on the target instance, but are missing in the data of the updated parent.
        /// </summary>
        Complete = 1,

        /// <summary>        
        /// The data of parent objects includes all existing child objects of the given type.
        /// Import/staging adds new child objects or updates existing ones. Does not delete child objects that exist on the target instance, but are missing in the data of the updated parent.
        /// </summary>
        Incremental = 2,

        /// <summary>
        /// Default inclusion. Do NOT assign this value manually.
        /// </summary>
        Default = 3,
    }
}