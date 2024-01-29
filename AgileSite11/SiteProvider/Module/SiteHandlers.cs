using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CMS.Base;
using CMS.DataEngine;
using CMS.EventLog;
using CMS.Localization;

namespace CMS.SiteProvider
{
    /// <summary>
    /// Site module event handlers
    /// </summary>
    internal class SiteHandlers
    {
        /// <summary>
        /// Initializes the site module handlers
        /// </summary>
        public static void Init()
        {
            CultureInfo.TYPEINFO.Events.Update.After += Culture_Change_After;
            CultureInfo.TYPEINFO.Events.Insert.After += Culture_Change_After;
            CultureInfo.TYPEINFO.Events.Delete.After += Culture_Change_After;
        }


        /// <summary>
        /// Fires after culture change - Clears cultures assigned to site
        /// </summary>
        private static void Culture_Change_After(object sender, ObjectEventArgs e)
        {
            CultureSiteInfoProvider.ClearSiteCultures(true);
        }
    }
}
