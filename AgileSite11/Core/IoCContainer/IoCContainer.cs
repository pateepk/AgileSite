using System;

using Castle.Windsor;

namespace CMS.Core
{
    /// <summary>
    /// Provides inversion of control (IoC) functionality for the system.
    /// </summary>
    internal sealed class IoCContainer : IDisposable
    {
        private readonly WindsorContainer container;
        private bool disposed;


        /// <summary>
        /// Gets the underlying IoC container instance.
        /// </summary>
        internal WindsorContainer Container
        {
            get
            {
                return container;
            }
        }


        /// <summary>
        /// Initializes an inversion of control container.
        /// </summary>
        public IoCContainer()
        {
            try
            {
                container = new WindsorContainer();
                container.Kernel.ReleasePolicy = new ObjectFactoryLikeReleasePolicy();
            }
            catch(Exception)
            {
                Dispose();

                throw;
            }
        }


        /// <summary>
        /// Registers <paramref name="registration"/> within this container.
        /// </summary>
        /// <param name="registration">Registration to register.</param>
        /// <returns>This instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="registration"/> is null.</exception>
        /// <exception cref="ServiceRegistrationException">Thrown when registration fails.</exception>
        public IoCContainer Register(IRegistration registration)
        {
            if (registration == null)
            {
                throw new ArgumentNullException(nameof(registration));
            }

            return registration.Register(this);
        }


        /// <summary>
        /// Indicates whether <paramref name="service"/> has an implementation registered.
        /// </summary>
        /// <param name="service">Service type in question.</param>
        /// <returns>True when <paramref name="service"/> is registered, otherwise false.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/> is null.</exception>
        public bool IsRegistered(Type service)
        {
            if (service == null)
            {
                throw new ArgumentNullException(nameof(service));
            }

            return container.Kernel.HasComponent(service);
        }


        /// <summary>
        /// Returns an instance of <typeparamref name="TService"/> as registered within container.
        /// </summary>
        /// <typeparam name="TService">Service type to be resolved.</typeparam>
        /// <returns>Returns an instance of <typeparamref name="TService"/>.</returns>
        /// <exception cref="ServiceResolutionException">
        /// Thrown when resolution of <typeparamref name="TService"/> fails. This typically occurs when no implementing type for <typeparamref name="TService"/> is registered
        /// or the implementing type has dependencies which cannot be satisfied.
        /// </exception>
        public TService Resolve<TService>()
        {
            try
            {
                return container.Resolve<TService>();
            }
            catch(Exception ex)
            {
                throw new ServiceResolutionException($"Resolution of '{typeof(TService).FullName}' failed with the following error: {ex.Message}", ex);
            }
        }


        /// <summary>
        /// Returns an instance of <paramref name="service"/> as registered within container.
        /// </summary>
        /// <param name="service">Service type to be resolved.</param>
        /// <returns>Returns an instance of <paramref name="service"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/> is null.</exception>
        /// <exception cref="ServiceResolutionException">
        /// Thrown when resolution of <paramref name="service"/> fails. This typically occurs when no implementing type for <paramref name="service"/> is registered
        /// or the implementing type has dependencies which cannot be satisfied.
        /// </exception>
        public object Resolve(Type service)
        {
            if (service == null)
            {
                throw new ArgumentNullException(nameof(service));
            }

            try
            {
                return container.Resolve(service);
            }
            catch (Exception ex)
            {
                throw new ServiceResolutionException($"Resolution of '{service.FullName}' failed with the following error: {ex.Message}", ex);
            }
        }


        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="IoCContainer"/> class.
        /// </summary>
        public void Dispose()
        {
            if (disposed)
            {
                return;
            }

            container?.Dispose();

            disposed = true;
        }
    }
}
