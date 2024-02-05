using System;

using CMS.Helpers;

namespace CMS.DeviceProfiles
{
    /// <summary>
    /// Provides extension methods for <see cref="CurrentDevice" />.
    /// </summary>
    public static class CurrentDeviceExtensions
    {
        /// <summary>
        /// Extends <see cref="CurrentDevice"/> for detection if <paramref name="device" /> is mobile.
        /// </summary>
        /// <param name="device">Device to extend.</param>
        /// <remarks>
        /// Uses the default .NET Framework mobile device recognition.
        /// </remarks>
        /// <returns>Returns true when <paramref name="device" /> is a mobile device.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="device"/> or is null.</exception>
        public static bool IsMobile(this CurrentDevice device)
        {
            if (device == null)
            {
                throw new ArgumentNullException(nameof(device));
            }

            return ValidationHelper.GetBoolean(device.Data["isMobileDevice"], false);
        }
    }
}
