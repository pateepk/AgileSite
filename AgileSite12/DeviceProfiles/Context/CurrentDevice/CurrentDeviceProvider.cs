namespace CMS.DeviceProfiles
{
    /// <summary>
    /// Provides methods for retrieving the current device.
    /// </summary>
    internal sealed class CurrentDeviceProvider : ICurrentDeviceProvider
    {
        /// <summary>
        /// Gets current device information.
        /// </summary>
        /// <returns>Return current device information.</returns>
        public CurrentDevice GetCurrentDevice()
        {
            return new CurrentDevice();
        }
    }
}
