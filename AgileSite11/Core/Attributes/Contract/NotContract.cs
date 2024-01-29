using System;

namespace CMS
{
    /// <summary>
    /// Marks the member not to be included into contract
    /// </summary>
    [AttributeUsage(AttributeTargets.All)]
    public class NotContract : Attribute
    {
    }
}
