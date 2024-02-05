using System;

using CMS.Core;

namespace CMS
{
    /// <summary>
    /// <para>
    /// Instructs <see cref="ObjectFactory{T}"/> to use <see cref="Implementation"/> as a default implementation when an instance of interface or class targeted by this attribute is requested.
    /// </para>
    /// <para>
    /// Important: The attribute performed registration of a <see cref="Service{ServiceInterface}"/>. The service's registration was backed by <see cref="ObjectFactory{T}"/>, therefore
    /// the attribute performed registration in <see cref="ObjectFactory{T}"/>. Since <see cref="Service{ServiceInterface}"/> registration is no longer backed by <see cref="ObjectFactory{T}"/>,
    /// the suggested replacement (<see cref="RegisterImplementationAttribute"/>) does not affect <see cref="ObjectFactory{T}"/>. If registration in object factory is intended,
    /// use <see cref="ObjectFactory{T}.SetObjectTypeTo{NewType}(bool, bool)"/>.
    /// </para>
    /// </summary>
    /// <remarks>
    /// The default implementation can be replaced using <see cref="RegisterImplementationAttribute"/> in any assembly (unless <see cref="CanBeReplaced"/> forbids it).
    /// </remarks>
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    [Obsolete("Use [assembly: CMS.RegisterImplementation(typeof(ImplementedType), typeof(Implementation), Priority = CMS.Core.RegistrationPriority.Fallback)]", true)]
    public sealed class DefaultImplementationAttribute : Attribute
    {
        /// <summary>
        /// Implementation of interface or class targeted by this attribute to be used by <see cref="ObjectFactory{T}"/>.
        /// </summary>
        public Type Implementation
        {
            get;
            private set;
        }


        /// <summary>
        /// Indicates whether <see cref="ObjectFactory{T}"/> allows for replacement of the default implementation.
        /// </summary>
        public bool CanBeReplaced
        {
            get;
            set;
        }


        /// <summary>
        /// Instructs <see cref="ObjectFactory{T}"/> to use <paramref name="implementation"/> whenever an instance of interface or class targeted by this attribute is requested.
        /// </summary>
        /// <param name="implementation">Default implementation of interface or class targeted by this attribute.</param>
        /// <param name="canBeReplaced">If true, the default implementation can be replaced.</param>
        public DefaultImplementationAttribute(Type implementation, bool canBeReplaced = true)
        {
            Implementation = implementation;
            CanBeReplaced = canBeReplaced;
        }
    }
}
