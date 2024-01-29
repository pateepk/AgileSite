using System.Collections.Specialized;

using CMS.DataEngine;

namespace CMS.ContactManagement.Web.UI.Internal
{
    /// <summary>
    /// Provides contract for obtaining source data required by contact demographics control.
    /// </summary>
    public interface IContactDemographicsDataRetriever
    {
        /// <summary>
        /// Gets contact object query suitable for contact demographics control.
        /// </summary>
        /// <param name="parameters">Collection of parameters specifying further the data to be loaded</param>
        ObjectQuery<ContactInfo> GetContactObjectQuery(NameValueCollection parameters);


        /// <summary>
        /// Gets caption that can be used as header of the demographics page, in breadcrumbs and in the page title.
        /// </summary>
        string GetCaption();
    }
}