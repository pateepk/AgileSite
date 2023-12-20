using System;

namespace CMS.DataEngine
{
    /// <summary>
    /// Aggregated query column e.g. "AVG(XYZ) AS Average"
    /// </summary>
    public class AggregatedColumn : SelectQueryColumnBase<AggregatedColumn>
    {
        #region "Properties"

        /// <summary>
        /// Aggregation type
        /// </summary>
        public AggregationType AggregationType
        {
            get;
            set;
        }


        /// <summary>
        /// Over clause for the aggregation. 
        /// When null, over clause is not generated at all, otherwise it is generated even when value is an empty string, in such case " OVER ()" is generated
        /// </summary>
        public string Over
        {
            get;
            set;
        }


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

        #endregion


        #region "Methods"

        /// <summary>
        /// Constructor
        /// </summary>
        public AggregatedColumn()
            : base(null)
        {
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="aggregation">Aggregation type</param>
        /// <param name="expression">Column name expression</param>
        public AggregatedColumn(AggregationType aggregation, string expression)
            : base(expression)
        {
            AggregationType = aggregation;
        }
        

        /// <summary>
        /// Gets the column expression
        /// </summary>
        protected internal override string GetValueExpression()
        {
            var result = base.GetValueExpression();

            // Apply the aggregation
            result = ApplyAggregation(result);
            result = ApplyOver(result);
                
            return result;
        }


        /// <summary>
        /// Applies the over clause to the given expression
        /// </summary>
        /// <param name="expression">Column expression</param>
        protected virtual string ApplyOver(string expression)
        {
            return SqlHelper.ApplyOver(expression, Over);
        }


        /// <summary>
        /// Applies the aggregation to the given expression
        /// </summary>
        /// <param name="expression">Column expression</param>
        protected virtual string ApplyAggregation(string expression)
        {
            return SqlHelper.GetAggregation(expression, AggregationType);
        }

        #endregion
    }
}
