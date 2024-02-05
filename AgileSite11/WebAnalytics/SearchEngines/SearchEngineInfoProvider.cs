using CMS.DataEngine;

namespace CMS.WebAnalytics
{
    /// <summary>
    /// Class providing SearchEngineInfo management.
    /// </summary>
    public class SearchEngineInfoProvider : AbstractInfoProvider<SearchEngineInfo, SearchEngineInfoProvider>
    {
        #region "Constructors"

        /// <summary>
        /// Constructor.
        /// </summary>
        public SearchEngineInfoProvider()
            : base(SearchEngineInfo.TYPEINFO, new HashtableSettings
                {
                    ID = true,
                    Name = true,
                    Load = LoadHashtableEnum.None
                })
        {
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Returns a query for all the SearchEngineInfo objects.
        /// </summary>
        public static ObjectQuery<SearchEngineInfo> GetSearchEngines()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns search engine with specified ID.
        /// </summary>
        /// <param name="engineId">Search engine ID</param>        
        public static SearchEngineInfo GetSearchEngineInfo(int engineId)
        {
            return ProviderObject.GetInfoById(engineId);
        }


        /// <summary>
        /// Returns search engine with specified name.
        /// </summary>
        /// <param name="engineName">Search engine name</param>                
        public static SearchEngineInfo GetSearchEngineInfo(string engineName)
        {
            return ProviderObject.GetInfoByCodeName(engineName);
        }


        /// <summary>
        /// Sets (updates or inserts) specified search engine.
        /// </summary>
        /// <param name="engineObj">Search engine to be set</param>
        public static void SetSearchEngineInfo(SearchEngineInfo engineObj)
        {
            ProviderObject.SetInfo(engineObj);
        }


        /// <summary>
        /// Deletes specified search engine.
        /// </summary>
        /// <param name="engineObj">Search engine to be deleted</param>
        public static void DeleteSearchEngineInfo(SearchEngineInfo engineObj)
        {
            ProviderObject.DeleteInfo(engineObj);
        }


        /// <summary>
        /// Deletes search engine with specified ID.
        /// </summary>
        /// <param name="engineId">Search engine ID</param>
        public static void DeleteSearchEngineInfo(int engineId)
        {
            SearchEngineInfo engineObj = GetSearchEngineInfo(engineId);
            DeleteSearchEngineInfo(engineObj);
        }

        #endregion


        #region "Internal methods"


        /// <summary>
        /// Inserts or Updates the object to the database.
        /// </summary>
        /// <param name="info">Object to insert / update</param>
        protected override void SetInfo(SearchEngineInfo info)
        {
            base.SetInfo(info);

            ClearHashtables(true);
        }


        /// <summary>
        /// Deletes the object to the database.
        /// </summary>
        /// <param name="info">Object to delete</param>
        protected override void DeleteInfo(SearchEngineInfo info)
        {
            base.DeleteInfo(info);

            ClearHashtables(true);
        }

        #endregion
    }
}