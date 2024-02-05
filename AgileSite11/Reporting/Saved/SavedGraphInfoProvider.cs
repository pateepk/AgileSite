using System;

using CMS.DataEngine;

namespace CMS.Reporting
{
    /// <summary>
    /// Class providing SavedGraphInfo management.
    /// </summary>
    public class SavedGraphInfoProvider : AbstractInfoProvider<SavedGraphInfo, SavedGraphInfoProvider>
    {
        #region "Public methods"

        /// <summary>
        /// Returns the SavedGraphInfo structure for the specified savedGraph.
        /// </summary>
        /// <param name="savedGraphId">SavedGraph id</param>
        public static SavedGraphInfo GetSavedGraphInfo(int savedGraphId)
        {
            return ProviderObject.GetInfoById(savedGraphId);
        }


        /// <summary>
        /// Returns the SavedGraphInfo structure for the specified graphGuid.
        /// </summary>
        /// <param name="graphGuid">Graph guid</param>
        public static SavedGraphInfo GetSavedGraphInfo(Guid graphGuid)
        {
            return ProviderObject.GetInfoByGuid(graphGuid);
        }


        /// <summary>
        /// Sets (updates or inserts) specified savedGraph.
        /// </summary>
        /// <param name="savedGraph">SavedGraph to set</param>
        public static void SetSavedGraphInfo(SavedGraphInfo savedGraph)
        {
            ProviderObject.SetInfo(savedGraph);
        }


        /// <summary>
        /// Deletes specified savedGraph.
        /// </summary>
        /// <param name="savedGraphObj">SavedGraph object</param>
        public static void DeleteSavedGraphInfo(SavedGraphInfo savedGraphObj)
        {
            ProviderObject.DeleteInfo(savedGraphObj);
        }


        /// <summary>
        /// Deletes specified savedGraph.
        /// </summary>
        /// <param name="savedGraphId">SavedGraph id</param>
        public static void DeleteSavedGraphInfo(int savedGraphId)
        {
            SavedGraphInfo savedGraphObj = GetSavedGraphInfo(savedGraphId);
            DeleteSavedGraphInfo(savedGraphObj);
        }

        #endregion
    }
}