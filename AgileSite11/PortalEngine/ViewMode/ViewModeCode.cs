using System;

using CMS.Base;

namespace CMS.PortalEngine
{
    /// <summary>
    /// Page mode code.
    /// </summary>
    public static class ViewModeCode
    {
        #region "Methods"

        /// <summary>
        /// Gets the view mode enumeration from the string value. Supports only page view modes
        /// </summary>
        /// <param name="viewMode">View mode</param>
        public static ViewModeEnum GetPageEnumFromString(string viewMode)
        {
            switch (viewMode.ToLowerCSafe())
            {
                case "design":
                    return ViewModeEnum.Design;

                case "preview":
                    return ViewModeEnum.Preview;

                case "edit":
                    return ViewModeEnum.Edit;

                case "editlive":
                    return ViewModeEnum.EditLive;

                case "sectionedit":
                    return ViewModeEnum.SectionEdit;
            }

            return ViewModeEnum.LiveSite;
        }


        /// <summary>
        /// Returns the enumeration representation of the Page mode code.
        /// </summary>
        /// <param name="code">View mode code</param>
        public static ViewModeEnum ToEnum(int code)
        {
            try
            {
                return (ViewModeEnum)code;
            }
            catch
            {
                return ViewModeEnum.LiveSite;
            }
        }


        /// <summary>
        /// Returns the page mode code from the enumeration value.
        /// </summary>
        /// <param name="value">Value to convert</param>
        public static int FromEnum(ViewModeEnum value)
        {
            return (int)value;
        }


        /// <summary>
        /// Gets the view mode enumeration from the given string
        /// </summary>
        /// <param name="mode">String mode to convert</param>
        public static ViewModeEnum FromString(string mode)
        {
            ViewModeEnum result;

            if (Enum.TryParse(mode, true, out result))
            {
                return result;
            }

            return ViewModeEnum.Unknown;
        }


        /// <summary>
        /// Returns TRUE if given mode is subset of 'Edit' mode.
        /// </summary>
        /// <param name="mode">Mode to evaluate</param>
        public static bool IsSubsetOfEditMode(ViewModeEnum mode)
        {
            switch (mode)
            {
                case ViewModeEnum.EditForm:
                case ViewModeEnum.Edit:
                case ViewModeEnum.EditLive:
                case ViewModeEnum.Design:
                    return true;
            }
            return false;
        }

        #endregion
    }
}