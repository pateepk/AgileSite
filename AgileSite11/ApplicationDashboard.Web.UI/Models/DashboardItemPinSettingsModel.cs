using System;
using System.ComponentModel;
using System.Linq;
using System.Text;

using Newtonsoft.Json;

namespace CMS.ApplicationDashboard.Web.UI.Internal
{
    /// <summary>
    /// Represents pin settings of dashboard item (application or single object). 
    /// </summary>
    /// <exclude />
    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class DashboardItemPinSettingsModel
    {
        /// <summary>
        /// GUID of application
        /// </summary>
        [JsonProperty("applicationGuid")]
        public Guid ApplicationGuid
        {
            get;
            set;
        }


        /// <summary>
        /// UI element GUID
        /// </summary>
        [JsonProperty("elementGuid", NullValueHandling = NullValueHandling.Ignore)]
        public Guid? ElementGuid
        {
            get;
            set;
        }


        /// <summary>
        /// Object code name
        /// </summary>
        [JsonProperty("objectName", NullValueHandling = NullValueHandling.Ignore)]
        public string ObjectName
        {
            get;
            set;
        }


        /// <summary>
        /// Object type
        /// </summary>
        [JsonProperty("objectType", NullValueHandling = NullValueHandling.Ignore)]
        public string ObjectType
        {
            get;
            set;
        }


        /// <summary>
        /// Object site name
        /// </summary>
        [JsonProperty("objectSiteName", NullValueHandling = NullValueHandling.Ignore)]
        public string ObjectSiteName
        {
            get;
            set;
        }


        /// <summary>
        /// Determines whether the pin is in pinned state or not.
        /// </summary>
        [JsonProperty("isPinned", NullValueHandling = NullValueHandling.Ignore)]
        public bool IsPinned
        {
            get;
            set;
        }
    }
}
