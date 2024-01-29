namespace CMS.Core
{
    /// <summary>
    /// Enumeration of possible priorities for implementation registration.
    /// </summary>
    /// <seealso cref="RegisterImplementationAttribute"/>
    public enum RegistrationPriority
    {
        /// <summary>
        /// Indicates that implementation being registered is to replace any implementation registered so far.
        /// </summary>
        Default = 0,


        /// <summary>
        /// Indicates that implementation being registered is to be considered a fallback implementation. Such implementation is used
        /// only when no other implementation is registered later in the application's life-cycle.
        /// </summary>
        /// <remarks>
        /// <para>
        /// A fallback implementation is typically used during early application initialization phase, when regular system implementation
        /// is yet to be registered.
        /// </para>
        /// <para>
        /// Registering more than one fallback implementation is an error and the behavior is undefined.
        /// </para>
        /// </remarks>
        Fallback = 1,


        /// <summary>
        /// Indicates that implementation being registered is to be considered a system default implementation. <see cref="Default"/> implementation
        /// has precedence when registering an implementation.
        /// </summary>
        /// <remarks>
        /// Registering more than one system default implementation is an error and the behavior is undefined.
        /// </remarks>
        SystemDefault = 2
    }
}
