using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace CMS.ApplicationDashboard.Web.UI.Internal
{
    /// <summary>
    /// Represents single live application tile.
    /// </summary>
    /// <exclude />
    [KnownType(typeof(ApplicationLiveTileModel))]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class ApplicationLiveTileModel : ITileModel
    {
        /// <summary>
        /// Name of the application which will be displayed as a title of a tile.
        /// </summary>
        public string DisplayName
        {
            get;
            set;
        }


        /// <summary>
        /// CSS class of the tile anchor.
        /// </summary>
        public string ListItemCssClass
        {
            get;
            set;
        }


        /// <summary>
        /// Model defining the way the tile icon should be displayed. Can be either the image, or CSS class defined in style sheets.
        /// </summary>
        public TileIconModel TileIcon
        {
            get;
            set;
        }


        /// <summary>
        /// URL address of an application. After clicking on a tile, user will be redirected to this address.
        /// </summary>
        public string Path
        {
            get;
            set;
        }


        /// <summary>
        /// Unique identifier of the application.
        /// </summary>
        public Guid ApplicationGuid
        {
            get;
            set;
        }

        /// <summary>
        /// Number value that will be displayed on a live tile.
        /// </summary>
        public decimal Value
        {
            get;
            set;
        }


        /// <summary>
        /// Text describing the meaning of the number on the live site.
        /// </summary>
        public string Description
        {
            get;
            set;
        }


        /// <summary>
        /// Represents type of the tile.
        /// </summary>
        public TileModelTypeEnum TileModelType
        {
            get
            {
                return TileModelTypeEnum.ApplicationLiveTileModel;
            }
        }


        /// <summary>
        /// Determines whether the object should be visible on dashboard or not (single object form another site, insufficient permission, license, etc.)
        /// </summary>
        public bool IsVisible
        {
            get;
            set;
        }
    }
}
