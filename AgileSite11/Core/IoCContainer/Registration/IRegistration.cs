using System;

namespace CMS.Core
{
    /// <summary>
    /// Represents a registration within an <see cref="IoCContainer"/>.
    /// </summary>
    internal interface IRegistration
    {
        /// <summary>
        /// Registers this registration within <paramref name="container"/>.
        /// </summary>
        /// <param name="container">Container to perform registration on.</param>
        /// <returns>The container instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="container"/> is null.</exception>
        /// <exception cref="ServiceRegistrationException">Thrown when registration fails.</exception>
        IoCContainer Register(IoCContainer container);
    }
}
