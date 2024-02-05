using CMS.DataEngine;
using CMS.Helpers;

namespace CMS.DataProtection
{
    /// <summary>
    /// Class providing <see cref="ConsentInfo"/> management.
    /// </summary>
    public class ConsentInfoProvider : AbstractInfoProvider<ConsentInfo, ConsentInfoProvider>
    {
        /// <summary>
        /// Creates an instance of <see cref="ConsentInfoProvider"/>.
        /// </summary>
        public ConsentInfoProvider()
            : base(ConsentInfo.TYPEINFO, new HashtableSettings
            {
                ID = true,
                Name = true,
                Load = LoadHashtableEnum.None
            })
        {
        }


        /// <summary>
        /// Returns a query for all the <see cref="ConsentInfo"/> objects.
        /// </summary>
        public static ObjectQuery<ConsentInfo> GetConsents()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns <see cref="ConsentInfo"/> with specified ID.
        /// </summary>
        /// <param name="id"><see cref="ConsentInfo"/> ID.</param>
        public static ConsentInfo GetConsentInfo(int id)
        {
            return ProviderObject.GetInfoById(id);
        }


        /// <summary>
        /// Returns <see cref="ConsentInfo"/> with specified name.
        /// </summary>
        /// <param name="name"><see cref="ConsentInfo"/> name.</param>
        public static ConsentInfo GetConsentInfo(string name)
        {
            return ProviderObject.GetInfoByCodeName(name);
        }


        /// <summary>
        /// Sets (updates or inserts) specified <see cref="ConsentInfo"/>.
        /// </summary>
        /// <param name="infoObj"><see cref="ConsentInfo"/> to be set.</param>
        public static void SetConsentInfo(ConsentInfo infoObj)
        {
            RecomputeHash(infoObj);
            ProviderObject.SetInfo(infoObj);
        }


        /// <summary>
        /// Deletes specified <see cref="ConsentInfo"/>.
        /// </summary>
        /// <param name="infoObj"><see cref="ConsentInfo"/> to be deleted.</param>
        public static void DeleteConsentInfo(ConsentInfo infoObj)
        {
            ProviderObject.DeleteInfo(infoObj);
        }


        /// <summary>
        /// Deletes <see cref="ConsentInfo"/> with specified ID.
        /// </summary>
        /// <param name="id"><see cref="ConsentInfo"/> ID.</param>
        public static void DeleteConsentInfo(int id)
        {
            ConsentInfo infoObj = GetConsentInfo(id);
            DeleteConsentInfo(infoObj);
        }


        private static void RecomputeHash(ConsentInfo infoObj)
        {
            if (infoObj.Generalized.ItemChanged("ConsentContent"))
            {
                infoObj.ConsentHash = SecurityHelper.GetSHA2Hash(infoObj.ConsentContent);
            }
        }
    }
}