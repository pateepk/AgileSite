using CMS.Core;

namespace CMS.Membership
{
    /// <summary>
    /// Represents the Membership module metadata.
    /// </summary>
    public class MembershipModuleMetadata : ModuleMetadata
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public MembershipModuleMetadata()
            : base(ModuleName.MEMBERSHIP)
        {
        }
    }
}