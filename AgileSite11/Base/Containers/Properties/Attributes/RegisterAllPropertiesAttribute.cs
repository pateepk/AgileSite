using System;

namespace CMS
{
    /// <summary>
    /// When used, defined if class registers its all public properties as properties of the object, except for ones excluded with ExcludeProperty attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class RegisterAllPropertiesAttribute : Attribute
    {
    }
}
