using CMS.Base.Web.UI;
using CMS.DataEngine;

namespace CMS.WebAnalytics.Web.UI.Internal
{
    /// <summary>
    /// Provides method for filtering the given <see cref="ObjectQuery"/> before new <see cref="BaseSelectorViewModel"/> is created.
    /// </summary>
    public interface ICampaignConversionItemFilter
    {
        /// <summary>
        /// Filters given <paramref name="objectQuery"/> and return the result.
        /// </summary>
        /// <param name="objectQuery">Query to be filtered</param>
        /// <returns>Filtered <paramref name="objectQuery"/></returns>
        ObjectQuery Filter(ObjectQuery objectQuery);
    }
}