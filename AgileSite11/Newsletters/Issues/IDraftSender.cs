using System;

using CMS;
using CMS.Newsletters;

[assembly: RegisterImplementation(typeof(IDraftSender), typeof(DraftSender), Priority = CMS.Core.RegistrationPriority.Fallback)]

namespace CMS.Newsletters
{
    /// <summary>
    /// Interface for sending drafts of newsletter issues (<see cref="IssueInfo"/>).
    /// </summary>
    public interface IDraftSender
    {
        /// <summary>
        /// Sends the <paramref name="issue"/> as draft to given e-mail addresses (<paramref name="recipients"/>).
        /// </summary>
        /// <param name="issue">Issue to be sent as draft.</param>
        /// <param name="recipients">Recipients delimited by semicolon.</param>
        /// <exception cref="ArgumentNullException"><paramref name="issue"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="recipients"/> is null or empty.</exception>
        /// <exception cref="InvalidOperationException">Thrown when newsletter the <paramref name="issue"/> is assigned to was not found.</exception>
        void Send(IssueInfo issue, string recipients);


        /// <summary>
        /// Asynchronously sends the <paramref name="issue"/> as draft to given e-mail addresses (<paramref name="recipients"/>).
        /// </summary>
        /// <param name="issue">Issue to be sent as draft.</param>
        /// <param name="recipients">Recipients delimited by semicolon.</param>
        /// <exception cref="ArgumentNullException"><paramref name="issue"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="recipients"/> is null or empty.</exception>
        void SendAsync(IssueInfo issue, string recipients);
    }
}