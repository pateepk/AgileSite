using System.ComponentModel;

namespace CMS.ContactManagement
{
    /// <summary>
    /// Representation of one setting in Online Marketing settings for inactive contacts' deletion.
    /// For registration of new items, a <see cref="RegisterDeleteContactsImplementationAttribute"/> should be used.
    /// </summary>
    /// <exclude />
    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class DeleteContactsSettingsItem
    {
        /// <summary>
        /// The resource string that will be resolved as a display name in the settings.
        /// </summary>
        public string DisplayNameResourceString
        {
            get;
            set;
        }


        /// <summary>
        /// The name that will be used as a value in the radio list in the settings.
        /// </summary>
        public string Name
        {
            get;
            set;
        }


        /// <summary>
        /// Instance of implementation of contact deletion.
        /// </summary>
        public IDeleteContacts Implementation
        {
            get;
            set;
        }
    }
}
