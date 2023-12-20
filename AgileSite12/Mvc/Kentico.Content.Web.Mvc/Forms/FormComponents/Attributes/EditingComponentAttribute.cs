using System;

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Denotes a property that can be edited using the specified form component. Use the optional properties of the attribute to configure the basic properties of the form component assigned or use
    /// <see cref="EditingComponentPropertyAttribute"/> to further configure the form component.
    /// </summary>
    /// <seealso cref="EditingComponentPropertyAttribute"/>
    /// <seealso cref="EditingComponentOrder"/>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class EditingComponentAttribute : Attribute
    {
        /// <summary>
        /// Identifier of the form component to be used for editing.
        /// </summary>
        /// <remarks>
        /// The identifier defines a <see cref="FormComponent{TProperties, TValue}"/> registered via <see cref="RegisterFormComponentAttribute"/>.
        /// </remarks>
        /// <seealso cref="RegisterFormComponentAttribute"/>
        public string FormComponentIdentifier { get; }


        /// <summary>
        /// Gets or sets the label of the form component.
        /// </summary>
        /// <seealso cref="FormComponentProperties.Label"/>
        public string Label { get; set; }


        /// <summary>
        /// Gets or sets the default value of the form component.
        /// </summary>
        /// <seealso cref="FormComponentProperties{TValue}.DefaultValue"/>
        public object DefaultValue { get; set; }


        /// <summary>
        /// Gets or sets the explanation text of the form component.
        /// </summary>
        /// <seealso cref="FormComponentProperties.ExplanationText"/>
        public string ExplanationText { get; set; }


        /// <summary>
        /// Gets or sets the tooltip of the form component.
        /// </summary>
        /// <seealso cref="FormComponentProperties.Tooltip"/>
        public string Tooltip { get; set; }


        /// <summary>
        /// Gets or sets the order weight of the form component.
        /// </summary>
        /// <seealso cref="EditingComponentOrder"/>
        public int Order { get; set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="EditingComponentAttribute"/> class.
        /// </summary>
        /// <param name="formComponentIdentifier">
        /// Defines identifier of a <see cref="FormComponent{TProperties, TValue}"/> registered via <see cref="RegisterFormComponentAttribute"/>
        /// which is used for editing the annotated property from Form builder's UI.
        /// </param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="formComponentIdentifier"/> is null or an empty string.</exception>
        public EditingComponentAttribute(string formComponentIdentifier)
        {
            if (String.IsNullOrEmpty(formComponentIdentifier))
            {
                throw new ArgumentException("Form component identifier cannot be empty.", nameof(formComponentIdentifier));
            }

            FormComponentIdentifier = formComponentIdentifier;
        }
    }
}