using System;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Inherit this class to implement validation rule which needs value of another field in order to function.
    /// </summary>
    /// <typeparam name="TValue">Type of the compared values.</typeparam>
    [Serializable]
    public abstract class CompareToFieldValidationRule<TValue> : ValidationRule<TValue>, ICompareToFieldValidationRule
    {
        /// <summary>
        /// Gets strongly typed value of the field validated against.
        /// </summary>
        [XmlIgnore]
        public virtual TValue DependeeFieldValue
        {
            get;
            private set;
        }


        /// <summary>
        /// GUID of the field validated against.
        /// </summary>
        [EditingComponent(CompareToFieldSelectorComponent.IDENTIFIER, Label = "{$kentico.formbuilder.validationrule.comparetofield.dependeefieldname.label$}")]
        [Required]
        public virtual Guid DependeeFieldGuid { get; set; }


        /// <summary>
        /// Sets value of the field validated against.
        /// </summary>
        /// <param name="value">Value of the field to be set.</param>
        void ICompareToFieldValidationRule.SetDependeeFieldValue(object value)
        {
            DependeeFieldValue = (TValue)value;
        }
    }
}
