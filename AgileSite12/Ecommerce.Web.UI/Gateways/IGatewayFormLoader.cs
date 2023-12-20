namespace CMS.Ecommerce.Web.UI
{
    /// <summary>
    /// Provides method for loading control from virtual path.
    /// </summary>
    public interface IGatewayFormLoader
    {
        /// <summary>
        /// Returns instance of <see cref="CMSPaymentGatewayForm"/> loaded from given <paramref name="path"/>
        /// </summary>
        CMSPaymentGatewayForm LoadFormControl(string path);
    }
}