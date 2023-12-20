using System.Threading;

namespace CMS.Base
{
    /// <summary>
    /// The <see cref="ThreadSettings" /> class specifies basic features of a <see cref="CMSThread" />.
    /// </summary>
    public sealed class ThreadSettings
    {
        /// <summary>
        /// Indicates whether <see cref="CMSThread"/> should create it's own log.
        /// </summary>
        public bool CreateLog
        {
            get;
            set;
        }


        /// <summary>
        /// Thread mode indicating whether the thread runs asynchronously, synchronously, or represents a wrapper for an anonymous thread.
        /// </summary>
        public ThreadModeEnum Mode
        {
            get;
            set;
        } = ThreadModeEnum.Async;


        /// <summary>
        /// Gets or sets a value indicating whether or not a thread is a background thread. Applies only to thread which run in <see cref="ThreadModeEnum.Async"/> mode.
        /// </summary>
        public bool IsBackground
        {
            get;
            set;
        }


        /// <summary>
        /// Specifies the scheduling priority. Applies only to thread which run in <see cref="ThreadModeEnum.Async"/> mode.
        /// </summary>
        public ThreadPriority Priority
        {
            get;
            set;
        } = ThreadPriority.Normal;


        /// <summary>
        /// If true, the thread uses a new empty context
        /// </summary>
        public bool UseEmptyContext
        {
            get;
            set;
        }
    }
}