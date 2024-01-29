using System;

using CMS.Core;

namespace CMS
{
    /// <summary>
    /// Marks an implementation as required, effectively protecting it from being replaced once it is registered in <see cref="ObjectFactory{T}"/>.
    /// </summary>
    /// <seealso cref="RegisterImplementationAttribute"/>
    /// <seealso cref="ObjectFactory{T}"/>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    [Obsolete("Use ObjectFactory<T>.SetObjectTypeTo<NewType>() to register an implementation which cannot be replaced.", true)]
    public sealed class RequiredImplementationAttribute : Attribute
    {
        // This attribute was designed for declarative registration within ObjectFactory. The declarative approach is now supported for Service only
        // and services do not guarantee any protection for registered implementations. Since SetObjectTypeTo method of ObjectFactory has a parameter
        // specifying whether implementation is required, this attribute is redundant.
    }
}
