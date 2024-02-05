using System;
using System.Linq;
using System.Text;

using CMS.DocumentEngine;

namespace CMS.StrandsRecommender
{
    /// <summary>
    /// Default implementation of <see cref="IStrandsCatalogCategoryMapper"/> which returns NodeClassID as category field.
    /// </summary>
    public sealed class DefaultStrandsCatalogCategoryMapper : IStrandsCatalogCategoryMapper
    {
        /// <summary>
        /// Retrieves NodeClassID which will be used as category field in the Strands catalog feed.
        /// </summary>
        /// <param name="catalogItem">Document</param>
        /// <returns>Category field</returns>
        /// <exception cref="ArgumentNullException">Parameter <paramref name="catalogItem"/> is null</exception>
        public string GetItemCategory(TreeNode catalogItem)
        {
            if (catalogItem == null)
            {
                throw new ArgumentNullException("catalogItem");
            }

            return catalogItem.GetValue("NodeClassID", "");
        }
    }
}
