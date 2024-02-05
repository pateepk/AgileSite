using System;

using CMS.Base;

namespace CMS.OnlineMarketing
{
    /// <summary>
    /// Variant type enumeration.
    /// </summary>
    public enum VariantTypeEnum
    {
        /// <summary>
        /// Zone
        /// </summary>
        Zone = 0,

        /// <summary>
        /// Web part
        /// </summary>
        WebPart = 1,

        /// <summary>
        /// Widget
        /// </summary>
        Widget = 2
    }

    /// <summary>
    /// Helper functions for webpart/zone/widget variant type enum.
    /// </summary>
    public static class VariantTypeFunctions
    {
        /// <summary>
        /// Returns VariantTypeEnum enum.
        /// </summary>
        /// <param name="variantType">The variant type</param>
        public static VariantTypeEnum GetVariantTypeEnum(string variantType)
        {
            if (string.IsNullOrEmpty(variantType))
            {
                return VariantTypeEnum.Zone;
            }

            switch (variantType.ToLowerCSafe())
            {
                case "webpart":
                    return VariantTypeEnum.WebPart;

                case "widget":
                    return VariantTypeEnum.Widget;

                default:
                    return VariantTypeEnum.Zone;
            }
        }


        /// <summary>
        /// Returns VariantType string.
        /// </summary>
        /// <param name="variantType">The variant type</param>
        public static string GetVariantTypeString(VariantTypeEnum variantType)
        {
            switch (variantType)
            {
                case VariantTypeEnum.WebPart:
                    return "webpart";

                case VariantTypeEnum.Widget:
                    return "widget";

                default:
                    return "zone";
            }
        }
    }
}