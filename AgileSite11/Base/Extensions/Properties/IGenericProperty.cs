using System;

namespace CMS.Base
{
    /// <summary>
    /// Interface for the generic extension
    /// </summary>
    public interface IGenericProperty : IGenericExtension
    {
        /// <summary>
        /// Parent property. If set, provides value to current extension
        /// </summary>
        IGenericProperty ParentProperty
        {
            get;
            set;
        }


        /// <summary>
        /// Property name
        /// </summary>
        string Name
        {
            get;
        }


        /// <summary>
        /// Type of the property
        /// </summary>
        Type Type
        {
            get;
        }


        /// <summary>
        /// Registers the extension as a property to the given type
        /// </summary>
        /// <param name="type">Target type</param>
        /// <param name="propertyName">Property name</param>
        void RegisterAsStaticPropertyTo(Type type, string propertyName);


        /// <summary>
        /// Gets the value of the property
        /// </summary>
        object GetValue();
    }
}
