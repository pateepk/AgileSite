using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using CMS.Base;
using CMS.Core;


namespace CMS.EmailEngine
{
    /// <summary>
    /// Register for email template types.
    /// </summary>
    /// <remarks>
    /// The register is thread-safe and its performance is tailored for read operations.
    /// </remarks>
    public class EmailTemplateTypeRegister
    {
        #region "Properties"

        // Dictionary of template type names for quick lookup
        private readonly SafeDictionary<string, EmailTemplateTypeRegisterItem> mTemplateTypeNames = new SafeDictionary<string, EmailTemplateTypeRegisterItem>(StringComparer.InvariantCultureIgnoreCase);

        // Template type register to be returned
        private List<EmailTemplateTypeRegisterItem> mTemplateTypeRegister = new List<EmailTemplateTypeRegisterItem>();

        // Read-only wrapper for template type register
        private ReadOnlyCollection<EmailTemplateTypeRegisterItem> mTemplateTypeRegisterReadOnly;

        // Synchronization root
        private readonly object mSyncRoot = new object();

        // Current singleton instance
        private static EmailTemplateTypeRegister mCurrent;

        // Default items to ensure compatibility with obsolete EmailTemplateTypeEnum
        private readonly string[] mDefaultItemNames =
        {
            EmailModule.GENERAL_EMAIL_TEMPLATE_TYPE_NAME,
            "blog",
            "blogsubscription",
            "boards",
            "boardssubscription",
            "bookingevent",
            "ecommerce",
            "ecommerceeproductexpiration",
            "forum",
            "forumsubscribtion",
            "groupmember",
            "groupinvitation",
            "groupmemberinvitation",
            "password",
            "registration",
            "registrationapproval",
            "membershipregistration",
            "membershipexpiration",
            "membershipchangepassword",
            "membershippasswordresetconfirmation",
            "membershipunlockaccount",
            "forgottenpassword",
            "newsletter",
            "workflow",
            "scoring",
            "translationservices",
            "reporting"
        };

        // Default global items
        private readonly string[] mDefaultGlobalItemNames =
        {
            "automation"
        };

        private static readonly object lockObject = new object();

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets current instance of email template types register.
        /// </summary>
        public static EmailTemplateTypeRegister Current
        {
            get
            {
                return LockHelper.Ensure(ref mCurrent, () => new EmailTemplateTypeRegister(), lockObject);
            }
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Adds a new template type to the register.
        /// </summary>
        /// <param name="registerItem">Register item to be added.</param>
        /// <exception cref="InvalidOperationException">Thrown when item with name <see cref="EmailTemplateTypeRegisterItem.Name"/> is already present in the register.</exception>
        public void AddTemplateType(EmailTemplateTypeRegisterItem registerItem)
        {
            // Create a new version of the list - perform the memory allocation before locking, the initial capacity might not be accurate
            List<EmailTemplateTypeRegisterItem> newRegister = new List<EmailTemplateTypeRegisterItem>(mTemplateTypeRegister.Count + 1);

            lock (mSyncRoot)
            {
                // Fill the new register, its capacity is sufficient if no more than 1 thread called this method in parallel
                newRegister.AddRange(mTemplateTypeRegister);

                // If item is already present in the list, throw exception
                if (mTemplateTypeNames.Contains(registerItem.Name))
                {
                    throw new InvalidOperationException("Register item with name '" + registerItem.Name + "' is already present.");
                }

                // Add register item to the dictionary first, then make it available via the GetTemplateTypes() method call so when someone enumerates it, the dictionary already knows it
                mTemplateTypeNames.Add(registerItem.Name, registerItem);
                newRegister.Add(registerItem);

                mTemplateTypeRegister = newRegister;

                // Make the new register version accessible atomically
                mTemplateTypeRegisterReadOnly = new ReadOnlyCollection<EmailTemplateTypeRegisterItem>(mTemplateTypeRegister);
            }
        }


        /// <summary>
        /// Gets read-only list containing all registered email template types.
        /// </summary>
        /// <returns>Email template types</returns>
        public IList<EmailTemplateTypeRegisterItem> GetTemplateTypes()
        {
            return mTemplateTypeRegisterReadOnly;
        }


        /// <summary>
        /// Gets registered email template type by its name.
        /// </summary>
        /// <param name="name">Name of the registered type.</param>
        /// <returns>Registered type, or null.</returns>
        public EmailTemplateTypeRegisterItem GetTemplateType(string name)
        {
            return mTemplateTypeNames[name];
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Adds default items to the register. Must be called from thread-safe method.
        /// </summary>
        /// <remarks>
        /// For backward compatibility reasons, the default values contain all the values from the original EmailTemplateTypeEnum.
        /// Modules registering their own template type should do so in their module's <see cref="ModuleEntry.OnInit"/> method.
        /// Since all the default data is imported to the DB at once, regardless of separable modules, the default items contain
        /// all email template types present in the default data.
        /// </remarks>
        private void AddDefaultItems()
        {
            AddItems(mDefaultItemNames, false);
            AddItems(mDefaultGlobalItemNames, true);

            mTemplateTypeRegisterReadOnly = new ReadOnlyCollection<EmailTemplateTypeRegisterItem>(mTemplateTypeRegister);
        }


        /// <summary>
        /// Adds items to the register. Must be called from thread-safe method.
        /// </summary>
        /// <param name="items">Items to be added to the register.</param>
        /// <param name="global">Flag if items to be added are only for global.</param>
        private void AddItems(IEnumerable<string> items, bool global)
        {
            foreach (var name in items)
            {
                var registerItem = new EmailTemplateTypeRegisterItem(name, "emailtemplate.type." + name, global);
                mTemplateTypeNames.Add(name, registerItem);
                mTemplateTypeRegister.Add(registerItem);
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Private constructor for singleton.
        /// </summary>
        private EmailTemplateTypeRegister()
        {
            AddDefaultItems();
        }

        #endregion
    }
}
