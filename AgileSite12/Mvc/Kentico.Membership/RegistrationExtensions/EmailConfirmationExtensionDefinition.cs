using System;

namespace Kentico.Membership
{
    /// <summary>
    /// Describes configuration for registration email confirmation extension.
    /// </summary>
    public sealed class EmailConfirmationExtensionDefinition
    {
        /// <summary>
        /// This action is invoked before the registration confirmation email is sent.
        /// </summary>
        /// <remarks>Gets ID of registered user as parameter.</remarks>
        public Action<int> BeforeConfirmationSentAction
        {
            get;
        }


        /// <summary>
        /// This action is invoked when the registration email is successfully confirmed.
        /// </summary>
        /// <remarks>Gets ID of registered user as parameter.</remarks>
        public Action<int> EmailConfirmedAction
        {
            get;
        }


        /// <summary>
        /// Initializes a new instance of <see cref="EmailConfirmationExtensionDefinition"/>.
        /// </summary>
        /// <param name="beforeConfirmationSentAction">Action which is invoked before the registration confirmation email is sent</param>
        /// <param name="emailConfirmedAction">Action which is invoked when the registration email is successfully confirmed</param>
        public EmailConfirmationExtensionDefinition(Action<int> beforeConfirmationSentAction, Action<int> emailConfirmedAction)
        {
            BeforeConfirmationSentAction = beforeConfirmationSentAction;
            EmailConfirmedAction = emailConfirmedAction;
        }
    }
}
