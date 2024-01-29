using System;

using CMS.SiteProvider;

namespace CMS.ContactManagement
{
    /// <summary>
    /// Class for automation action that modifies relation between contact group and contact
    /// </summary>
    public class ContactGroupAction : ContactAutomationAction
    {
        #region "Parameters"

        /// <summary>
        /// Contact group identifier.
        /// </summary>
        public virtual string ContactGroupName
        {
            get
            {
                return GetResolvedParameter("ContactGroupName", string.Empty);
            }
        }


        /// <summary>
        /// Gets current action - 0 for ADD, 1 for REMOVE contact from contact group.
        /// </summary>
        public virtual int ContactAction
        {
            get
            {
                return GetResolvedParameter("ContactAction", 0);
            }
        }

        #endregion


        /// <summary>
        /// Executes current action
        /// </summary>
        public override void Execute()
        {
            if (Contact != null)
            {
                ContactGroupInfo group = ContactGroupInfoProvider.GetContactGroupInfo(ContactGroupName);

                if (group != null)
                {
                    if (ContactAction == 1)
                    {
                        // Remove from contact group
                        ContactGroupMemberInfoProvider.DeleteContactGroupMemberInfo(group.ContactGroupID, Contact.ContactID, ContactGroupMemberTypeEnum.Contact);
                    }
                    else
                    {
                        // Add to contact group
                        ContactGroupMemberInfoProvider.SetContactGroupMemberInfo(group.ContactGroupID, Contact.ContactID, ContactGroupMemberTypeEnum.Contact, MemberAddedHowEnum.Manual);
                    }
                }
            }
        }
    }
}
