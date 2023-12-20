using System;
using System.Linq;

using CMS;
using CMS.Base;
using CMS.DataEngine;
using CMS.MacroEngine;

[assembly: RegisterExtension(typeof(SimpleDataContainerMethods), typeof(ISimpleDataContainer))]

namespace CMS.MacroEngine
{
    /// <summary>
    /// Wrapper class to provide methods working with ISimpleDataContainer in the MacroEngine.
    /// </summary>
    internal class SimpleDataContainerMethods : MacroMethodContainer
    {
        /// <summary>
        /// Gets the specified value from the object.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(object), "Gets the specified value from the object.", 2)]
        [MacroMethodParam(0, "container", typeof(ISimpleDataContainer), "Data container.")]
        [MacroMethodParam(1, "column", typeof(string), "Column name.")]
        [MacroMethodParam(2, "defaultValue", typeof(object), "Default value which is returned when the requested item is null.")]
        public static object GetValue(EvaluationContext context, params object[] parameters)
        {
            if (parameters.Length > 1)
            {
                object result = null;

                if (parameters[0] is ISimpleDataContainer)
                {
                    string columnName = GetStringParam(parameters[1], context.Culture);

                    // Check the access of the sensitive columns
                    if (parameters[0] is BaseInfo)
                    {
                        var parentInfo = (BaseInfo)parameters[0];

                        var ti = parentInfo.TypeInfo;
                        if (ti.SensitiveColumns != null)
                        {
                            if (ti.SensitiveColumns.Any(col => col.EqualsCSafe(columnName, true)))
                            {
                                return null;
                            }
                        }
                    }

                    ISimpleDataContainer obj = (ISimpleDataContainer)parameters[0];
                    result = obj.GetValue(columnName);
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