using CMS.Base;

namespace CMS.CustomTables
{
    /// <summary>
    /// Custom table item event arguments
    /// </summary>
    public class CustomTableItemEventArgs : CMSEventArgs
    {
        /// <summary>
        /// Processed custom table item
        /// </summary>
        public CustomTableItem Item
        {
            get;
            set;
        }
    }
}