using System;
using System.Data;
using System.Linq;

using CMS.DataEngine;
using CMS.Helpers;
using CMS.Base;

namespace CMS.Globalization
{
    using TypedDataSet = InfoDataSet<StateInfo>;

    /// <summary>
    /// Class providing StateInfo management.
    /// </summary>
    public class StateInfoProvider : AbstractInfoProvider<StateInfo, StateInfoProvider>
    {
        #region "Constructors"

        /// <summary>
        /// Constructor.
        /// </summary>
        public StateInfoProvider()
            : base(StateInfo.TYPEINFO, new HashtableSettings
                {
                    ID = true,
                    Name = true,
                    Load = LoadHashtableEnum.All
                })
        {
        }

        #endregion


        #region "Properties"

        /// <summary>
        /// States indexed by state code.
        /// </summary>
        private static readonly CMSStatic<ProviderInfoDictionary<string>> mStateInfosByCode = new CMSStatic<ProviderInfoDictionary<string>>();


        /// <summary>
        /// Table lock for loading.
        /// </summary>
        private static readonly object tableLock = new object();


        /// <summary>
        /// States indexed by state code.
        /// </summary>
        private static ProviderInfoDictionary<string> StateInfosByCode
        {
            get
            {
                return mStateInfosByCode;
            }
            set
            {
                mStateInfosByCode.Value = value;
            }
        }

        #endregion


        #region "Public static methods"

        /// <summary>
        /// Returns the StateInfo structure for the specified state.
        /// </summary>
        /// <param name="stateId">State id</param>
        public static StateInfo GetStateInfo(int stateId)
        {
            return ProviderObject.GetInfoById(stateId);
        }


        /// <summary>
        /// Returns the StateInfo structure for the specified state.
        /// </summary>
        /// <param name="stateName">StateName</param>
        public static StateInfo GetStateInfo(string stateName)
        {
            return ProviderObject.GetInfoByCodeName(stateName);
        }


        /// <summary>
        /// Returns DataSet with states of the specified country.
        /// </summary>
        /// <param name="countryId">Country id</param>
        [Obsolete("Use member GetStates() instead")]
        public static TypedDataSet GetCountryStates(int countryId)
        {
            return ProviderObject.GetCountryStatesInternal(countryId);
        }


        /// <summary>
        /// Returns states query.
        /// </summary>
        public static ObjectQuery<StateInfo> GetStates()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        ///  Returns the StateInfo structure for the specified state code.
        /// </summary>
        /// <param name="stateCode">Code of the state</param>
        public static StateInfo GetStateInfoByCode(string stateCode)
        {
            return ProviderObject.GetStateInfoByCodeInternal(stateCode);
        }


        /// <summary>
        /// Sets (updates or inserts) specified state.
        /// </summary>
        /// <param name="state">State to set</param>
        public static void SetStateInfo(StateInfo state)
        {
            ProviderObject.SetStateInfoInternal(state);
        }


        /// <summary>
        /// Deletes specified state.
        /// </summary>
        /// <param name="stateObj">State object</param>
        public static void DeleteStateInfo(StateInfo stateObj)
        {
            ProviderObject.DeleteStateInfoInternal(stateObj);
        }


        /// <summary>
        /// Deletes specified state.
        /// </summary>
        /// <param name="stateId">State id</param>
        public static void DeleteStateInfo(int stateId)
        {
            StateInfo stateObj = GetStateInfo(stateId);
            DeleteStateInfo(stateObj);
        }


        /// <summary>
        /// Check dependencies.
        /// </summary>
        /// <param name="stateId">State ID</param>
        [Obsolete("Method was not intended for public use and will be removed in the next version.")]
        public static bool CheckDependencies(int stateId)
        {
            var infoObj = ProviderObject.GetInfoById(stateId);
            if (infoObj != null)
            {
                return infoObj.Generalized.CheckDependencies();
            }
            return false;
        }


        /// <summary>
        /// Deletes all country states.
        /// </summary>
        /// <param name="countryId">Country ID</param>
        [Obsolete("Method was not intended for public use and will be removed in the next version.")]
        public static void DeleteCountryStates(int countryId)
        {
            // Delete all states
            DataSet statesDS = ProviderObject.GetObjectQuery().WhereEquals("CountryID", countryId).TypedResult;
            if (!DataHelper.DataSourceIsEmpty(statesDS))
            {
                foreach (DataRow dr in statesDS.Tables[0].Rows)
                {
                    int stateId = ValidationHelper.GetInteger(dr["StateID"], 0);
                    DeleteStateInfo(stateId);
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

            // Clear countries
            lock (tableLock)
            {
                StateInfosByCode?.Clear(logTasks);
            }
        }


        /// <summary>
        /// Returns DataSet with states of the specified country.
        /// </summary>
        /// <param name="countryId">Country id</param>
        [Obsolete("Use member GetStates() instead")]
        protected virtual TypedDataSet GetCountryStatesInternal(int countryId)
        {
            return GetObjectQuery().WhereEquals("CountryID", countryId).TypedResult;
        }


        /// <summary>
        ///  Returns the StateInfo structure for the specified state code.
        /// </summary>
        /// <param name="stateCode">Code of the state</param>
        protected virtual StateInfo GetStateInfoByCodeInternal(string stateCode)
        {
            StateInfo result = null;

            if (!string.IsNullOrEmpty(stateCode))
            {
                // Load hashtables
                LoadStates();

                // Try to get state from hashtable
                var info = StateInfosByCode[stateCode];
                if (info == InfoHelper.EmptyInfo)
                {
                    return null;
                }

                result = info as StateInfo;
                if (result == null)
                {
                    // Get state from DB when not found
                    result = GetObjectQuery().WhereEquals("StateCode", stateCode).FirstOrDefault();
                    StateInfosByCode.Update(stateCode, result ?? InfoHelper.EmptyInfo);
                }
            }

            return result;
        }


        /// <summary>
        /// Sets (updates or inserts) specified state.
        /// </summary>
        /// <param name="stateObj">State to set</param>
        protected virtual void SetStateInfoInternal(StateInfo stateObj)
        {
            if (stateObj != null)
            {
                // Load hashtables
                LoadStates();

                // When editing existing state
                if (stateObj.StateID > 0)
                {
                    StateInfo originalState = GetInfoById(stateObj.StateID);
                    if (originalState != null)
                    {
                        // Check if code has changed
                        if (originalState.StateCode != stateObj.StateCode)
                        {
                            StateInfosByCode.Delete(originalState.StateCode);
                        }
                    }
                }

                // Save changes
                SetInfo(stateObj);

                // Update hashtables
                StateInfosByCode.Update(stateObj.StateCode, stateObj);
            }
        }


        /// <summary>
        /// Deletes specified state.
        /// </summary>
        /// <param name="stateObj">State object</param>
        protected virtual void DeleteStateInfoInternal(StateInfo stateObj)
        {
            if (stateObj != null)
            {
                // Load hashtables
                LoadStates();

                // Delete info
                DeleteInfo(stateObj);

                // Update hashtables
                StateInfosByCode.Delete(stateObj.StateCode);
            }
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Loads states to hashtables.
        /// </summary>
        private static void LoadStates()
        {
            if (ProviderHelper.LoadTables(StateInfosByCode))
            {
                lock (tableLock)
                {
                    if (ProviderHelper.LoadTables(StateInfosByCode))
                    {
                        // Prepare the tables
                        var tempStates = new ProviderInfoDictionary<string>(StateInfo.OBJECT_TYPE, "StateCode");

                        if (ProviderHelper.LoadHashTables(StateInfo.OBJECT_TYPE, LoadHashtableEnum.All) != LoadHashtableEnum.None)
                        {
                            // Add all states to hashtable
                            var states = GetStates();
                            foreach (var state in states)
                            {
                                tempStates[state.StateCode] = state;
                            }
                        }

                        StateInfosByCode = tempStates;
                    }
                }
            }
        }

        #endregion
    }
}