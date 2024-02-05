using System;

namespace CMS.Core
{
    /// <summary>
    /// Object factory interface
    /// </summary>
    public interface IObjectFactory
    {
        /// <summary>
        /// Returns the type created by this factory
        /// </summary>
        Type CreatedType
        {
            get;
        }


        /// <summary>
        /// Singleton instance of the created object type
        /// </summary>
        object Singleton
        {
            get;
        }


        /// <summary>
        /// Returns true if the factory is able to create the object of the given type
        /// </summary>
        bool CanCreateObject(object parameter);


        /// <summary>
        /// Creates and returns a new object
        /// </summary>
        object CreateNewObject();
    }
}
