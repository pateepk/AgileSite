using System;
using System.Linq;
using System.Text;

using CMS.Base;

namespace CMS
{
    /// <summary>
    /// Marks the class as extension of the given class
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public class RegisterExtensionAttribute : Attribute, IPreInitAttribute
    {
        /// <summary>
        /// Gets the extension type
        /// </summary>
        public Type ExtensionType
        {
            get;
            protected set;
        }


        /// <summary>
        /// Gets the extended type
        /// </summary>
        public Type MarkedType
        {
            get;
            protected set;
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="extensionType">Extension type</param>
        /// <param name="extendedType">Extended type</param>
        public RegisterExtensionAttribute(Type extensionType, Type extendedType)
        {
            ExtensionType = extensionType;
            MarkedType = extendedType;
        }


        /// <summary>
        /// Initializes the attribute
        /// </summary>
        public void PreInit()
        {
            IGenericExtension ext = GetExtensionObject();

            ext.RegisterAsExtensionTo(MarkedType);
        }


        /// <summary>
        /// Creates an extension object based on extension type
        /// </summary>
        protected IGenericExtension GetExtensionObject()
        {
            return Extension.CreateFromType(ExtensionType);
        }
    }
}
