using CMS.Core;

namespace CMS.ContactManagement
{
    /// <summary>
    /// Represents the Contact Management module metadata.
    /// </summary>
    internal class ContactManagementModuleMetadata : ModuleMetadata
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public ContactManagementModuleMetadata()
            : base(ModuleName.CONTACTMANAGEMENT)
        {
            RootPath = "~/CMSModules/ContactManagement/";
        }
    }
}