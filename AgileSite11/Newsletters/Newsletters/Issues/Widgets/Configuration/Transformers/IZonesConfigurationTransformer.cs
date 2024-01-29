namespace CMS.Newsletters.Issues.Widgets.Configuration
{
    internal interface IZonesConfigurationTransformer
    {
        /// <summary>
        /// Transforms <see cref="ZonesConfiguration"/>.
        /// </summary>
        /// <param name="configuration">Zones configuration.</param>
        ZonesConfiguration Transform(ZonesConfiguration configuration);
    }
}
