using System;
using System.Linq;
using System.Reflection;

namespace Kentico.PageBuilder.Web.Mvc
{
    /// <summary>
    /// Processor that can modify all components properties of the give <typeparamref name="TPropertyType"/>.
    /// </summary>
    /// <typeparam name="TPropertyType">The type of the property type.</typeparam>
    internal class ComponentPropertiesProcessor<TPropertyType>
    {
        private readonly Func<TPropertyType, TPropertyType> propertyProcessor;


        /// <summary>
        /// Initializes a new instance of the <see cref="ComponentPropertiesProcessor{TPropertyType}"/> class.
        /// </summary>
        /// <param name="propertyProcessor">The property processor.</param>
        /// <exception cref="ArgumentNullException">propertyProcessor is <c>null</c>.</exception>
        public ComponentPropertiesProcessor(Func<TPropertyType, TPropertyType> propertyProcessor)
        {
            propertyProcessor = propertyProcessor ?? throw new ArgumentNullException(nameof(propertyProcessor));

            this.propertyProcessor = propertyProcessor;
        }


        /// <summary>
        /// Applies the processor <see cref="ComponentPropertiesProcessor(Func{TPropertyType, TPropertyType})"/> on all the components' properties of the type <typeparamref name="TPropertyType"/>.
        /// </summary>
        /// <param name="configuration">The page builder configuration to be processed.</param>
        /// <exception cref="ArgumentNullException">configuration is <c>null</c>.</exception>
        public void ApplyProcessor(PageBuilderConfiguration configuration)
        {
            configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

            foreach (var area in configuration.Page?.EditableAreas)
            {
                foreach (var section in area.Sections)
                {
                    ProcessComponentProperties(section.Properties);

                    foreach (var zone in section.Zones)
                    {
                        foreach (var widget in zone.Widgets)
                        {
                            foreach (var variant in widget.Variants)
                            {
                                ProcessComponentProperties(variant.Properties);
                            }
                        }
                    }
                }
            }

            ProcessComponentProperties(configuration.PageTemplate?.Properties);
        }


        private void ProcessComponentProperties(object properties)
        {
            if (properties == null)
            {
                return;
            }

            // Get all component properties of the specific type
            var filteredProperties = properties.GetType()
                                             .GetProperties(BindingFlags.Public |
                                                            BindingFlags.Instance)
                                             .Where(prop => prop.PropertyType.Equals(typeof(TPropertyType)));

            foreach (var property in filteredProperties)
            {
                // Filter only properties with a public setter
                MethodInfo setter = property.GetSetMethod();
                if (setter != null)
                {
                    // Get the property value
                    TPropertyType value = (TPropertyType)property.GetValue(properties);

                    // Apply the property processor
                    var newValue = propertyProcessor(value);

                    // Set the new property value
                    setter.Invoke(properties, new object[] { newValue });
                }
            }
        }
    }
}
