using System;

using CMS.Base;

namespace CMS.PortalEngine
{


    #region "Variant mode enum"

    /// <summary>
    /// Variant mode enumeration.
    /// </summary>
    public enum VariantModeEnum
    {
        /// <summary>
        /// MVT
        /// </summary>
        MVT = 0,

        /// <summary>
        /// Content personalization
        /// </summary>
        ContentPersonalization = 1,

        /// <summary>
        /// Both content personalization and MVT modes together - this is not allowed
        /// </summary>
        Conflicted = 2,

        /// <summary>
        /// No variant mode set
        /// </summary>
        None = 3
    }

    #endregion


    #region "Variant mode enum functions"

    /// <summary>
    /// Variant mode enumeration support functions.
    /// </summary>
    public static class VariantModeFunctions
    {
        /// <summary>
        /// Returns VariantModeEnum enum.
        /// </summary>
        /// <param name="variantMode">The variant mode</param>
        public static VariantModeEnum GetVariantModeEnum(string variantMode)
        {
            if (string.IsNullOrEmpty(variantMode))
            {
                return VariantModeEnum.None;
            }

            switch (variantMode.ToLowerCSafe())
            {
                case "mvt":
                    return VariantModeEnum.MVT;

                case "personalization":
                    return VariantModeEnum.ContentPersonalization;

                case "conflicted":
                    return VariantModeEnum.Conflicted;

                default:
                    return VariantModeEnum.None;
            }
        }


        /// <summary>
        /// Returns VariantModo string.
        /// </summary>
        /// <param name="variantMode">The variant mode</param>
        public static string GetVariantModeString(VariantModeEnum variantMode)
        {
            switch (variantMode)
            {
                case VariantModeEnum.MVT:
                    return "mvt";

                case VariantModeEnum.ContentPersonalization:
                    return "personalization";

                case VariantModeEnum.Conflicted:
                    return "conflicted";

                default:
                    return "none";
            }
        }
    }

    #endregion
}