namespace CMS.Base
{
    /// <summary>
    /// Thread event arguments
    /// </summary>
    public class ThreadEventArgs : CMSEventArgs
    {
        /// <summary>
        /// Thread
        /// </summary>
        public CMSThread Thread
        {
            get;
            set;
        }
    }
}