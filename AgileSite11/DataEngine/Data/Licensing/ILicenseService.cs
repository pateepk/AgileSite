namespace CMS.DataEngine
{
    /// <summary>
    /// Interface for check license 
    /// </summary>
    public interface ILicenseService
    {
        /// <summary>
        /// Checks the license based on feature and perform action based on given arguments
        /// </summary>
        /// <param name="feature">Feature to check</param>
        /// <param name="domain">Domain to check. If null, function tries to get domain from HttpContext</param>
        /// <param name="throwError">Indicates whether throw error after false check</param>
        bool CheckLicense(FeatureEnum feature, string domain = null, bool throwError = true);


        /// <summary>
        /// Checks whether the given <paramref name="feature"/> is available on given <paramref name="domain"/>.
        /// </summary>
        /// <remarks>
        /// If the method is called outside the request and <paramref name="domain"/> is <c>null</c>, check is performed against the best license available on the instance.
        /// </remarks>
        /// <param name="feature">Feature to be checked</param>
        /// <param name="domain">Domain the <paramref name="feature"/> is checked against. If <c>null</c>, the domain will be obtained from the context</param>
        /// <returns><c>true</c> if the <paramref name="feature"/> is available on given <paramref name="domain"/>; otherwise, <c>false</c></returns>
        bool IsFeatureAvailable(FeatureEnum feature, string domain = null);
    }
}
