using System;
using System.Linq;
using System.Text;

using CMS.Core;
using CMS.DocumentEngine;

namespace CMS.StrandsRecommender
{
    /// <summary>
    /// Methods in this class map properties of the document to the ones used in the Strands catalog feed.
    /// </summary>
    public static class StrandsCatalogPropertiesMapper
    {
        /// <summary>
        /// Retrieves document NodeID which will be used as ItemID in the Strands catalog feed.
        /// Behavior of this method can't be changed because implementation of Strands integration is dependent on it.
        /// </summary>
        /// <param name="catalogItem">Document</param>
        /// <returns>ID of the catalog item</returns>
        /// <exception cref="ArgumentNullException">Parameter <paramref name="catalogItem"/> is null</exception>
        public static string GetItemID(TreeNode catalogItem)
        {
            if (catalogItem == null)
            {
                throw new ArgumentNullException("catalogItem");
            }

            return catalogItem.NodeID.ToString();
        }


        /// <summary>
        /// Retrieves NodeClassID which will be used as category field in the Strands catalog feed.
        /// Returned value can be changed easily by using different implementation of IStrandsCatalogCategoryMapper.
        /// This can be done by calling ObjectFactory&lt;IStrandsCatalogCategoryMapper&gt;.SetDefaultObjectTypeTo&lt;NameOfDifferentImplementation&gt;() somewhere during Init phase.
        /// </summary>
        /// <param name="catalogItem">Document</param>
        /// <returns>Category field specified by implementation of <see cref="IStrandsCatalogCategoryMapper"/></returns>
        /// <exception cref="ArgumentNullException">Parameter <paramref name="catalogItem"/> is null</exception>
        public static string GetItemCategory(TreeNode catalogItem)
        {
            if (catalogItem == null)
            {
                throw new ArgumentNullException("catalogItem");
            }
            
            return Service.Resolve<IStrandsCatalogCategoryMapper>().GetItemCategory(catalogItem);
        }
    }
}
