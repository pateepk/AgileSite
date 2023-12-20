using System;
using System.Linq;
using System.Text;

using CMS.Helpers;

namespace CMS.ModuleUsageTracking
{
    /// <summary>
    /// Represents supported module usage data types.
    /// </summary>
    internal enum ModuleUsageDataTypeEnum
    {
        [EnumDefaultValue]
        [EnumStringRepresentation("String")]
        [EnumTypeRepresentation(typeof(string))]
        String = 0,

        [EnumStringRepresentation("Binary")]
        [EnumTypeRepresentation(typeof(Byte[]))]
        Binary = 1,

        [EnumStringRepresentation("Boolean")]
        [EnumTypeRepresentation(typeof(bool))]
        Boolean = 2,

        [EnumStringRepresentation("DateTime")]
        [EnumTypeRepresentation(typeof(DateTime))]
        DateTime = 3,

        [EnumStringRepresentation("Double")]
        [EnumTypeRepresentation(typeof(double))]
        Double = 4,

        [EnumStringRepresentation("Guid")]
        [EnumTypeRepresentation(typeof(Guid))]
        Guid = 5,

        [EnumStringRepresentation("Int")]
        [EnumTypeRepresentation(typeof(int))]
        Int = 6,

        [EnumStringRepresentation("Long")]
        [EnumTypeRepresentation(typeof(long))]
        Long = 7
    }


    /// <summary>
    /// Provides extension methods for working with the <see cref="ModuleUsageDataTypeEnum"/> type.
    /// </summary>
    internal static class ModuleUsageDataTypeEnumExtensions
    {
        /// <summary>
        /// Converts the enum value to it's type representation.
        /// </summary>
        /// <param name="value">Enum value</param>
        /// <returns>
        /// Returns the data type representation of the enum value if it is specified using the <see cref="EnumTypeRepresentationAttribute"/>.
        /// Otherwise returns typeof(object).
        /// </returns>
        public static Type ToTypeRepresentation(this ModuleUsageDataTypeEnum value)
        {
            var fieldName = Enum.GetName(typeof(ModuleUsageDataTypeEnum), value);
            var field = typeof(ModuleUsageDataTypeEnum).GetField(fieldName);

            // Try to get the type representation
            var attribute = (EnumTypeRepresentationAttribute)field.GetCustomAttributes(typeof(EnumTypeRepresentationAttribute), false).FirstOrDefault();
            if (attribute != null)
            {
                // Return the type representation
                return attribute.TypeRepresentation;
            }

            // Return the object type
            return typeof(object);
        }


        /// <summary>
        /// Converts the data type representation of the enum value to the actual enum value.
        /// </summary>
        /// <param name="typeRepresentation">Data type representation of the enum value</param>
        /// <returns>
        /// Returns the enum value if it is specified using the <see cref="EnumTypeRepresentationAttribute"/>.
        /// Otherwise returns the default enum value - ModuleUsageDataTypeEnum.String.
        /// </returns>
        public static ModuleUsageDataTypeEnum ToModuleUsageDataTypeEnum(this Type typeRepresentation)
        {
            // Try to get the corresponding enum value
            var field = typeof(ModuleUsageDataTypeEnum).GetFields().FirstOrDefault(f => f.GetCustomAttributes(typeof(EnumTypeRepresentationAttribute), false).Select(a => (EnumTypeRepresentationAttribute)a).Any(a => a.TypeRepresentation == typeRepresentation));
            if (field != null)
            {
                // Return the corresponding enum value
                return (ModuleUsageDataTypeEnum)field.GetValue(null);
            }

            // Return default value
            return ModuleUsageDataTypeEnum.String;
        }
    }
}
