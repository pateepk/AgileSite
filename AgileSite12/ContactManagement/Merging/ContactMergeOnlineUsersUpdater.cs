using System.Collections.Generic;
using System.Linq;

using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Membership;

namespace CMS.ContactManagement
{
    internal class ContactMergeOnlineUsersUpdater : IContactMergeOnlineUsersUpdater
    {
        private const string CONTACT_ID_COLUMN_NAME = "SessionContactID";

        internal ILicenseService mLicenseService;
        private readonly ISettingsService mSettingsService;


        public ContactMergeOnlineUsersUpdater(ISettingsService settingsService)
        {
            mLicenseService = ObjectFactory<ILicenseService>.StaticSingleton();
            mSettingsService = settingsService;
        }


        public void Update(ContactInfo sourceContact, ContactInfo targetContact)
        {
            if (!IsApplicableForTargetContact(targetContact))
            {
                return;
            }

            var query = new DataQuery(OnlineUserInfo.OBJECT_TYPE, QueryName.GENERALUPDATE).WhereEquals(CONTACT_ID_COLUMN_NAME, sourceContact.ContactID);
            var updateParameters = new UpdateQueryExpression(new Dictionary<string, object> { { CONTACT_ID_COLUMN_NAME, targetContact.ContactID } });
            var valuesExpression = query.IncludeDataParameters(updateParameters.Parameters, updateParameters.GetExpression());
            query.EnsureParameters().AddMacro(QueryMacros.VALUES, valuesExpression);
            query.Execute();
        }


        private bool IsApplicableForTargetContact(ContactInfo targetContact)
        {
            return mSettingsService["CMSSessionUseDBRepository"].ToBoolean(false) &&
                   mLicenseService.IsFeatureAvailable(FeatureEnum.OnlineUsers, RequestContext.CurrentDomain) &&
                   !OnlineUserAlreadyExistsForContact(targetContact);
        }


        private bool OnlineUserAlreadyExistsForContact(ContactInfo contact)
        {
            return GetOnlineUserForContact(contact) != null;
        }


        private OnlineUserInfo GetOnlineUserForContact(ContactInfo contact)
        {
            return new ObjectQuery<OnlineUserInfo>().WhereEquals(CONTACT_ID_COLUMN_NAME, contact.ContactID).FirstOrDefault();
        }
    }
}
