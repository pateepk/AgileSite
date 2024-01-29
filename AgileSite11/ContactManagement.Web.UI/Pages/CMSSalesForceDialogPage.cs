using CMS.DataEngine;
using CMS.Helpers;
using CMS.LicenseProvider;
using CMS.UIControls;

namespace CMS.ContactManagement.Web.UI
{

    /// <summary>
    /// Represents a modal dialog web page of SalesForce module.
    /// </summary>
    public class CMSSalesForceDialogPage : CMSModalPage
    {

        #region "Private methods"

        /// <summary>
        /// Checks the license and redirects the user to a special page when the license does not include Salesforce.com Connector.
        /// </summary>
        protected void AuthorizeRequest()
        {
            LicenseHelper.CheckFeatureAndRedirect(RequestContext.CurrentDomain, FeatureEnum.SalesForce);
        }

        #endregion

    }

}