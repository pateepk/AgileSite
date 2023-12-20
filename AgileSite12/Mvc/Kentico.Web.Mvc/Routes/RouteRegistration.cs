using System;
using System.Collections.Generic;
using System.Threading;
using System.Web.Routing;

namespace Kentico.Web.Mvc.Internal
{
    /// <summary>
    /// Performs registration of system routes to route collection.
    /// </summary>
    public class RouteRegistration
    {
        private readonly Queue<Action<RouteCollection>> routeRegistrations = new Queue<Action<RouteCollection>>();
        private bool routeRegistrationExecuted;

        private static RouteRegistration instance;
        private readonly object routeRegistrationLock = new object();


        /// <summary>
        /// Initializes a new instance of the <see cref="RouteRegistration"/> class.
        /// </summary>
        internal RouteRegistration()
        {
        }


        /// <summary>
        /// Gets the current instance of the <see cref="RouteRegistration"/> class.
        /// </summary>
        public static RouteRegistration Instance
        {
            get
            {
                if (instance == null)
                {
                    Interlocked.CompareExchange(ref instance, new RouteRegistration(), null);
                }
                return instance;
            }
            internal set
            {
                instance = value;
            }
        }


        /// <summary>
        /// <para>
        /// Adds route registration to be executed on route collection supplied in <see cref="Execute"/> call.
        /// </para>
        /// <para>
        /// Route registrations are queued and performed on the route collection passed to the <see cref="Execute"/> method, once the method is called.
        /// After being executed, no additional route registrations can be added (i.e. route registration execution must not precede 'use feature' methods calls so that features
        /// can add their specific routes).
        /// </para>
        /// </summary>
        /// <param name="routeRegistration">Route registration to be executed on the route collection passed to <see cref="Execute"/>, once it is called.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="routeRegistration"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when <see cref="Execute"/> has already been called.</exception>
        public void Add(Action<RouteCollection> routeRegistration)
        {
            if (routeRegistration == null)
            {
                throw new ArgumentNullException(nameof(routeRegistration));
            }

            lock (routeRegistrationLock)
            {
                if (routeRegistrationExecuted)
                {
                    throw new InvalidOperationException($"Additional system routes cannot be added once the system routes have been mapped. Make sure all required features are enabled before calling {typeof(RouteCollectionAddRoutesMethods).FullName}.{nameof(RouteCollectionAddRoutesMethods.MapRoutes)}().");
                }

                routeRegistrations.Enqueue(routeRegistration);
            }
        }


        /// <summary>
        /// <para>
        /// Executes registration of routes collected via the <see cref="Add"/> method into <paramref name="routeCollection"/>.
        /// </para>
        /// <para>
        /// Once the registration executes, no more routes can be added.
        /// </para>
        /// </summary>
        /// <param name="routeCollection">Route collection to register routes into.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="routeCollection"/> is null.</exception>
        public void Execute(RouteCollection routeCollection)
        {
            if (routeCollection == null)
            {
                throw new ArgumentNullException(nameof(routeCollection));
            }

            lock (routeRegistrationLock)
            {
                while(routeRegistrations.Count > 0)
                {
                    var routeRegistration = routeRegistrations.Dequeue();
                    routeRegistration(routeCollection);
                }

                routeRegistrationExecuted = true;
            }
        }
    }
}
