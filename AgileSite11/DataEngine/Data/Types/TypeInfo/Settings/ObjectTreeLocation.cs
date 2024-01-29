namespace CMS.DataEngine
{
    /// <summary>
    /// Defines a location for an object type within the object tree in the export/import or staging interface.
    /// </summary>
    public class ObjectTreeLocation
    {
        #region "Properties"

        /// <summary>
        /// Object type of the actual node.
        /// </summary>
        public string ObjectType
        {
            get;
            set;
        }


        /// <summary>
        /// Parent path in the object type tree.
        /// </summary>
        public string ParentPath
        {
            get;
            set;
        }

        #endregion


        /// <summary>
        /// Constructor. Accepts string parameters, each representing a category (level) in the object tree.
        /// You can use either the default category constants or custom strings.
        /// The tree node containing the objects is added under the final category.
        /// </summary>
        /// <param name="path">Parts of the path</param>
        public ObjectTreeLocation(params string[] path)
        {
            if ((path != null) && (path.Length > 0))
            {
                ParentPath = string.Join("/", path);
            }
        }
    }
}