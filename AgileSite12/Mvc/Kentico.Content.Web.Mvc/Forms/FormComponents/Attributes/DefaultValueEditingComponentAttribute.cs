using System;

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Attribute to be used with <see cref="FormComponentProperties{TValue}.DefaultValue"/> property overrides to specify the editing form component.
    /// Sets the system properties such as <see cref="EditingComponentAttribute.Label"/> or <see cref="EditingComponentAttribute.Order"/>
    /// to proper values.
    /// </summary>
    public class DefaultValueEditingComponentAttribute : EditingComponentAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultValueEditingComponentAttribute"/> class.
        /// Sets the <see cref="EditingComponentAttribute.Label"/> to a proper localization string and <see cref="EditingComponentAttribute.Order"/> to <see cref="EditingComponentOrder.DEFAULT_VALUE"/>.
        /// </summary>
        /// <param name="formComponentIdentifier">
        /// Defines identifier of a <see cref="FormComponent{TProperties, TValue}"/> registered via <see cref="RegisterFormComponentAttribute"/>
        /// which is used for editing the annotated property from Form builder's UI.
        /// </param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="formComponentIdentifier"/> is null or an empty string.</exception>
        public DefaultValueEditingComponentAttribute(string formComponentIdentifier)
            : base(formComponentIdentifier)
        {
            Label = "{$formbuilder.defaultvalue$}";
            Order = EditingComponentOrder.DEFAULT_VALUE;
        }
    }
}
