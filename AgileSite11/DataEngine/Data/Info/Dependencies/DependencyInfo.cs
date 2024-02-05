using System;

namespace CMS.DataEngine
{
    /// <summary>
    /// Wrapper class for a depending object.
    /// </summary>
    internal class DependencyInfo
    {
        /// <summary>
        /// Object type of the dependency.
        /// </summary>
        public string ObjectType
        {
            get;
            set;
        }


        /// <summary>
        /// ID of the dependency.
        /// </summary>
        public int ObjectID
        {
            get;
            set;
        }
        
        
        /// <summary>
        /// Creates new <see cref="DependencyInfo"/>.
        /// </summary>
        /// <param name="objectType">Object type of the dependency</param>
        /// <param name="objectId">ID of the dependency</param>
        public DependencyInfo(string objectType, int objectId)
        {
            ObjectType = objectType;
            ObjectID = objectId;
        }
    }
}