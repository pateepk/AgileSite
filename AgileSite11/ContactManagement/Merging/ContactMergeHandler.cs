using System;

using CMS.Base;

namespace CMS.ContactManagement.Internal
{
    /// <summary>
    /// Event handler for the event fired once merging of <see cref="ContactInfo"/> is in the progress.
    /// </summary>
    public class ContactMergeHandler : SimpleHandler<ContactMergeHandler, CMSEventArgs<ContactMergeModel>>
    {
        /// <summary>
        /// Initiates the event.
        /// </summary>
        /// <param name="source">Contact that is being merged to <paramref name="target"/></param>
        /// <param name="target">Contact the <paramref name="source"/> is being merged to</param>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <c>null</c> -or- <paramref name="target"/> is <c>null</c></exception>
        public CMSEventArgs<ContactMergeModel> StartEvent(ContactInfo source, ContactInfo target)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (target == null)
            {
                throw new ArgumentNullException("target");
            }

            var e = new CMSEventArgs<ContactMergeModel>
            {
                Parameter = new ContactMergeModel
                {
                    SourceContact = source,
                    TargetContact = target
                }
            };

            return StartEvent(e);
        }
    }
}