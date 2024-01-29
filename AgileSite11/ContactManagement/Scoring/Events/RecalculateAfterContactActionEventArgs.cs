using System.Collections;

using CMS.Base;

namespace CMS.ContactManagement
{
    /// <summary>
    /// Arguments for event fired when score is recalculated for one contact after contact's action (activity, property change, merge or split).
    /// </summary>
    public class RecalculateAfterContactActionEventArgs : CMSEventArgs
    {
        /// <summary>
        /// Gets or sets contact whose score is being recalculated.
        /// </summary>
        public ContactInfo Contact
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
        public RecalculateAfterContactActionEventArgs()
        {
            CustomArguments = new Hashtable();
        }
    }
}