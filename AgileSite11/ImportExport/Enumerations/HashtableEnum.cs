using System;
using System.Collections;
using System.Data;

namespace CMS.CMSImportExport
{
    /// <summary>
    /// Hashtable type enumeration.
    /// </summary>
    public enum HashtableEnum
    {
        /// <summary>
        /// Global selected obejct types hashtable.
        /// </summary>
        GlobalSelectedObjects = 0,

        /// <summary>
        /// Site selected object types hashtable.
        /// </summary>
        SiteSelectedObjects = 1,

        /// <summary>
        /// Global processed object types hashtable.
        /// </summary>
        GlobalProcessedObjects = 2,

        /// <summary>
        /// Site processed object types hashtable.
        /// </summary>
        SiteProcessedObjects = 3,

        /// <summary>
        /// Additional settings hashtable.
        /// </summary>
        AdditionalSettings = 4
    }
}