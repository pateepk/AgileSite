namespace CMS.OnlineMarketing.Internal
{
    /// <summary>
    /// Source of the configuration for A/B test variant.
    /// </summary>
    public class VariantConfigurationSource
    {
        /// <summary>
        /// Page builder widgets configuration.
        /// </summary>
        public string Widgets { get; set; }


        /// <summary>
        /// Page template configuration.
        /// </summary>
        public string PageTemplate { get; set; }
    }
}
