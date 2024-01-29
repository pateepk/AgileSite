using System;
using System.Data;

namespace CMS.UIControls
{
    /// <summary>
    /// Control for displaying document flags.
    /// </summary>
    public abstract class DocumentFlagsControl : CMSUserControl
    {
        #region "Public properties"

        /// <summary>
        /// Gets or sets the item URL
        /// </summary>
        public abstract string ItemUrl
        {
            get;
            set;
        }


        /// <summary>
        /// Name of the javascript function called when flag is clicked.
        /// </summary>
        public abstract string SelectJSFunction
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets Node ID.
        /// </summary>
        public abstract int NodeID
        {
            get;
            set;
        }

        /// <summary>
        /// Number of columns.
        /// </summary>
        public abstract int RepeatColumns
        {
            get;
            set;
        }


        /// <summary>
        /// Data source object.
        /// </summary>
        public abstract object DataSource
        {
            get;
            set;
        }


        /// <summary>
        /// DataSet containing all site cultures.
        /// </summary>
        public abstract DataSet SiteCultures
        {
            get;
            set;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Reloads control data.
        /// </summary>
        public abstract void ReloadData();

        #endregion
    }
}