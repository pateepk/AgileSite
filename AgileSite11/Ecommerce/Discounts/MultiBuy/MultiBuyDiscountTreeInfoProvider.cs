using System.Linq;

using CMS.DataEngine;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Class providing <see cref="MultiBuyDiscountTreeInfo"/> management.
    /// </summary>
    public class MultiBuyDiscountTreeInfoProvider : AbstractInfoProvider<MultiBuyDiscountTreeInfo, MultiBuyDiscountTreeInfoProvider>
    {
        /// <summary>
        /// Returns all <see cref="MultiBuyDiscountTreeInfo"/> bindings.
        /// </summary>
        public static ObjectQuery<MultiBuyDiscountTreeInfo> GetMultiBuyDiscountTrees()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns <see cref="MultiBuyDiscountTreeInfo"/> binding structure.
        /// </summary>
        /// <param name="multibuydiscountId">Buy X Get Y discount ID.</param>
        /// <param name="nodeId">Node ID.</param>  
        public static MultiBuyDiscountTreeInfo GetMultiBuyDiscountTreeInfo(int multibuydiscountId, int nodeId)
        {
            return ProviderObject.GetObjectQuery().TopN(1)
                .WhereEquals("MultiBuyDiscountID", multibuydiscountId)
                .WhereEquals("NodeID", nodeId)
                .FirstOrDefault();
        }


        /// <summary>
        /// Sets specified <see cref="MultiBuyDiscountTreeInfo"/>.
        /// </summary>
        /// <remarks>
        /// Seting the <see cref="MultiBuyDiscountTreeInfo"/> does not affect other existing info objects like <see cref="MultiBuyDiscountSKUInfo"/> or <see cref="MultiBuyDiscountBrandInfo"/> or <see cref="MultiBuyDiscountCollectionInfo"/> or <see cref="MultiBuyDiscountDepartmentInfo"/>
        /// This could lead to extended discount application than needed.
        /// </remarks>
        /// <param name="infoObj"><see cref="MultiBuyDiscountTreeInfo"/> to set.</param>
        public static void SetMultiBuyDiscountTreeInfo(MultiBuyDiscountTreeInfo infoObj)
        {
            ProviderObject.SetInfo(infoObj);
        }


        /// <summary>
        /// Deletes specified <see cref="MultiBuyDiscountTreeInfo"/> binding.
        /// </summary>
        /// <remarks>
        /// Deleting the <see cref="MultiBuyDiscountTreeInfo"/> does not affect other existing info objects like <see cref="MultiBuyDiscountSKUInfo"/> or <see cref="MultiBuyDiscountBrandInfo"/> or <see cref="MultiBuyDiscountCollectionInfo"/> or <see cref="MultiBuyDiscountDepartmentInfo"/>
        /// This could lead to extended discount application than needed.
        /// </remarks>
        /// <param name="infoObj"><see cref="MultiBuyDiscountTreeInfo"/> object.</param>
        public static void DeleteMultiBuyDiscountTreeInfo(MultiBuyDiscountTreeInfo infoObj)
        {
            ProviderObject.DeleteInfo(infoObj);
        }


        /// <summary>
        /// Deletes <see cref="MultiBuyDiscountTreeInfo"/> binding.
        /// </summary>
        /// <remarks>
        /// Removing the <see cref="MultiBuyDiscountTreeInfo"/> does not affect other existing info objects like <see cref="MultiBuyDiscountSKUInfo"/> or <see cref="MultiBuyDiscountBrandInfo"/> or <see cref="MultiBuyDiscountCollectionInfo"/> or <see cref="MultiBuyDiscountDepartmentInfo"/>
        /// This could lead to extended discount application than needed.
        /// </remarks>
        /// <param name="multibuydiscountId">Buy X Get Y discount ID.</param>
        /// <param name="nodeId">Node ID.</param>  
        public static void RemoveMultiBuyDiscountFromTree(int multibuydiscountId, int nodeId)
        {
            var infoObj = GetMultiBuyDiscountTreeInfo(multibuydiscountId, nodeId);
            if (infoObj != null)
            {
                DeleteMultiBuyDiscountTreeInfo(infoObj);
            }
        }


        /// <summary>
        /// Creates <see cref="MultiBuyDiscountTreeInfo"/> binding.
        /// </summary>
        /// <remarks>
        /// Adding the <see cref="MultiBuyDiscountTreeInfo"/> does not affect other existing info objects like <see cref="MultiBuyDiscountSKUInfo"/> or <see cref="MultiBuyDiscountBrandInfo"/> or <see cref="MultiBuyDiscountCollectionInfo"/> or <see cref="MultiBuyDiscountDepartmentInfo"/>
        /// This could lead to extended discount application than needed.
        /// </remarks>
        /// <param name="multibuydiscountId">Buy X Get Y discount ID.</param>
        /// <param name="nodeId">Node ID.</param>
        /// <param name="included">Include flag.</param>   
        public static void AddMultiBuyDiscountToTree(int multibuydiscountId, int nodeId, bool included)
        {
            // Create new binding
            var infoObj = new MultiBuyDiscountTreeInfo();
            infoObj.MultiBuyDiscountID = multibuydiscountId;
            infoObj.NodeID = nodeId;
            infoObj.NodeIncluded = included;

            // Save to the database
            SetMultiBuyDiscountTreeInfo(infoObj);
        }
    }
}