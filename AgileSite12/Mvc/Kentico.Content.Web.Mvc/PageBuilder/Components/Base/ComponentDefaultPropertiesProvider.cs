using System;

using Kentico.Forms.Web.Mvc;

namespace Kentico.PageBuilder.Web.Mvc
{
    /// <summary>
    /// Provides default component properties based on provided type identifier.
    /// </summary>
    internal class ComponentDefaultPropertiesProvider : IComponentDefaultPropertiesProvider
    {
        /// <summary>
        /// Gets component default properties.
        /// </summary>
        /// <param name="propertiesType">Type of the component properties.</param>
        /// <typeparam name="TPropertiesInterface">Type of the component properties interface.</typeparam>
        public TPropertiesInterface Get<TPropertiesInterface>(Type propertiesType)
            where TPropertiesInterface : IComponentProperties
        {
            if (propertiesType == null || !typeof(TPropertiesInterface).IsAssignableFrom(propertiesType))
            {
                return default(TPropertiesInterface);
            }

            var properties = (TPropertiesInterface)Activator.CreateInstance(propertiesType);

            EditingComponentUtils.SetDefaultPropertiesValues(properties);

            return properties;
        }


        /// <summary>
        /// Gets component default properties.
        /// </summary>
        /// <typeparam name="TPropertiesType">Type of the component properties.</typeparam>
        public TPropertiesType Get<TPropertiesType>()
            where TPropertiesType : class, IComponentProperties, new()
        {
            var properties = new TPropertiesType();

            EditingComponentUtils.SetDefaultPropertiesValues(properties);

            return properties;
        }
    }
}
