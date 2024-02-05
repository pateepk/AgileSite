using System;
using System.Collections.Generic;

using CMS.Base;

namespace CMS.ContactManagement
{
    /// <summary>
    /// Event arguments for event that fires when <see cref="ContactInfoProvider.DeleteContactInfos"/> deletes some contacts.
    /// </summary>
    public sealed class ContactInfosDeletedHandlerEventArgs : CMSEventArgs
    {
        /// <summary>
        /// IDs of deleted contacts.
        /// </summary>
        public IEnumerable<int> DeletedContactsIds
        {
            get;
            internal set;
        }


        /// <summary>
        /// GUIDs of deleted contacts.
        /// </summary>
        public IEnumerable<Guid> DeletedContactsGUIDs
        {
            get;
            internal set;
        }
    }
}