using CMS;
using CMS.MacroEngine;
using CMS.PortalEngine;

[assembly: RegisterExtension(typeof(WidgetInfoMethods), typeof(WidgetInfo))]


namespace CMS.PortalEngine
{    
    /// <summary>
    /// Macro methods for class WidgetInfo
    /// </summary>
    public class WidgetInfoMethods : MacroMethodContainer
    {
        #region "Methods"

        /// <summary>
        /// Macro method for method GetUsageObjectTypes
        /// </summary>
        [MacroMethod(typeof(System.Collections.Generic.IEnumerable<System.String>), "Gets the list of object types that may use the widget", 0)]
        [MacroMethodParam(0, "obj", typeof(WidgetInfo), "Object instance")]
        public static object GetUsageObjectTypes(EvaluationContext context, params object[] parameters)
        {
            if (parameters.Length >= 1)
            {
                // Prepare the parameters
                WidgetInfo obj = GetParamValue(parameters, 0, default(WidgetInfo));

                // Call the method
                return obj.GetUsageObjectTypes();
            }

            throw new System.NotSupportedException();
        }


        /// <summary>
        /// Macro method for method GetUsages
        /// </summary>
        [MacroMethod(typeof(CMS.DataEngine.IDataQuery), "Gets the objects using the widget as a query with result columns ObjectType, ObjectID, Source, ItemID and ItemObjectType.", 0)]
        [MacroMethodParam(0, "obj", typeof(WidgetInfo), "Object instance")]
        public static object GetUsages(EvaluationContext context, params object[] parameters)
        {
            if (parameters.Length >= 1)
            {
                // Prepare the parameters
                WidgetInfo obj = GetParamValue(parameters, 0, default(WidgetInfo));

                // Call the method
                return obj.GetUsages();
            }

            throw new System.NotSupportedException();
        }

        #endregion
    }
}
