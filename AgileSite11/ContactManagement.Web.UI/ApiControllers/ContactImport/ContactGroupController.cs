using System;
using System.Linq;

using CMS.ContactManagement.Web.UI;
using CMS.Core;
using CMS.Helpers;
using CMS.Membership;
using CMS.SiteProvider;
using CMS.WebApi;

[assembly: RegisterCMSApiController(typeof(ContactGroupController))]

namespace CMS.ContactManagement.Web.UI
{
    /// <summary>
    /// Handles posting of contact group to the server.
    /// </summary>
    /// <exclude />
    [ContactImportExceptionHandling]
    public sealed class ContactGroupController : CMSApiController
    {
        /// <summary>
        /// Creates contact group by given name on specified site.
        /// </summary>
        /// <param name="contactGroupName">Contact group name. Will be used as Display name and also as a code name.</param>
        public dynamic Post(string contactGroupName)
        {
            if (string.IsNullOrEmpty(contactGroupName))
            {
                throw new ContactImportException("Contact group name not specified", "om.contact.importcsv.segmentation.nocontactgroupname");
            }

            if (!IsAuthorized(SiteContext.CurrentSite))
            {
                throw new UnauthorizedAccessException();
            }

            // Create contact group - if code name not unique, add _x to code name
            var group = new ContactGroupInfo
            {
                ContactGroupDisplayName = contactGroupName,
                ContactGroupName = ValidationHelper.GetCodeName(contactGroupName, "contactgroup", 200, useUnicode: false)
            };
            group.ContactGroupName = group.Generalized.GetUniqueCodeName();
            ContactGroupInfoProvider.SetContactGroupInfo(group);

            return new
            {
                ContactGroupGuid = group.ContactGroupGUID
            };
        }


        private bool IsAuthorized(SiteInfo site)
        {
            var user = MembershipContext.AuthenticatedUser;
            if (user == null)
            {
                return false;
            }

            string[] requiredPermissions =
            {
                "Read",
                "Modify"
            };

            return requiredPermissions.All(permission => user.IsAuthorizedPerResource(ModuleName.CONTACTMANAGEMENT, permission, site.SiteName));
        }
    }
}