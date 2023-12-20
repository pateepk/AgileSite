namespace CMS.Core
{
    /// <summary>
    /// Enumeration of possible lifestyles of a service registered via <see cref="RegisterImplementationAttribute"/>.
    /// </summary>
    public enum Lifestyle
    {
        /// <summary>
        /// Singleton lifestyle.
        /// </summary>
        Singleton,


        /// <summary>
        /// Transient lifestyle.
        /// </summary>
        Transient
    }
}
