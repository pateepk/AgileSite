using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace CMS.ContactManagement
{
    /// <summary>
    /// Contains <see cref="IDeleteContacts"/> implementations that can be used to delete inactive contacts. 
    /// </summary>
    /// <remarks>
    /// For registering to this container use <see cref="RegisterDeleteContactsImplementationAttribute"/>.
    /// </remarks>
    /// <exclude />
    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class DeleteContactsSettingsContainer
    {
        private static readonly List<DeleteContactsSettingsItem> mSettingsItems = new List<DeleteContactsSettingsItem>();

        /// <summary>
        /// Returns all the items that are currently registered as delete contact options.
        /// </summary>
        public static IEnumerable<DeleteContactsSettingsItem> SettingsItems
        {
            get
            {
                return mSettingsItems.AsEnumerable();
            }
        }


        /// <summary>
        /// Return implementation given by name. Implementations should be registered via <see cref="RegisterDeleteContactsImplementationAttribute"/>.
        /// </summary>
        /// <param name="name">Implementation name</param>
        /// <returns>If no implementation found null is returned.</returns>
        public static IDeleteContacts GetImplementation(string name)
        {
            var settingsItem = mSettingsItems.SingleOrDefault(x => x.Name == name);
            if(settingsItem != null)
            {
                return settingsItem.Implementation;
            }
            return null;
        }


        /// <summary>
        /// Registers a settings item which will be displayed in settings for contact deletion. 
        /// All registration should be done through <see cref="RegisterDeleteContactsImplementationAttribute"/>.
        /// </summary>
        /// <param name="item">Item that will be shown in the settings.</param>
        internal static void RegisterImplementation(DeleteContactsSettingsItem item)
        {
            if(item == null)
            {
                throw new ArgumentNullException("item");
            }

            if (string.IsNullOrEmpty(item.Name))
            {
                throw new ArgumentException("[DeleteContactsImplementationContainer.RegisterImplementation]: Name cannot be empty");
            }

            if (item.Implementation == null)
            {
                throw new ArgumentException("[DeleteContactsImplementationContainer.RegisterImplementation]: Implementation cannot be null");
            }

            mSettingsItems.Add(item);
        }
    }
}