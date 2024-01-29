namespace CMS.DataEngine
{
    /// <summary>
    /// Predefined query returning IDs of the given object type, can be used as a nested query in IN and NOT IN statements
    /// </summary>
    public class IDQuery<ObjectType> : ObjectQuery<ObjectType>
        where ObjectType : BaseInfo, new()
    {
        #region "Constructors"
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="resultColumn">Resulting column</param>
        public IDQuery(string resultColumn = null)
        {
            InitIDQuery(resultColumn);
        }

        #endregion
    }
    

    /// <summary>
    /// Predefined query returning IDs of the given object type, can be used as a nested query in IN and NOT IN statements
    /// </summary>
    public class IDQuery : ObjectQuery
    {
        #region "Methods"
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="objectType">Object type</param>
        /// <param name="resultColumn">Resulting column</param>
        /// <param name="useObjectTypeCondition">If true, the query uses the object type condition. The condition applies only when object type is specified.</param>
        public IDQuery(string objectType, string resultColumn = null, bool useObjectTypeCondition = true)
            : base(objectType)
        {
            UseObjectTypeCondition = useObjectTypeCondition;

            InitIDQuery(resultColumn);
        }

        #endregion
    }
}
