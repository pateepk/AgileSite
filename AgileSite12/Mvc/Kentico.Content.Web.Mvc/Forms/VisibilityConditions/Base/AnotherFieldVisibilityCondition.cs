using System;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Inherit this class to implement visibility condition which needs value of another field in order to function.
    /// </summary>
    /// <typeparam name="TValue">Type of the other field value. For value types, use their <see cref="Nullable{T}"/> equivalent.</typeparam>
    [Serializable]
    public abstract class AnotherFieldVisibilityCondition<TValue> : VisibilityCondition, IAnotherFieldVisibilityCondition
    {
        /// <summary>
        /// Gets strongly typed value of the field the condition depends on.
        /// </summary>
        [XmlIgnore]
        public virtual TValue DependeeFieldValue
        {
            get;
            private set;
        }


        /// <summary>
        /// GUID of the field the condition depends on.
        /// </summary>
        [Required]
        public virtual Guid DependeeFieldGuid { get; set; }


        /// <summary>
        /// Sets value of the other field.
        /// </summary>
        /// <param name="value">Value of the field to be set.</param>
        void IAnotherFieldVisibilityCondition.SetDependeeFieldValue(object value)
        {
            DependeeFieldValue = (TValue)value;
        }
    }
}
