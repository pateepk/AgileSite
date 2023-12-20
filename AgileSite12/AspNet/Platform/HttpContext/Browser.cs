using System.Web;

using CMS.Base;

namespace CMS.AspNet.Platform
{
    /// <summary>
    /// Wrapper over <see cref="HttpBrowserCapabilitiesBase"/> object implementing <see cref="IBrowser"/> interface.
    /// </summary>
    internal class BrowserImpl : IBrowser
    {
#pragma warning disable BH1007 // 'Request.Browser' should not be used. Use 'BrowserHelper.GetBrowser()' instead.

        private readonly HttpBrowserCapabilitiesBase mBrowser;


        public BrowserImpl(HttpBrowserCapabilitiesBase browser)
        {
            mBrowser = browser;
        }


        public string this[string key] => mBrowser[key];


        public bool Crawler => mBrowser.Crawler;


        public int MajorVersion => mBrowser.MajorVersion;


        public double MinorVersion => mBrowser.MinorVersion;


        public string Version => mBrowser.Version;


        public bool IsMobileDevice => mBrowser.IsMobileDevice;


        public bool Win32 => mBrowser.Win32;


        string IBrowser.Browser => mBrowser.Browser;

#pragma warning restore BH1007 // 'Request.Browser' should not be used. Use 'BrowserHelper.GetBrowser()' instead.
    }
}
