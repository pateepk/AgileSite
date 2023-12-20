using CMS;
using CMS.Base;

[assembly: RegisterImplementation(typeof(ISiteService), typeof(DefaultSiteService), Priority = CMS.Core.RegistrationPriority.Fallback)]

namespace CMS.Base
{
    /// <summary>
    /// Interface for site service
    /// </summary>
    public interface ISiteService
    {
        /// <summary>
        /// Current context site
        /// </summary>
        ISiteInfo CurrentSite
        {
            get;
        }


        /// <summary>
        /// Returns true, if the current context executes on live site
        /// </summary>
        bool IsLiveSite
        {
            get;
        }
    }
}
