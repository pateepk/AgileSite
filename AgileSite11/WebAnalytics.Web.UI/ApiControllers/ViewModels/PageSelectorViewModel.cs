using System;

using CMS.Base.Web.UI;
using CMS.DataEngine;
using CMS.DocumentEngine;

using Newtonsoft.Json;

namespace CMS.WebAnalytics.Web.UI
{
    /// <summary>
    /// View model for page selector.
    /// </summary>
    public class PageSelectorViewModel : BaseSelectorViewModel
    {

        /// <summary>
        /// Page path.
        /// </summary>
        [JsonProperty(PropertyName = "path")]
        public string Path
        {
            get;
            set;
        }


        /// <summary>
        /// Page type icon markup.
        /// </summary>
        [JsonProperty(PropertyName = "icon")]
        public string Icon
        {
            get;
            set;
        }
    }
}
