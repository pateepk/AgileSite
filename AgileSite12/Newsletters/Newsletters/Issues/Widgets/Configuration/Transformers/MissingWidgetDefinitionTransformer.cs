using CMS.DataEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CMS.Newsletters.Issues.Widgets.Configuration
{
    /// <summary>
    /// Mark widgets with missing widget definition <see cref="EmailWidgetInfo"/>.
    /// </summary>
    internal sealed class MissingWidgetDefinitionZoneConfigurationTransformer : IZonesConfigurationTransformer
    {
        private readonly SiteInfoIdentifier site;


        /// <summary>
        /// Instantiate a new filter using for marking widgets without definition.
        /// </summary>
        /// <param name="site"></param>
        public MissingWidgetDefinitionZoneConfigurationTransformer(SiteInfoIdentifier site)
        {
            this.site = site;
        }

        /// <summary>
        /// Marks widgets with missing definition.
        /// </summary>
        /// <param name="configuration"></param>
        public ZonesConfiguration Transform(ZonesConfiguration configuration)
        {
            var usedWidgetTypeIdentifiers = configuration.Zones
                .SelectMany(zone => zone
                    .Widgets.Select(widget => widget.TypeIdentifier))
                .Distinct()
                .ToList();

            var emailWidgetDefinitions = EmailWidgetInfoProvider.GetEmailWidgets()
                .Columns("EmailWidgetGuid")
                .WhereEquals("EmailWidgetSiteId", site.ObjectID)
                .WhereIn("EmailWidgetGuid", usedWidgetTypeIdentifiers)
                .GetListResult<Guid>();

            if (emailWidgetDefinitions.Count == usedWidgetTypeIdentifiers.Count)
            {
                return configuration;
            }

            var missingWidgetDefinitions = usedWidgetTypeIdentifiers.Except(emailWidgetDefinitions);
            return UpdateMissingWidgetConfiguration(configuration, missingWidgetDefinitions);
        }


        private static ZonesConfiguration UpdateMissingWidgetConfiguration(ZonesConfiguration configuration, IEnumerable<Guid> missingWidgetDefinitions)
        {
            configuration.Zones
                .SelectMany(zone => zone
                    .Widgets.Where(widget => missingWidgetDefinitions.Contains(widget.TypeIdentifier)))
                .ToList()
                .ForEach(i => i.WidgetDefinitionNotFound = true);
            return configuration;
        }
    }
}
