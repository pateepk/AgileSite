using System;

namespace CMS.Base
{
    /// <summary>
    /// Extension attribute. Place above particular class to register its extension.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class ExtensionAttribute : Attribute
    {
        /// <summary>
        /// Extension type
        /// </summary>
        public Type ExtensionType 
        {
            get; 
            protected set; 
        }


        /// <summary>
        /// Extends the class with the specified extension
        /// </summary>
        /// <param name="extensionType">Extension type</param>
        public ExtensionAttribute(Type extensionType)
        {
            ExtensionType = extensionType;
        }


        /// <summary>
        /// Registers the extension within the system
        /// </summary>
        /// <param name="type">Object type to which the extension will be registered</param>
        public virtual void RegisterTo(Type type)
        {
            IGenericExtension ext = GetExtensionObject();

            ext.RegisterAsExtensionTo(type);
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
