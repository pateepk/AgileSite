namespace CMS.DataEngine
{
    /// <summary>
    /// Type condition related data. Every item is one condition expression.
    /// </summary>
    internal class TypeConditionItem
    {
        /// <summary>
        /// Gets or sets the name of the column the value of which can distinguish between the object types.
        /// </summary>      
        public string Column
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the value of the condition column which determines specified type.
        /// </summary>
        public object Value
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the default value of the condition column that would be set into info object in case of type condition application.
        /// </summary>
        public object DefaultColumnValue
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the operator used to compare column value.
        /// </summary>
        public QueryOperator Operator
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets if the value of the condition column is allowing null.
        /// </summary>
        public bool AllowNull
        {
            get;
            set;
        }
    }
}