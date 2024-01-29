using System;

namespace CMS
{
    /// <summary>
    /// Marks the class with the contract using the specific interface
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class StaticContractAttribute : ContractAttribute
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="interfaceType">Type of the interface for the contract</param>
        /// <param name="includeAllPublicMembers">If true (default), all public members of the implementation are included into the contract</param>
        public StaticContractAttribute(Type interfaceType, bool includeAllPublicMembers = true)
            : base(interfaceType, includeAllPublicMembers)
        {
        }
    }
}
