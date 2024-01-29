namespace CMS.DataEngine
{
    /// <summary>
    /// Query column defined by the nested query e.g. "(SELECT UserID FROM CMS_User WHERE ...) AS RelatedUserID"
    /// </summary>
    public class NestedSelectQueryColumn : SelectQueryColumnBase<NestedSelectQueryColumn>
    {
        /// <summary>
        /// Returns true if this column represents a single column
        /// </summary>
        public override bool IsSingleColumn
        {
            get
            {
                return true;
            }
        }


        /// <summary>
        /// Constructor
        /// </summary>
        public NestedSelectQueryColumn()
            : base(null)
        {
        }

        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="nestedQuery">Nested query</param>
        public NestedSelectQueryColumn(IDataQuery nestedQuery)
            : base(null)
        {
            var nestedParameters = nestedQuery.GetCompleteQueryParameters();
            var nestedQueryText = nestedParameters.GetFullQueryText(EnsureParameters());

            Expression = nestedQueryText;
        }
    }
}
