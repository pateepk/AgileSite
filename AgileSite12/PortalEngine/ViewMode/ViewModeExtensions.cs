using System;
using System.Linq;
using System.Text;

namespace CMS.PortalEngine
{
    /// <summary>
    /// Extensions for the view mode enum
    /// </summary>
    public static class ViewModeExtensions
    {
        /// <summary>
        /// Returns true if the view mode is preview view mode
        /// </summary>
        /// <param name="viewMode">View mode to check</param>
        public static bool IsPreview(this ViewModeEnum viewMode)
        {
            return viewMode == ViewModeEnum.Preview;
        }
        

        /// <summary>
        /// Returns true if the view mode is editing view mode for live site
        /// </summary>
        /// <param name="viewMode">View mode to check</param>
        public static bool IsEditLive(this ViewModeEnum viewMode)
        {
            return viewMode == ViewModeEnum.EditLive;
        }


        /// <summary>
        /// Returns true if the view mode is design view mode
        /// </summary>
        /// <param name="viewMode">View mode to check</param>
        /// <param name="includeDesignDisabled">if set to <c>true</c> returns <c>true</c> for DesignDisabled as well.</param>
        public static bool IsDesign(this ViewModeEnum viewMode, bool includeDesignDisabled = false)
        {
            bool isDesign = (viewMode == ViewModeEnum.Design);
            if (includeDesignDisabled)
            {
                isDesign |= (viewMode == ViewModeEnum.DesignDisabled);
            }

            return isDesign;
        }


        /// <summary>
        /// Returns true if the view mode is editing view mode
        /// </summary>
        /// <param name="viewMode">View mode to check</param>
        /// <param name="includeEditDisabled">if set to <c>true</c> returns <c>true</c> for EditDisabled as well.</param>
        public static bool IsEdit(this ViewModeEnum viewMode, bool includeEditDisabled = false)
        {
            bool isEdit = (viewMode == ViewModeEnum.Edit);
            if (includeEditDisabled)
            {
                isEdit |= (viewMode == ViewModeEnum.EditDisabled);
            }

            return isEdit;
        }


        /// <summary>
        /// Returns true if the view mode is live site view mode
        /// </summary>
        /// <param name="viewMode">View mode to check</param>
        public static bool IsLiveSite(this ViewModeEnum viewMode)
        {
            return viewMode == ViewModeEnum.LiveSite;
        }


        /// <summary>
        /// Returns true if the view mode is one of the given view modes
        /// </summary>
        /// <param name="viewMode">View mode to check</param>
        /// <param name="modes">View modes to match</param>
        public static bool IsOneOf(this ViewModeEnum viewMode, params ViewModeEnum[] modes)
        {
            foreach (var vm in modes)
            {
                if (viewMode == vm)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
