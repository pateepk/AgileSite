using System;
using System.Collections.Specialized;

using CMS.Base;
using CMS.ContactManagement;
using CMS.ContactManagement.Web.UI.Internal;
using CMS.Core;
using CMS.DataEngine;

namespace CMS.Newsletters.Web.UI
{
    internal class OpenedEmailContactDemographicsDataRetriever : EmailContactDemographicsDataRetrieverBase, IContactDemographicsDataRetriever
    {
        private readonly ILocalizationService mLocalizationService;


        public OpenedEmailContactDemographicsDataRetriever(ILocalizationService localizationService)
        {
            mLocalizationService = localizationService;
        }


        public ObjectQuery<ContactInfo> GetContactObjectQuery(NameValueCollection parameters)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            var issueIds = GetIssueIDs(parameters.Get("issueID").ToInteger(0));

            return ContactInfoProvider.GetContacts()
                                      .WhereIn("ContactEmail", OpenedEmailInfoProvider.GetOpenedEmails().WhereIn("OpenedEmailIssueID", issueIds)
                                                                                      .Column("OpenedEmailEmail"));
        }


        public string GetCaption()
        {
            return mLocalizationService.GetString("newsletter.issue.demographics.openedemail");
        }
    }
}