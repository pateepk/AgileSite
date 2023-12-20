namespace Kentico.Membership.Internal
{
    /// <summary>
    /// Membership events.
    /// </summary>
    public static class MembershipEvents
    {
        /// <summary>
        /// Fires when registration of the user is completed.
        /// </summary>
        public static RegistrationCompletedHandler RegistrationCompleted = new RegistrationCompletedHandler { Name = "MembershipEvents.RegistrationCompleted" };
    }
}
