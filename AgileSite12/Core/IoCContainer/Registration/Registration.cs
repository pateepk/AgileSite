using System;

using Castle.MicroKernel.Registration;

namespace CMS.Core
{
    /// <summary>
    /// Represents a registration of type for an <see cref="IoCContainer"/>.
    /// </summary>
    internal class Registration : IRegistration
    {
        private readonly RegistrationCore<object> registration;


        /// <summary>
        /// Initializes registration for registering service type <paramref name="service"/>.
        /// </summary>
        /// <param name="service">Type of service to be registered within <see cref="IoCContainer"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/> is null.</exception>
        public Registration(Type service)
        {
            if (service == null)
            {
                throw new ArgumentNullException(nameof(service));
            }

            registration = new RegistrationCore<object>(Component.For(service));
        }


        /// <summary>
        /// Sets implementation of type represented by this registration to <paramref name="implementation"/>.
        /// </summary>
        /// <param name="implementation">Implementation type for type represented by this registration.</param>
        /// <returns>This instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="implementation"/> is null.</exception>
        public Registration ImplementedBy(Type implementation)
        {
            if (implementation == null)
            {
                throw new ArgumentNullException(nameof(implementation));
            }

            registration.ComponentRegistration = registration.ComponentRegistration.ImplementedBy(implementation);

            return this;
        }


        /// <summary>
        /// Sets implementation of type represented by this registration to <paramref name="instance"/>.
        /// </summary>
        /// <param name="instance">An instance implementing type represented by this registration.</param>
        /// <returns>This instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="instance"/> is null.</exception>
        public Registration Instance(object instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            registration.Instance(instance);

            return this;
        }


        /// <summary>
        /// Sets implementation of type represented by this registration to an instance provided by <paramref name="factoryMethod"/>.
        /// </summary>
        /// <param name="factoryMethod">Method providing an instance of type represented by this registration.</param>
        /// <returns>This instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="factoryMethod"/> is null.</exception>
        public Registration FactoryMethod(Func<object> factoryMethod)
        {
            if (factoryMethod == null)
            {
                throw new ArgumentNullException(nameof(factoryMethod));
            }

            registration.FactoryMethod(factoryMethod);

            return this;
        }


        /// <summary>
        /// Sets lifestyle of type represented by this registration to singleton.
        /// </summary>
        /// <returns>This instance.</returns>
        public Registration LifestyleSingleton()
        {
            registration.LifestyleSingleton();

            return this;
        }


        /// <summary>
        /// Sets lifestyle of type represented by this registration to transient.
        /// </summary>
        /// <returns>This instance.</returns>
        public Registration LifestyleTransient()
        {
            registration.LifestyleTransient();

            return this;
        }


        /// <summary>
        /// Marks the implementation of type represented by this registration as a fallback implementation. A fallback implementation is used
        /// only when no other implementation registration is performed.
        /// </summary>
        /// <returns>This instance.</returns>
        public Registration IsFallback()
        {
            registration.IsFallback();

            return this;
        }


        /// <summary>
        /// <para>
        /// Makes the implementation of type represented by this registration current default. This overrides any previous registrations.
        /// </para>
        /// <para>
        /// When an implementation is being registered and is not marked as default, then if becomes current default only if no other registration
        /// was already performed, or when all previous registrations were marked as fallback implementations (i.e. the first non-fallback registration wins
        /// unless explicitly overridden by this method).
        /// </para>
        /// </summary>
        /// <returns>This instance.</returns>
        /// <seealso cref="IsFallback"/>
        public Registration IsDefault()
        {
            registration.IsDefault();

            return this;
        }


        /// <summary>
        /// Sets an explicit name of the registration. If not set, the name is automatically inferred from the implementation being set. Name must be unique.
        /// </summary>
        /// <param name="name">Name to be set.</param>
        /// <returns>This instance.</returns>
        /// <remarks>
        /// The name does not need to be explicitly set unless registering one implementation multiple times (for various interfaces).
        /// </remarks>
        public Registration Named(string name)
        {
            registration.Named(name);

            return this;
        }


        /// <summary>
        /// Registers this registration within <paramref name="container"/>.
        /// </summary>
        /// <param name="container">Inversion of control container in which to perform the registration.</param>
        /// <returns>The container instance passed as parameter.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="container"/> is null.</exception>
        /// <exception cref="ServiceRegistrationException">Thrown when registration fails.</exception>
        public IoCContainer Register(IoCContainer container)
        {
            if (container == null)
            {
                throw new ArgumentNullException(nameof(container));
            }
            registration.Register(container.Container);

            return container;
        }
    }
}
