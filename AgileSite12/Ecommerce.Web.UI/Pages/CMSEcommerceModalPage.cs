using CMS.Core;
using CMS.DataEngine;
using CMS.UIControls;

namespace CMS.Ecommerce.Web.UI
{
    /// <summary>
    /// Base page for the CMS Ecommerce modal pages to apply global settings to the pages.
    /// </summary>
    [Security(Resource = ModuleName.ECOMMERCE, ResourceSite = true)]
    [CheckLicence(FeatureEnum.Ecommerce)]
    public abstract class CMSEcommerceModalPage : CMSModalPage
    {
    }
}