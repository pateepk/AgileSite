using CMS.DataEngine;
using CMS.SiteProvider;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Class providing <see cref="BrandInfo"/> management.
    /// </summary>
    public class BrandInfoProvider : AbstractInfoProvider<BrandInfo, BrandInfoProvider>
    {
        /// <summary>
        /// Creates an instance of <see cref="BrandInfoProvider"/>.
        /// </summary>
        public BrandInfoProvider()
            : base(BrandInfo.TYPEINFO, new HashtableSettings
            {
                ID = true,
                Name = true
            })
        {
        }


        /// <summary>
        /// Returns a query for all the <see cref="BrandInfo"/> objects.
        /// </summary>
        public static ObjectQuery<BrandInfo> GetBrands()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns <see cref="BrandInfo"/> with specified ID.
        /// </summary>
        /// <param name="id"><see cref="BrandInfo"/> ID.</param>
        public static BrandInfo GetBrandInfo(int id)
        {
            return ProviderObject.GetInfoById(id);
        }


        /// <summary>
        /// Returns <see cref="BrandInfo"/> with specified name.
        /// </summary>
        /// <param name="name"><see cref="BrandInfo"/> name.</param>
        /// <param name="siteName">Site name.</param>
        public static BrandInfo GetBrandInfo(string name, string siteName)
        {
            return ProviderObject.GetInfoByCodeName(name, SiteInfoProvider.GetSiteID(siteName));
        }


        /// <summary>
        /// Sets (updates or inserts) specified <see cref="BrandInfo"/>.
        /// </summary>
        /// <param name="infoObj"><see cref="BrandInfo"/> to be set.</param>
        public static void SetBrandInfo(BrandInfo infoObj)
        {
            ProviderObject.SetInfo(infoObj);
        }


        /// <summary>
        /// Deletes specified <see cref="BrandInfo"/>.
        /// </summary>
        /// <param name="infoObj"><see cref="BrandInfo"/> to be deleted.</param>
        public static void DeleteBrandInfo(BrandInfo infoObj)
        {
            ProviderObject.DeleteInfo(infoObj);
        }


        /// <summary>
        /// Deletes <see cref="BrandInfo"/> with specified ID.
        /// </summary>
        /// <param name="id"><see cref="BrandInfo"/> ID.</param>
        public static void DeleteBrandInfo(int id)
        {
            BrandInfo infoObj = GetBrandInfo(id);
            DeleteBrandInfo(infoObj);
        }
    }
}