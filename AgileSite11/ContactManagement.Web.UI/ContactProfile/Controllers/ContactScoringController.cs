using System.Collections.Generic;

using CMS.ContactManagement.Web.UI.Internal;
using CMS.Core;
using CMS.WebApi;

[assembly: RegisterCMSApiController(typeof(ContactScoringController))]

namespace CMS.ContactManagement.Web.UI.Internal
{
	/// <summary>
	/// Provides endpoint for retrieving the data required for the contact's scorings.
	/// </summary>
	[AllowOnlyEditor]
	public sealed class ContactScoringController : CMSApiController
    {
		private readonly IContactScoringService mContactScoringService;

		/// <summary>
		/// Instantiates new instance of <see cref="ContactScoringController"/>.
		/// </summary>
		public ContactScoringController()
			: this(Service.Resolve<IContactScoringService>())
		{

		}


		internal ContactScoringController(IContactScoringService contactScoringService)
		{
			mContactScoringService = contactScoringService;
		}


		/// <summary>
		/// Retrieves instances of <see cref="ContactScoringViewModel"/> for the given <paramref name="contactId"/>.
		/// </summary>
		/// <param name="contactId">ID of a contact for which the view models are retrieved.</param>
		/// <returns>Scoring view models for the given contact.</returns>
		public IEnumerable<ContactScoringViewModel> Get(int contactId)
		{
			return mContactScoringService.GetScoringViewModel(contactId);
		}
	}
}