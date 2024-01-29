using System;
using System.Data;

using CMS.DataEngine;
using CMS.Helpers;
using CMS.Base;

namespace CMS.Localization
{
    /// <summary>
    /// Class to provide the culture management.
    /// </summary>
    public class CultureInfoProvider : AbstractInfoProvider<CultureInfo, CultureInfoProvider>
    {
        #region "Private fields"

        /// Culture info table indexed by culture code.
        private static CMSStatic<ProviderInfoDictionary<string>> mCultureInfos = new CMSStatic<ProviderInfoDictionary<string>>();

        // Culture info table indexed by culture alias.
        private static CMSStatic<ProviderInfoDictionary<string>> mCultureInfosByAlias = new CMSStatic<ProviderInfoDictionary<string>>();

        // Number of UI cultures.
        private static int? mNumberOfUICultures;

        #endregion


        #region "Properties"

        /// <summary>
        /// Culture info table indexed by culture code.
        /// </summary>
        private static ProviderInfoDictionary<string> CultureInfos
        {
            get
            {
                return mCultureInfos;
            }
            set
            {
                mCultureInfos.Value = value;
            }
        }


        /// <summary>
        /// Culture info table indexed by culture alias.
        /// </summary>
        private static ProviderInfoDictionary<string> CultureInfosByAlias
        {
            get
            {
                return mCultureInfosByAlias;
            }
            set
            {
                mCultureInfosByAlias.Value = value;
            }
        }


        /// <summary>
        /// Gets number of UI cultures.
        /// </summary>
        public static int NumberOfUICultures
        {
            get
            {
                if (mNumberOfUICultures == null)
                {
                    mNumberOfUICultures = GetUICultures().Count;
                }

                return mNumberOfUICultures.Value;
            }
        }

        #endregion


        #region "Public static methods"

        /// <summary>
        /// Returns object with specified GUID.
        /// </summary>
        /// <param name="guid">Object GUID</param>
        public static CultureInfo GetCultureInfoByGUID(Guid guid)
        {
            return ProviderObject.GetCultureInfoByGUIDInternal(guid);
        }


        /// <summary>
        /// Returns CultureInfo object for specified culture code or culture alias.
        /// </summary>
        /// <param name="cultureCode">Code of culture or culture alias to retrieve</param>
        public static CultureInfo GetCultureInfoForCulture(string cultureCode)
        {
            return ProviderObject.GetCultureInfoForCultureInternal(cultureCode);
        }


        /// <summary>
        /// Returns CultureInfo object for specified culture code.
        /// </summary>
        /// <param name="cultureCode">Code of culture to retrieve</param>
        public static CultureInfo GetCultureInfo(string cultureCode)
        {
            return ProviderObject.GetCultureInfoInternal(cultureCode);
        }


        /// <summary>
        /// Returns CultureInfo object for specified culture ID.
        /// </summary>
        /// <param name="cultureId">ID of culture to retrieve</param>
        public static CultureInfo GetCultureInfo(int cultureId)
        {
            return ProviderObject.GetInfoById(cultureId);
        }


        /// <summary>
        /// Sets the specified culture info data.
        /// </summary>
        /// <param name="infoObj">CultureInfo object to set (save as new or update existing)</param>
        public static void SetCultureInfo(CultureInfo infoObj)
        {
            ProviderObject.SetCultureInfoInternal(infoObj);
        }


        /// <summary>
        /// Deletes specified culture.
        /// </summary>
        /// <param name="ci">Culture object</param>
        public static void DeleteCultureInfo(CultureInfo ci)
        {
            ProviderObject.DeleteCultureInfoInternal(ci);
        }


        /// <summary>
        /// Deletes specified culture.
        /// </summary>
        /// <param name="cultureId">Culture ID</param>
        public static void DeleteCultureInfo(int cultureId)
        {
            CultureInfo ci = GetCultureInfo(cultureId);
            DeleteCultureInfo(ci);
        }


        /// <summary>
        /// Delete specified culture.
        /// </summary>
        /// <param name="cultureCode">Code name of the culture to delete</param>
        public static void DeleteCultureInfo(string cultureCode)
        {
            CultureInfo ci = GetCultureInfo(cultureCode);
            DeleteCultureInfo(ci);
        }


        /// <summary>
        /// Returns all culture records.
        /// </summary>
        public static ObjectQuery<CultureInfo> GetCultures()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns the DataSet of all the culture records with where condition and order by clause applied.
        /// </summary>
        /// <param name="where">Where condition</param>
        /// <param name="orderBy">Order by clause</param>
        /// <param name="topN">Top N users</param>
        /// <param name="columns">Columns to get</param>
        public static InfoDataSet<CultureInfo> GetCultures(string where, string orderBy = null, int topN = -1, string columns = null)
        {
            return GetCultures()
                .Where(where)
                .OrderBy(orderBy)
                .TopN(topN)
                .Columns(SqlHelper.ParseColumnList(columns))
                .TypedResult;
        }


        /// <summary>
        /// Returns all UI culture records.
        /// </summary>
        public static ObjectQuery<CultureInfo> GetUICultures()
        {
            return ProviderObject.GetUICulturesInternal();
        }

        /// <summary>
        /// Gets all UI cultures.
        /// </summary>
        /// <param name="where">Where condition</param>
        /// <param name="orderBy">Order by clause</param>
        /// <param name="topN">Top N users</param>
        /// <param name="columns">Columns to get</param>
        public static InfoDataSet<CultureInfo> GetUICultures(string where, string orderBy = null, int topN = -1, string columns = null)
        {
            return GetUICultures()
                .Where(where)
                .OrderBy(orderBy)
                .TopN(topN)
                .Columns(SqlHelper.ParseColumnList(columns))
                .TypedResult;
        }


        /// <summary>
        /// Gets the culture ID.
        /// </summary>
        /// <param name="cultureCode">Culture code</param>
        public static int GetCultureID(string cultureCode)
        {
            var ci = GetCultureInfo(cultureCode);
            if (ci != null)
            {
                return ci.CultureID;
            }

            return 0;
        }

        #endregion


        #region "Private static methods"

        /// <summary>
        /// Table lock for loading.
        /// </summary>
        private static readonly object tableLock = new object();


        /// <summary>
        /// Loads culture information.
        /// </summary>
        private static void LoadCultures()
        {
            if (ProviderHelper.LoadTables(CultureInfos, CultureInfosByAlias))
            {
                lock (tableLock)
                {
                    if (ProviderHelper.LoadTables(CultureInfos, CultureInfosByAlias))
                    {
                        // Prepare the tables
                        var infos = new ProviderInfoDictionary<string>(CultureInfo.OBJECT_TYPE, "CultureCode");
                        var infosByAlias = new ProviderInfoDictionary<string>(CultureInfo.OBJECT_TYPE, "CultureAlias");

                        // Load the data if configured
                        if (ProviderHelper.LoadHashTables(CultureInfo.OBJECT_TYPE, LoadHashtableEnum.All) != LoadHashtableEnum.None)
                        {
                            GetCultures().ForEachObject(culture =>
                            {
                                var keyName = culture.CultureCode;
                                infos[keyName] = culture;
                                if (!String.IsNullOrEmpty(culture.CultureAlias))
                                {
                                    infosByAlias[culture.CultureAlias] = culture;
                                }
                            });
                        }

                        CultureInfos = infos;
                        CultureInfosByAlias = infosByAlias;
                    }
                }
            }
        }

        #endregion


        #region "Internal methods"

        /// <summary>
        /// Clear hashtables.
        /// </summary>
        /// <param name="logTasks">If true, web farm tasks are logged</param>
        protected override void ClearHashtables(bool logTasks)
        {
            base.ClearHashtables(logTasks);

            lock (tableLock)
            {
                if (CultureInfos != null)
                {
                    CultureInfos.Clear(logTasks);
                    CultureInfosByAlias.Clear(logTasks);
                }
            }
        }


        /// <summary>
        /// Returns object with specified GUID.
        /// </summary>
        /// <param name="guid">Object GUID</param>
        protected virtual CultureInfo GetCultureInfoByGUIDInternal(Guid guid)
        {
            return GetObjectQuery().WhereEquals("CultureGUID", guid).FirstObject;
        }


        /// <summary>
        /// Returns CultureInfo object for specified culture code or culture alias.
        /// </summary>
        /// <param name="cultureCodeOrAlias">Culture code or culture alias</param>
        protected virtual CultureInfo GetCultureInfoForCultureInternal(string cultureCodeOrAlias)
        {
            CultureInfo result = null;

            if (cultureCodeOrAlias != null)
            {
                LoadCultures();

                result = (CultureInfo)(CultureInfos[cultureCodeOrAlias]);

                // if not found in hashtable, try to find it in DB
                if (result == null)
                {
                    // Try get by culture alias
                    result = (CultureInfo)(CultureInfosByAlias[cultureCodeOrAlias]);

                    // Try load by culture code/alias from DB
                    if (result == null)
                    {
                        // try get from db for culture code
                        result = GetCultureInfoFromDB(cultureCodeOrAlias);
                        // If exists save to the cultures collection
                        if (result != null)
                        {
                            CultureInfos[cultureCodeOrAlias] = result;
                        }
                        else
                        {
                            // Try get from DB for alias
                            result = GetCultureInfoFromDB(cultureCodeOrAlias, true);
                            CultureInfosByAlias[cultureCodeOrAlias] = result;
                        }
                    }
                }
            }
            return result;
        }


        /// <summary>
        /// Returns CultureInfo object for specified culture code.
        /// </summary>
        /// <param name="cultureCode">Code of culture to retrieve</param>
        protected virtual CultureInfo GetCultureInfoInternal(string cultureCode)
        {
            CultureInfo result = null;

            if (cultureCode != null)
            {
                LoadCultures();

                result = (CultureInfo)(CultureInfos[cultureCode]);

                // if not found in hashtable, try to find it in DB
                if (result == null)
                {
                    // Try load by culture code/alias from DB
                    if (result == null)
                    {
                        result = GetCultureInfoFromDB(cultureCode);
                        CultureInfos[cultureCode] = result;
                    }
                }
            }

            return result;
        }


        /// <summary>
        /// Returns the CultureInfo structure for the specified culture from database.
        /// </summary>
        /// <param name="cultureCode">Culture code name or culture alias</param>
        /// <param name="getAlias">Indicates whether culture should be selected by culture code or alias</param>
        protected virtual CultureInfo GetCultureInfoFromDB(string cultureCode, bool getAlias)
        {
            var where = new WhereCondition().WhereEquals(getAlias ? "CultureAlias" : "CultureCode", cultureCode);
            DataSet ds = GetCultures(where.ToString(true), null, 1);
            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                return new CultureInfo(ds.Tables[0].Rows[0]);
            }

            return null;
        }


        /// <summary>
        /// Returns the CultureInfo structure for the specified culture from database.
        /// </summary>
        /// <param name="cultureCode">Culture code name</param>
        protected virtual CultureInfo GetCultureInfoFromDB(string cultureCode)
        {
            return GetCultureInfoFromDB(cultureCode, false);
        }


        /// <summary>
        /// Checks the culture object uniqueness for culture code and culture alias
        /// </summary>
        /// <param name="info">Culture info object</param>
        /// <exception cref="CultureNotUniqueException">Throws an exception for culture or alias that is already present.</exception>
        private void CheckCultureUniqueness(CultureInfo info)
        {
            // Culture vs alias within the object
            if (CMSString.Compare(info.CultureCode, info.CultureAlias, true) == 0)
            {
                throw new CultureNotUniqueException("[CultureInfoProvider.SetCultureInfo]: Culture code cannot be same as culture alias.");
            }

            var where = new WhereCondition()
                // Current culture vs other culture
                .WhereEquals("CultureCode", info.CultureCode)
                .Or()
                // Current culture alias vs other culture
                .WhereEquals("CultureAlias", info.CultureCode);

            if (!String.IsNullOrEmpty(info.CultureAlias))
            {
                where
                    .Or()
                    // Other culture code vs current culture alias
                    .WhereEquals("CultureCode", info.CultureAlias)
                    .Or()
                    // Other culture alias vs current culture alias
                    .WhereEquals("CultureAlias", info.CultureAlias);
            }

            // Do not check current object for update
            where = new WhereCondition()
                    .WhereNotEquals("CultureID", info.CultureID)
                    .Where(where);

            // Try gets existing items from database and throw an exception if something was found
            var data = GetCultures(where.ToString(true), null, 1, "CultureID");
            if (!DataHelper.DataSourceIsEmpty(data))
            {
                throw new CultureNotUniqueException("[CultureInfoProvider.CheckCultureUniqueness] Culture code '" + info.CultureCode + "' or culture alias '" + info.CultureAlias + "' already exists.");
            }
        }


        /// <summary>
        /// Sets the specified culture info data.
        /// </summary>
        /// <param name="infoObj">CultureInfo object to set (save as new or update existing)</param>
        /// <exception cref="CultureNotUniqueException">Throws an <see cref="CultureNotUniqueException" /> exception for culture or alias that is already present.</exception>
        protected virtual void SetCultureInfoInternal(CultureInfo infoObj)
        {
            if (infoObj != null)
            {
                LoadCultures();

                CheckCultureUniqueness(infoObj);

                // Try get column alias value before update if alias changed
                string originalAliasValue = null;
                if (infoObj.ItemChanged("CultureAlias"))
                {
                    originalAliasValue = Convert.ToString(infoObj.GetOriginalValue("CultureAlias"));
                }

                // If exists -> update existing data
                if (infoObj.CultureID > 0)
                {
                    infoObj.Generalized.UpdateData();
                }
                // Else insert new data
                else
                {
                    infoObj.Generalized.InsertData();
                }

                // Update hashtables
                CultureInfos.Update(infoObj.CultureCode, infoObj);

                // Remove original alias from cache
                if (!String.IsNullOrEmpty(originalAliasValue))
                {
                    CultureInfosByAlias.Update(originalAliasValue, null);
                }

                // Update new alias in cache if specified
                if (!String.IsNullOrEmpty(infoObj.CultureAlias))
                {
                    CultureInfosByAlias.Update(infoObj.CultureAlias, infoObj);
                }
            }
        }


        /// <summary>
        /// Deletes specified culture.
        /// </summary>
        /// <param name="ci">Culture object</param>
        protected virtual void DeleteCultureInfoInternal(CultureInfo ci)
        {
            if (ci == null)
            {
                return;
            }

            LoadCultures();

            // Delete the data
            DeleteInfo(ci);

            // Remove from the CultureInfos table               
            CultureInfos.Delete(ci.CultureCode);
            CultureInfosByAlias.Delete(ci.CultureAlias);
        }


        /// <summary>
        /// Returns all UI culture records.
        /// </summary>
        protected virtual ObjectQuery<CultureInfo> GetUICulturesInternal()
        {
            return ProviderObject.GetObjectQuery().WhereTrue("CultureIsUICulture");
        }

        #endregion
    }
}