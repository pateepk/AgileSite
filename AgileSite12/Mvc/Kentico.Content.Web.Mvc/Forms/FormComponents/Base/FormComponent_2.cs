using System;

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Represents a base class for a form component.
    /// Inherit this class to define a custom form component.
    /// </summary>
    /// <typeparam name="TProperties">Properties type of the form component.</typeparam>
    /// <typeparam name="TValue">Value type of the form component.</typeparam>
    /// <remarks>
    /// Properties which are expected to have automatically bound values after form submit,
    /// have to be annotated with <see cref="BindablePropertyAttribute"/>.
    /// </remarks>
    public abstract class FormComponent<TProperties, TValue> : FormComponent where TProperties : FormComponentProperties<TValue>, new()
    {
        /// <summary>
        /// Gets the form component's properties.
        /// </summary>
        public TProperties Properties
        {
            get;
            protected set;
        }


        /// <summary>
        /// Gets the form component's properties as its base type.
        /// </summary>
        public override FormComponentProperties BaseProperties
        {
            get
            {
                return Properties;
            }
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="FormComponent{TProperties, TValue}"/> class.
        /// </summary>
        protected FormComponent()
            : base(typeof(TProperties))
        {
        }


        /// <summary>
        /// Gets the value of the form component. Implement in a subclass to return value of a desired property.
        /// Method can also be used to return a value composed of multiple properties if required.
        /// </summary>
        /// <returns>Returns the value of the form component.</returns>
        public abstract TValue GetValue();


        /// <summary>
        /// Sets the value of the form component. Implement in a subclass to set value of a desired property.
        /// Method can also be used to decompose passed value into several properties rendered in the resulting form component markup.
        /// </summary>
        /// <param name="value">Value to be set.</param>
        public abstract void SetValue(TValue value);


        /// <summary>
        /// Gets the value of the form component. The <see cref="GetValue"/> method can be used
        /// directly to obtain the typed value.
        /// </summary>
        /// <returns>Returns the value of the form component.</returns>
        public override object GetObjectValue()
        {
            return GetValue();
        }


        /// <summary>
        /// Sets the value of the form component.
        /// The <paramref name="value"/> must be of proper type or an exception is thrown.
        /// The <see cref="SetValue"/> method can be used directly to set the typed default value.
        /// </summary>
        /// <param name="value">Value to be set.</param>
        public override void SetObjectValue(object value)
        {
            // Do nothing when trying to set null to non-nullable type.
            if (value == null && default(TValue) != null)
            {
                return;
            }

            SetValue((TValue)value);
        }


        /// <summary>
        /// Loads properties of the form component. The actual <paramref name="properties"/> type must match the <see cref="FormComponent.PropertiesType"/>.
        /// </summary>
        /// <param name="properties">Form component properties to be loaded.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="properties"/> are null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="properties"/> do not match the <see cref="FormComponent.PropertiesType"/>.</exception>
        public override void LoadProperties(FormComponentProperties properties)
        {
            if (properties == null)
            {
                throw new ArgumentNullException(nameof(properties));
            }

            var typedProperties = properties as TProperties;
            Properties = typedProperties ?? throw new ArgumentException($"Properties for the '{GetType().FullName}' form component must be of type '{PropertiesType.FullName}'. Given instance of properties of type '{properties.GetType().FullName}' cannot be used.");
        }
    }
}
