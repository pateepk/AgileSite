using System;

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Use this attribute to enable value binding for a form component property.
    /// </summary>
    /// <seealso cref="FormComponent{TProperties, TValue}"/>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class BindablePropertyAttribute : Attribute
    {
    }
}
