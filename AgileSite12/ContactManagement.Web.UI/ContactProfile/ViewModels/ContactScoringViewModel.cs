namespace CMS.ContactManagement.Web.UI
{
	/// <summary>
	/// View model for contact's scoring in contact details component.
	/// </summary>
	public class ContactScoringViewModel
	{
		/// <summary>
		/// Name of the scoring.
		/// </summary>
		public string Name
		{
			get;
			set;
		}

		/// <summary>
		/// Points the contact has accumulated for the given scoring.
		/// </summary>
		public int Points
		{
			get;
			set;
		}
	}
}