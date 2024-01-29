using System;

using CMS.DataEngine;
using CMS.Newsletters.Issues.Widgets.Configuration;
using CMS.Newsletters.Filters;

namespace CMS.Newsletters.Issues.Widgets.Content
{
    /// <summary>
    /// Provides the content for a widget.
    /// </summary>
    internal sealed class WidgetContentProvider
    {
        private readonly IWidgetContentFilter widgetContentFilter;
        private readonly SiteInfoIdentifier site;


        /// <summary>
        /// Creates an instance of <see cref="WidgetContentProvider"/> class.
        /// </summary>
        /// <param name="site">Site of the issue with widgets.</param>
        /// <param name="widgetContentFilter">Filter applied to the widget content.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="site"/> is null.</exception>
        public WidgetContentProvider(SiteInfoIdentifier site, IWidgetContentFilter widgetContentFilter)
        {
            if (site == null)
            {
                throw new ArgumentNullException(nameof(site));
            }

            this.site = site;
            this.widgetContentFilter = widgetContentFilter;
        }


        /// <summary>
        /// Gets the content for given widget.
        /// </summary>
        /// <param name="widget">Widget configuration.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="widget"/> is null.</exception>
        public WidgetContent GetContent(Widget widget)
        {
            if (widget == null)
            {
                throw new ArgumentNullException(nameof(widget));
            }

            var definition = new WidgetDefinition(widget.TypeIdentifier, site);
            if(definition.WidgetDefinitionNotFound)
            {
                return new WidgetContent(widget.Identifier, true);
            }

            var code = ApplyWidgetContentFilter(definition.Code, widget);
            return new WidgetContent(widget.Identifier, code, widget.HasUnfilledRequiredProperty);
        }


        private string ApplyWidgetContentFilter(string code, Widget widget)
        {
            return widgetContentFilter == null ? code : widgetContentFilter.Apply(code, widget);
        }
    }
}
