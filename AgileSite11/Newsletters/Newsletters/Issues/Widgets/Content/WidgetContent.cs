using System;

namespace CMS.Newsletters.Issues.Widgets.Content
{
    /// <summary>
    /// Represents the content of a widget.
    /// </summary>
    public sealed class WidgetContent
    {
        /// <summary>
        /// Identifier of the widget instance.
        /// </summary>
        public Guid Identifier
        {
            get;
            internal set;
        }


        /// <summary>
        /// Widget HTML.
        /// </summary>
        public string Html
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates that widget contains at least one unfilled required property.
        /// </summary>
        public bool HasUnfilledRequiredProperty
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates that widget definition was not properly loaded.
        /// </summary>
        public bool WidgetDefinitionNotFound
        {
            get;
            set;
        }


        /// <summary>
        /// Creates an instance of <see cref="WidgetContent"/> class.
        /// </summary>
        /// <param name="identifier">Widget identifier.</param>
        /// <param name="html">Widget HTML.</param>
        /// <param name="hasUnfilledRequiredProperty">Widget contains at least one unfilled required property.</param>
        public WidgetContent(Guid identifier, string html, bool hasUnfilledRequiredProperty)
        {
            Identifier = identifier;
            Html = html;
            HasUnfilledRequiredProperty = hasUnfilledRequiredProperty;
        }


        /// <summary>
        /// Creates an instance of <see cref="WidgetContent"/> class without the HTML.
        /// </summary>
        /// <param name="identifier">Widget identifier.</param>
        /// <param name="widgetDefinitionNotFound">Indicates that widget definition was not properly loaded.</param>
        public WidgetContent(Guid identifier, bool widgetDefinitionNotFound)
        {
            Identifier = identifier;
            WidgetDefinitionNotFound = widgetDefinitionNotFound;
        }
    }
}
