using System;

using CMS.DataEngine;
using CMS.FormEngine;
using CMS.Helpers;
using CMS.MacroEngine;


namespace CMS.Newsletters.Issues.Widgets
{
    /// <summary>
    /// Provides macro resolvers for widgets.
    /// </summary>
    public static class WidgetResolvers
    {
        private const string WIDGET_RESLOVER_PREFIX = "EmailWidgetProperties.";


        /// <summary>
        /// Gets resolver for resolving default values of widget properties.
        /// </summary>
        /// <param name="definition">Widget definition.</param>
        internal static MacroResolver GetWidgetDefaultPropertiesResolver(WidgetDefinition definition)
        {
            return MacroResolver.GetInstance();
        }


        private static MacroResolver GetVirtualWidgetContentResolver(string name)
        {
            if (name == null || !name.StartsWith(WIDGET_RESLOVER_PREFIX, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            var widgetId = GetWidgetIdFromName(name);
            if (widgetId == 0)
            {
                return null;
            }

            var resolver = NewsletterResolvers.GetVirtualResolver().CreateChild();
            var widget = EmailWidgetInfoProvider.GetEmailWidgetInfo(widgetId);
            var formInfo = new FormInfo(widget.EmailWidgetProperties);

            foreach (var field in formInfo.GetFields(true, true))
            {
                var type = DataTypeManager.GetDataType(TypeEnum.Field, field.DataType);
                resolver.SetNamedSourceData(field.Name, type.ObjectDefaultValue);
            }

            return resolver;
        }


        private static int GetWidgetIdFromName(string name)
        {
            // Resolver name in format - EmailWidgetProperties.{emailwidgetid}.resolver
            var items = name.Split('.');
            return items.Length > 1 ? ValidationHelper.GetInteger(items[1], 0) : 0;
        }


        /// <summary>
        /// Registers widget resolvers to the resolvers storage to be accessible via names.
        /// </summary>
        internal static void Register()
        {
            MacroResolverStorage.RegisterGetResolverHandler(GetVirtualWidgetContentResolver);
        }


        /// <summary>
        /// Compose name of email widget specific resolver holding properties definition.
        /// </summary>
        /// <param name="emailWidgetDefinitionID">Id of email widget definition.</param>
        public static string GetResolverName(int emailWidgetDefinitionID)
        {
            return $"{WIDGET_RESLOVER_PREFIX}{emailWidgetDefinitionID}";
        }
    }
}
