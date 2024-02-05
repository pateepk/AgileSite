namespace CMS.ApplicationDashboard.Web.UI
{
    /// <summary>
    /// Interface that provides access to <see cref="LiveTileModel"/>.
    /// </summary>
    public interface ILiveTileModelProvider
    {
        /// <summary>
        /// Loads model for the dashboard live tile. Null should be returned if live tile should stay dead.
        /// </summary>
        /// <param name="liveTileContext">Context of the live tile. Contains information about the user and the site the model is requested for</param>
        /// <returns>Live tile model or null if tile should stay in the dead state</returns>
        LiveTileModel GetModel(LiveTileContext liveTileContext);
    }
}