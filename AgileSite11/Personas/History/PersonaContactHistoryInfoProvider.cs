using System.Collections.Generic;

using CMS.DataEngine;

namespace CMS.Personas
{
    /// <summary>
    /// Class providing PersonaContactHistoryInfo management.
    /// </summary>
    public class PersonaContactHistoryInfoProvider : AbstractInfoProvider<PersonaContactHistoryInfo, PersonaContactHistoryInfoProvider>
    {
        #region "Constructors"

        /// <summary>
        /// Constructor
        /// </summary>
        public PersonaContactHistoryInfoProvider()
            : base(PersonaContactHistoryInfo.TYPEINFO)
        {
        }

        #endregion


        #region "Public methods - Basic"

        /// <summary>
        /// Returns a query for all the PersonaContactHistoryInfo objects.
        /// </summary>
        public static ObjectQuery<PersonaContactHistoryInfo> GetPersonaContactHistory()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns PersonaContactHistoryInfo with specified ID.
        /// </summary>
        /// <param name="id">PersonaContactHistoryInfo ID</param>
        public static PersonaContactHistoryInfo GetPersonaContactHistoryInfo(int id)
        {
            return ProviderObject.GetInfoById(id);
        }


        /// <summary>
        /// Sets (updates or inserts) specified PersonaContactHistoryInfo.
        /// </summary>
        /// <param name="infoObj">PersonaContactHistoryInfo to be set</param>
        public static void SetPersonaContactHistoryInfo(PersonaContactHistoryInfo infoObj)
        {
            ProviderObject.SetInfo(infoObj);
        }


        /// <summary>
        /// Deletes specified PersonaContactHistoryInfo.
        /// </summary>
        /// <param name="infoObj">PersonaContactHistoryInfo to be deleted</param>
        public static void DeletePersonaContactHistoryInfo(PersonaContactHistoryInfo infoObj)
        {
            ProviderObject.DeleteInfo(infoObj);
        }


        /// <summary>
        /// Deletes PersonaContactHistoryInfo with specified ID.
        /// </summary>
        /// <param name="id">PersonaContactHistoryInfo ID</param>
        public static void DeletePersonaContactHistoryInfo(int id)
        {
            PersonaContactHistoryInfo infoObj = GetPersonaContactHistoryInfo(id);
            DeletePersonaContactHistoryInfo(infoObj);
        }


        /// <summary>
        /// Bulk inserts the given <paramref name="personaContactHistoryObjects"/>.
        /// </summary>
        /// <param name="personaContactHistoryObjects">Objects to be bulk inserted</param>
        public static void BulkInsert(IEnumerable<PersonaContactHistoryInfo> personaContactHistoryObjects)
        {
            ProviderObject.BulkInsertInfos(personaContactHistoryObjects);
        }

        #endregion
    }
}
