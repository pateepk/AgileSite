using System;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using System.Xml.Serialization;

using Kentico.Web.Mvc;
using Kentico.Content.Web.Mvc;

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Represents a base class for a form component validation rule.
    /// Do not inherit this class directly, inherit the <see cref="ValidationRule{TValue}"/> class instead.
    /// </summary>
    /// <seealso cref="ValidationRuleEditingComponentOrder"/>
    [Serializable]
    public abstract class ValidationRule : IModelMetadataModifier
    {
        /// <summary>
        /// Gets or sets identifier of the validation rule instance.
        /// </summary>
        [EditingComponent(HiddenGuidInputComponent.IDENTIFIER, Order = ValidationRuleEditingComponentOrder.INSTANCE_IDENTIFIER)]
        public Guid InstanceIdentifier
        {
            get;
            set;
        }


        /// <summary>
        /// Gets a title for this instance of validation rule.
        /// </summary>
        /// <seealso cref="GetTitle"/>
        [XmlIgnore]
        public string Title => GetTitle();


        /// <summary>
        /// Gets or sets the error message to be shown when the form component's value is invalid.
        /// </summary>
        [EditingComponent(TextAreaComponent.IDENTIFIER, Label = "{$general.errormessage$}", Order = ValidationRuleEditingComponentOrder.ERROR_MESSAGE )]
        [Required]
        public string ErrorMessage
        {
            get;
            set;
        }


        /// <summary>
        /// Validates the form component's value and returns <c>true</c> if the value is valid.
        /// </summary>
        /// <param name="value">Value of the form component.</param>
        /// <returns><c>true</c> if the value is valid, otherwise false.</returns>
        public virtual bool IsValueValid(object value)
        {
            return true;
        }


        /// <summary>
        /// Gets a title for this instance of validation rule such as 'Maximum length is 100'.
        /// </summary>
        /// <returns>Returns text describing the validation rule.</returns>
        /// <see cref="Title"/>
        public abstract string GetTitle();


        /// <summary>
        /// Modifies <paramref name="modelMetadata"/> according to the current object.
        /// </summary>
        /// <remarks>Sets display names for properties handled by editing component. Editing component's label is used.</remarks>
        /// <param name="modelMetadata">Metadata to modify.</param>
        public void ModifyMetadata(ModelMetadata modelMetadata)
        {
            ModelMetadataHelper.ModifyModelMetadata(modelMetadata, this);
        }
    }
}