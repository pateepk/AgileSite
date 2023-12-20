using System;

using Castle.MicroKernel.Registration;
using Castle.Windsor;

namespace CMS.Core
{
    /// <summary>
    /// Represents a registration for an <see cref="IoCContainer"/>.
    /// </summary>
    /// <typeparam name="TService">Type of service for strongly typed registrations.</typeparam>
    /// <remarks>
    /// This class serves as an aid to reduce amount of code for registration within Castle Windsor (CW) container. CW represents non-generic registration by <typeparamref name="TService"/> of type <see cref="object"/>
    /// while using different constructor to indicate that <typeparamref name="TService"/> does not represent the actual type being registered (see how <see cref="ComponentRegistration"/> is being initialized).
    /// </remarks>
    internal class RegistrationCore<TService> where TService : class
    {
        /// <summary>
        /// Gets or sets the component registration to be executed when <see cref="Register"/> is called.
        /// </summary>
        internal ComponentRegistration<TService> ComponentRegistration
        {
            get;
            set;
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="RegistrationCore{TService}"/> class with given <paramref name="initialComponentRegistration"/>.
        /// </summary>
        public RegistrationCore(ComponentRegistration<TService> initialComponentRegistration)
        {
            ComponentRegistration = initialComponentRegistration;
        }


        /// <summary>
        /// Sets implementation to an instance of <typeparamref name="TImplementation"/>.
        /// </summary>
        public void Instance<TImplementation>(TImplementation instance) where TImplementation : TService
        {
            ComponentRegistration = ComponentRegistration.Instance(instance);
        }


        /// <summary>
        /// Sets implementation to an instance provided by <paramref name="factoryMethod"/>.
        /// </summary>
        public void FactoryMethod<TImplementation>(Func<TImplementation> factoryMethod) where TImplementation : TService
        {
            ComponentRegistration = ComponentRegistration.UsingFactoryMethod(factoryMethod);
        }


        /// <summary>
        /// Sets lifestyle of type represented by this registration to singleton.
        /// </summary>
        public void LifestyleSingleton()
        {
            ComponentRegistration = ComponentRegistration.LifestyleSingleton();
        }


        /// <summary>
        /// Sets lifestyle of type represented by this registration to transient.
        /// </summary>
        public void LifestyleTransient()
        {
            ComponentRegistration = ComponentRegistration.LifestyleTransient();
        }


        /// <summary>
        /// Marks the implementation of type represented by this registration as a fallback implementation. A fallback implementation is used
        /// only when no other implementation registration is performed.
        /// </summary>
        public void IsFallback()
        {
            ComponentRegistration = ComponentRegistration.IsFallback();
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
        public void IsDefault()
        {
            ComponentRegistration = ComponentRegistration.IsDefault();
        }


        /// <summary>
        /// Sets an explicit name of the registration. If not set, the name is automatically inferred from the implementation being set. Name must be unique.
        /// </summary>
        /// <param name="name">Name to be set.</param>
        /// <remarks>
        /// The name does not need to be explicitly set unless registering one implementation multiple times (for various interfaces).
        /// </remarks>
        public void Named(string name)
        {
            ComponentRegistration = ComponentRegistration.Named(name);
        }


        /// <summary>
        /// Registers this registration within <paramref name="container"/>.
        /// </summary>
        /// <param name="container">Inversion of control container in which to perform the registration.</param>
        /// <returns>The container instance passed as parameter.</returns>
        /// <exception cref="ServiceRegistrationException">Thrown when registration fails.</exception>
        public void Register(WindsorContainer container)
        {
            try
            {
                container.Register(ComponentRegistration);
            }
            catch (Exception ex)
            {
                throw new ServiceRegistrationException($"Registration of a service failed with the following error: {ex.Message}", ex);
            }
        }
    }
}
