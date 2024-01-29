using System;
using System.Collections.Specialized;

using CMS.Newsletters.Filters;
using CMS.Newsletters.Issues.Widgets.Content;

namespace CMS.Newsletters.Issues.Widgets.Configuration
{
    /// <summary>
    /// Interface for service managing the <see cref="IssueInfo.IssueWidgets"/> configuration.
    /// </summary>
    /// <exclude />
    public interface IZonesConfigurationService
    {
        /// <summary>
        /// Inserts a new widget configuration to the <see cref="IssueInfo"/>.
        /// </summary>
        /// <param name="widgetTypeIdentifier">Widget type identifier.</param>
        /// <param name="zoneIdentifier">Identifier of the zone where the widget configuration should be inserted.</param>
        /// <param name="index">Index within the zone where the widget configuration should be inserted.</param>
        /// <exception cref="InvalidOperationException">Thrown when trying to insert a widget into an issue which is in read-only mode.</exception>
        /// <returns>Returns the inserted widget.</returns>
        Widget InsertWidget(Guid widgetTypeIdentifier, string zoneIdentifier, int index);


        /// <summary>
        /// Removes the widget from configuration based on identifier.
        /// </summary>
        /// <param name="widgetIdentifier">Widget identifier.</param>
        /// <exception cref="InvalidOperationException">Thrown when trying to remove a widget from an issue which is in read-only mode.</exception>
        void RemoveWidget(Guid widgetIdentifier);


        /// <summary>
        /// Moves the existing widget configuration within the <see cref="IssueInfo"/>.
        /// </summary>
        /// <param name="widgetIdentifier">Identifier of the widget configuration which should be moved.</param>
        /// <param name="zoneIdentifier">Identifier of the zone where the widget configuration should be moved.</param>
        /// <param name="index">Index within the zone where the widget configuration should be moved.</param>
        /// <exception cref="InvalidOperationException">Thrown when trying to move a widget in an issue which is in read-only mode.</exception>
        void MoveWidget(Guid widgetIdentifier, string zoneIdentifier, int index);


        /// <summary>
        /// Gets the widget content based on identifier.
        /// </summary>
        /// <param name="widgetIdentifier">Widget identifier.</param>
        /// <param name="filter">Filter applied to the widget content.</param>
        WidgetContent GetWidgetContent(Guid widgetIdentifier, IWidgetContentFilter filter);


        /// <summary>
        /// Gets the email content including zones and widgets.
        /// </summary>
        /// <param name="filter">Filter applied to the widget content.</param>
        EmailContent GetEmailContent(IWidgetContentFilter filter);


        /// <summary>
        /// Gets the widget configuration including stored properties.
        /// </summary>
        /// <param name="widgetIdentifier">Identifier of the widget configuration.</param>
        Widget GetWidgetConfiguration(Guid widgetIdentifier);


        /// <summary>
        /// Indicates whether some widget in current configuration has missing required property value.
        /// </summary>
        bool HasWidgetWithUnfilledRequiredProperty();


        /// <summary>
        /// Indicates whether some widget in current configuration has missing definition.
        /// </summary>
        bool HasWidgetWithMissingDefinition();

        
        /// <summary>
        /// Stores new values for widget properties.
        /// </summary>
        /// <param name="widgetIdentifier">Identifier of the widget configuration.</param>
        /// <param name="properties">Collection of widget properties to store.</param>
        /// <exception cref="InvalidOperationException">Thrown when trying to store properties of a widget in an issue which is in read-only mode.</exception>
        void StoreWidgetProperties(Guid widgetIdentifier, NameValueCollection properties);
    }
}
