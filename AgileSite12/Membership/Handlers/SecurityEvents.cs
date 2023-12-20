namespace CMS.Membership
{
    /// <summary>
    /// Security events
    /// </summary>
    public static class SecurityEvents
    {
        /// <summary>
        /// Fires when sign out of the user is performed
        /// </summary>
        public static SignOutHandler SignOut = new SignOutHandler { Name = "SecurityEvents.SignOut" };


        /// <summary>
        /// Fires when current request is redirected to the secured area.
        /// </summary>
        /// <seealso cref="AuthenticationHelper.IsAuthenticationRedirect"/>
        public static AuthenticationRequestHandler AuthenticationRequested = new AuthenticationRequestHandler { Name = "SecurityEvents.AuthenticationRequested" };


        /// <summary>
        /// Fires when authentication of the user is performed, allows to connect the authentication process to the external authentication provider
        /// </summary>
        public static AuthenticationHandler Authenticate = new AuthenticationHandler { Name = "SecurityEvents.Authenticate" };


        /// <summary>
        /// Fires when multi-factor authentication of the user is performed, allows to customize way of sending passcode
        /// </summary>
        public static AuthenticationHandler MultiFactorAuthenticate = new AuthenticationHandler { Name = "SecurityEvents.MFAuthenticate" };


        /// <summary>
        /// Fires when resource permission authorization is requested
        /// </summary>
        public static AuthorizationHandler AuthorizeResource = new AuthorizationHandler { Name = "SecurityEvents.AuthorizeResource" };


        /// <summary>
        /// Fires when class permission authorization is requested
        /// </summary>
        public static AuthorizationHandler AuthorizeClass = new AuthorizationHandler { Name = "SecurityEvents.AuthorizeClass" };


        /// <summary>
        /// Fires when UI element authorization is requested
        /// </summary>
        public static AuthorizationHandler AuthorizeUIElement = new AuthorizationHandler { Name = "SecurityEvents.AuthorizeUIElement" };
    }
}