using System;
using System.ComponentModel;
using System.Linq;
using System.Text;

using Newtonsoft.Json;

namespace CMS.ApplicationDashboard.Web.UI.Internal
{
    /// <summary>
    /// User dashboard setting.
    /// Represents one tile on dashboard. Either application or single object.
    /// </summary>
    /// <exclude />
    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class UserDashboardSetting
    {
        /// <summary>
        /// GUID of application.
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
        /// Object site name.
        /// </summary>
        [JsonProperty("objectSiteName", NullValueHandling = NullValueHandling.Ignore)]
        public string ObjectSiteName
        {
            get; 
            set;
        }
    }
}
