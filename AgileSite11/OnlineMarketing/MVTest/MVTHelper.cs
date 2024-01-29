using System;
using System.Linq;
using System.Text;

using CMS.PortalEngine;

namespace CMS.OnlineMarketing
{
    /// <summary>
    /// Helper class for MVTests
    /// </summary>
    public class MVTHelper
    {
        /// <summary>
        /// Loads all the MVT/Content personalization variants for this web part instance.
        /// </summary>
        /// <param name="forceLoad">Indicates if already loaded variants should be reloaded</param>
        /// <param name="variantMode">Specifies which variants should be loaded (MVT/ContentPersonalization/None - means both MVT+CP variants should try to load)</param>
        /// <param name="wpInstance">Web part's instance</param>
        /// <param name="documentId">Document ID if the instance holds a widget</param>
        public static void LoadVariants(bool forceLoad, VariantModeEnum variantMode, WebPartInstance wpInstance, int documentId)
        {
            // Load the web part variants
            if (!wpInstance.IsVariant)
            {
                if ((wpInstance.PartInstanceVariants == null) || forceLoad)
                {
                    if ((wpInstance.ParentZone != null)
                        && (wpInstance.ParentZone.ParentTemplateInstance != null)
                        && (wpInstance.ParentZone.ParentTemplateInstance.ParentPageTemplate != null))
                    {
                        // Load the variants only if there are no conflicts between MVT and CP (means - there cannot be MVT and CP variants for the same web part)
                        if (wpInstance.VariantMode != VariantModeEnum.Conflicted)
                        {
                            if (PortalContext.MVTVariantsEnabled && ((variantMode == VariantModeEnum.MVT) || (variantMode == VariantModeEnum.None)))
                            {
                                // Load MVT variants
                                MVTVariantInfoProvider.LoadMVTVariants(wpInstance, false, documentId);
                            }

                            if (PortalContext.ContentPersonalizationEnabled && ((variantMode == VariantModeEnum.ContentPersonalization) || (variantMode == VariantModeEnum.None)))
                            {
                                // Load Content personalization variants
                                ContentPersonalizationVariantInfoProvider.LoadContentPersonalizationVariants(wpInstance, false, documentId);
                            }
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Loads all the MVT/Content personalization variants for this zone instance.
        /// </summary>
        /// <param name="forceLoad">Indicates if already loaded variants should be reloaded</param>
        /// <param name="variantMode">Specifies which variants should be loaded (MVT/ContentPersonalization/None - means both MVT+CP variants should try to load)</param>
        /// <param name="wpZoneInstance">Zone's instance</param>
        public static void LoadZoneVariants(bool forceLoad, VariantModeEnum variantMode, WebPartZoneInstance wpZoneInstance)
        {
            bool loadVariants = (PortalContext.MVTVariantsEnabled || PortalContext.ContentPersonalizationEnabled);

            // Load variants only for the web part zones and the design mode
            if (loadVariants && ((wpZoneInstance.ZoneInstanceVariants == null) || forceLoad))
            {
                // Load zone variants
                if (!wpZoneInstance.IsVariant)
                {
                    if ((wpZoneInstance.ZoneInstanceVariants == null) || forceLoad)
                    {
                        // Load the variants only if there are no conflicts between MVT and CP (means - there cannot be MVT and CP variants for the same web part)
                        if (wpZoneInstance.VariantMode != VariantModeEnum.Conflicted)
                        {
                            if ((wpZoneInstance.ParentTemplateInstance != null)
                                && (wpZoneInstance.ParentTemplateInstance.ParentPageTemplate != null))
                            {
                                if (PortalContext.MVTVariantsEnabled && ((variantMode == VariantModeEnum.MVT) || (variantMode == VariantModeEnum.None)))
                                {
                                    // Load MVT variants and set the flag indicating if MVT variants have been loaded correctly
                                    wpZoneInstance.ParentTemplateInstance.MVTVariantsLoaded = MVTVariantInfoProvider.LoadMVTVariants(wpZoneInstance, true);
                                }

                                if (PortalContext.ContentPersonalizationEnabled && ((variantMode == VariantModeEnum.ContentPersonalization) || (variantMode == VariantModeEnum.None)))
                                {
                                    // Load Content personalization variants and set the flag indicating if Content personalization variants have been loaded correctly
                                    wpZoneInstance.ParentTemplateInstance.ContentPersonalizationVariantsLoaded = ContentPersonalizationVariantInfoProvider.LoadContentPersonalizationVariants(wpZoneInstance, true);
                                }
                            }
                        }

                        // Stop loading variants if there are variants for this zone in MVT and Content personalization together
                        if (wpZoneInstance.VariantMode == VariantModeEnum.Conflicted)
                        {
                            return;
                        }

                        // If no MVT/CP variants have been found for this zone, try to load variants for the zone web parts
                        if (wpZoneInstance.VariantMode == VariantModeEnum.None)
                        {
                            // Try to find web parts' variants
                            foreach (WebPartInstance webpart in wpZoneInstance.WebParts)
                            {
                                // Reset the variant mode if both MVT/CP variants should be loaded
                                if (variantMode == VariantModeEnum.None)
                                {
                                    webpart.VariantMode = VariantModeEnum.None;
                                }

                                webpart.LoadVariants(forceLoad, variantMode);
                            }
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Updates web parts variants 
        /// </summary>
        /// <param name="zone">Zone instance</param>
        /// <param name="targetZone">Target zone instance</param>
        /// <param name="pti">Zone's page template info</param>
        public static void UpdateWebPartsVariants(WebPartZoneInstance zone, WebPartZoneInstance targetZone, PageTemplateInfo pti)
        {
            if ((targetZone.ZoneID != zone.ZoneID) || ((targetZone.ZoneID == zone.ZoneID) && (targetZone.VariantID != zone.VariantID)))
            {
                if ((pti != null) && (pti.TemplateInstance != null))
                {
                    // Update all the web part variants of the source zone with the target zone id
                    if (zone.WebPartsContainVariants)
                    {
                        MVTVariantInfoProvider.UpdateWebPartVariants(pti.PageTemplateId, zone.ZoneID, Guid.Empty, targetZone.ZoneID);

                        ContentPersonalizationVariantInfoProvider.UpdateWebPartVariants(pti.PageTemplateId, zone.ZoneID, Guid.Empty, targetZone.ZoneID);
                    }

                    zone.LoadVariants(true, VariantModeEnum.None);
                    targetZone.LoadVariants(true, VariantModeEnum.None);
                }
            }
        }


        /// <summary>
        /// Indicates whether the given zone instance contains a valid page info object.
        /// </summary>
        /// <param name="zoneInstance">The zone instance.</param>
        internal protected static bool ContainsPageInfoObject(WebPartZoneInstance zoneInstance)
        {
            return ((zoneInstance != null)
                && (zoneInstance.ParentTemplateInstance != null)
                && (zoneInstance.ParentTemplateInstance.ParentPageTemplate != null)
                && (zoneInstance.ParentTemplateInstance.ParentPageTemplate.ParentPageInfo != null));
        }
    }
}
