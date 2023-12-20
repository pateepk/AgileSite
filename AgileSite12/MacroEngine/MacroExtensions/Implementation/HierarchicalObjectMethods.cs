using System;
using System.Linq;

using CMS;
using CMS.Base;
using CMS.DataEngine;
using CMS.MacroEngine;

[assembly: RegisterExtension(typeof(HierarchicalObjectMethods), typeof(IHierarchicalObject))]

namespace CMS.MacroEngine
{
    /// <summary>
    /// Wrapper class to provide methods working with IHierarchicalObjectContainer in the MacroEngine.
    /// </summary>
    internal class HierarchicalObjectMethods : MacroMethodContainer
    {
        /// <summary>
        /// Gets the specified property from the object.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(object), "Gets the specified property from the object.", 2)]
        [MacroMethodParam(0, "collection", typeof(IHierarchicalObject), "DataContainer .")]
        [MacroMethodParam(1, "property", typeof(string), "Property name.")]
        [MacroMethodParam(2, "defaultValue", typeof(object), "Default value which is returned when the requested item is null.")]
        public static object GetProperty(EvaluationContext context, params object[] parameters)
        {
            if (parameters.Length > 1)
            {
                object result = null;

                if (parameters[0] is IHierarchicalObject)
                {
                    string columnName = GetStringParam(parameters[1], context.Culture);

                    // Check the access of the sensitive columns
                    if (parameters[0] is BaseInfo)
                    {
                        var parentInfo = (BaseInfo) parameters[0];
                        var ti = parentInfo.TypeInfo;
                        if (ti.SensitiveColumns != null)
                        {
                            if (ti.SensitiveColumns.Any(col => col.EqualsCSafe(columnName, true)))
                            {
                                return null;
                            }
                        }
                    }

                    IHierarchicalObject obj = (IHierarchicalObject)parameters[0];
                    result = obj.GetProperty(columnName);
                }

                // If default value is defined, return it
                if ((parameters.Length == 3) && (result == null))
                {
                    result = parameters[2];
                }

                return result;
            }
            else
            {
                throw new NotSupportedException();
            }
        }
    }
}