using System;

namespace CMS
{
    /// <summary>
    /// When used, the given property is not registered within the object
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class NotRegisterPropertyAttribute : Attribute
    {
    }
}
