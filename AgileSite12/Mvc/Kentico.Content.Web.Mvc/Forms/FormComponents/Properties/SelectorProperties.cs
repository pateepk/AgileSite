using CMS.DataEngine;

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Provides basic component support for selectors.
    /// </summary>
    public class SelectorProperties : FormComponentProperties<string>
    {
        /// <summary>
        /// Gets or sets the default value of the component.
        /// </summary>
        [DefaultValueEditingComponent(TextInputComponent.IDENTIFIER)]
        public override string DefaultValue
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the data populated in <see cref="SelectorFormComponent{TProperties}"/> represented by string.
        /// </summary>
        [EditingComponent(TextAreaComponent.IDENTIFIER, Label = "{$kentico.formbuilder.component.selector.properties.options$}", Tooltip = "{$kentico.formbuilder.component.selector.properties.options.tooltip$}")]
        public string DataSource
        {
            get;
            set;
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="SelectorProperties"/> class.
        /// </summary>
        /// <remarks>
        /// The constructor initializes the base class to data type <see cref="FieldDataType.Text"/>.
        /// </remarks>
        public SelectorProperties()
            : base(FieldDataType.Text)
        {
        }
    }
}
