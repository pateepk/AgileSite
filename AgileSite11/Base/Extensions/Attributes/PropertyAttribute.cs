using System;

namespace CMS.Base
{
    /// <summary>
    /// Extension attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class PropertyAttribute : ExtensionAttribute
    {
        /// <summary>
        /// Property name
        /// </summary>
        public string PropertyName
        {
            get;
            protected set;
        }


        /// <summary>
        /// Extends the class with the specified extension
        /// </summary>
        /// <param name="extensionType">Extension type</param>
        /// <param name="propertyName">Property name</param>
        public PropertyAttribute(Type extensionType, string propertyName)
            : base(extensionType)
        {
            PropertyName = propertyName;
        }


        /// <summary>
        /// Registers the extension within the system
        /// </summary>
        /// <param name="type">Object type to which the extension will be registered</param>
        public override void RegisterTo(Type type)
        {
            IGenericExtension ext = GetExtensionObject();
            
            ext.RegisterAsPropertyTo(type, PropertyName);
        }
    }
}
