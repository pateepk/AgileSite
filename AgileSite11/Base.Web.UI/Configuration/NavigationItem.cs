using CMS.Base.Web.UI.JsonConverters;
using CMS.Helpers.JsonConverters;

using Newtonsoft.Json;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Base class for navigation items. (Tab items, menu items etc.)
    /// </summary>
    public class NavigationItem
    {
        #region "Properties"

        /// <summary>
        /// Specifies the the caption of the item.
        /// Sample value: "Caption string"
        /// (Has higher priority than the resource string.)
        /// </summary>
        [JsonConverter(typeof(ResourceStringConverter))]
        public virtual string Text
        {
            get;
            set;
        }


        /// <summary>
        /// Specifies the the tooltip of the item (string.empty: empty, null: usually copies caption or custom string).
        /// (Has higher priority than the tooltip resource string.)
        /// </summary>
        public virtual string Tooltip
        {
            get;
            set;
        }


        /// <summary>
        /// CSS class of the item.
        /// </summary>
        public virtual string CssClass
        {
            get;
            set;
        }


        /// <summary>
        /// The JavaScript executed on client click event.
        /// </summary>
        public virtual string OnClientClick
        {
            get;
            set;
        }


        /// <summary>
        /// The URL for redirection.
        /// </summary>
        [JsonConverter(typeof(URLConverter))]
        public virtual string RedirectUrl
        {
            get;
            set;
        }

        #endregion
    }
}
