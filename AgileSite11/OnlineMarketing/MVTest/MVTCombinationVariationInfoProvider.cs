using System;
using System.Linq;

using CMS.DataEngine;

namespace CMS.OnlineMarketing
{
    /// <summary>
    /// Class providing MVTCombinationVariationInfo management.
    /// </summary>
    public class MVTCombinationVariationInfoProvider : AbstractInfoProvider<MVTCombinationVariationInfo, MVTCombinationVariationInfoProvider>
    {
        #region "Public static methods"
        
        /// <summary>
        /// Returns relationship between specified MVT combination and MVT variant.
        /// </summary>
        /// <param name="combinationId">MVT combination ID</param>
        /// <param name="variantId">MVT variant ID</param>
        public static MVTCombinationVariationInfo GetMVTCombinationVariationInfo(int combinationId, int variantId)
        {
            return ProviderObject.GetMVTCombinationVariationInfoInternal(combinationId, variantId);
        }


        /// <summary>
        /// Returns query of all relationships between MVT combinations and MVT variants.
        /// </summary>
        public static ObjectQuery<MVTCombinationVariationInfo> GetCombinationVariations()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Sets relationship between specified MVT combination and MVT variant.
        /// </summary>
        /// <param name="combinationObj">MVT combination - MVT variant relationship to be set</param>
        public static void SetMVTCombinationVariationInfo(MVTCombinationVariationInfo combinationObj)
        {
            ProviderObject.SetInfo(combinationObj);
        }


        /// <summary>
        /// Sets relationship between specified MVT combination and MVT variant.
        /// </summary>	
        /// <param name="combinationId">MVT combination ID</param>
        /// <param name="variantId">MVT variant ID</param>
        public static void AddRelationship(int combinationId, int variantId)
        {
            MVTCombinationVariationInfo infoObj = ProviderObject.CreateInfo();

            infoObj.MVTCombinationID = combinationId;
            infoObj.MVTVariantID = variantId;

            SetMVTCombinationVariationInfo(infoObj);
        }


        /// <summary>
        /// Deletes relationship between specified MVT combination and MVT variant.
        /// </summary>
        /// <param name="infoObj">MVT combination - MVT variant relationship to be deleted</param>
        public static void DeleteMVTCombinationVariationInfo(MVTCombinationVariationInfo infoObj)
        {
            ProviderObject.DeleteInfo(infoObj);
        }


        /// <summary>
        /// Deletes relationship between specified MVT combination and MVT variant using specified condition.
        /// </summary>
        /// <param name="whereCondition">Where condition</param>
        public static void DeleteMVTCombinationVariationInfo(string whereCondition)
        {
            ProviderObject.DeleteMVTCombinationVariationInfoInternal(whereCondition);
        }


        /// <summary>
        /// Deletes relationship between specified MVT combination and MVT variant.
        /// </summary>
        /// <param name="combinationId">MVT combination ID</param>
        /// <param name="variantId">MVT variant ID</param>
        public static void RemoveRelationship(int combinationId, int variantId)
        {
            MVTCombinationVariationInfo infoObj = GetMVTCombinationVariationInfo(combinationId, variantId);
            DeleteMVTCombinationVariationInfo(infoObj);
        }

        #endregion


        #region "Public methods - Advanced"

        /// <summary>
        /// Combines a new variant with existing combinations.
        /// </summary>
        /// <param name="originalCombinationId">The original combination id</param>
        /// <param name="newCombinationId">The new combination id</param>
        /// <param name="newVariantId">The new variant id</param>
        public static void CombineCombinationVariants(int originalCombinationId, int newCombinationId, int newVariantId)
        {
            ProviderObject.CombineCombinationVariantsInternal(originalCombinationId, newCombinationId, newVariantId);
        }

        #endregion


        #region "Internal methods"

        /// <summary>
        /// Returns relationship between specified MVT combination and MVT variant.
        /// </summary>
        /// <param name="combinationId">MVT combination ID</param>
        /// <param name="variantId">MVT variant ID</param>
        protected virtual MVTCombinationVariationInfo GetMVTCombinationVariationInfoInternal(int combinationId, int variantId)
        {
            var condition = new WhereCondition()
                .WhereEquals("MVTCombinationID", combinationId)
                .WhereEquals("MVTVariantID", variantId);

            return GetObjectQuery().TopN(1).Where(condition).FirstOrDefault();
        }


        /// <summary>
        /// Deletes relationship between specified MVT combination and MVT variant using specified condition.
        /// </summary>
        /// <param name="whereCondition">Where condition</param>
        protected virtual void DeleteMVTCombinationVariationInfoInternal(string whereCondition)
        {
            if (whereCondition == null)
            {
                throw new ArgumentNullException("whereCondition");
            }

            BulkDelete(new WhereCondition(whereCondition));
        }

        #endregion


        #region "Internal methods - Advanced"

        /// <summary>
        /// Combines a new variant with existing combinations.
        /// </summary>
        /// <param name="originalCombinationId">The original combination id</param>
        /// <param name="newCombinationId">The new combination id</param>
        /// <param name="newVariantId">The new variant id</param>
        protected virtual void CombineCombinationVariantsInternal(int originalCombinationId, int newCombinationId, int newVariantId)
        {
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@OriginalCombinationID", originalCombinationId);
            parameters.Add("@NewCombinationID", newCombinationId);
            parameters.Add("@NewVariantID", newVariantId);

            ConnectionHelper.ExecuteQuery("OM.MVTCombinationVariation.CombineCombinationVariation", parameters);
        }

        #endregion
    }
}