using System;

using CMS.Base;

namespace CMS.DocumentEngine.Web.UI
{
    /// <summary>
    /// Determines what type of server control is displayed in the editable region.
    /// </summary>
    public enum CMSEditableRegionTypeEnum
    {
        /// <summary>
        /// Single-line textbox.
        /// </summary>
        TextBox = 0,

        /// <summary>
        /// Multi-line textbox.
        /// </summary>
        TextArea = 1,

        /// <summary>
        /// HTML editor control.
        /// </summary>
        HtmlEditor = 2
    }


    /// <summary>
    /// Helper functions for CMSEditableRegionType enum.
    /// </summary>
    public static class CMSEditableRegionTypeEnumFunctions
    {
        /// <summary>
        /// Returns CMSEditableRegionType enum.
        /// </summary>
        /// <param name="regionType">The region type</param>
        public static CMSEditableRegionTypeEnum GetRegionTypeEnum(string regionType)
        {
            if (string.IsNullOrEmpty(regionType))
            {
                return CMSEditableRegionTypeEnum.TextBox;
            }

            switch (regionType.ToLowerCSafe())
            {
                case "htmleditor":
                    return CMSEditableRegionTypeEnum.HtmlEditor;
                case "textarea":
                    return CMSEditableRegionTypeEnum.TextArea;
                default:
                    return CMSEditableRegionTypeEnum.TextBox;
            }
        }


        /// <summary>
        /// Returns RegionType string.
        /// </summary>
        /// <param name="regionType">The region type</param>
        public static string GetRegionTypeString(CMSEditableRegionTypeEnum regionType)
        {
            switch (regionType)
            {
                case CMSEditableRegionTypeEnum.HtmlEditor:
                    return "htmleditor";
                case CMSEditableRegionTypeEnum.TextArea:
                    return "textarea";
                default:
                    return "textbox";
            }
        }
    }
}