using System;

namespace CMS.Helpers
{
    /// <summary>
    /// Constants for the system cookie levels
    /// </summary>
    public static class CookieLevel
    {
        /// <summary>
        /// Level value to disable all cookies "-1000"
        /// </summary>
        public const int None = -1000;

        /// <summary>
        /// Level value for the system cookies required to determine the user cookie level "-100"
        /// </summary>
        public const int System = -100;

        /// <summary>
        /// Level value for the essential cookies (required to run the system) "0"
        /// </summary>
        public const int Essential = 0;

        /// <summary>
        /// Level value for the cookies required for editors (for the editing UI to work properly) "100"
        /// </summary>
        public const int Editor = 100;

        /// <summary>
        /// Level value for normal visitor cookies (not necessarily required to run the system) "200"
        /// </summary>
        public const int Visitor = 200;

        /// <summary>
        /// Level value to allow all available cookies "1000"
        /// </summary>
        public const int All = 1000;
    }
}
