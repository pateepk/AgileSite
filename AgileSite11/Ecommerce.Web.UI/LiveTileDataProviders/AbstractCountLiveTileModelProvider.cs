using System;

using CMS.ApplicationDashboard.Web.UI;
using CMS.Helpers;

namespace CMS.Ecommerce.Web.UI
{
    internal abstract class AbstractCountLiveTileModelProvider : ILiveTileModelProvider
    {
        /// <summary>
        /// Loads model for the dashboard live tile.
        /// </summary>
        /// <param name="liveTileContext">Context of the live tile. Contains information about the user and the site the model is requested for.</param>
        /// <exception cref="ArgumentNullException"><paramref name="liveTileContext"/> is null</exception>
        /// <returns>Live tile model</returns>
        public LiveTileModel GetModel(LiveTileContext liveTileContext)
        {
            if (liveTileContext == null)
            {
                throw new ArgumentNullException(nameof(liveTileContext));
            }

            return CacheHelper.Cache(() =>
            {
                var count = GetCount(liveTileContext);

                // Returns dead tile if no records found
                if (count == 0)
                {
                    return null;
                }

                return new LiveTileModel
                {
                    Value = count,
                    Description = GetDescription(count)
                };
            }, GetCacheSettings(liveTileContext));
        }


        /// <summary>
        /// Returns count to be shown on live tile.
        /// </summary>
        /// <param name="context">Context of the live tile. Contains information about the user and the site the model is for.</param>
        protected abstract int GetCount(LiveTileContext context);


        /// <summary>
        /// Returns description text to be shown on live tile.
        /// </summary>
        /// <param name="count">Number shown on the live tile.</param>
        protected abstract string GetDescription(int count);


        /// <summary>
        /// Returns cache settings for live tile.
        /// </summary>
        /// <param name="context">Context of the live tile. Contains information about the user and the site the model is for.</param>
        protected abstract CacheSettings GetCacheSettings(LiveTileContext context);
    }
}
