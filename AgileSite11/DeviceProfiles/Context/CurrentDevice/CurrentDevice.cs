using CMS.Base;

namespace CMS.DeviceProfiles
{
    /// <summary>
    /// Class holding the current device information.
    /// </summary>
    public class CurrentDevice : AbstractDataContainer<CurrentDevice>
    {
        private ISimpleDataContainer mData;


        /// <summary>
        /// Data container of all available device properties. 
        /// Use the property names available in the <see cref="System.Web.HttpBrowserCapabilities"/> class as a key to obtain the desired device information.
        /// </summary>
        [RegisterColumn]
        public ISimpleDataContainer Data
        {
            get
            {
                if (mData == null)
                {
                    mData = new HttpBrowserCapabilitiesAdapter();
                }

                return mData;
            }
        }
    }
}
