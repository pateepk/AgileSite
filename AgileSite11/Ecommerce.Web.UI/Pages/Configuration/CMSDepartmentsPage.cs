namespace CMS.Ecommerce.Web.UI
{
    /// <summary>
    /// Base page for the E-commerce departments to apply global settings to the pages.
    /// </summary>
    public class CMSDepartmentsPage : CMSEcommerceConfigurationPage
    {
        /// <summary>
        /// Creates child controls and sets global objects key name.
        /// </summary>
        protected override void CreateChildControls()
        {
            // Set object type for allowing global departments
            SiteOrGlobalObjectType = DepartmentInfo.OBJECT_TYPE;

            base.CreateChildControls();
        }
    }
}