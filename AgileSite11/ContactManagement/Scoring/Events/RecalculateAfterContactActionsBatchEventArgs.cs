using System.Collections;
using System.Collections.Generic;

using CMS.Base;

namespace CMS.ContactManagement
{
    /// <summary>
    /// Arguments for event fired when score is recalculated for contacts batch after their actions (activity, property change, merge or split).
    /// </summary>
    public class RecalculateAfterContactActionsBatchEventArgs : CMSEventArgs
    {
        /// <summary>
        /// Gets or sets contacts whose score is being recalculated.
        /// </summary>
        public ISet<int> ContactIDs
        {
            get;
            set;
        }


        /// <summary>
        /// Container for custom arguments which can be used by event subscribers.
        /// </summary>
        /// <remarks>Is never null</remarks>
        public Hashtable CustomArguments
        {
            get;
            private set;
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        public RecalculateAfterContactActionsBatchEventArgs()
        {
            CustomArguments = new Hashtable();
        }
    }
}