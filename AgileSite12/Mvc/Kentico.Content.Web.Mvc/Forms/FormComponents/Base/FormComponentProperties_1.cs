using CMS.DataEngine;

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Defines properties which are common to all form components. Inherit this class to create properties of custom form components.
    /// </summary>
    /// <typeparam name="TValue">Value type of the form component this properties are designed for. Determines the <see cref="DefaultValue"/> type.</typeparam>
    /// <seealso cref="EditingComponentAttribute"/>
    /// <seealso cref="EditingComponentPropertyAttribute"/>
    /// <seealso cref="EditingComponentOrder"/>
    public abstract class FormComponentProperties<TValue> : FormComponentProperties
    {
        /// <summary>
        /// When overridden in a derived class, gets or sets the default value of the form component and underlying field.
        /// Use the <see cref="DefaultValueEditingComponentAttribute"/> to specify a form component for editing.
        /// </summary>
        public abstract TValue DefaultValue
        {
            get;
            set;
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="FormComponentProperties{TValue}"/> class using the data type, size and precision given.
        /// Specifying the <paramref name="size"/> and <paramref name="precision"/> is optional and depends on whether the <paramref name="dataType"/>
        /// requires them.
        /// </summary>
        /// <param name="dataType">Default data type of values (typically a constant from <see cref="FieldDataType"/>).</param>
        /// <param name="size">Default size of values.</param>
        /// <param name="precision">Default precision of values.</param>
        /// <seealso cref="FieldDataType"/>
        /// <seealso cref="DataTypeManager"/>
        protected FormComponentProperties(string dataType, int size = -1, int precision = -1) : base(dataType, size, precision)
        {
        }


        /// <summary>
        /// Gets the default value. The <see cref="DefaultValue"/> property can be used
        /// directly to obtain the typed default value.
        /// </summary>
        /// <returns>Returns the default value.</returns>
        public override object GetDefaultValue()
        {
            return DefaultValue;
        }


        /// <summary>
        /// Sets the default value to <paramref name="value"/>.
        /// The <paramref name="value"/> must be of type <typeparamref name="TValue"/> or an exception is thrown.
        /// The <see cref="DefaultValue"/> property can be used directly to set the typed default value.
        /// </summary>
        /// <param name="value">Value to be set as the default value.</param>
        public override void SetDefaultValue(object value)
        {
            DefaultValue = (TValue)value;
        }
    }
}
