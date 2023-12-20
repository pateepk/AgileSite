using CMS.DocumentEngine;

namespace Kentico.PageBuilder.Web.Mvc
{
    /// <summary>
    /// View model for a component without properties.
    /// </summary>
    public class ComponentViewModel : IComponentViewModel
    {
        /// <summary>
        /// Page where the component is placed.
        /// </summary>
        public TreeNode Page { get; internal set; }


        /// <summary>
        /// Loads the model properties.
        /// </summary>
        /// <param name="page">The page where is the component used.</param>
        /// <param name="properties">Component properties.</param>
        void IComponentViewModel.LoadProperties(TreeNode page, IComponentProperties properties)
        {
            Page = page;
        }
    }


    /// <summary>
    /// View model for a component with properties.
    /// </summary>
    /// <typeparam name="TPropertiesType">Type of the component properties.</typeparam>
    public class ComponentViewModel<TPropertiesType> : ComponentViewModel, IComponentViewModel
             where TPropertiesType : class, IComponentProperties, new()
    {
        /// <summary>
        /// Component properties.
        /// </summary>
        public TPropertiesType Properties { get; set; }


        /// <summary>
        /// Loads the model properties.
        /// </summary>
        /// <param name="page">The page where is the component used.</param>
        /// <param name="properties">Component properties.</param>
        void IComponentViewModel.LoadProperties(TreeNode page, IComponentProperties properties)
        {
            Page = page;
            Properties = properties as TPropertiesType;
        }
    }
}
