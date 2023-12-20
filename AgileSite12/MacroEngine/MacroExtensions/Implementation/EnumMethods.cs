using System;

using CMS.Core;
using CMS.Helpers;
using CMS.Base;

namespace CMS.MacroEngine
{
    /// <summary>
    /// Wrapper class to provide methods from System.Enums namespace in the MacroEngine.
    /// </summary>
    internal class EnumMethods : MacroMethodContainer
    {
        /// <summary>
        /// Returns the string representation of the enum value.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(string), "Returns the string representation of the enum value.", 3)]
        [MacroMethodParam(0, "value", typeof (int), "Enumeration value.")]
        [MacroMethodParam(1, "assemblyName", typeof(object), "Enumeration assembly name.")]
        [MacroMethodParam(2, "className", typeof(object), "Enumeration full class name.")]
        public static object ToStringRepresentation(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 3:
                    int value = ValidationHelper.GetInteger(parameters[0], 0);
                    string assemblyName = ValidationHelper.GetString(parameters[1], null);
                    string typeName = ValidationHelper.GetString(parameters[2], null);
                    Type enumType = TryGetEnumType(assemblyName, typeName);
                    if (enumType != null)
                    {
                        var x = (Enum)Enum.Parse(enumType, value.ToString());
                        return CoreServices.Localization.GetString(x.ToStringRepresentation());
                    }

                    return value;

                default:
                    throw new NotSupportedException();
            }
        }


        private static Type TryGetEnumType(string assemblyname, string typeName)
        {
            var type = ClassHelper.GetClassType(assemblyname, typeName);

            if ((type != null) && type.IsEnum)
            {
                return type;
            }

            return null;
        }
    }
}