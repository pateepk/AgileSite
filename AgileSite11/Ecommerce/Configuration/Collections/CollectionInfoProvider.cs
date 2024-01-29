using CMS.DataEngine;
using CMS.SiteProvider;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Class providing <see cref="CollectionInfo"/> management.
    /// </summary>
    public class CollectionInfoProvider : AbstractInfoProvider<CollectionInfo, CollectionInfoProvider>
    {
        #region "Constructors"

        /// <summary>
        /// Creates an instance of <see cref="CollectionInfoProvider"/>.
        /// </summary>
        public CollectionInfoProvider()
            : base(CollectionInfo.TYPEINFO, new HashtableSettings
            {
                ID = true,
                Name = true
            })
        {
        }

        #endregion


        #region "Public methods - Basic"

        /// <summary>
        /// Returns a query for all the <see cref="CollectionInfo"/> objects.
        /// </summary>
        public static ObjectQuery<CollectionInfo> GetCollections()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns <see cref="CollectionInfo"/> with specified ID.
        /// </summary>
        /// <param name="id"><see cref="CollectionInfo"/> ID.</param>
        public static CollectionInfo GetCollectionInfo(int id)
        {
            return ProviderObject.GetInfoById(id);
        }


        /// <summary>
        /// Returns <see cref="CollectionInfo"/> with specified name.
        /// </summary>
        /// <param name="name"><see cref="CollectionInfo"/> name.</param>
        /// <param name="siteName">Site name.</param>
        public static CollectionInfo GetCollectionInfo(string name, string siteName)
        {
            return ProviderObject.GetInfoByCodeName(name, SiteInfoProvider.GetSiteID(siteName));
        }


        /// <summary>
        /// Sets (updates or inserts) specified <see cref="CollectionInfo"/>.
        /// </summary>
        /// <param name="infoObj"><see cref="CollectionInfo"/> to be set.</param>
        public static void SetCollectionInfo(CollectionInfo infoObj)
        {
            ProviderObject.SetInfo(infoObj);
        }


        /// <summary>
        /// Deletes specified <see cref="CollectionInfo"/>.
        /// </summary>
        /// <param name="infoObj"><see cref="CollectionInfo"/> to be deleted.</param>
        public static void DeleteCollectionInfo(CollectionInfo infoObj)
        {
            ProviderObject.DeleteInfo(infoObj);
        }


        /// <summary>
        /// Deletes <see cref="CollectionInfo"/> with specified ID.
        /// </summary>
        /// <param name="id"><see cref="CollectionInfo"/> ID.</param>
        public static void DeleteCollectionInfo(int id)
        {
            CollectionInfo infoObj = GetCollectionInfo(id);
            DeleteCollectionInfo(infoObj);
        }

        #endregion
    }
}