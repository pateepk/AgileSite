using CMS.DataEngine;
using CMS.Helpers;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Class providing TaxClassStateInfo management.
    /// </summary>
    public class TaxClassStateInfoProvider : AbstractInfoProvider<TaxClassStateInfo, TaxClassStateInfoProvider>, IFullNameInfoProvider
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public TaxClassStateInfoProvider()
            : base(TaxClassStateInfo.TYPEINFO, new HashtableSettings
            {
                FullName = true,
                Load = LoadHashtableEnum.All,
                UseWeakReferences = true
            })
        {
        }


        #region "Public methods - Basic"

        /// <summary>
        /// Returns the query for all relationships between tax classes and states.
        /// </summary>
        public static ObjectQuery<TaxClassStateInfo> GetTaxClassStates()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns relationship between specified tax class and state.
        /// </summary>
        /// <param name="taxClassId">Tax class ID</param>
        /// <param name="stateId">State ID</param>
        public static TaxClassStateInfo GetTaxClassStateInfo(int taxClassId, int stateId)
        {
            return ProviderObject.GetTaxClassStateInfoInternal(taxClassId, stateId);
        }


        /// <summary>
        /// Sets relationship between specified tax class and state.
        /// </summary>
        /// <param name="infoObj">Tax class-state relationship to be set</param>
        public static void SetTaxClassStateInfo(TaxClassStateInfo infoObj)
        {
            ProviderObject.SetInfo(infoObj);
        }


        /// <summary>
        /// Deletes relationship between specified tax class and state.
        /// </summary>
        /// <param name="infoObj">Tax class-state relationship to be deleted</param>
        public static void DeleteTaxClassStateInfo(TaxClassStateInfo infoObj)
        {
            ProviderObject.DeleteInfo(infoObj);
        }


        /// <summary>
        /// Sets tax value defined for specified state.
        /// </summary>
        /// <param name="taxClassId">Tax class ID</param>
        /// <param name="stateId">Country ID</param>
        /// <param name="value">Tax rate</param>
        public static void SetStateTaxValue(int taxClassId, int stateId, decimal value)
        {
            var info = GetTaxClassStateInfo(taxClassId, stateId);
            info = info ?? new TaxClassStateInfo { StateID = stateId, TaxClassID = taxClassId };

            info.TaxValue = value;

            SetTaxClassStateInfo(info);
        }


        /// <summary>
        /// Removes tax value defined for specified state.
        /// </summary>
        /// <param name="taxClassId">Tax class ID</param>
        /// <param name="stateId">State ID</param>
        public static void RemoveStateTaxValue(int taxClassId, int stateId)
        {
            var infoObj = GetTaxClassStateInfo(taxClassId, stateId);
            DeleteTaxClassStateInfo(infoObj);
        }

        #endregion


        #region "Internal methods - Basic"

        /// <summary>
        /// Returns relationship between specified tax class and state.
        /// </summary>
        /// <param name="taxClassId">Tax class ID</param>
        /// <param name="stateId">State ID</param>
        protected virtual TaxClassStateInfo GetTaxClassStateInfoInternal(int taxClassId, int stateId)
        {
            return GetInfoByFullName(ObjectHelper.BuildFullName(taxClassId.ToString(), stateId.ToString()));
        }

        #endregion


        #region "Internal methods - Advanced"

        /// <summary>
        /// Creates new dictionary for caching the objects by full name.
        /// </summary>
        public ProviderInfoDictionary<string> GetFullNameDictionary()
        {
            return new ProviderInfoDictionary<string>(TaxClassStateInfo.OBJECT_TYPE, "TaxClassID;StateID");
        }


        /// <summary>
        /// Returns where condition for TaxClassState selection using full name (in format TaxClassID.StateID)
        /// </summary>
        /// <param name="fullName">TaxClassState full name</param>
        public string GetFullNameWhereCondition(string fullName)
        {
            string taxClassID;
            string stateID;

            // Parse the full name
            if (ObjectHelper.ParseFullName(fullName, out taxClassID, out stateID))
            {
                return new WhereCondition()
                    .WhereEquals("TaxClassID", ValidationHelper.GetInteger(taxClassID, 0))
                    .WhereEquals("StateID", ValidationHelper.GetInteger(stateID, 0))
                    .ToString(true);
            }

            return null;
        }

        #endregion
    }
}