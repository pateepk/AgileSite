using System;

namespace CMS.DataEngine
{
    /// <summary>
    /// Enumeration of the hashtable loading options.
    /// </summary>
    public enum LoadHashtableEnum : int
    {
        /// <summary>
        /// Do not load any objects.
        /// </summary>
        None = 0,

        /// <summary>
        /// Load all objects.
        /// </summary>
        All = 1,

        /// <summary>
        /// Load objects by generations. The property GenerationColumn must be set in order to use this type of loading.
        /// </summary>
        [Obsolete("Behaves similarly as None and will be removed.")]
        Generations = 2,

        /// <summary>
        /// Load by generations. The property GenerationColumn must be set in order to use this type of loading.
        /// </summary>
        [Obsolete("Behaves similarly as None and will be removed.")]
        FirstGeneration = 3
    }
}