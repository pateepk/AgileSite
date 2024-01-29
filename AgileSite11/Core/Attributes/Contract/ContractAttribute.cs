using System;

namespace CMS
{
    /// <summary>
    /// Marks the class with the contract using the specific interface
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ContractAttribute : Attribute
    {
        /// <summary>
        /// If true (default), all public members of the implementation are included into the contract
        /// </summary>
        public bool IncludeAllPublicMembers
        {
            get;
            protected set;
        }


        /// <summary>
        /// If true (default false), the generated files are generated into the same folder.
        /// </summary>
        public bool IsLocal
        {
            get;
            set;
        }


        /// <summary>
        /// Interface type
        /// </summary>
        protected Type InterfaceType
        {
            get;
            set;
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="interfaceType">Type of the interface for the contract</param>
        /// <param name="includeAllPublicMembers">If true (default), all public members of the implementation are included into the contract</param>
        public ContractAttribute(Type interfaceType, bool includeAllPublicMembers = true)
        {
            InterfaceType = interfaceType;
            IncludeAllPublicMembers = includeAllPublicMembers;
        }
    }
}
