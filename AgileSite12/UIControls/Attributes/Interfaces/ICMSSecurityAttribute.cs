namespace CMS.UIControls
{
    /// <summary>
    /// Security attribute interface.
    /// </summary>
    public interface ICMSSecurityAttribute : ICMSAttribute
    {
        /// <summary>
        /// Does the security check
        /// </summary>
        /// <remarks>
        /// This method can perform redirect or raise an exception if check fails.
        /// </remarks>
        /// <param name="page">Page for which is check performed</param>
        void Check(CMSPage page);
    }
}