using System;
using System.Data;

namespace CMS.UIControls
{
    /// <summary>
    /// Values table base class.
    /// </summary>
    public class ValuesTable : CMSUserControl
    {
        #region "Variables"

        private DataTable mTable = null;
        private string mResourcePrefix = "General.";
        private string mTitle = null;

        #endregion


        #region "Properties"

        /// <summary>
        /// Cache log.
        /// </summary>
        public DataTable Table
        {
            get
            {
                return mTable;
            }
            set
            {
                mTable = value;
            }
        }


        /// <summary>
        /// Resource prefix for the column names.
        /// </summary>
        public override string ResourcePrefix
        {
            get
            {
                return mResourcePrefix;
            }
            set
            {
                mResourcePrefix = value;
            }
        }


        /// <summary>
        /// Table title.
        /// </summary>
        public string Title
        {
            get
            {
                return mTitle;
            }
            set
            {
                mTitle = value;
            }
        }

        #endregion
    }
}