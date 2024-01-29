using System;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace CMS.ApplicationDashboard.Web.UI.Internal
{
    /// <summary>
    /// Wrapper for single tile on the dashboard.
    /// </summary>
    /// <exclude />
    [EditorBrowsable(EditorBrowsableState.Never)]
    public interface ITileModel
    {
        /// <summary>
        /// Represents type of the tile.
        /// </summary>
        TileModelTypeEnum TileModelType
        {
            get;
        }


        /// <summary>
        /// Determines whether the tile should be visible on dashboard or not (single object form another site, insufficient permission, license, etc.)
        /// </summary>
        bool IsVisible
        {
            get;
        }
    }
}
