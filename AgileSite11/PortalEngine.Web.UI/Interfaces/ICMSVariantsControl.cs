using System;

namespace CMS.PortalEngine.Web.UI
{
    /// <summary>
    /// Interface for the portal controls that have variants
    /// </summary>
    public interface ICMSVariantsControl : ICMSPortalControl
    {
        /// <summary>
        /// Returns true if the child components have any variants
        /// </summary>
        bool ChildrenHaveVariants
        {
            get;
        }


        /// <summary>
        /// Returns true if the parent component has any variants
        /// </summary>
        bool ParentHasVariants
        {
            get;
        }


        /// <summary>
        /// Returns true if the component has any variants
        /// </summary>
        bool HasVariants
        {
            get;
        }
    }
}