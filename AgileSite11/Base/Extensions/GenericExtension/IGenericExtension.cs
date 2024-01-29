using System;

namespace CMS.Base
{
    /// <summary>
    /// Interface for the generic extension
    /// </summary>
    public interface IGenericExtension
    {
        /// <summary>
        /// Parent extension. If set, provides instance object to current extension
        /// </summary>
        IGenericExtension ParentExtension
        {
            get;
            set;
        }


        /// <summary>
        /// Returns the extension object instance
        /// </summary>
        object GetInstance();


        /// <summary>
        /// Sets the extension object instance
        /// </summary>
        /// <param name="value">New instance value</param>
        void SetInstance(object value);
            

        /// <summary>
        /// Registers the extension as a property to the given type
        /// </summary>
        /// <param name="type">Target type</param>
        /// <param name="propertyName">Property name</param>
        void RegisterAsPropertyTo(Type type, string propertyName);


        /// <summary>
        /// Registers the extension as an extension to the given type
        /// </summary>
        /// <param name="type">Target type</param>
        void RegisterAsExtensionTo(Type type);


        /// <summary>
        /// Creates a new property inferred from this extension
        /// </summary>
        IGenericProperty NewGenericProperty(object obj);
    }
}
