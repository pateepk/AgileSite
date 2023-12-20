using CMS.DocumentEngine;

namespace Kentico.PageBuilder.Web.Mvc
{
    /// <summary>
    /// Interface for loading the component view model with data.
    /// </summary>
    internal interface IComponentViewModel
    {
        /// <summary>
        /// Loads the model properties.
        /// </summary>
        /// <param name="page">The page where is the component used.</param>
        /// <param name="properties">Component properties.</param>
        void LoadProperties(TreeNode page, IComponentProperties properties);
    }
}
