using System;
using System.Collections.Specialized;
using System.Linq;

namespace CMS.Newsletters.Issues.Widgets.Configuration
{
    /// <summary>
    /// Manager for <see cref="ZonesConfiguration"/> objects.
    /// </summary>
    internal class ZonesConfigurationManager
    {
        private readonly ZonesConfiguration configuration;


        /// <summary>
        /// Creates a new instance of <see cref="ZonesConfigurationManager"/> class.
        /// </summary>
        /// <param name="configuration">Zones configuration to manage.</param>
        /// <param name="transformer">Transformer for configuration enrichment.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="configuration"/> is null.</exception>
        public ZonesConfigurationManager(ZonesConfiguration configuration, IZonesConfigurationTransformer transformer = null)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            if (transformer != null)
            {
                configuration = transformer.Transform(configuration);
            }

            this.configuration = configuration;
        }


        /// <summary>
        /// Inserts a new widget configuration to the <see cref="ZonesConfiguration"/> object.
        /// </summary>
        /// <param name="widget">Widget configuration.</param>
        /// <param name="zoneIdentifier">Identifier of the zone where the widget configuration is inserted.</param>.
        /// <param name="index">Index within the zone where the widget configuration is inserted.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="widget"/> is null.</exception>
        public void InsertWidget(Widget widget, string zoneIdentifier, int index)
        {
            if (widget == null)
            {
                throw new ArgumentNullException(nameof(widget));
            }

            var zone = EnsureZone(zoneIdentifier);

            zone.Widgets.Insert(index, widget);
        }


        /// <summary>
        /// Gets widget configuration based on identifier.
        /// </summary>
        /// <param name="widgetIdentifier">Widget identifier.</param>
        /// <remarks>Returns <c>null</c> if no widget configuration with given identifier found.</remarks>
        public Widget GetWidget(Guid widgetIdentifier)
        {
            foreach (var zone in configuration.Zones)
            {
                var widget = zone.Widgets.FirstOrDefault(w => w.Identifier == widgetIdentifier);
                if (widget != null)
                {
                    return widget;
                }
            }

            return null;
        }


        /// <summary>
        /// Gets the managed <see cref="ZonesConfiguration"/> object.
        /// </summary>
        public ZonesConfiguration GetConfiguration()
        {
            return configuration;
        }


        /// <summary>
        /// Removes widget from the <see cref="Zone"/> in <see cref="ZonesConfiguration"/> object based on widget identifier.
        /// </summary>
        /// <param name="widgetIdentifier">Widget identifier of widget to remove.</param>
        /// <returns>Returns <c>true</c> if widget was removed.</returns>
        public bool RemoveWidget(Guid widgetIdentifier)
        {
            return configuration.Zones.Any(zone => RemoveWidgetFromZone(zone, widgetIdentifier));
        }


        /// <summary>
        /// Moves the existing widget configuration within the <see cref="ZonesConfiguration"/> object.
        /// </summary>
        /// <param name="widgetIdentifier">Identifier of the widget configuration which should be moved.</param>
        /// <param name="zoneIdentifier">Identifier of the zone where the widget configuration should be moved.</param>
        /// <param name="index">Index within the zone where the widget configuration should be moved.</param>
        /// <returns>Returns <c>false</c> if widget does not exist, otherwise returns <c>true</c>.</returns>
        public bool MoveWidget(Guid widgetIdentifier, string zoneIdentifier, int index)
        {
            var widget = GetWidget(widgetIdentifier);
            if (widget == null)
            {
                return false;
            }

            RemoveWidget(widget.Identifier);

            InsertWidget(widget, zoneIdentifier, index);

            return true;
        }


        /// <summary>
        /// Sets new values for widget properties.
        /// </summary>
        /// <param name="widgetIdentifier">Identifier of the widget configuration which should be moved.</param>
        /// <param name="properties">Collection of new properties</param>
        /// <returns>Returns <c>false</c> if widget does not exist, otherwise returns <c>true</c>.</returns>
        /// <remarks>Collection of current properties is replaced by the collection of properties and values from <paramref name="properties"/>.</remarks>
        public bool SetWidgetProperties(Guid widgetIdentifier, NameValueCollection properties)
        {
            var widget = GetWidget(widgetIdentifier);
            if (widget == null)
            {
                return false;
            }

            widget.Properties.Clear();
            if (properties != null)
            {
                foreach (string name in properties.AllKeys)
                {
                    widget.Properties.Add(new WidgetProperty(name, properties[name]));
                }
            }

            // This method is called when widget properties are explicitly saved in UI. It means the UIFrom perform validation 
            // and the method is called only if all fields are valid. So HasUnfilledRequiredProperty can be reset because all
            // required fields have to have a value at the moment.
            widget.HasUnfilledRequiredProperty = false;

            return true;
        }


        private Zone EnsureZone(string zoneIdentifier)
        {
            var zone = configuration.Zones.FirstOrDefault(z => z.Identifier == zoneIdentifier);
            if (zone == null)
            {
                zone = new Zone(zoneIdentifier);
                configuration.Zones.Add(zone);
            }

            return zone;
        }


        private static bool RemoveWidgetFromZone(Zone zone, Guid widgetIdentifier)
        {
            return zone.Widgets.RemoveAll(widget => widgetIdentifier == widget.Identifier) > 0;
        }
    }
}
