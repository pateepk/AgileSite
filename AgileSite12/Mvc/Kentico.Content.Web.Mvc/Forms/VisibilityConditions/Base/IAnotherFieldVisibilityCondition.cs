using System;

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Denotes a visibility condition which depends on another field. The other field's value can be used
    /// to evaluate the visibility condition.
    /// This interface is used by the system to get the GUID of the depending field and supply its value.
    /// </summary>
    internal interface IAnotherFieldVisibilityCondition
    {
        /// <summary>
        /// Gets or sets GUID of the other field.
        /// </summary>
        Guid DependeeFieldGuid { get; set; }


        /// <summary>
        /// Sets value of the other field.
        /// </summary>
        /// <param name="value">Value of the field to be set.</param>
        void SetDependeeFieldValue(object value);
    }
}
