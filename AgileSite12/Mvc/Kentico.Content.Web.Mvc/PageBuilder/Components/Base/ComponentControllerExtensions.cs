using System;

namespace Kentico.PageBuilder.Web.Mvc
{
    /// <summary>
    /// Extension methods for component controller.
    /// </summary>
    internal static class ComponentControllerExtensions
    {
        /// <summary>
        /// Gets model for a component without custom controller.
        /// </summary>
        /// <param name="controller">Component controller.</param>
        public static IComponentViewModel GetModel(this ComponentController controller)
        {
            var definition = controller.GetComponentDefinition();
            var propertiesType = definition.PropertiesType;
            var modelType = (propertiesType == null) ?
                typeof(ComponentViewModel) :
                typeof(ComponentViewModel<>).MakeGenericType(propertiesType);

            var model = Activator.CreateInstance(modelType) as IComponentViewModel;
            var page = controller.GetPage();
            var properties = propertiesType != null ? controller.GetPropertiesRetriever().Retrieve(propertiesType) : null;

            model.LoadProperties(page, properties);

            return model;
        }


        /// <summary>
        /// Gets system view name from component views location.
        /// </summary>
        /// <param name="controller">Component controller.</param>
        public static string GetViewName(this ComponentController controller)
        {
            var definition = controller.GetComponentDefinition();
            var viewName = (definition as ICustomViewNameComponentDefinition)?.ViewName;
            if (viewName == null)
            {
                throw new NotSupportedException($"The '{definition.Identifier}' component type doesn't support custom view names.");
            }

            return viewName;
        }
    }
}