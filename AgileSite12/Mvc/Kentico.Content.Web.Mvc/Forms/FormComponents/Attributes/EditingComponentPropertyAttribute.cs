using System;

using CMS.Helpers;

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Attribute specifying form component property's value.
    /// The attribute is to be used in conjunction with <see cref="EditingComponentAttribute"/> to further configure an assigned component.
    /// </summary>
    /// <seealso cref="EditingComponentAttribute"/>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true, Inherited = false)]
    public class EditingComponentPropertyAttribute : Attribute
    {
        /// <summary>
        /// Name of the property whose value is being configured. The name must be a valid property name within corresponding <see cref="FormComponent{TProperties, TValue}.Properties"/>.
        /// </summary>
        /// <seealso cref="FormComponent{TProperties, TValue}.Properties"/>
        /// <seealso cref="FormComponentProperties"/>
        public string PropertyName { get; }


        /// <summary>
        /// The value of the property this attribute was initialized with. The actual value assigned to the property is obtained
        /// via the <see cref="GetPropertyValue"/> method which can further process the value.
        /// </summary>
        public object PropertyValue { get; }


        /// <summary>
        /// Initializes a new instance of the <see cref="EditingComponentPropertyAttribute"/> class.
        /// </summary>
        /// <param name="propertyName">Name of the property within corresponding <see cref="FormComponent{TProperties, TValue}.Properties"/>.</param>
        /// <param name="propertyValue">Defines the value of the property.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="propertyName"/> is null.</exception>
        public EditingComponentPropertyAttribute(string propertyName, object propertyValue)
        {
            PropertyName = propertyName ?? throw new ArgumentNullException(nameof(propertyName));
            PropertyValue = propertyValue;
        }


        /// <summary>
        /// Returns the value of the property to be assigned to the editing component.
        /// </summary>
        public virtual object GetPropertyValue()
        {
            if (PropertyValue is string str)
            {
                return ResHelper.LocalizeString(str);
            }

            return PropertyValue;
        }
    }
}
