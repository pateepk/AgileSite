using System;

namespace CMS.ContactManagement
{
    /// <summary>
    /// Registers implementation for deleting inactive contacts. All registered implementations will be shown in the settings as an option for an administrator.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public sealed class RegisterDeleteContactsImplementationAttribute : Attribute, IPreInitAttribute
    {
        private readonly string mName;
        private readonly string mResourceString;

        /// <summary>
        /// <see cref="IDeleteContacts"/> that will be used when in the settings.
        /// </summary>
        public Type MarkedType
        {
            get;
            private set;
        }


        /// <summary>
        /// Attribute constructor.
        /// </summary>
        /// <param name="name">Implementation name. Will be used as a value in the settings.</param>
        /// <param name="resourceString">Resource string that will be resolved and shown in the UI in the settings.</param>
        /// <param name="deleteInactiveContactsType"><see cref="IDeleteContacts"/> that will be used when <paramref name="name"/> is chosen in the settings.</param>
        /// <exception cref="ArgumentNullException">If <see paramref="name"/> is null.</exception>
        /// <exception cref="ArgumentException">If <see paramref="deleteInactiveContactsType"/> is null or if does not implement <see cref="IDeleteContacts"/>.</exception>
        public RegisterDeleteContactsImplementationAttribute(string name, string resourceString, Type deleteInactiveContactsType)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            if (deleteInactiveContactsType == null)
            {
                throw new ArgumentNullException("deleteInactiveContactsType");
            }

            if(resourceString == null)
            {
                throw new ArgumentNullException("resourceString");
            }

            if (!typeof(IDeleteContacts).IsAssignableFrom(deleteInactiveContactsType))
            {
                throw new ArgumentException("Provided type does not implement IDeleteInactiveContacts interface.", "deleteInactiveContactsType");
            }

            mResourceString = resourceString;
            mName = name;
            MarkedType = deleteInactiveContactsType;
        }


        /// <summary>
        /// Registers the <see cref="IDeleteContacts"/> factory to the <see cref="DeleteContactsSettingsContainer"/>.
        /// </summary>
        public void PreInit()
        {
            DeleteContactsSettingsContainer.RegisterImplementation(new DeleteContactsSettingsItem()
            {
                DisplayNameResourceString = mResourceString,
                Name = mName,
                Implementation = (IDeleteContacts)Activator.CreateInstance(MarkedType)
            });
        }
    }
}