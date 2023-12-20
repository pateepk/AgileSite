using System;
using System.Linq;
using System.Collections.Specialized;

using CMS;
using CMS.Core;
using CMS.DataEngine;
using CMS.Newsletters.Filters;
using CMS.Newsletters.Issues.Widgets.Configuration;
using CMS.Newsletters.Issues.Widgets.Content;

[assembly: RegisterImplementation(typeof(IZonesConfigurationService), typeof(ZonesConfigurationService), Priority = CMS.Core.RegistrationPriority.SystemDefault)]

namespace CMS.Newsletters.Issues.Widgets.Configuration
{
    /// <summary>
    /// Service for managing the <see cref="IssueInfo.IssueWidgets"/> configuration.
    /// </summary>
    internal sealed class ZonesConfigurationService : IZonesConfigurationService
    {
        private readonly IDataContractSerializerService<ZonesConfiguration> zonesConfigurationXmlSerializer = Service.Resolve<IDataContractSerializerService<ZonesConfiguration>>();
        private readonly ZonesConfigurationManager configurationManager;
        private readonly IssueInfo issue;
        private readonly WidgetCreator widgetCreator;


        /// <summary>
        /// Creates an instance of <see cref="ZonesConfigurationService"/> class.
        /// </summary>
        /// <param name="issue">Issue with widgets to manage.</param>
        /// <param name="guidGenerator">Custom generator used for <see cref="Widget.Identifier"/> when creating a new instance of <see cref="Widget"/>. If not provided, default <see cref="GuidGenerator"/> is used.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="issue"/> is null.</exception>
        public ZonesConfigurationService(IssueInfo issue, GuidGenerator guidGenerator = null)
        {
            this.issue = issue ?? throw new ArgumentNullException(nameof(issue));
            guidGenerator = guidGenerator ?? new GuidGenerator();

            var configuration = GetConfiguration();
            var missingWidgetDefinitionTransformer = new MissingWidgetDefinitionZoneConfigurationTransformer(issue.IssueSiteID);
            configurationManager = new ZonesConfigurationManager(configuration, missingWidgetDefinitionTransformer);

            widgetCreator = new WidgetCreator(issue.IssueSiteID, guidGenerator);
        }


        /// <summary>
        /// Inserts a new widget configuration to the <see cref="IssueInfo"/>.
        /// </summary>
        /// <param name="widgetTypeIdentifier">Widget type identifier.</param>
        /// <param name="zoneIdentifier">Identifier of the zone where the widget configuration should be inserted.</param>
        /// <param name="index">Index within the zone where the widget configuration should be inserted.</param>
        /// <returns>Returns the content of inserted widget.</returns>
        /// <exception cref="InvalidOperationException">Thrown when trying to insert a widget into an issue which is in read-only mode.</exception>
        public Widget InsertWidget(Guid widgetTypeIdentifier, string zoneIdentifier, int index)
        {
            var widget = widgetCreator.Create(widgetTypeIdentifier);
            configurationManager.InsertWidget(widget, zoneIdentifier, index);
            StoreConfiguration();

            return widget;
        }


        /// <summary>
        /// Gets the widget content based on identifier.
        /// </summary>
        /// <param name="widgetIdentifier">Widget identifier.</param>
        /// <param name="filter">Filter applied to the widget content.</param>
        public WidgetContent GetWidgetContent(Guid widgetIdentifier, IWidgetContentFilter filter)
        {
            var widget = configurationManager.GetWidget(widgetIdentifier);

            return GetWidgetContent(widget, filter);
        }


        /// <summary>
        /// Gets the email content of the <see cref="IssueInfo"/> including zones and widgets.
        /// </summary>
        /// <param name="filter">Filter applied to the widget content.</param>
        public EmailContent GetEmailContent(IWidgetContentFilter filter)
        {
            var configuration = configurationManager.GetConfiguration();

            return new EmailContentProvider(configuration, issue.IssueSiteID, filter).GetContent();
        }


        /// <summary>
        /// Removes the widget from configuration based on identifier.
        /// </summary>
        /// <param name="widgetIdentifier">Widget identifier.</param>
        /// <exception cref="InvalidOperationException">Thrown when trying to remove a widget from an issue which is in read-only mode.</exception>
        public void RemoveWidget(Guid widgetIdentifier)
        {
            if (configurationManager.RemoveWidget(widgetIdentifier))
            {
                StoreConfiguration();
            }
        }


        /// <summary>
        /// Moves the existing widget configuration within the <see cref="IssueInfo"/>.
        /// </summary>
        /// <param name="widgetIdentifier">Identifier of the widget configuration which should be moved.</param>
        /// <param name="zoneIdentifier">Identifier of the zone where the widget configuration should be moved.</param>
        /// <param name="index">Index within the zone where the widget configuration should be moved.</param>
        /// <exception cref="InvalidOperationException">Thrown when trying to move a widget in an issue which is in read-only mode.</exception>
        public void MoveWidget(Guid widgetIdentifier, string zoneIdentifier, int index)
        {
            if (configurationManager.MoveWidget(widgetIdentifier, zoneIdentifier, index))
            {
                StoreConfiguration();
            }
        }


        /// <summary>
        /// Gets the widget configuration including stored properties.
        /// </summary>
        /// <param name="widgetIdentifier">Identifier of the widget configuration.</param>
        public Widget GetWidgetConfiguration(Guid widgetIdentifier)
        {
            return configurationManager.GetWidget(widgetIdentifier);
        }


        /// <summary>
        /// Stores new values for widget properties.
        /// </summary>
        /// <param name="widgetIdentifier">Identifier of the widget configuration.</param>
        /// <param name="properties">Collection of widget properties to store.</param>
        /// <exception cref="InvalidOperationException">Thrown when trying to store properties of a widget in an issue which is in read-only mode.</exception>
        public void StoreWidgetProperties(Guid widgetIdentifier, NameValueCollection properties)
        {
            if (configurationManager.SetWidgetProperties(widgetIdentifier, properties))
            {
                StoreConfiguration();
            }
        }


        /// <summary>
        /// Indicates whether some widget in current configuration has missing required property value.
        /// </summary>
        public bool HasWidgetWithUnfilledRequiredProperty()
        {
            return configurationManager.GetConfiguration().Zones.Any(zone =>
                zone.Widgets.Any(widget =>
                    widget.HasUnfilledRequiredProperty
                )
            );
        }


        /// <summary>
        /// Indicates whether some widget in current configuration has missing definition.
        /// </summary>
        public bool HasWidgetWithMissingDefinition()
        {
            return configurationManager.GetConfiguration().Zones.Any(zone =>
                zone.Widgets.Any(widget =>
                    widget.WidgetDefinitionNotFound
                )
            );
        }


        private WidgetContent GetWidgetContent(Widget widget, IWidgetContentFilter filter)
        {
            return new WidgetContentProvider(issue.IssueSiteID, filter).GetContent(widget);
        }


        private void StoreConfiguration()
        {
            if (issue.IssueStatus != IssueStatusEnum.Idle)
            {
                throw new InvalidOperationException("Cannot modify issue in read-only mode.");
            }

            var configuration = configurationManager.GetConfiguration();
            issue.IssueWidgets = zonesConfigurationXmlSerializer.Serialize(configuration);
            issue.Update();
        }


        private ZonesConfiguration GetConfiguration()
        {
            return !string.IsNullOrEmpty(issue.IssueWidgets) ? zonesConfigurationXmlSerializer.Deserialize(issue.IssueWidgets) : new ZonesConfiguration();
        }
    }
}