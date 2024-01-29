using System;
using System.Linq;
using System.Text;


namespace CMS.DataEngine.Internal
{
    /// <summary>
    /// Specifies dependency of an object type on a child object type
    /// to allow child count re-computation when selecting the default data.
    /// </summary>
    /// <remarks>
    /// This class is for internal use only and should not be used in custom code.
    /// </remarks>
    public class DefaultDataChildDependency
    {
        /// <summary>
        /// Name of the column which serves as an identifier of master object (parent).
        /// Usually the ID column.
        /// </summary>
        public string IdColumn
        {
            get;
            private set;
        }


        /// <summary>
        /// Name of the column within master object (parent) containing count of child objects.
        /// </summary>
        public string ChildCountColumn
        {
            get;
            private set;
        }


        /// <summary>
        /// Object type of the child objects.
        /// </summary>
        public string ChildObjectType
        {
            get;
            private set;
        }


        /// <summary>
        /// Name of the column which identifies referenced parent object within child objects
        /// (the name of the foreign key column).
        /// </summary>
        public string ParentIdColumn
        {
            get;
            private set;
        }


        /// <summary>
        /// Creates new default data child dependency specification.
        /// </summary>
        /// <param name="idColumn">Name of the column which serves as an identifier of master object (parent).</param>
        /// <param name="childCountColumn">Name of the column within master object (parent) containing count of child objects.</param>
        /// <param name="childObjectType">Object type of the child objects.</param>
        /// <param name="parentIdColumn">Name of the column which identifies referenced parent object within child objects (the name of the foreign key column).</param>
        public DefaultDataChildDependency(string idColumn, string childCountColumn, string childObjectType, string parentIdColumn)
        {
            IdColumn = idColumn;
            ChildCountColumn = childCountColumn;
            ChildObjectType = childObjectType;
            ParentIdColumn = parentIdColumn;
        }
    }
}
