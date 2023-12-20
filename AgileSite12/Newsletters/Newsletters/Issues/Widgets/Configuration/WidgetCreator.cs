using System;
using System.Collections.Generic;
using System.Linq;

using CMS.DataEngine;
using CMS.FormEngine;
using CMS.MacroEngine;

namespace CMS.Newsletters.Issues.Widgets.Configuration
{
    /// <summary>
    /// Encapsulates creating of <see cref="Widget"/> based on the <see cref="EmailWidgetInfo"/>.
    /// </summary>
    internal sealed class WidgetCreator
    {
        private readonly SiteInfoIdentifier site;
        private readonly GuidGenerator guidGenerator;


        /// <summary>
        /// Creates an instance of the <see cref="WidgetCreator"/> class.
        /// </summary>
        /// <param name="site">Site of the issue with widgets to manage.</param>
        /// <param name="guidGenerator">Generator used for <see cref="Widget.Identifier"/> when creating a new instance of <see cref="Widget"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="site"/> is null.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="guidGenerator"/> is null.</exception>
        public WidgetCreator(SiteInfoIdentifier site, GuidGenerator guidGenerator)
        {
            if (site == null)
            {
                throw new ArgumentNullException(nameof(site));
            }

            if (guidGenerator == null)
            {
                throw new ArgumentNullException(nameof(guidGenerator));
            }

            this.site = site;
            this.guidGenerator = guidGenerator;
        }


        /// <summary>
        /// Creates a <see cref="Widget"/> with initialized <see cref="Widget.Properties"/> based on default values of <see cref="EmailWidgetInfo.EmailWidgetProperties"/>.
        /// </summary>
        /// <param name="widgetTypeIdentifier">Widget type identifier.</param>
        /// <exception cref="InvalidOperationException">Thrown when email widget type could not be found.</exception>
        public Widget Create(Guid widgetTypeIdentifier)
        {
            var widget = new Widget(guidGenerator.Generate(), widgetTypeIdentifier);
            var definition = new WidgetDefinition(widgetTypeIdentifier, site);
           
            AddProperties(widget, definition);
            widget.HasUnfilledRequiredProperty = definition.HasRequiredPropertyWithoutDefaultValue();

            return widget;
        }


        private void AddProperties(Widget widget, WidgetDefinition definition)
        {
            var resolver = WidgetResolvers.GetWidgetDefaultPropertiesResolver(definition);
            var properties = ConvertFieldsToWidgetProperties(definition.Fields, resolver);
            widget.Properties.AddRange(properties);
        }


        private static IEnumerable<WidgetProperty> ConvertFieldsToWidgetProperties(IEnumerable<FormFieldInfo> fields, MacroResolver resolver)
        {
            return fields.Select(field =>
                {
                    var value = field.GetDefaultValue(FormResolveTypeEnum.AllFields, resolver);
                    resolver.SetNamedSourceData(field.Name, value);
                    return new WidgetProperty(field.Name, value);
                }
            );
        }
    }
}
