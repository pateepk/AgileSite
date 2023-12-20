using CMS;
using CMS.Core;
using CMS.OnlineMarketing;

[assembly: RegisterImplementation(typeof(ICachedABTestManager), typeof(CachedABTestManager), Priority = RegistrationPriority.SystemDefault)]

namespace CMS.OnlineMarketing
{
    /// <summary>
    /// Acts as a decorator for <see cref="IABTestManager"/> and adds caching to methods.
    /// </summary>
    public interface ICachedABTestManager : IABTestManager
    {
    }
}
