using System;
using System.Collections.Generic;

using CMS.DataEngine;
using CMS.Newsletters.Issues.Widgets.Configuration;
using CMS.Newsletters.Filters;

namespace CMS.Newsletters.Issues.Widgets.Content
{
    /// <summary>
    /// Provides the content for an email.
    /// </summary>
    internal sealed class EmailContentProvider
    {
        private readonly ZonesConfiguration configuration;
        private readonly SiteInfoIdentifier site;
        private readonly IWidgetContentFilter widgetContentFilter;


        /// <summary>
        /// Creates an instance of <see cref="EmailContentProvider"/> class.
        /// </summary>
        /// <param name="configuration">Zones configuration containing widgets.</param>
        /// <param name="site">Site of the issue with widgets.</param>
        /// <param name="widgetContentFilter">Filter applied to the widget content.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="configuration"/> is null.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="site"/> is null.</exception>
        public EmailContentProvider(ZonesConfiguration configuration, SiteInfoIdentifier site, IWidgetContentFilter widgetContentFilter)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            if (site == null)
            {
                throw new ArgumentNullException(nameof(site));
            }

            this.configuration = configuration;
            this.site = site;
            this.widgetContentFilter = widgetContentFilter;
        }


        /// <summary>
        /// Gets the content of an email.
        /// </summary>
        public EmailContent GetContent()
        {
            var emailContent = new EmailContent();
            return AddZonesWithWidgets(emailContent);
        }


        private EmailContent AddZonesWithWidgets(EmailContent emailContent)
        {
            foreach (var zone in configuration.Zones)
            {
                var contentZone = new EmailZone(zone.Identifier);

                contentZone = AddWidgetsToZone(contentZone, zone.Widgets);
                emailContent.Zones.Add(contentZone);
            }

            return emailContent;
        }


        private EmailZone AddWidgetsToZone(EmailZone contentZone, IEnumerable<Widget> widgets)
        {
            foreach (var widget in widgets)
            {
                var widgetContent = GetWidgetContent(widget);
                contentZone.Widgets.Add(widgetContent);
            }

            return contentZone;
        }


        private WidgetContent GetWidgetContent(Widget widget)
        {
            return new WidgetContentProvider(site, widgetContentFilter).GetContent(widget);
        }
    }
}
