using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;

using CMS.Helpers;

using Kentico.Web.Mvc;

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Represents a base class for a form component containing members used by the system.
    /// Do not inherit this class directly, inherit the <see cref="FormComponent{TProperties, TValue}"/> class instead.
    /// </summary>
    /// <remarks>
    /// Properties which are expected to have automatically bound values after form submit,
    /// have to be annotated with <see cref="BindablePropertyAttribute"/>.
    /// Implements the <see cref="IValidatableObject"/> to support custom validation.
    /// </remarks>
    public abstract class FormComponent : IValidatableObject, IModelMetadataModifier
    {
        /// <summary>
        /// Gets the properties type of the form component.
        /// </summary>
        public Type PropertiesType
        {
            get;
        }


        /// <summary>
        /// Gets the form component's properties as its base type.
        /// </summary>
        public abstract FormComponentProperties BaseProperties
        {
            get;
        }


        /// <summary>
        /// Gets or sets the name of the corresponding form field.
        /// The name is used as a prefix when naming the HTML input.
        /// </summary>
        /// <seealso cref="LabelForPropertyName"/>
        public virtual string Name
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets a value indicating whether there are another form components
        /// whose visibility condition depends on this component's value.
        /// </summary>
        /// <seealso cref="UpdatableMvcForm.NOT_OBSERVED_ELEMENT_ATTRIBUTE_NAME"/>
        public virtual bool HasDependingFields
        {
            get;
            set;
        }


        /// <summary>
        /// If true, prevents built-in visibility condition handling for this component.
        /// </summary>
        /// <seealso cref="UpdatableMvcForm.NOT_OBSERVED_ELEMENT_ATTRIBUTE_NAME"/>
        public virtual bool CustomAutopostHandling
        {
            get;
        }


        /// <summary>
        /// Gets the name of the property representing the editing field in the resulting HTML markup.
        /// The name is used to infer a proper label <c>for</c> attribute value in the rendered HTML markup.
        /// Defaults to first found property annotated with <see cref="BindablePropertyAttribute"/>.
        /// </summary>
        /// <seealso cref="Name"/>
        public virtual string LabelForPropertyName
        {
            get
            {
                return GetType()
                    .GetProperties()
                    .FirstOrDefault(p => p.GetCustomAttributes(typeof(BindablePropertyAttribute), false).Length > 0)
                    .Name;
            }
        }


        /// <summary>
        /// Gets or sets the definition under which the form component is registered in the system.
        /// </summary>
        public FormComponentDefinition Definition
        {
            get;
            set;
        }


        /// <summary>
        /// Also shows validation errors of partial values of the component.
        /// If false, each individual component should contain validation message in its partial view.
        /// </summary>
        public bool ShowPartialValidationMessages
        {
            get;
            set;
        } = true;


        /// <summary>
        /// Gets or sets the context the form component is being used in.
        /// </summary>
        internal FormComponentContext FormComponentContext
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the model's property the form component represents, if the form component
        /// has been created for a strongly typed model.
        /// </summary>
        internal PropertyInfo ModelProperty
        {
            get;
            set;
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="FormComponent"/> class for the specified properties type.
        /// </summary>
        /// <param name="propertiesType">Properties type of the form component.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="propertiesType"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="propertiesType"/> specifies a type which does not inherit <see cref="FormComponentProperties"/>.</exception>
        protected FormComponent(Type propertiesType)
        {
            if (propertiesType == null)
            {
                throw new ArgumentNullException(nameof(propertiesType));
            }

            ValidatePropertiesType(propertiesType);

            PropertiesType = propertiesType;
        }


        /// <summary>
        /// Gets the value of the form component.
        /// </summary>
        /// <returns>Returns the value of the form component.</returns>
        public abstract object GetObjectValue();


        /// <summary>
        /// Sets the value of the form component.
        /// The <paramref name="value"/> must be of proper type or an exception is thrown.
        /// </summary>
        /// <param name="value">Value to be set.</param>
        public abstract void SetObjectValue(object value);


        /// <summary>
        /// Loads properties of the form component. The actual <paramref name="properties"/> type must match the <see cref="PropertiesType"/>.
        /// </summary>
        /// <param name="properties">Form component properties to be loaded.</param>
        public abstract void LoadProperties(FormComponentProperties properties);


        /// <summary>
        /// <para>
        /// Binds contextual information to the form component. The actual type of <paramref name="context"/>
        /// depends on where the component is being used.
        /// </para>
        /// <para>
        /// The component can throw an exception if being used in a context for which it is not designed. The base implementation
        /// provided by this class must be called prior to throwing an exception, however.
        /// </para>
        /// </summary>
        public virtual void BindContext(FormComponentContext context)
        {
            FormComponentContext = context;
        }


        /// <summary>
        /// Gets a collection of custom attributes applied to the model's property
        /// this form component represents.
        /// </summary>
        /// <typeparam name="T">The type of attribute to search for.</typeparam>
        /// <param name="inherit">A value indicating whether to inspect the ancestors of model's property.</param>
        /// <returns>Returns a collection of the custom attributes, or null if this form component was not instantiated for a model.</returns>
        /// <remarks>
        /// The enumeration is available for form components created for a strongly typed model only. Form components representing
        /// forms built using the Form builder do not have a strongly typed model bound (i.e. null is returned).
        /// </remarks>
        public IEnumerable<T> GetModelPropertyCustomAttributes<T>(bool inherit) where T : Attribute
        {
            return ModelProperty?.GetCustomAttributes<T>(inherit);
        }


        /// <summary>
        /// Determines whether the specified object is valid.
        /// Only invoked if validation by validation attributes passes for all fields.
        /// </summary>
        /// <param name="validationContext">The validation context.</param>
        /// <returns>A collection that holds failed-validation information.</returns>
        public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            return Enumerable.Empty<ValidationResult>();
        }


        private void ValidatePropertiesType(Type propertiesType)
        {
            if (!typeof(FormComponentProperties).IsAssignableFrom(propertiesType))
            {
                throw new ArgumentException($"Properties of form component '{GetType().FullName}' must inherit '{typeof(FormComponentProperties).FullName}'. Given properties of type '{propertiesType.FullName}' are not valid.");
            }
        }


        /// <summary>
        /// Returns non-localized string representing component name.
        /// Component <see cref="FormComponentProperties.Name"/> is returned in case of empty <see cref="FormComponentProperties.Label"/>.
        /// </summary>
        public string GetDisplayName()
        {
            // Get component label
            var fieldCaption = BaseProperties.Label;
            if (String.IsNullOrEmpty(fieldCaption))
            {
                fieldCaption = BaseProperties.Name;
            }

            return fieldCaption;
        }


        /// <summary>
        /// Modifies <paramref name="modelMetadata"/> according to the current object.
        /// </summary>
        /// <remarks>
        /// Sets display names for properties annotated by <see cref="BindablePropertyAttribute"/> according to <see cref="FormComponentProperties.Label"/>.
        /// If property is annotated with <see cref="DisplayAttribute"/> then the value of the attribute is used.
        /// </remarks>
        /// <param name="modelMetadata">Metadata to modify.</param>
        public void ModifyMetadata(ModelMetadata modelMetadata)
        {
            var bindableProperties = new HashSet<string>(this.GetBindablePropertyNames(), StringComparer.OrdinalIgnoreCase);
            foreach (var propertyModelMetadata in modelMetadata.Properties.Where(p => bindableProperties.Contains(p.PropertyName) && String.IsNullOrEmpty(p.DisplayName) &&
                                                                                                                                !String.IsNullOrEmpty(BaseProperties.Label)))
            {
                propertyModelMetadata.DisplayName = ResHelper.LocalizeString(BaseProperties.Label);
            }
        }
    }
}
