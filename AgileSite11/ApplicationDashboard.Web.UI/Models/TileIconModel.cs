using System.ComponentModel;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace CMS.ApplicationDashboard.Web.UI.Internal
{
    /// <summary>
    /// Data class containing information to display proper icon within the <see cref="ITileModel"/>.
    /// </summary>
    /// <exclude />
    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class TileIconModel
    {
        /// <summary>
        /// Type of the tile icon.
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public TileIconTypeEnum IconType
        {
            get;
            set;
        }


        /// <summary>
        /// CSS class containing the tile icon.
        /// </summary>
        public string IconCssClass
        {
            get;
            set;
        }


        /// <summary>
        /// Path to the icon image.
        /// </summary>
        public string IconImagePath
        {
            get;
            set;
        }
    }
}
