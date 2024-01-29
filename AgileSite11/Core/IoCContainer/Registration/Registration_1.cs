using System;

using Castle.MicroKernel.Registration;

namespace CMS.Core
{
    /// <summary>
    /// Represents a registration of type <typeparamref name="TService"/> for an <see cref="IoCContainer"/>.
    /// </summary>
    /// <typeparam name="TService">Type of service to be registered within <see cref="IoCContainer"/>.</typeparam>
    internal class Registration<TService> : IRegistration where TService : class
    {
        private readonly RegistrationCore<TService> registration = new RegistrationCore<TService>();


        /// <summary>
        /// Initializes registration for registering service type <typeparamref name="TService"/>.
        /// </summary>
        public Registration()
        {
            registration.componentRegistrationBuilder = Component.For<TService>;
        }


        /// <summary>
        /// Sets implementation of type <typeparamref name="TService"/> to <typeparamref name="TImplementation"/>.
        /// </summary>
        /// <typeparam name="TImplementation">Implementation type for <typeparamref name="TService"/>.</typeparam>
        /// <returns>This instance.</returns>
        public Registration<TService> ImplementedBy<TImplementation>() where TImplementation : TService
        {
            var baseComponentRegistrationBuilder = registration.componentRegistrationBuilder;

            registration.componentRegistrationBuilder = () => baseComponentRegistrationBuilder().ImplementedBy<TImplementation>();

            return this;
        }


        /// <summary>
        /// Sets implementation of type <typeparamref name="TService"/> to an instance of <typeparamref name="TImplementation"/>.
        /// </summary>
        /// <typeparam name="TImplementation">Implementation type for <typeparamref name="TService"/>.</typeparam>
        /// <param name="instance">An instance implementing <typeparamref name="TService"/>.</param>
        /// <returns>This instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="instance"/> is null.</exception>
        public Registration<TService> Instance<TImplementation>(TImplementation instance) where TImplementation : TService
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            registration.Instance(instance);

            return this;
        }


        /// <summary>
        /// Sets implementation of type <typeparamref name="TService"/> to an instance provided by <paramref name="factoryMethod"/>.
        /// </summary>
        /// <typeparam name="TImplementation">Implementation type for <typeparamref name="TService"/>.</typeparam>
        /// <param name="factoryMethod">Method providing an instance of <typeparamref name="TService"/>.</param>
        /// <returns>This instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="factoryMethod"/> is null.</exception>
        public Registration<TService> FactoryMethod<TImplementation>(Func<TImplementation> factoryMethod) where TImplementation : TService
        {
            if (factoryMethod == null)
            {
                throw new ArgumentNullException(nameof(factoryMethod));
            }

            registration.FactoryMethod(factoryMethod);

            return this;
        }


        /// <summary>
        /// Sets lifestyle of type <typeparamref name="TService"/> represented by this registration to singleton.
        /// </summary>
        /// <returns>This instance.</returns>
        public Registration<TService> LifestyleSingleton()
        {
            registration.LifestyleSingleton();

            return this;
        }


        /// <summary>
        /// Sets lifestyle of type <typeparamref name="TService"/> represented by this registration to transient.
        /// </summary>
        /// <returns>This instance.</returns>
        public Registration<TService> LifestyleTransient()
        {
            registration.LifestyleTransient();

            return this;
        }


        /// <summary>
        /// Marks the implementation of type represented by this registration as a fallback implementation. A fallback implemenatation is used
        /// only when no other implementation registration is performed.
        /// </summary>
        /// <returns>This instance.</returns>
        public Registration<TService> IsFallback()
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
        public Registration<TService> IsDefault()
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
        public Registration<TService> Named(string name)
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

            return registration.Register(container);
        }
    }
}
