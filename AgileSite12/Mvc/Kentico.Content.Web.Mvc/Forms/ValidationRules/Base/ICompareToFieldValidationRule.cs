using System;

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Denotes a validation rule which depends on another field. The other field's value can be used
    /// to evaluate the validation rule.
    /// This interface is used by the system to get the GUID of the depending field and supply its value.
    /// </summary>
    internal interface ICompareToFieldValidationRule
    {
        /// <summary>
        /// GUID of the field validated against.
        /// </summary>
        Guid DependeeFieldGuid { get; }


        /// <summary>
        /// Sets value of the field validated against.
        /// </summary>
        /// <param name="value">Value of the field to be set.</param>
        void SetDependeeFieldValue(object value);
    }
}
