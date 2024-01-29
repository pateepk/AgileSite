using System.Collections.Generic;
using System.Linq;

using CMS.Activities;
using CMS.Core;
using CMS.Helpers;
using CMS.PortalEngine;
using CMS.PortalEngine.Internal;
using CMS.SiteProvider;

namespace CMS.OnlineForms.Web.UI
{
    /// <summary>
    /// Provides service methods regarding contact and its submitted forms.
    /// </summary>
    internal class ContactSubmittedFormsControllerService : IContactSubmittedFormsControllerService
    {
        private readonly IUILinkProvider mUILinkProvider;

        /// <summary>
        /// Instantiates new instance of <see cref="ContactSubmittedFormsControllerService"/>.
        /// </summary>
        /// <param name="uiLinkProvider">Provides link for an object</param>
        public ContactSubmittedFormsControllerService(IUILinkProvider uiLinkProvider)
        {
            mUILinkProvider = uiLinkProvider;
        }


        /// <summary>
        /// Gets instance of <see cref="ContactSubmittedFormsViewModel"/> for the given <paramref name="contactID"/>. Returns empty list if no activity or form is found for given <paramref name="contactID"/>.
        /// </summary>
        /// <param name="contactID">ID of contact the <see cref="ContactSubmittedFormsViewModel"/> is obtained for</param>
        /// <returns>Instance of <see cref="ContactSubmittedFormsViewModel"/> for the given <paramref name="contactID"/>, or empty list if no activity or form is found</returns>
        public IEnumerable<ContactSubmittedFormsViewModel> GetSubmittedForms(int contactID)
        {
            var activities = GetActivities(contactID);
            var forms = GetForms(activities.Select(activity => activity.ActivityItemID).Distinct().ToList());

            var result = new List<ContactSubmittedFormsViewModel>();
            foreach (var activity in activities)
            {
                var form = forms.FirstOrDefault(x => x.FormID == activity.ActivityItemID);
                if (form != null)
                {
                    result.Add(CreateViewModel(activity, form.FormDisplayName));
                }
            }

            return result.OrderBy(form => form.FormSubmissionDate);
        }


        private List<ActivityInfo> GetActivities(int contactID)
        {
            return ActivityInfoProvider.GetActivities()
                                .WhereEquals("ActivityContactID", contactID)
                                .WhereEquals("ActivityType", PredefinedActivityType.BIZFORM_SUBMIT)
                                .Columns("ActivityItemID", "ActivityCreated", "ActivitySiteID")
                                .ToList();
        }


        private List<BizFormInfo> GetForms(List<int> activitiesId)
        {
            return BizFormInfoProvider.GetBizForms()
                .WhereIn("FormID", activitiesId)
                .Columns("FormID", "FormDisplayName")
                .ToList();
        }


        private ContactSubmittedFormsViewModel CreateViewModel(ActivityInfo activity, string formDisplayName)
        {
            var result = new ContactSubmittedFormsViewModel
			{
                FormDisplayName = formDisplayName,
                FormSubmissionDate = activity.ActivityCreated,
                SiteDisplayName = SiteInfoProvider.GetSiteInfo(activity.ActivitySiteID).DisplayName,
                FormUrl = GetFormURL(activity.ActivityItemID)
            };

            return result;
        }


        private string GetFormURL(int formID)
        {
            var siteId = BizFormInfoProvider.GetBizFormInfo(formID).FormSiteID;
            var site = SiteInfoProvider.GetSiteInfo(siteId);

            var objectDetailLinkParameters = new ObjectDetailLinkParameters
            {
                AllowNavigationToListing = true,
                ObjectIdentifier = formID
            };
            var formLink = mUILinkProvider.GetSingleObjectLink(ModuleName.BIZFORM, "Forms.Properties",objectDetailLinkParameters);
            return URLHelper.GetAbsoluteUrl(formLink, site.DomainName);
        }
    }
}
