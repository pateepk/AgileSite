using System;
using System.Collections;
using System.Linq;

using CMS;
using CMS.Helpers;
using CMS.MacroEngine;

[assembly: RegisterExtension(typeof(ObjectMethods), typeof(object))]

namespace CMS.MacroEngine
{
    /// <summary>
    /// Wrapper class to provide methods working with any object in the MacroEngine.
    /// </summary>
    internal class ObjectMethods : MacroMethodContainer
    {
        /// <summary>
        /// Returns true if at least one object of the enumerable is equal to specified object.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">
        /// Obejct to compare each item of the collection to;
        /// Enumerable collection;
        /// </param>
        [MacroMethod(typeof(bool), "Returns true if there is at least one object in the collection which is equal to the specified object.", 2)]
        [MacroMethodParam(0, "object", typeof(object), "Object to compare with.")]
        [MacroMethodParam(1, "enumerable", typeof(IEnumerable), "Collection of items.")]
        public static object EqualsAny(EvaluationContext context, params object[] parameters)
        {
            if (parameters.Length < 2)
            {
                throw new NotSupportedException();
            }

            bool isEqual = false;
            for (int i = 1; i < parameters.Length; i++)
            {
                if ((parameters[i] is IEnumerable) && !(parameters[i] is string))
                {
                    IEnumerable collection = (IEnumerable)parameters[i];
                    IEnumerator enumerator = collection.GetEnumerator();
                    while (enumerator.MoveNext())
                    {
                        isEqual = ValidationHelper.GetBoolean(EqualsAny(context, parameters[0], enumerator.Current), false);

                        if (isEqual)
                        {
                            return true;
                        }
                    }
                }
                else
                {
                    isEqual = ExpressionEvaluator.IsEqual(parameters[0], parameters[i], context);
                }

                if (isEqual)
                {
                    return true;
                }
            }

            return false;
        }
    }
}