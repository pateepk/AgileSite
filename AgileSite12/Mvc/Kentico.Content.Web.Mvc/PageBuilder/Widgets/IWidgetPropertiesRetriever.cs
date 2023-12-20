using System;

namespace Kentico.PageBuilder.Web.Mvc
{
    /// <summary>
    /// Provides an interface for retrieving widget properties.
    /// </summary>
    /// <typeparam name="TPropertiesType">Type of the widget properties.</typeparam>
    [Obsolete("Use IComponentPropertiesRetriever<TPropertiesType> interface instead.")]
    public interface IWidgetPropertiesRetriever<out TPropertiesType>
        where TPropertiesType : class, IWidgetProperties, new()
    {
        /// <summary>
        /// Retrieves widget properties.
        /// </summary>
        TPropertiesType Retrieve();
    }
}