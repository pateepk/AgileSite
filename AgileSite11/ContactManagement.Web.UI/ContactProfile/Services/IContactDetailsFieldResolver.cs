using System;

namespace CMS.ContactManagement.Web.UI.Internal
{
    /// <summary>
    /// Provides method for resolving special contact detail fields. It can be used in cases the primitive and reference types are not sufficient for contact detail component.
    /// </summary>
    public interface IContactDetailsFieldResolver
    {
        /// <summary>
        /// Resolves detail field for given <paramref name="contact"/>. 
        /// </summary>
        /// <param name="contact">Contact the detail field is resolved for</param>
        /// <exception cref="ArgumentNullException"><paramref name="contact"/> is <c>null</c></exception>
        /// <returns>Resolved field. This object will be serialized to JSON in contact component</returns>
        object ResolveField(ContactInfo contact);
    }
}