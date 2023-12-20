using CMS.Base;

namespace CMS.IO
{
    /// <summary>
    /// Event arguments for the IO event handler
    /// </summary>
    public class IOEventArgs : CMSEventArgs
    {
        /// <summary>
        /// File system path
        /// </summary>
        public string Path
        {
            get;
            set;
        }
    }
}