using System;
using System.Linq;
using System.Text;

using CMS;
using CMS.DocumentEngine;
using CMS.StrandsRecommender;

[assembly: RegisterImplementation(typeof(IStrandsCatalogCategoryMapper), typeof(DefaultStrandsCatalogCategoryMapper), Priority = CMS.Core.RegistrationPriority.Fallback)]

namespace CMS.StrandsRecommender
{
    /// <summary>
    /// Maps properties of the document to the ones used in the Strands catalog feed and other places.
    /// </summary>
    public interface IStrandsCatalogCategoryMapper
    {
        /// <summary>
        /// Retrieves property which will be used as category field in the Strands catalog feed.
        /// </summary>
        /// <param name="catalogItem">Document</param>
        /// <returns>Category field</returns>
        string GetItemCategory(TreeNode catalogItem);
    }
}
