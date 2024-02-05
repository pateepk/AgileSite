using System;
using System.Runtime.Serialization;

using CMS.Base;

namespace CMS.DataEngine
{
    /// <summary>
    /// Interface for the info objects
    /// </summary>
    public interface IInfo : IAdvancedDataContainer, ISerializable, ICMSObject, IComparable, IRelatedData, IMacroObject
    {
        /// <summary>
        /// Object settings
        /// </summary>
        ObjectSettingsInfo ObjectSettings
        {
            get;
        }


        /// <summary>
        /// Object type info
        /// </summary>
        ObjectTypeInfo TypeInfo
        {
            get;
        }


        /// <summary>
        /// Generalized interface of this object.
        /// </summary>
        GeneralizedInfo Generalized
        {
            get;
        }


        /// <summary>
        /// If true, the object allows partial updates.
        /// </summary>
        bool AllowPartialUpdate
        {
            get;
            set;
        }


        /// <summary>
        /// Submits the changes in the object to the database.
        /// </summary>
        /// <param name="withCollections">If true, also submits the changes in the underlying collections of the object (Children, ChildDependencies, Bindings, OtherBindings)</param>
        void SubmitChanges(bool withCollections);


        /// <summary>
        /// Updates the database entity using appropriate provider
        /// </summary>
        void Update();


        /// <summary>
        /// Deletes the object using appropriate provider
        /// </summary>
        bool Delete();


        /// <summary>
        /// Destroys the object including its version history using appropriate provider
        /// </summary>
        bool Destroy();


        /// <summary>
        /// Inserts the object using appropriate provider
        /// </summary>
        void Insert();


        /// <summary>
        /// Executes the given action using original data of the object
        /// </summary>
        /// <param name="action">Action to execute</param>
        void ExecuteWithOriginalData(Action action);


        /// <summary>
        /// Creates a clone of the object
        /// </summary>
        /// <param name="clear">If true, the object is cleared to be able to create new object</param>
        BaseInfo CloneObject(bool clear);
    }
}
