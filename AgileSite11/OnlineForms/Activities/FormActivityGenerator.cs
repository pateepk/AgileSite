using System.ComponentModel;

using CMS.Activities;
using CMS.Base;
using CMS.Core;

namespace CMS.OnlineForms
{
    /// <summary>
    /// Contains methods for generating sample activities data.
    /// </summary>
    /// <exclude />
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class FormActivityGenerator
    {
        private readonly IActivityLogService mActivityLogService = Service.Resolve<IActivityLogService>();


        /// <summary>
        /// Generates form submit <see cref="IActivityInfo"/> for given <paramref name="formItem"/>.
        /// </summary>
        /// <param name="formItem">Form item to generate activity for</param>
        /// <param name="document">Document to generate activity for</param>
        /// <param name="contactID">ID of contact the activity is generated for</param>
        /// <param name="siteId">Site to generate activity in</param>
        public void GenerateFormSubmitActivity(BizFormItem formItem, ITreeNode document, int contactID, int siteId)
        {
            var activityInitializer = new FormSubmitActivityInitializer(formItem, document)
                                            .WithContactId(contactID)
                                            .WithSiteId(siteId);

            mActivityLogService.LogWithoutModifiersAndFilters(activityInitializer);
        }
    }
}
