using System;

using Kentico.Content.Web.Mvc;

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Definition of a registered form component.
    /// </summary>
    public class FormComponentDefinition : ComponentDefinitionBase, IFormBuilderDefinition
    {
        internal const int IDENTIFIER_MAX_LENGTH = 100;


        /// <summary>
        /// Gets the type of the form component. The type inherits <see cref="FormComponent{TProperties, TValue}"/>.
        /// </summary>
        public Type ComponentType { get; }


        /// <summary>
        /// Gets type of the form component's properties. The type inherits <see cref="FormComponentProperties"/>.
        /// </summary>
        public Type FormComponentPropertiesType { get; }
        

        /// <summary>
        /// Gets form component's value type.
        /// </summary>
        public Type ValueType { get; }


        /// <summary>
        /// Gets or sets the name of the view to render.
        /// </summary>
        /// <remarks>
        /// If not set default location is used ("FormComponents/_{Identifier}").
        /// </remarks>
        public string ViewName { get; set; }


        /// <summary>
        /// Gets or sets the description of the form component.
        /// </summary>
        public string Description { get; set; }


        /// <summary>
        /// Gets or sets icon CSS class of the form component.
        /// </summary>
        public string IconClass { get; set; }


        /// <summary>
        /// Determines if the form component can be added via Form builder UI to a form.
        /// Set to false to allow the component to be used programmatically only.
        /// True by default.
        /// </summary>
        public bool IsAvailableInFormBuilderEditor { get; set; }


        Type IFormBuilderDefinition.DefinitionType => ComponentType;


        /// <summary>
        /// Initializes a new instance of the <see cref="ComponentType"/> class using given identifier, form component type and name.
        /// </summary>
        /// <param name="identifier">Unique identifier of the form component.</param>
        /// <param name="formComponentType">Type of the form component.</param>
        /// <param name="name">Name of the form component.</param>
        /// <exception cref="ArgumentException">
        /// <para>Specified <paramref name="identifier"/> is null, an empty string or identifier does not specify a valid code name.</para>
        /// <para>-or-</para>
        /// <para>Specified <paramref name="formComponentType"/> does not inherit <see cref="FormComponent{TProperties, TValue}"/>, is an abstract type or is a generic type which is not constructed.</para>
        /// <para>-or-</para>
        /// <para>Specified <paramref name="name"/> is null or an empty string.</para>
        /// </exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="formComponentType"/> is null.</exception>
        public FormComponentDefinition(string identifier, Type formComponentType, string name)
            : base(identifier, name)
        {
            ValidateIdentifier(identifier);
            ValidateFormComponentType(formComponentType);

            ComponentType = formComponentType;
            FormComponentPropertiesType = GetPropertiesType(formComponentType);
            ValueType = GetValueType(formComponentType);
        }


        /// <summary>
        /// Returns <see cref="Type"/> used by the <see cref="FormComponent"/>.
        /// If <see cref="ValueType"/> is type of <see cref="Nullable{T}"/> then underlying value type is returned.
        /// </summary>
        internal Type GetNonNullableValueType()
        {
            var nullableType = ValueType.FindTypeByGenericDefinition(typeof(Nullable<>));
            if (nullableType != null)
            {
                return ValueType.GetGenericArguments()[0];
            }

            return ValueType;
        }


        private void ValidateIdentifier(string identifier)
        {
            if (identifier.Length > IDENTIFIER_MAX_LENGTH)
            {
                throw new ArgumentException($"The form component identifier exceeds maximal length of {IDENTIFIER_MAX_LENGTH} characters.", nameof(identifier));
            }
        }


        private void ValidateFormComponentType(Type formComponentType)
        {
            if (formComponentType == null)
            {
                throw new ArgumentNullException(nameof(formComponentType), "The form component type must be specified.");
            }

            if (formComponentType.FindTypeByGenericDefinition(typeof(FormComponent<,>)) == null)
            {
                throw new ArgumentException($"Implementation of the '{formComponentType.FullName}' form component must inherit the '{typeof(FormComponent<,>).FullName}' class.");
            }

            if (formComponentType.IsAbstract)
            {
                throw new ArgumentException($"Implementation of the '{formComponentType.FullName}' form component type cannot be abstract.", nameof(formComponentType));
            }

            if (formComponentType.IsGenericType && !formComponentType.IsConstructedGenericType)
            {
                throw new ArgumentException($"Implementation of the '{formComponentType.FullName}' form component must be a constructed generic type.", nameof(formComponentType));
            }
        }


        private Type GetPropertiesType(Type formComponentType)
        {
            return formComponentType.FindTypeByGenericDefinition(typeof(FormComponent<,>)).GetGenericArguments()[0];
        }


        private Type GetValueType(Type formComponentType)
        {
            return formComponentType.FindTypeByGenericDefinition(typeof(FormComponent<,>)).GetGenericArguments()[1];
        }
    }
}
