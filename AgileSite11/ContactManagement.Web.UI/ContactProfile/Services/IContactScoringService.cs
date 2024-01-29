using System.Collections.Generic;

using CMS;
using CMS.ContactManagement.Web.UI;

[assembly: RegisterImplementation(typeof(IContactScoringService), typeof(ContactScoringService), Priority = CMS.Core.RegistrationPriority.Fallback)]

namespace CMS.ContactManagement.Web.UI
{
	/// <summary>
	/// Provides manipulation of contact's scoring view models.
	/// </summary>
	internal interface IContactScoringService
	{
		/// <summary>
		/// Retrieves instances of <see cref="ContactScoringViewModel"/> for a contact.
		/// </summary>
		/// <param name="contactId">ID of a contact for which the view models are retrieved.</param>
		/// <returns>Scoring view models for the given contact.</returns>
		IEnumerable<ContactScoringViewModel> GetScoringViewModel(int contactId);
	}
}