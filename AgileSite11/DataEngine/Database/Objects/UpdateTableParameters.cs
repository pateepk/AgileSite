using System;
using System.Linq;

namespace CMS.DataEngine
{
    /// <summary>
    /// Parameters for method to update database table by definition.
    /// </summary>
    internal class UpdateTableParameters
    {
        private bool mUseOriginalDefinition = true;

        #region "Properties"

        /// <summary>
        /// Data class info
        /// </summary>
        public DataClassInfo ClassInfo
        {
            get;
            set;
        }


        /// <summary>
        /// Original form definition
        /// </summary>
        public string OriginalDefinition
        {
            get;
            set;
        }
        

        /// <summary>
        /// Indicates if original definition should be used to detect changes. If false, current database table structure is compared to the new definition to detect changes.
        /// </summary>
        public bool UseOriginalDefinition
        {
            get
            {
                return mUseOriginalDefinition;
            }
            set
            {
                mUseOriginalDefinition = value;
            }
        }

        #endregion
    }
}