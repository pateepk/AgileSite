using System;

using CMS.Core;

namespace CMS
{
    /// <summary>
    /// Marks class as implementation of <see cref="ImplementedType"/>. The application collects implementations during its initialization
    /// in order to register them within inversion of control container.
    /// </summary>
    /// <remarks>
    /// Since version 11 the attribute no longer registers implementation within <see cref="ObjectFactory{T}"/>. To register an implementation within object factory,
    /// use <see cref="ObjectFactory{T}.SetObjectTypeTo{NewType}(bool, bool)"/>.
    /// </remarks>
    /// <seealso cref="Service"/>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true, Inherited = false)]
    public sealed class RegisterImplementationAttribute : Attribute
    {
        /// <summary>
        /// Type for which an implementation is being registered.
        /// </summary>
        public Type ImplementedType
        {
            get;
            private set;
        }


        /// <summary>
        /// Implementation of <see cref="ImplementedType"/> to be used by <see cref="ObjectFactory{T}"/>.
        /// </summary>
        public Type Implementation
        {
            get;
            private set;
        }


        /// <summary>
        /// Lifestyle of service's implementation registered by this attribute. <see cref="Lifestyle.Singleton"/> by default.
        /// </summary>
        public Lifestyle Lifestyle
        {
            get;
            set;
        } = Lifestyle.Singleton;


        /// <summary>
        /// Sets an explicit name of the registration. If not set, the name is automatically inferred from the implementation being registered.
        /// </summary>
        public string Name
        {
            get;
            set;
        }


        /// <summary>
        /// Priority of service's registration. <see cref="RegistrationPriority.Default"/> by default.
        /// </summary>
        /// <remarks>
        /// The order of registrations is determined by the order in which modules (assemblies) are initialized. The order respects assemblies' references.
        /// There is no guarantee on order within an assembly.
        /// </remarks>
        public RegistrationPriority Priority
        {
            get;
            set;
        } = RegistrationPriority.Default;


        /// <summary>
        /// Instructs <see cref="ObjectFactory{T}"/> to use <paramref name="implementation"/> whenever an instance of <paramref name="implementedType"/> is requested.
        /// </summary>
        /// <param name="implementedType">Interface to associate implementation with.</param>
        /// <param name="implementation">Implementation of <paramref name="implementedType"/>.</param>
        public RegisterImplementationAttribute(Type implementedType, Type implementation)
        {
            if (!implementedType.IsAssignableFrom(implementation))
            {
                throw new ArgumentException($"Implementation '{implementation.FullName}' cannot be registered for type '{implementedType.FullName}' as it is of incompatible type.", nameof(implementation));
            }

            ImplementedType = implementedType;
            Implementation = implementation;
        }
    }
}