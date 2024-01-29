using System;
using System.ComponentModel;
using System.Linq;
using System.Text;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace CMS.ApplicationDashboard.Web.UI.Internal
{
    /// <summary>
    /// Represents type of the dashboard tile.
    /// </summary>
    /// <exclude />
    [EditorBrowsable(EditorBrowsableState.Never)]
    [JsonConverter(typeof(StringEnumConverter))]
	public enum TileModelTypeEnum
	{
        /// <summary>
        /// Single application tile without live data provider.
        /// </summary>
		ApplicationTileModel,

        /// <summary>
        /// Single application tile with live data provider.
        /// </summary>
        ApplicationLiveTileModel,

        /// <summary>
        /// Single object tile without live data provider.  
        /// </summary>
		SingleObjectTileModel,
	}
}
