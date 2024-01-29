using CMS.Base;

namespace CMS.ContactManagement
{
    /// <summary>
    /// Injects data from other objects to <see cref="ContactInfo"/> object.
    /// </summary>
    public interface IContactDataInjector
    {
        /// <summary>
        /// Injects provided <paramref name="data"/> to a <see cref="ContactInfo"/> identified by <paramref name="contactId"/>.
        /// </summary>
        /// <param name="data">Data to update contact with.</param>
        /// <param name="contactId">Contact ID.</param>
        /// <param name="mapper">Mapper to map values from data to contact.</param>
        /// <param name="checker">Optionally checks whether the injection is allowed. If not provided, data is updated.</param>
        void Inject(ISimpleDataContainer data, int contactId, IContactDataMapper mapper, IContactDataPropagationChecker checker = null);
    }
}
