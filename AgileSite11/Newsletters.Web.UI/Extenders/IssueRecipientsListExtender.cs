using System.Collections.Generic;
using System.Linq;

using CMS;
using CMS.Base.Web.UI;
using CMS.Helpers;
using CMS.UIControls;
using CMS.Newsletters.Web.UI;
using CMS.Base;
using CMS.ContactManagement;
using CMS.Core;
using CMS.FormEngine.Web.UI;
using CMS.Localization;
using CMS.Newsletters.Web.UI.Internal;
using CMS.SiteProvider;

[assembly: RegisterCustomClass("IssueRecipientsListExtender", typeof(IssueRecipientsListExtender))]

namespace CMS.Newsletters.Web.UI
{
    /// <summary>
    /// Extends Unsubscription listing unigrid.
    /// </summary>
    public class IssueRecipientsListExtender : ControlExtender<UniGrid>
    {
        private ObjectTransformationDataProvider contactFullNameDataProvider;
        private ObjectTransformationDataProvider contactGroupsDataProvider;
        private ObjectTransformationDataProvider statusDataProvider;
        private IssueInfo issue;
        private readonly IssueRecipientsListService mIssueRecipientsListService = new IssueRecipientsListService();


        /// <summary>
        /// Initializes extender.
        /// </summary>
        public override void OnInit()
        {
            issue = UIContext.Current.EditedObject as IssueInfo;

            contactFullNameDataProvider = new ObjectTransformationDataProvider();
            contactFullNameDataProvider.SetDefaultDataHandler(GetFullNamesDataHandler);

            contactGroupsDataProvider = new ObjectTransformationDataProvider();
            contactGroupsDataProvider.SetDefaultDataHandler(GetCountsDataHandler);

            statusDataProvider = new ObjectTransformationDataProvider();
            statusDataProvider.SetDefaultDataHandler(GetStatusesDataHandler);

            if ((Control != null) && (issue != null))
            {
                Control.OnExternalDataBound += Control_OnExternalDataBound;
            }
        }


        /// <summary>
        /// Returns dictionary of contacts' full name through issue. Key of the dictionary is the ID of contact.
        /// </summary>
        /// <param name="type">Object type (ignored).</param>
        /// <param name="contactIds">IDs of contacts for which full name is to be fill with.</param>
        protected IGeneralIndexable<int, IDataContainer> GetFullNamesDataHandler(string type, IEnumerable<int> contactIds)
        {
            var contactsWithFullName = mIssueRecipientsListService.GetContactsWithFullName(contactIds);
            var result = new SafeDictionary<int, IDataContainer>();

            foreach (var contact in contactsWithFullName)
            {
                var dataContainer = new DataContainer();
                dataContainer["ContactFullName"] = contact.Value;

                result[contact.Key] = dataContainer;
            }

            return result;
        }


        /// <summary>
        /// Returns dictionary of contact groups per contact through issue contact group assignment. Key of the dictionary is the ID of contact.
        /// </summary>
        /// <param name="type">Object type (ignored).</param>
        /// <param name="contactIds">IDs of contacts for which status is to be filled with.</param>
        protected IGeneralIndexable<int, IDataContainer> GetStatusesDataHandler(string type, IEnumerable<int> contactIds)
        {
            var idList = contactIds.ToList();
            var result = new SafeDictionary<int, IDataContainer>();

            // First mark all input IDs as marketable
            foreach (var id in idList)
            {
                result[id] = GetDataContainer(EmailRecipientStatusEnum.Marketable);
            }

            // Mark globally unsubscribed
            var unsubscribeProvider = Service.Resolve<IUnsubscriptionProvider>();

            var globalUnsubscribedContactsIds = ContactInfoProvider.GetContacts()
                                                                   .WhereIn("ContactID", idList)
                                                                   .WhereIn("ContactEmail", unsubscribeProvider.GetUnsubscriptionsFromAllNewsletters().Column("UnsubscriptionEmail"))
                                                                   .Column("ContactID")
                                                                   .GetListResult<int>();
            foreach (var id in globalUnsubscribedContactsIds)
            {
                result[id] = GetDataContainer(EmailRecipientStatusEnum.OptedOut);
            }

            // Mark bounced
            var bounceLimit = NewsletterHelper.BouncedEmailsLimit(SiteContext.CurrentSiteName);
            if (bounceLimit > 0)
            {
                var bounced = mIssueRecipientsListService.GetBouncedContactsIds(idList, bounceLimit);

                foreach (var id in bounced)
                {
                    result[id] = GetDataContainer(EmailRecipientStatusEnum.Bounced);
                }
            }

            return result;
        }


        private IDataContainer GetDataContainer(EmailRecipientStatusEnum value)
        {
            string resourceString = string.Format("emailmarketing.ui.status.{0}", value);
            bool isMarketable = value == EmailRecipientStatusEnum.Marketable;

            var container = new DataContainer();
            container.SetValue("Value", LocalizationHelper.GetString(resourceString));
            container.SetValue("IsMarketable", isMarketable);
            return container;
        }


        /// <summary>
        /// Returns dictionary of contact groups per contact through issue contact group assignment. Key of the dictionary is the ID of contact.
        /// </summary>
        /// <param name="type">Object type (ignored).</param>
        /// <param name="contactIds">IDs of contacts for which count of contact groups is to be filled with.</param>
        protected IGeneralIndexable<int, IDataContainer> GetCountsDataHandler(string type, IEnumerable<int> contactIds)
        {
            var contactsAndGroups = mIssueRecipientsListService.GetContactsAndContactGroups(contactIds, issue.IssueID);

            var contactGroupsByContactId =
                contactsAndGroups
                    .GroupBy(item => item.Item1);

            var result =
                contactGroupsByContactId
                    .ToDictionary(
                        byId => byId.Key,
                        byId => new EnumerableDataContainer<string>(byId.Select(item => item.Item2).ToList())
                    );

            return new SafeDictionary<int, IDataContainer>(result);
        }


        private object Control_OnExternalDataBound(object sender, string sourceName, object parameter)
        {
            switch (sourceName)
            {
                case "name":
                    {
                        var contactId = ValidationHelper.GetInteger(parameter, 0);

                        return new ObjectTransformation
                        {
                            ObjectType = ContactInfo.OBJECT_TYPE,
                            ObjectID = contactId,
                            DataProvider = contactFullNameDataProvider,
                            Transformation = "{% ContactFullName %}"
                        };
                    }

                case "groups":
                    {
                        var contactId = ValidationHelper.GetInteger(parameter, 0);

                        return new ObjectTransformation
                        {
                            ObjectType = IssueContactGroupInfo.OBJECT_TYPE,
                            ObjectID = contactId,
                            DataProvider = contactGroupsDataProvider,
                            TransformationName = "~/CMSModules/Newsletters/Controls/RecipientContactGroups.ascx"
                        };
                    }

                case "status":
                    {
                        var contactId = ValidationHelper.GetInteger(parameter, 0);

                        return new ObjectTransformation
                        {
                            ObjectType = IssueContactGroupInfo.OBJECT_TYPE,
                            ObjectID = contactId,
                            DataProvider = statusDataProvider,
                            EncodeOutput = false,
                            Transformation = "<span class=\"tag {% IsMarketable ? \"tag-active\" : \"tag-incomplete\" %}\">{% Value %}</span>"
                        };
                    }
            }

            return parameter;
        }
    }
}