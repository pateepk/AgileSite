using System;

namespace CMS.Core
{
    /// <summary>
    /// Provides service management and resolution functionality for the system.
    /// </summary>
    /// <seealso cref="RegisterImplementationAttribute"/>
    public static class Service
    {
        /// <summary>
        /// Sets <typeparamref name="TImplementation"/> to be used as implementation of <typeparamref name="TService"/>.
        /// </summary>
        /// <typeparam name="TService">Service being implemented.</typeparam>
        /// <typeparam name="TImplementation">Implementing type for <typeparamref name="TService"/>.</typeparam>
        /// <param name="name">Sets an explicit name of the registration. If not set, the name is automatically inferred from the implementation being registered. Name must be unique.</param>
        /// <param name="transient">Indicates whether service implementation lifestyle should be transient, or default to singleton.</param>
        /// <exception cref="ServiceRegistrationException">Thrown when registration fails (e.g. <paramref name="name"/> is not unique).</exception>
        /// <remarks>
        /// The <paramref name="name"/> does not need to be explicitly set unless registering one implementation multiple times (for various interfaces).
        /// </remarks>
        public static void Use<TService, TImplementation>(string name = null, bool transient = false)
            where TImplementation : TService where TService : class
        {
            var registration = new Registration<TService>().ImplementedBy<TImplementation>().IsDefault();

            if (!String.IsNullOrEmpty(name))
            {
                registration.Named(name);
            }

            if (transient)
            {
                registration.LifestyleTransient();
            }
            else
            {
                registration.LifestyleSingleton();
            }

            TypeManager.IoCContainer.Register(registration);
        }


        /// <summary>
        /// Sets <paramref name="implementation"/> to be used as implementation of <paramref name="service"/>.
        /// </summary>
        /// <param name="service">Type of service being implemented.</param>
        /// <param name="implementation">Implementing type for <paramref name="service"/>.</param>
        /// <param name="name">Sets an explicit name of the registration. If not set, the name is automatically inferred from the implementation being registered. Name must be unique.</param>
        /// <param name="transient">Indicates whether service implementation lifestyle should be transient, or default to singleton.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/> or <paramref name="implementation"/> is null.</exception>
        /// <exception cref="ServiceRegistrationException">Thrown when registration fails (e.g. <paramref name="name"/> is not unique).</exception>
        /// <remarks>
        /// The <paramref name="name"/> does not need to be explicitly set unless registering one implementation multiple times (for various interfaces).
        /// </remarks>
        public static void Use(Type service, Type implementation, string name = null, bool transient = false)
        {
            var registration = new Registration(service).ImplementedBy(implementation).IsDefault();

            if (!String.IsNullOrEmpty(name))
            {
                registration.Named(name);
            }

            if (transient)
            {
                registration.LifestyleTransient();
            }
            else
            {
                registration.LifestyleSingleton();
            }

            TypeManager.IoCContainer.Register(registration);
        }


        /// <summary>
        /// Sets <paramref name="instance"/> to be used as implementation of <typeparamref name="TService"/>.
        /// </summary>
        /// <typeparam name="TService">Service being implemented.</typeparam>
        /// <param name="instance">Instance of <typeparamref name="TService"/> implementation to be used.</param>
        /// <param name="name">Sets an explicit name of the registration. If not set, the name is automatically inferred from the implementation being registered. Name must be unique.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="instance"/> is null.</exception>
        /// <exception cref="ServiceRegistrationException">Thrown when registration fails (e.g. <paramref name="name"/> is not unique).</exception>
        /// <remarks>
        /// The <paramref name="name"/> does not need to be explicitly set unless registering one implementation multiple times (for various interfaces).
        /// </remarks>
        public static void Use<TService>(object instance, string name = null)
        {
            var registration = new Registration(typeof(TService)).Instance(instance).IsDefault();

            if (!String.IsNullOrEmpty(name))
            {
                registration.Named(name);
            }

            TypeManager.IoCContainer.Register(registration);
        }


        /// <summary>
        /// Sets <paramref name="instance"/> to be used as implementation of <paramref name="service"/>.
        /// </summary>
        /// <param name="service">Type of service being implemented.</param>
        /// <param name="instance">Instance of <paramref name="service"/> implementation to be used.</param>
        /// <param name="name">Sets an explicit name of the registration. If not set, the name is automatically inferred from the implementation being registered. Name must be unique.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/> or <paramref name="instance"/> is null.</exception>
        /// <exception cref="ServiceRegistrationException">Thrown when registration fails (e.g. <paramref name="name"/> is not unique).</exception>
        /// <remarks>
        /// The <paramref name="name"/> does not need to be explicitly set unless registering one implementation multiple times (for various interfaces).
        /// </remarks>
        public static void Use(Type service, object instance, string name = null)
        {
            var registration = new Registration(service).Instance(instance).IsDefault();

            if (!String.IsNullOrEmpty(name))
            {
                registration.Named(name);
            }

            TypeManager.IoCContainer.Register(registration);
        }


        /// <summary>
        /// Sets <paramref name="factoryMethod"/> to be used as a provider of implementation of <typeparamref name="TService"/>.
        /// </summary>
        /// <typeparam name="TService">Service being implemented.</typeparam>
        /// <param name="factoryMethod">Method providing an instance of <typeparamref name="TService"/>.</param>
        /// <param name="name">Sets an explicit name of the registration. If not set, the name is automatically inferred from the implementation being registered. Name must be unique.</param>
        /// <param name="transient">Indicates whether service implementation lifestyle should be transient, or default to singleton.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="factoryMethod"/> is null.</exception>
        /// <exception cref="ServiceRegistrationException">Thrown when registration fails (e.g. <paramref name="name"/> is not unique).</exception>
        /// <remarks>
        /// The <paramref name="name"/> does not need to be explicitly set unless registering one implementation multiple times (for various interfaces).
        /// </remarks>
        public static void Use<TService>(Func<object> factoryMethod, string name = null, bool transient = false)
        {
            var registration = new Registration(typeof(TService)).FactoryMethod(factoryMethod).IsDefault();

            if (!String.IsNullOrEmpty(name))
            {
                registration.Named(name);
            }

            if (transient)
            {
                registration.LifestyleTransient();
            }
            else
            {
                registration.LifestyleSingleton();
            }

            TypeManager.IoCContainer.Register(registration);
        }


        /// <summary>
        /// Sets <paramref name="factoryMethod"/> to be used as a provider of implementation of <paramref name="service"/>.
        /// </summary>
        /// <param name="service">Type of service being implemented.</param>
        /// <param name="factoryMethod">Method providing an instance of <paramref name="service"/>.</param>
        /// <param name="name">Sets an explicit name of the registration. If not set, the name is automatically inferred from the implementation being registered. Name must be unique.</param>
        /// <param name="transient">Indicates whether service implementation lifestyle should be transient, or default to singleton.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/> or <paramref name="factoryMethod"/> is null.</exception>
        /// <exception cref="ServiceRegistrationException">Thrown when registration fails (e.g. <paramref name="name"/> is not unique).</exception>
        /// <remarks>
        /// The <paramref name="name"/> does not need to be explicitly set unless registering one implementation multiple times (for various interfaces).
        /// </remarks>
        public static void Use(Type service, Func<object> factoryMethod, string name = null, bool transient = false)
        {
            var registration = new Registration(service).FactoryMethod(factoryMethod).IsDefault();

            if (!String.IsNullOrEmpty(name))
            {
                registration.Named(name);
            }

            if (transient)
            {
                registration.LifestyleTransient();
            }
            else
            {
                registration.LifestyleSingleton();
            }

            TypeManager.IoCContainer.Register(registration);
        }


        /// <summary>
        /// Indicates whether <paramref name="service"/> has an implementation registered.
        /// </summary>
        /// <param name="service">Service type in question.</param>
        /// <returns>True when <paramref name="service"/> is registered, otherwise false.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/> is null.</exception>
        public static bool IsRegistered(Type service)
        {
            return TypeManager.IoCContainer.IsRegistered(service);
        }


        /// <summary>
        /// Returns an instance of <typeparamref name="TService"/> as registered within the system.
        /// </summary>
        /// <typeparam name="TService">Service type to be resolved.</typeparam>
        /// <returns>Returns an instance of <typeparamref name="TService"/>.</returns>
        /// <exception cref="ServiceResolutionException">
        /// Thrown when resolution of <typeparamref name="TService"/> fails. This typically occurs when no implementing type for <typeparamref name="TService"/> is registered
        /// or the implementing type has dependencies which cannot be satisfied.
        /// </exception>
        public static TService Resolve<TService>()
        {
            return TypeManager.IoCContainer.Resolve<TService>();
        }


        /// <summary>
        /// Returns an instance of <paramref name="service"/> as registered within the system.
        /// </summary>
        /// <param name="service">Service type to be resolved.</param>
        /// <returns>Returns an instance of <paramref name="service"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/> is null.</exception>
        /// <exception cref="ServiceResolutionException">
        /// Thrown when resolution of <paramref name="service"/> fails. This typically occurs when no implementing type for <paramref name="service"/> is registered
        /// or the implementing type has dependencies which cannot be satisfied.
        /// </exception>
        public static object Resolve(Type service)
        {
            return TypeManager.IoCContainer.Resolve(service);
        }
    }
}