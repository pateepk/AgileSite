using CMS;
using CMS.Core;
using CMS.DeviceProfiles;

[assembly: RegisterImplementation(typeof(ICurrentDeviceProvider), typeof(CurrentDeviceProvider), Priority = RegistrationPriority.SystemDefault, Lifestyle = Lifestyle.Transient)]

namespace CMS.DeviceProfiles
{
    /// <summary>
    /// Interface for the provider retrieving the current device.
    /// </summary>
    public interface ICurrentDeviceProvider
    {
        /// <summary>
        /// Gets the current device information.
        /// </summary>
        /// <returns>Return the current device information.</returns>
        CurrentDevice GetCurrentDevice();
    }
}
