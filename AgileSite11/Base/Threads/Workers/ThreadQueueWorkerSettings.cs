using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMS.Base
{
    /// <summary>
    /// Settings common for all <see cref="ThreadQueueWorker{TItem,TWorker}"/>s.
    /// </summary>
    internal static class ThreadQueueWorkerSettings
    {
        private static bool mAllowEnqueue = true;


        /// <summary>
        /// Indicates whether <see cref="ThreadQueueWorker{TItem,TWorker}.Enqueue"/> method can put items in a queue for asynchronous processing. True by default. Work items are processed synchronously when set to false.
        /// </summary>
        public static bool AllowEnqueue
        {
            get
            {
                return mAllowEnqueue;
            }
            set
            {
                mAllowEnqueue = value;
            }
        }
    }
}
