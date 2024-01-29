using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using CMS.DataEngine;
using CMS.Helpers;
using CMS.MacroEngine;

namespace CMS.ContactManagement
{
    /// <summary>
    /// Rebuilds the whole contact group by fetching all contacts into the memory and evaluating macro condition on them one by one.
    /// </summary>
    internal class ContactGroupMemoryRebuilder
    {
        /// <summary>
        /// Cached MacroResolver.
        /// </summary>
        private static readonly ThreadLocal<MacroResolver> mCachedResolver = new ThreadLocal<MacroResolver>(() => MacroResolver.GetInstance());


        /// <summary>
        /// Size of page used when recalculating contacts.
        /// </summary>
        private const int PAGE_SIZE = 10000;


        /// <summary>
        /// Rebuilds a contact group.
        /// </summary>
        public void RebuildGroup(ContactGroupInfo contactGroup)
        {
            if (contactGroup == null)
            {
                throw new ArgumentNullException("contactGroup");
            }

            var contactsQuery = ContactInfoProvider.GetContacts();

            Rebuild(contactGroup, contactsQuery);
        }


        /// <summary>
        /// Rebuilds only contacts with specified contact IDs.
        /// </summary>
        public void RebuildPartOfContactGroup(ContactGroupInfo contactGroup, IEnumerable<int> contactIDs)
        {
            if (contactGroup == null)
            {
                throw new ArgumentNullException("contactGroup");
            }
            if (contactIDs == null)
            {
                throw new ArgumentNullException("contactIDs");
            }


            var contactsQuery = ContactInfoProvider.GetContacts()
                                                   .WhereIn("ContactID", contactIDs.ToList());

            Rebuild(contactGroup, contactsQuery);
        }


        /// <summary>
        /// Rebuilds membership in all contact groups for given contact.
        /// </summary>
        public void RebuildGroupsForContact(ContactInfo contact, List<ContactGroupInfo> contactGroups)
        {
            if (contact == null)
            {
                throw new ArgumentNullException("contact");
            }
            if (contactGroups == null)
            {
                throw new ArgumentNullException("contactGroups");
            }

            // Filter contact groups to be sure that there are no needless in the list
            contactGroups = contactGroups.Where(x => !String.IsNullOrEmpty(x.ContactGroupDynamicCondition)).ToList();

            if (!contactGroups.Any())
            {
                return;
            }

            var allList = new List<int>();
            var passList = new List<int>();
            foreach (var contactGroup in contactGroups)
            {
                if (EvaluateDynamicMembership(contactGroup, contact))
                {
                    passList.Add(contactGroup.ContactGroupID);
                }
                allList.Add(contactGroup.ContactGroupID);
            }

            ContactGroupMemberInfoProvider.SetContactAsDynamic(contact, passList, allList);
        }


        private void Rebuild(ContactGroupInfo contactGroup, ObjectQuery<ContactInfo> baseContactsQuery)
        {
            // Use order by ContactID to get better performance for paging
            var contactsQuery = baseContactsQuery.OrderBy("ContactID");

            // Evaluate contacts one by one - This code could be optimized ...
            contactsQuery.ForEachPage(page =>
            {
                var passList = new List<int>();
                var allList = new List<int>(PAGE_SIZE);

                foreach (var contact in page)
                {
                    if (EvaluateDynamicMembership(contactGroup, contact))
                    {
                        passList.Add(contact.ContactID);
                    }

                    allList.Add(contact.ContactID);
                }

                ContactGroupMemberInfoProvider.SetContactsAsDynamic(contactGroup, passList, allList);
            }, PAGE_SIZE);
        }


        /// <summary>
        /// Evaluates dynamic membership of a contact in given contact group.
        /// </summary>
        private bool EvaluateDynamicMembership(ContactGroupInfo contactGroup, ContactInfo contact)
        {
            if (contactGroup == null)
            {
                throw new ArgumentNullException("contactGroup");
            }
            if (contact == null)
            {
                throw new ArgumentNullException("contact");
            }

            var resolver = mCachedResolver.Value;

            // Set parameters of resolver
            resolver.SetNamedSourceData("Contact", contact);
            resolver.SetNamedSourceData("ContactGroup", contactGroup);

            // Resolve contact group condition and determine if current contact passed condition criteria
            return ValidationHelper.GetBoolean(resolver.ResolveMacros(contactGroup.ContactGroupDynamicCondition), false);
        }
    }
}