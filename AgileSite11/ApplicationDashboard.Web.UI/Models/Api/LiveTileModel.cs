namespace CMS.ApplicationDashboard.Web.UI
{
    /// <summary>
    /// Crate for data in LiveTiles, use concrete <see cref="ILiveTileModelProvider"/> to get an instance.
    /// </summary>
    public sealed class LiveTileModel
    {
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
    }
}
