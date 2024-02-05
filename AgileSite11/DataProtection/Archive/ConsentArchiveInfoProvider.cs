using CMS.DataEngine;

namespace CMS.DataProtection
{
    /// <summary>
    /// Class providing <see cref="ConsentArchiveInfo"/> management.
    /// </summary>
    public class ConsentArchiveInfoProvider : AbstractInfoProvider<ConsentArchiveInfo, ConsentArchiveInfoProvider>
    {
        /// <summary>
        /// Creates an instance of <see cref="ConsentArchiveInfoProvider"/>.
        /// </summary>
        public ConsentArchiveInfoProvider()
            : base(ConsentArchiveInfo.TYPEINFO)
        {
        }


        /// <summary>
        /// Returns a query for all the <see cref="ConsentArchiveInfo"/> objects.
        /// </summary>
        public static ObjectQuery<ConsentArchiveInfo> GetConsentArchives()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns <see cref="ConsentArchiveInfo"/> with specified ID.
        /// </summary>
        /// <param name="id"><see cref="ConsentArchiveInfo"/> ID.</param>
        public static ConsentArchiveInfo GetConsentArchiveInfo(int id)
        {
            return ProviderObject.GetInfoById(id);
        }


        /// <summary>
        /// Sets (updates or inserts) specified <see cref="ConsentArchiveInfo"/>.
        /// </summary>
        /// <param name="infoObj"><see cref="ConsentArchiveInfo"/> to be set.</param>
        public static void SetConsentArchiveInfo(ConsentArchiveInfo infoObj)
        {
            ProviderObject.SetInfo(infoObj);
        }


        /// <summary>
        /// Deletes specified <see cref="ConsentArchiveInfo"/>.
        /// </summary>
        /// <param name="infoObj"><see cref="ConsentArchiveInfo"/> to be deleted.</param>
        public static void DeleteConsentArchiveInfo(ConsentArchiveInfo infoObj)
        {
            ProviderObject.DeleteInfo(infoObj);
        }


        /// <summary>
        /// Deletes <see cref="ConsentArchiveInfo"/> with specified ID.
        /// </summary>
        /// <param name="id"><see cref="ConsentArchiveInfo"/> ID.</param>
        public static void DeleteConsentArchiveInfo(int id)
        {
            ConsentArchiveInfo infoObj = GetConsentArchiveInfo(id);
            DeleteConsentArchiveInfo(infoObj);
        }
    }
}