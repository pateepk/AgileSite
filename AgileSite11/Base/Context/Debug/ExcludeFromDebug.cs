using System;
using System.Linq;
using System.Text;

namespace CMS.Base
{
    /// <summary>
    /// Marks the method as not being included in debug
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class ExcludeFromDebug : Attribute
    {
    }
}
