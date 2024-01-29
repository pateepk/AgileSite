using System;
using System.Linq;
using System.Text;

namespace CMS.DataEngine
{
    /// <summary>
    /// Specifies to which database column the property maps.
    /// Marks this column as an ID column.
    /// </summary>
    /// <remarks>
    /// Information about ID column is used in unit tests to properly fake given type.
    /// Use this attribute to mark an ID column of object with dynamic typeinfo. 
    /// Objects with fixed typeinfo has this information placed in code and therefore fakes can be initialized without using this attribute. 
    /// Using this attribute on object with fixed typeinfo overrides ID column information in typeinfo.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class DatabaseIDFieldAttribute : DatabaseFieldAttribute
    {
    }
}
