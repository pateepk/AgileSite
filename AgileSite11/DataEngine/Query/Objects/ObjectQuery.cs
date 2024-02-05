namespace CMS.DataEngine
{
    /// <summary>
    /// Queries particular database data or defines parameters for data selection
    /// </summary>
    public class ObjectQuery<TObject> : ObjectQueryBase<ObjectQuery<TObject>, TObject>
        where TObject : BaseInfo
    {
        #region "Constructors"
        
        /// <summary>
        /// Constructor
        /// </summary>
        public ObjectQuery()
            : this(null)
        {
        }
        

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="objectType">Object type</param>
        /// <param name="useObjectTypeCondition">If true, the query uses the object type condition. The condition applies only when object type is specified.</param>
        public ObjectQuery(string objectType, bool useObjectTypeCondition = true)
            : base(null)
        {
            if (!string.IsNullOrEmpty(objectType))
            {
                // Use specific object type
                ObjectType = objectType;
                UseObjectTypeCondition = useObjectTypeCondition;
            }
            else
            {
                InitFromType<TObject>();
            }
        }

        #endregion
        

        #region "Operators"

        /// <summary>
        /// Operator for conversion from typed ObjectQuery class to typed InfoDataSet
        /// </summary>
        /// <param name="query">Query object</param>
        public static explicit operator InfoDataSet<TObject>(ObjectQuery<TObject> query)
        {
            if (query == null)
            {
                return null;
            }

            return query.TypedResult;
        }

        #endregion
    }


    /// <summary>
    /// Predefined query returning given object type. Uses the .selectall query internally.
    /// </summary>
    public class ObjectQuery : ObjectQueryBase<ObjectQuery, BaseInfo>
    {
        #region "Constructors"

        /// <summary>
        /// Constructor
        /// </summary>
        public ObjectQuery()
            : base(null)
        {
        }
        

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="objectType">Object type</param>
        /// <param name="useObjectTypeCondition">If true, the query uses the object type condition. The condition applies only when object type is specified.</param>
        public ObjectQuery(string objectType, bool useObjectTypeCondition = true)
            : base(objectType)
        {
            UseObjectTypeCondition = useObjectTypeCondition;
        }

        #endregion
    }
}
