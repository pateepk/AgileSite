using System.Data;

using CMS.DataEngine;
using CMS.Helpers;

namespace CMS.BannerManagement
{
    /// <summary>
    /// Class providing BannerInfo management.
    /// </summary>
    public class BannerInfoProvider : AbstractInfoProvider<BannerInfo, BannerInfoProvider>
    {
        #region "Public methods - Basic"

        /// <summary>
        /// Returns object query of all banners matching the specified parameters.
        /// </summary>
        /// <param name="where">Where condition.</param>
        /// <param name="orderBy">Order by expression.</param>
        /// <param name="topN">Number of records to be selected.</param>        
        /// <param name="columns">Columns to be selected.</param>
        public static ObjectQuery<BannerInfo> GetBanners(string where, string orderBy = null, int topN = -1, string columns = null)
        {
            return GetBanners().Where(where).OrderBy(orderBy).TopN(topN).Columns(columns);
        }


        /// <summary>
        /// Returns banner with specified ID.
        /// </summary>
        /// <param name="bannerId">Banner ID.</param>        
        public static BannerInfo GetBannerInfo(int bannerId)
        {
            return ProviderObject.GetInfoById(bannerId);
        }


        /// <summary>
        /// Sets (updates or inserts) specified banner.
        /// </summary>
        /// <param name="bannerObj">Banner to be set.</param>
        public static void SetBannerInfo(BannerInfo bannerObj)
        {
            ProviderObject.SetInfo(bannerObj);
        }


        /// <summary>
        /// Deletes specified banner.
        /// </summary>
        /// <param name="bannerObj">Banner to be deleted.</param>
        public static void DeleteBannerInfo(BannerInfo bannerObj)
        {
            ProviderObject.DeleteInfo(bannerObj);
        }


        /// <summary>
        /// Deletes banner with specified ID.
        /// </summary>
        /// <param name="bannerId">Banner ID.</param>
        public static void DeleteBannerInfo(int bannerId)
        {
            BannerInfo bannerObj = GetBannerInfo(bannerId);
            DeleteBannerInfo(bannerObj);
        }


        /// <summary>
        /// Returns the query for all banners.
        /// </summary>        
        public static ObjectQuery<BannerInfo> GetBanners()
        {
            return ProviderObject.GetObjectQuery();
        }

        #endregion


        #region "Public methods - Advanced"

        /// <summary>
        /// Decrement clicks left of banner with <paramref name="bannerId"/>.
        /// 
        /// Does nothing if banner has clicks left set to NULL.
        /// </summary>
        /// <param name="bannerId">ID of a banner</param>
        public static void DecrementClicksLeft(int bannerId)
        {
            ProviderObject.DecrementClicksLeftInternal(bannerId);
        }


        /// <summary>
        /// Returns the BannerInfo structure for the random banner.
        /// 
        /// Banner has to be:
        ///  - enabled
        ///  - valid now (ValidFrom and ValidTo) or ValidFrom and ValidTo can be null
        ///  - and has more than 0 hits and clicks left (or null - unlimited)
        /// </summary>
        /// <param name="categoryId">Category ID.</param>
        /// <param name="decrementHitsLeft">If true, hits left (after reaching 0, this banner won't be returned) of the returned banner will be decremented</param>
        public static BannerInfo GetRandomValidBanner(int categoryId, bool decrementHitsLeft)
        {
            return ProviderObject.GetRandomValidBannerInternal(categoryId, decrementHitsLeft);
        }

        #endregion


        #region "Internal methods - Advanced"

        /// <summary>
        /// Decrement clicks left of banner with <paramref name="bannerId"/>.
        /// 
        /// Does nothing if banner has clicks left set to NULL.
        /// </summary>
        /// <param name="bannerId">ID of a banner</param>
        protected virtual void DecrementClicksLeftInternal(int bannerId)
        {
            // Prepare the parameters
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@BannerID", bannerId);

            ConnectionHelper.ExecuteQuery("CMS.Banner.decrementclicksleft", parameters);
        }


        /// <summary>
        /// Returns the BannerInfo structure for the random banner.
        /// 
        /// Banner has to be:
        ///  - enabled
        ///  - valid now (ValidFrom and ValidTo) or ValidFrom and ValidTo can be null
        ///  - and has more than 0 hits and clicks left (or null - unlimited)
        /// </summary>
        /// <param name="categoryId">Category ID.</param>
        /// <param name="decrementHitsLeft">If true, hits left (after reaching 0, this banner won't be returned) of the returned banner will be decremented</param>
        protected virtual BannerInfo GetRandomValidBannerInternal(int categoryId, bool decrementHitsLeft)
        {
            // Prepare the parameters
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@BannerCategoryID", categoryId);
            parameters.Add("@DecrementHitsLeft", decrementHitsLeft);

            // Get the data
            DataSet ds = ConnectionHelper.ExecuteQuery("CMS.Banner.getrandombanner", parameters);
            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                return new BannerInfo(ds.Tables[0].Rows[0]);
            }

            return null;
        }

        #endregion
    }
}
