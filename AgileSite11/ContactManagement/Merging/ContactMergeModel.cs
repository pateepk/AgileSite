namespace CMS.ContactManagement
{
    /// <summary>
    /// Represents pair of contacts that are being merged.
    /// </summary>
    public class ContactMergeModel
    {
        /// <summary>
        /// Gets or sets the <see cref="ContactInfo"/> that is being merged to <see cref="TargetContact"/>.
        /// </summary>
        public ContactInfo SourceContact
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the <see cref="ContactInfo"/> the <see cref="SourceContact"/> is being merged to.
        /// </summary>
        public ContactInfo TargetContact
        {
            get;
            set;
        }
    }
}