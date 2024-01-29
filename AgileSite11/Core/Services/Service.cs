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
        /// Returns the service interface
        /// </summary>
        /// <exception cref="ServiceResolutionException">
        /// Thrown when resolution of <typeparamref name="ServiceInterface"/> fails. This typically occurs when no implementing type for <typeparamref name="ServiceInterface"/> is registered
        /// or the implementing type has dependencies which cannot be satisfied.
        /// </exception>
        [Obsolete("Use Resolve<TService>() instead.")]
        public static ServiceInterface Entry<ServiceInterface>() 
            where ServiceInterface : class
        {
            return Service<ServiceInterface>.Entry();
        }

        
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
            where TImplementation : TService, new() where TService : class
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
        /// <param name="transient">Indicates whether service implementation lifestyle should be transient, or default to singleton.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="instance"/> is null.</exception>
        /// <exception cref="ServiceRegistrationException">Thrown when registration fails (e.g. <paramref name="name"/> is not unique).</exception>
        /// <remarks>
        /// The <paramref name="name"/> does not need to be explicitly set unless registering one implementation multiple times (for various interfaces).
        /// </remarks>
        public static void Use<TService>(object instance, string name = null, bool transient = false)
        {
            var registration = new Registration(typeof(TService)).Instance(instance).IsDefault();

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
        /// Sets <paramref name="instance"/> to be used as implementation of <paramref name="service"/>.
        /// </summary>
        /// <param name="service">Type of service being implemented.</param>
        /// <param name="instance">Instance of <paramref name="service"/> implementation to be used.</param>
        /// <param name="name">Sets an explicit name of the registration. If not set, the name is automatically inferred from the implementation being registered. Name must be unique.</param>
        /// <param name="transient">Indicates whether service implementation lifestyle should be transient, or default to singleton.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/> or <paramref name="instance"/> is null.</exception>
        /// <exception cref="ServiceRegistrationException">Thrown when registration fails (e.g. <paramref name="name"/> is not unique).</exception>
        /// <remarks>
        /// The <paramref name="name"/> does not need to be explicitly set unless registering one implementation multiple times (for various interfaces).
        /// </remarks>
        public static void Use(Type service, object instance, string name = null, bool transient = false)
        {
            var registration = new Registration(service).Instance(instance).IsDefault();

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


    /// <summary>
    /// Provides service management and resolution functionality for the system.
    /// </summary>
    /// <seealso cref="RegisterImplementationAttribute"/>
    /// <seealso cref="Service"/>
    [Obsolete("Use CMS.Core.Service class instead. See individual obsoleted members' messages to find suitable replacements.")]
    public static class Service<ServiceInterface>
        where ServiceInterface : class
    {
        static Service()
        {
            TypeManager.RegisterGenericType(typeof (Service<ServiceInterface>));
        }


        /// <summary>
        /// Attempts to execute the given lambda expression over the service instance. If service instance is not available, does not execute and returns default value of ReturnType
        /// </summary>
        [Obsolete("This functionality is no longer supported by Service<ServiceInterface> class. Resolve the service by an explicit call to CMS.Core.Service.Resolve<TService>() and execute the lambda over the service instance. Use CMS.Core.Service.IsRegistered(Type) to test whether service is registered.", true)]
        public static ResultType TryExecute<ResultType>(Func<ServiceInterface, ResultType> lambda)
        {
            var s = Service.Resolve<ServiceInterface>();
            if (s == null)
            {
                return default(ResultType);
            }

            return lambda(s);
        }


        /// <summary>
        /// Checks whether the service is available
        /// </summary>
        [Obsolete("Use CMS.Core.Service.IsRegistered(Type) instead.")]
        public static bool IsAvailable
        {
            get
            {
                return Service.IsRegistered(typeof(ServiceInterface));
            }
        }


        /// <summary>
        /// Returns the service implementation as registered within the system.
        /// </summary>
        /// <exception cref="ServiceResolutionException">
        /// Thrown when resolution of <typeparamref name="ServiceInterface"/> fails. This typically occurs when no implementing type for <typeparamref name="ServiceInterface"/> is registered
        /// or the implementing type has dependencies which cannot be satisfied.
        /// </exception>
        [Obsolete("Use CMS.Core.Service.Resolve<TService>() instead.")]
        public static ServiceInterface Entry()
        {
            return Service.Resolve<ServiceInterface>();
        }


        /// <summary>
        /// Returns service interface as a static singleton for the given object type
        /// </summary>
        [Obsolete("This functionality is no longer supported by Service class. The CMS.Core.ObjectFactory<ServiceInterface>.StaticSingleton<ObjectType>() can be a suitable replacement if ServiceInterface is actually managed by ObjectFactory. However, system services are no longer managed by ObjectFactory.", true)]
        public static ServiceInterface ForObjectType<ObjectType>()
        {
            return ObjectFactory<ServiceInterface>.StaticSingleton<ObjectType>();
        }


        /// <summary>
        /// Sets <typeparamref name="ServiceType"/> to be used as implementation of <typeparamref name="ServiceInterface"/>.
        /// </summary>
        /// <param name="name">Sets an explicit name of the registration. If not set, the name is automatically inferred from the implementation being registered. Name must be unique.</param>
        /// <param name="transient">Indicates whether service implementation lifestyle should be transient, or default to singleton.</param>
        /// <exception cref="ServiceRegistrationException">Thrown when registration fails (e.g. <paramref name="name"/> is not unique).</exception>
        /// <remarks>
        /// The <paramref name="name"/> does not need to be explicitly set unless registering one implementation multiple times (for various interfaces).
        /// </remarks>
        [Obsolete("Use CMS.Core.Service.Use<TService, TImplementation>(string, bool) instead.")]
        public static void Use<ServiceType>(string name = null, bool transient = false)
            where ServiceType : class, ServiceInterface, new()
        {
            var registration = new Registration<ServiceInterface>().ImplementedBy<ServiceType>().IsDefault();

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
    }
}
