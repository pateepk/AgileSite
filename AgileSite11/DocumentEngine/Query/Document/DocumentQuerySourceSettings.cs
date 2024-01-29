using System;
using System.Linq;
using System.Text;

using CMS.DataEngine;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Configuration settings for document query source
    /// </summary>
    internal class DocumentQuerySourceSettings
    {
        /// <summary>
        /// If true, NOLOCK hint is used in the query sources to execute the query in uncommitted read isolation level.
        /// </summary>
        public bool? UseNoLock
        {
            get;
            set;
        }


        /// <summary>
        /// If true, NOEXPAND hint is used in the query sources forcing usage of view indexes.
        /// </summary>
        public bool? UseNoExpand
        {
            get;
            set;
        }


        /// <summary>
        /// Returns true if the NOEXPAND hint should be used for the query
        /// </summary>
        internal bool UseNoExpandHint()
        {
            // Add no expand by default or if explicitly requested
            return 
                ((UseNoExpand == null) && AbstractSqlGenerator.GenerateWithExpand && TableManager.USE_INDEXED_VIEWS) ||
                (UseNoExpand == true);
        }


        /// <summary>
        /// Returns true if the NOLOCK hint should be used for the query
        /// </summary>
        internal bool UseNoLockHint()
        {
            // Add no lock by default or if explicitly requested
            return
                (UseNoLock == null) ||
                (UseNoLock == true);
        }


        /// <summary>
        /// Returns the clone of the object
        /// </summary>
        internal DocumentQuerySourceSettings Clone()
        {
            return (DocumentQuerySourceSettings)MemberwiseClone();
        }
    }
}
