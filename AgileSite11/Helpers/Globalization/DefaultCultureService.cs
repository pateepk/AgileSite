namespace CMS.Helpers
{
    /// <summary>
    /// Default <see cref="ICultureService"/> implementation
    /// </summary>
    internal sealed class DefaultCultureService : ICultureService
    {
        /// <summary>
        /// Returns true when culture with given code is defined as UICulture.
        /// </summary>
        public bool IsUICulture(string cultureCode)
        {
            return true;
        }
    }
}