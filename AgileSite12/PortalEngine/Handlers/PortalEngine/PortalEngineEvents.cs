using System;
using System.Linq;
using System.Text;

namespace CMS.PortalEngine
{
    /// <summary>
    /// Events for portal engine
    /// </summary>
    public class PortalEngineEvents
    {
        /// <summary>
        /// Fired when request for availability of variants is raised
        /// </summary>
        public static MVTVariantsEnabledHandler MVTVariantsEnabled = new MVTVariantsEnabledHandler { Name = "PortalEngineEvents.MVTVariantsEnabled" };

        /// <summary>
        /// Fired when request for variants collection is raised.
        /// </summary>
        public static GetVariantsHandler GetVariants = new GetVariantsHandler { Name = "PortalEngineEvents.GetVariants" };

        /// <summary>
        /// Fired when request for single variant is raised.
        /// </summary>
        public static GetVariantHandler GetVariant = new GetVariantHandler { Name = "PortalEngineEvents.GetVariant" };

        /// <summary>
        /// Fired when variant is to be saved.
        /// </summary>
        public static SetVariantHandler SetVariantWebParts = new SetVariantHandler { Name = "PortalEngineEvents.SetVariantWebParts" };

        /// <summary>
        /// Fired when variant's properties are to be saved.
        /// </summary>
        public static SetVariantHandler SetVariant = new SetVariantHandler { Name = "PortalEngineEvents.SetVariant" };

        /// <summary>
        /// Fired when variants are to be reseted.
        /// </summary>
        public static DeleteWidgetVariantsHandler DeleteWidgetVariants = new DeleteWidgetVariantsHandler { Name = "PortalEngineEvents.DeleteWidgetVariants" };
    }
}
