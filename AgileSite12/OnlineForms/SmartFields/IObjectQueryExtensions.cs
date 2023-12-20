using System;
using System.Linq;

using CMS.DataEngine;

namespace CMS.OnlineForms
{
    /// <summary>
    /// Extension methods for <see cref="ObjectQuery{BizFormItem}"/>
    /// </summary>
    public static class IObjectQueryExtensions
    {
        /// <summary>
        /// Returns true when form item for combination given <paramref name="query"/> and given <paramref name="contactGuid"/> exists.
        /// </summary>
        /// <param name="query">Object query of form items.</param>
        /// <param name="formInfo">Form description.</param>
        /// <param name="contactGuid">Guid of contact.</param>
        /// <param name="item">When item exists, it's assigned to parameter.</param>
        /// <returns>True when item exists, false otherwise.</returns>
        public static bool HasExistingItemForContact(this ObjectQuery<BizFormItem> query, BizFormInfo formInfo, Guid? contactGuid, out BizFormItem item)
        {
            item = GetExistingItemForContact(query, formInfo, contactGuid);

            return item != null;
        }


        /// <summary>
        /// Returns existing form item from given <paramref name="query"/> for given <paramref name="contactGuid"/>.
        /// </summary>
        /// <param name="query">Object query of form items.</param>
        /// <param name="formInfo">Form description.</param>
        /// <param name="contactGuid">Guid of contact.</param>
        /// <returns>Instance of <see cref="BizFormItem"/> when such item exists, false otherwise.</returns>
        public static BizFormItem GetExistingItemForContact(this ObjectQuery<BizFormItem> query, BizFormInfo formInfo, Guid? contactGuid)
        {
            if (contactGuid == null || !SmartFieldLicenseHelper.HasLicense() || !formInfo.Form.ContainsSmartField())
            {
                return null;
            }

            return query.WhereEquals(SmartFieldConstants.CONTACT_COLUMN_NAME, contactGuid).FirstOrDefault();
        }
    }
}
