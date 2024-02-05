using System;

using CMS.Base;
using CMS.Helpers;
using CMS.Modules;

namespace CMS.UIControls
{
    /// <summary>
    /// Provides functionality for working with applications.
    /// </summary>
    public class ApplicationCSSHelper
    {
        /// <summary>
        /// Returns application icon CSS class.
        /// </summary>
        /// <param name="elementGuid">UI element GUID</param>
        public static string GetApplicationIconCssClass(Guid elementGuid)
        {
            UIElementInfo elementInfo = UIElementInfoProvider.GetUIElementInfo(elementGuid);
            if (elementInfo == null)
            {
                return String.Empty;
            }

            string className = String.Empty;

            if (elementInfo.IsApplication)
            {
                UIElementInfo parentInfo = UIElementInfoProvider.GetUIElementInfo(elementInfo.ElementParentID);
                if (parentInfo != null)
                {
                    string categoryCodeName = ValidationHelper.GetIdentifier(parentInfo.ElementName, "-");
                    string appCodeName = ValidationHelper.GetIdentifier(elementInfo.ElementName, "-");
                    className = "cat-" + categoryCodeName.ToLowerCSafe() + " app-" + appCodeName.ToLowerCSafe();
                    className = className.Replace('_', '-');
                }
            }

            return className;
        }
    }
}
