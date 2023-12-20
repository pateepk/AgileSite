using CMS.DataEngine;

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Represents properties of a <see cref="RecaptchaComponent"/>.
    /// </summary>
    public class RecaptchaProperties : FormComponentProperties<string>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RecaptchaProperties"/> class.
        /// </summary>
        /// <remarks>
        /// The constructor initializes the base class to data type <see cref="FieldDataType.Text"/> and size 1.
        /// </remarks>
        public RecaptchaProperties() : base(FieldDataType.Text, 1)
        {
        }


        /// <summary>
        /// Gets or sets the default value of the form component and underlying field.
        /// </summary>
        public override string DefaultValue
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets value indicating whether the underlying field is required. False by default.
        /// If false, the form component's implementation must accept nullable input.
        /// </summary>
        public override bool Required
        {
            get;
            set;
        }


        /// <summary>
        /// Represents the color theme of the component (light or dark).
        /// </summary>
        [EditingComponent(DropDownComponent.IDENTIFIER, Label = "{$kentico.formbuilder.component.recaptcha.properties.theme$}", Order = 1)]
        [EditingComponentProperty(nameof(DropDownProperties.DataSource), "light;{$kentico.formbuilder.component.recaptcha.properties.theme.light$}\r\ndark;{$kentico.formbuilder.component.recaptcha.properties.theme.dark$}")]
        public string Theme
        {
            get;
            set;
        }


        /// <summary>
        /// Represents the layout of the component (normal or compact).
        /// </summary>
        [EditingComponent(DropDownComponent.IDENTIFIER, Label = "{$kentico.formbuilder.component.recaptcha.properties.layout$}", Order = 2)]
        [EditingComponentProperty(nameof(DropDownProperties.DataSource), "normal;{$kentico.formbuilder.component.recaptcha.properties.layout.normal$}\r\ncompact;{$kentico.formbuilder.component.recaptcha.properties.layout.compact$}")]
        public string Layout
        {
            get;
            set;
        }


        /// <summary>
        /// Represents the type of the component (image or audio).
        /// </summary>
        [EditingComponent(DropDownComponent.IDENTIFIER, Label = "{$kentico.formbuilder.component.recaptcha.properties.type$}", Order = 3)]
        [EditingComponentProperty(nameof(DropDownProperties.DataSource), "image;{$kentico.formbuilder.component.recaptcha.properties.type.image$}\r\naudio;{$kentico.formbuilder.component.recaptcha.properties.type.audio$}")]
        public string Type
        {
            get;
            set;
        }
    }
}