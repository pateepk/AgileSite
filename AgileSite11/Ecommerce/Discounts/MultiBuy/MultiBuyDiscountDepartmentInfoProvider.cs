using System.Linq;

using CMS.DataEngine;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Class providing <see cref="MultiBuyDiscountDepartmentInfo"/> management.
    /// </summary>
    public class MultiBuyDiscountDepartmentInfoProvider : AbstractInfoProvider<MultiBuyDiscountDepartmentInfo, MultiBuyDiscountDepartmentInfoProvider>
    {
        #region "Public methods"

        /// <summary>
        /// Returns all MultiBuyDiscountDepartmentInfo bindings.
        /// </summary>
        public static ObjectQuery<MultiBuyDiscountDepartmentInfo> GetMultiBuyDiscountDepartments()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns MultiBuyDiscountDepartmentInfo binding structure.
        /// </summary>
        /// <param name="multiBuyDiscountId">Multi buy discount ID</param>
        /// <param name="departmentId">Department ID</param>  
        public static MultiBuyDiscountDepartmentInfo GetMultiBuyDiscountDepartmentInfo(int multiBuyDiscountId, int departmentId)
        {
            return ProviderObject.GetMultiBuyDiscountDepartmentInfoInternal(multiBuyDiscountId, departmentId);
        }


        /// <summary>
        /// Sets specified MultiBuyDiscountDepartmentInfo.
        /// </summary>
        /// <remarks>
        /// Seting the <see cref="MultiBuyDiscountDepartmentInfo"/> does not affect other existing info objects like <see cref="MultiBuyDiscountSKUInfo"/> or <see cref="MultiBuyDiscountBrandInfo"/> or <see cref="MultiBuyDiscountCollectionInfo"/> or <see cref="MultiBuyDiscountTreeInfo"/>
        /// This could lead to extended discount application than needed.
        /// </remarks>
        /// <param name="infoObj">MultiBuyDiscountDepartmentInfo to set</param>
        public static void SetMultiBuyDiscountDepartmentInfo(MultiBuyDiscountDepartmentInfo infoObj)
        {
            ProviderObject.SetInfo(infoObj);
        }


        /// <summary>
        /// Deletes specified MultiBuyDiscountDepartmentInfo binding.
        /// </summary>
        /// <remarks>
        /// Deleting the <see cref="MultiBuyDiscountDepartmentInfo"/> does not affect other existing info objects like <see cref="MultiBuyDiscountSKUInfo"/> or <see cref="MultiBuyDiscountBrandInfo"/> or <see cref="MultiBuyDiscountCollectionInfo"/> or <see cref="MultiBuyDiscountTreeInfo"/>
        /// This could lead to extended discount application than needed.
        /// </remarks>
        /// <param name="infoObj">MultiBuyDiscountDepartmentInfo object</param>
        public static void DeleteMultiBuyDiscountDepartmentInfo(MultiBuyDiscountDepartmentInfo infoObj)
        {
            ProviderObject.DeleteInfo(infoObj);
        }


        /// <summary>
        /// Deletes MultiBuyDiscountDepartmentInfo binding.
        /// </summary>
        /// <remarks>
        /// Removing the <see cref="MultiBuyDiscountDepartmentInfo"/> does not affect other existing info objects like <see cref="MultiBuyDiscountSKUInfo"/> or <see cref="MultiBuyDiscountBrandInfo"/> or <see cref="MultiBuyDiscountCollectionInfo"/> or <see cref="MultiBuyDiscountTreeInfo"/>
        /// This could lead to extended discount application than needed.
        /// </remarks>
        /// <param name="multiBuyDiscountId">Multi buy discount ID</param>
        /// <param name="departmentId">Department ID</param>  
        public static void RemoveMultiBuyDiscountFromDepartment(int multiBuyDiscountId, int departmentId)
        {
            ProviderObject.RemoveMultiBuyDiscountFromDepartmentInternal(multiBuyDiscountId, departmentId);
        }


        /// <summary>
        /// Creates MultiBuyDiscountDepartmentInfo binding. 
        /// </summary>
        /// <remarks>
        /// Adding the <see cref="MultiBuyDiscountDepartmentInfo"/> does not affect other existing info objects like <see cref="MultiBuyDiscountSKUInfo"/> or <see cref="MultiBuyDiscountBrandInfo"/> or <see cref="MultiBuyDiscountCollectionInfo"/> or <see cref="MultiBuyDiscountTreeInfo"/>
        /// This could lead to extended discount application than needed.
        /// </remarks>
        /// <param name="multiBuyDiscountId">Multi buy discount ID</param>
        /// <param name="departmentId">Department ID</param>   
        public static void AddMultiBuyDiscountToDepartment(int multiBuyDiscountId, int departmentId)
        {
            ProviderObject.AddMultiBuyDiscountToDepartmentInternal(multiBuyDiscountId, departmentId);
        }

        #endregion


        #region "Internal methods"

        /// <summary>
        /// Returns the MultiBuyDiscountDepartmentInfo structure.
        /// Null if binding doesn't exist.
        /// </summary>
        /// <param name="multiBuyDiscountId">Multi buy discount ID</param>
        /// <param name="departmentId">Department ID</param>  
        protected virtual MultiBuyDiscountDepartmentInfo GetMultiBuyDiscountDepartmentInfoInternal(int multiBuyDiscountId, int departmentId)
        {
            return ProviderObject.GetObjectQuery().TopN(1)
                .WhereEquals("MultiBuyDiscountID", multiBuyDiscountId)
                .WhereEquals("DepartmentID", departmentId)
                .FirstOrDefault();
        }


        /// <summary>
        /// Deletes MultiBuyDiscountDepartmentInfo binding.
        /// </summary>
        /// <param name="multiBuyDiscountId">Multi buy discount ID</param>
        /// <param name="departmentId">Department ID</param>  
        protected virtual void RemoveMultiBuyDiscountFromDepartmentInternal(int multiBuyDiscountId, int departmentId)
        {
            var infoObj = GetMultiBuyDiscountDepartmentInfo(multiBuyDiscountId, departmentId);
            if (infoObj != null)
            {
                DeleteMultiBuyDiscountDepartmentInfo(infoObj);
            }
        }


        /// <summary>
        /// Creates MultiBuyDiscountDepartmentInfo binding. 
        /// </summary>
        /// <param name="multiBuyDiscountId">Multi buy discount ID</param>
        /// <param name="departmentId">Department ID</param>   
        protected virtual void AddMultiBuyDiscountToDepartmentInternal(int multiBuyDiscountId, int departmentId)
        {
            var infoObj = new MultiBuyDiscountDepartmentInfo
            {
                MultiBuyDiscountID = multiBuyDiscountId,
                DepartmentID = departmentId
            };

            SetMultiBuyDiscountDepartmentInfo(infoObj);
        }

        #endregion
    }
}