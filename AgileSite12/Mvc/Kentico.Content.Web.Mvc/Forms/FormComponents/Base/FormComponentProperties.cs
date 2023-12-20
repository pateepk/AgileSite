using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using CMS.DataEngine;

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Defines properties which are common to all form components.
    /// Do not inherit this class directly, inherit the <see cref="FormComponentProperties{TValue}"/> class instead.
    /// </summary>
    /// <seealso cref="EditingComponentAttribute"/>
    /// <seealso cref="EditingComponentPropertyAttribute"/>
    /// <seealso cref="EditingComponentOrder"/>
    public abstract class FormComponentProperties
    {
        /// <summary>
        /// Gets or sets the underlying field data type.
        /// </summary>
        /// <seealso cref="FieldDataType"/>
        /// <seealso cref="Size"/>
        /// <seealso cref="Precision"/>
        public string DataType
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the underlying field size.
        /// </summary>
        /// <seealso cref="DataType"/>
        public int Size
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the underlying field precision.
        /// </summary>
        /// <seealso cref="DataType"/>
        public int Precision
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the underlying field GUID.
        /// </summary>
        public Guid Guid
        {
            get;
            set;
        } = Guid.NewGuid();


        /// <summary>
        /// Gets or sets the underlying field name.
        /// </summary>
        [EditingComponent(NameComponent.IDENTIFIER, Label = "{$kentico.formbuilder.fieldname.label$}", Tooltip = "{$kentico.formbuilder.fieldname.tooltip$}", Order = EditingComponentOrder.NAME)]
        [Required]
        public virtual string Name
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets validation rule configurations assigned to the form component.
        /// </summary>
        public IList<ValidationRuleConfiguration> ValidationRuleConfigurations
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets visibility condition configuration assigned to the form component.
        /// </summary>
        public VisibilityConditionConfiguration VisibilityConditionConfiguration
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the explanation text of the form component.
        /// </summary>
        [EditingComponent(TextInputComponent.IDENTIFIER, Label = "{$formbuilder.explanation$}", Order = EditingComponentOrder.EXPLANATION_TEXT)]
        public virtual string ExplanationText
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the label of the form component.
        /// </summary>
        [EditingComponent(TextInputComponent.IDENTIFIER, Label = "{$general.label$}", Order = EditingComponentOrder.LABEL)]
        public virtual string Label
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets value indicating whether the underlying field is required. False by default.
        /// If false, the form component's implementation must accept nullable input.
        /// </summary>
        [EditingComponent(CheckBoxComponent.IDENTIFIER, Label = "", Order = EditingComponentOrder.REQUIRED)]
        [EditingComponentProperty(nameof(CheckBoxProperties.Text), "{$kentico.formbuilder.required.label$}")]
        public virtual bool Required
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets value indicating whether the underlying field is smart field. False by default.
        /// </summary>
        [EditingComponent(CheckBoxComponent.IDENTIFIER, Label = "", Tooltip = "{$kentico.formbuilder.smartfield.tooltip$}", Order = EditingComponentOrder.SMART_FIELD)]
        [EditingComponentProperty(nameof(CheckBoxProperties.Text), "{$kentico.formbuilder.smartfield.label$}")]
        [SmartFieldPropertyValidation]
        [RequiresFeatures(FeatureEnum.FullContactManagement)]
        public virtual bool SmartField
        {
            get;
            set;
        } = false;


        /// <summary>
        /// Gets or sets the tooltip of the form component.
        /// </summary>
        [EditingComponent(TextInputComponent.IDENTIFIER, Label = "{$formbuilder.tooltip$}", Order = EditingComponentOrder.TOOLTIP)]
        public virtual string Tooltip
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the value displayed as placeholder in the input.
        /// Override and annotate with <see cref="EditingComponentAttribute"/> to make this property
        /// editable in properties panel.
        /// </summary>
        public virtual string Placeholder
        {
            get;
            set;
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="FormComponentProperties"/> class using the data type, size and precision given.
        /// </summary>
        /// <param name="dataType">Default data type of values (typically a constant from <see cref="FieldDataType"/>).</param>
        /// <param name="size">Default size of values.</param>
        /// <param name="precision">Default precision of values.</param>
        /// <seealso cref="FieldDataType"/>
        /// <seealso cref="DataTypeManager"/>
        protected FormComponentProperties(string dataType, int size, int precision)
        {
            DataType = dataType;
            Size = size;
            Precision = precision;
            ValidationRuleConfigurations = new List<ValidationRuleConfiguration>();
        }


        /// <summary>
        /// When overridden in a derived class, gets the default value.
        /// </summary>
        /// <returns>Returns the default value.</returns>
        public abstract object GetDefaultValue();


        /// <summary>
        /// When overridden in a derived class, sets the default value to <paramref name="value"/>.
        /// The <paramref name="value"/> must be of proper type or an exception is thrown.
        /// </summary>
        /// <param name="value">Value to be set as the default value.</param>
        public abstract void SetDefaultValue(object value);
    }
}
