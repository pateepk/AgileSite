using System;
using System.Collections.Generic;
using System.Linq;

using CMS.Core;
using CMS.DataEngine;
using CMS.EventLog;
using CMS.Helpers;
using CMS.LicenseProvider;
using CMS.SiteProvider;

namespace CMS.ContactManagement
{
    /// <summary>
    /// Class for contact group rebuilding.
    /// </summary>
    public class ContactGroupRebuilder
    {
        private readonly IEventLogService mEventLogService;


        /// <summary>
        /// Initializes new instance of <see cref="ContactGroupRebuilder"/>.
        /// </summary>
        public ContactGroupRebuilder()
            :this(Service.Resolve<IEventLogService>())
        {
        }


        internal ContactGroupRebuilder(IEventLogService eventLogService)
        {
            mEventLogService = eventLogService;
        }


        /// <summary>
        /// Rebuilds the whole contact group using the fastest possible rebuilder. This method works synchronously.
        /// </summary>
        /// <param name="contactGroup">Contact group to be rebuilt</param>
        public void RebuildGroup(ContactGroupInfo contactGroup)
        {
            if (contactGroup == null)
            {
                throw new ArgumentNullException("contactGroup");
            }

            if (InsufficientLicense())
            {
                string message = "ContactGroupRebuilder.RebuildGroup";
                mEventLogService.LogEvent(EventType.WARNING, message, LicenseHelper.LICENSE_LIMITATION_EVENTCODE, message);
                return;
            }

            var sqlRebuilder = new ContactGroupSqlRebuilder(contactGroup);
            if (sqlRebuilder.CanBeRebuilt())
            {
                sqlRebuilder.RebuildGroup();
            }
            else
            {
                EventLogProvider.LogWarning("ContactGroupRebuilder", "SLOWMACRO", null, SiteContext.CurrentSiteID,
                    "Contact group '" + contactGroup.ContactGroupDisplayName + "' is rebuilt using memory rebuilder which is significantly slower than SQL rebuilder. " +
                    "Please consider using only macro rules which are translatable to ObjectQuery in your contact " +
                    "group dynamic condition. Please refer to documentation to learn more: " + DocumentationHelper.GetDocumentationTopicUrl(DocumentationLinks.ContactGroups.CUSTOM_MACRO_RECALCULATION_SPEED));

                new ContactGroupMemoryRebuilder().RebuildGroup(contactGroup);
            }
        }


        /// <summary>
        /// Adds or removes specific contactIDs into the contact group.
        /// </summary>
        /// <param name="contactGroup">Contact group to be rebuilded</param>
        /// <param name="contactIDs">Contacts whose membership in the group will be reevaluated</param>
        public void RebuildPartOfContactGroup(ContactGroupInfo contactGroup, ISet<int> contactIDs)
        {
            if (contactGroup == null)
            {
                throw new ArgumentNullException("contactGroup");
            }
            if (contactIDs == null)
            {
                throw new ArgumentNullException("contactIDs");
            }

            if (InsufficientLicense())
            {
                return;
            }

            var sqlRebuilder = new ContactGroupSqlRebuilder(contactGroup);
            if (sqlRebuilder.CanBeRebuilt())
            {
                sqlRebuilder.RebuildPartOfContactGroup(contactIDs);
            }
            else
            {
                new ContactGroupMemoryRebuilder().RebuildPartOfContactGroup(contactGroup, contactIDs);
            }
        }


        private static bool InsufficientLicense()
        {
            return !LicenseHelper.IsFeatureAvailableInBestLicense(FeatureEnum.FullContactManagement, ModuleName.CONTACTMANAGEMENT);
        }
    }
}
