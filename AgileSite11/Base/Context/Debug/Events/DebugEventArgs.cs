using System.Data;

using CMS.Base;

namespace CMS.Base
{
    /// <summary>
    /// Debug event arguments
    /// </summary>
    public class DebugEventArgs : CMSEventArgs
    {
        /// <summary>
        /// DataRow with debug information.
        /// </summary>
        public DataRow DebugData
        {
            get;
            set;
        }
    }
}