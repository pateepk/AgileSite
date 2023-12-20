using System;

namespace CMS.Core
{
    /// <summary>
    /// Contains extension methods to work with <see cref="IoCContainer"/>.
    /// </summary>
    internal static class IoCContainerExtensions
    {
        /// <summary>
        /// Processes <paramref name="implementationAttribute"/> by associating implementation with implemented type.
        /// </summary>
        /// <param name="container">Inversion of control container to perform registration on.</param>
        /// <param name="implementationAttribute">Register implementation attribute to be processed.</param>
        public static void RegisterImplementation(this IoCContainer container, RegisterImplementationAttribute implementationAttribute)
        {
            if (container == null)
            {
                throw new ArgumentNullException(nameof(container));
            }
            if (implementationAttribute == null)
            {
                throw new ArgumentNullException(nameof(implementationAttribute));
            }

            var registration = new Registration(implementationAttribute.ImplementedType).ImplementedBy(implementationAttribute.Implementation);

            switch (implementationAttribute.Priority)
            {
                case RegistrationPriority.Default:
                    registration.IsDefault();

                    break;

                case RegistrationPriority.Fallback:
                    registration.IsFallback();

                    break;

                case RegistrationPriority.SystemDefault:
                    // No action needed
                    break;

                default:
                    throw new InvalidOperationException($"Registration priority '{implementationAttribute.Priority}' cannot be processed. Only values corresponding to '{nameof(RegistrationPriority.Default)}', '{nameof(RegistrationPriority.Fallback)}' and '{nameof(RegistrationPriority.SystemDefault)}' priorities are supported by this method.");
            }

            if (!string.IsNullOrEmpty(implementationAttribute.Name))
            {
                registration.Named(implementationAttribute.Name);
            }

            if (implementationAttribute.Lifestyle == Lifestyle.Singleton)
            {
                registration.LifestyleSingleton();
            }
            else if (implementationAttribute.Lifestyle == Lifestyle.Transient)
            {
                registration.LifestyleTransient();
            }
            else
            {
                throw new InvalidOperationException($"Cannot set lifestyle of '{implementationAttribute.ImplementedType.FullName}' to value '{implementationAttribute.Lifestyle}'. Only '{nameof(Lifestyle.Singleton)}' and '{nameof(Lifestyle.Transient)}' registrations are supported by this method.");
            }

            container.Register(registration);
        }
    }
}
