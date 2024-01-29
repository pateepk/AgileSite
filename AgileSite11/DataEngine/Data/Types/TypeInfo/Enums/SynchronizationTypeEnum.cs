namespace CMS.DataEngine
{
    /// <summary>
    /// Determines how the system logs staging synchronization tasks for object types.
    /// </summary>
    /// <remarks>
    /// Use for the value of the <see cref="SynchronizationSettings.LogSynchronization"/> property.
    /// </remarks>
    public enum SynchronizationTypeEnum : byte
    {
        /// <summary>
        /// The system does not log staging tasks for objects of the type.
        /// </summary>
        None = 0,

        /// <summary>
        /// The system logs staging tasks when objects of the type are created, updated or deleted.
        /// </summary>
        LogSynchronization = 1,

        /// <summary>        
        /// For object types that have a parent type. Creating, modifying or deleting an object of the child type triggers an update of the parent object.
        /// The update generates staging tasks according to the SynchronizationSettings of the parent type.
        /// </summary>
        TouchParent = 2,

        /// <summary>
        /// Default value used internally. Do NOT assign this value manually in the object type information.
        /// </summary>
        Default = 3
    }
}