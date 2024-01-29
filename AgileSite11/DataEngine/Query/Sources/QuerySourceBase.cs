using System;

namespace CMS.DataEngine
{
    /// <summary>
    /// Defines base class for the query source
    /// </summary>
    public class QuerySourceBase<TSource> : QueryParametersBase<TSource>, IQuerySource
        where TSource : QuerySourceBase<TSource>, new()
    {
        #region "Variables"

        private string mSourceExpression;

        #endregion


        #region "Properties"

        /// <summary>
        /// Source expression
        /// </summary>
        public string SourceExpression
        {
            get
            {
                return mSourceExpression;
            }
            set
            {
                mSourceExpression = value;

                // Set the right source name
                if (RightSourceName == null)
                {
                    RightSourceName = value;
                }

                Changed();
            }
        }


        /// <summary>
        /// Left source name
        /// </summary>
        protected string LeftSourceName
        {
            get;
            set;
        }


        /// <summary>
        /// Left source name
        /// </summary>
        protected string RightSourceName
        {
            get;
            set;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Default constructor
        /// </summary>
        protected QuerySourceBase()
        {
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="source">Source table</param>
        public QuerySourceBase(QuerySourceTable source)
        {
            // Translate the source properties to accommodate proper syntax
            TranslateSource(source);

            SourceExpression = source.GetFullExpression();
            RightSourceName = source.GetName();
        }


        /// <summary>
        /// Translates the source to the final query expression
        /// </summary>
        /// <param name="source">Source table</param>
        protected virtual void TranslateSource(QuerySourceTable source)
        {
            // No translation by default
        }


        /// <summary>
        /// Gets the join condition for the given columns
        /// </summary>
        /// <param name="leftColumn">Left column</param>
        /// <param name="rightColumn">Right column</param>
        private string GetJoinCondition(string leftColumn, string rightColumn)
        {
            leftColumn = SqlHelper.EnsureFullName(LeftSourceName, leftColumn);
            rightColumn = SqlHelper.EnsureFullName(RightSourceName, rightColumn);

            return String.Format("{0} = {1}", SqlHelper.AddSquareBrackets(leftColumn), SqlHelper.AddSquareBrackets(rightColumn));
        }


        /// <summary>
        /// Gets the source expression represented as string
        /// </summary>
        /// <param name="parameters">Query parameters. If provided, the column parameters are included into the parameters and column expression is altered accordingly</param>
        /// <param name="expand">If true, the result expression is expanded with parameters</param>
        public string GetSourceExpression(QueryDataParameters parameters, bool expand = false)
        {
            var result = SourceExpression;

            if (parameters != null)
            {
                // Include the parameters and transform the expression if necessary
                result = parameters.IncludeDataParameters(Parameters, result);
            }

            // Expand the whole result with parameters
            if (expand && (parameters != null))
            {
                result = parameters.Expand(result);
            }

            return result;
        }

        #endregion


        #region "Setup methods"

        /// <summary>
        /// Joins the given source with another
        /// </summary>
        /// <param name="source">Source table</param>
        /// <param name="condition">Join condition</param>
        /// <param name="joinType">Type of the join</param>
        public TSource Join(QuerySourceTable source, IWhereCondition condition, JoinTypeEnum joinType = JoinTypeEnum.Inner)
        {
            if (condition != null)
            {
                var baseCondition = condition.WhereCondition;
                var result = Join(source, baseCondition, joinType);

                result.IncludeDataParameters(condition.Parameters, baseCondition);

                return result;
            }

            return Join(source, "", joinType);
        }


        /// <summary>
        /// Joins the given source with another
        /// </summary>
        /// <param name="source">Source table</param>
        /// <param name="condition">Join condition</param>
        /// <param name="joinType">Type of the join</param>
        public TSource Join(QuerySourceTable source, string condition = null, JoinTypeEnum joinType = JoinTypeEnum.Inner)
        {
            var result = GetTypedQuery();

            // Get the right expression
            result.TranslateSource(source);

            var rightExpression = source.GetFullExpression();

            // Build the join string
            result.SourceExpression = SqlHelper.GetJoin(result.SourceExpression, rightExpression, condition, joinType);

            // Move the source names
            result.LeftSourceName = result.RightSourceName;
            result.RightSourceName = source.GetName();

            return result;
        }


        /// <summary>
        /// Joins the given source with another
        /// </summary>
        /// <param name="source">Source table</param>
        /// <param name="leftColumn">Left column</param>
        /// <param name="rightColumn">Right column</param>
        /// <param name="additionalCondition">Additional JOIN condition, this will be added with AND operator to the base condition</param>
        /// <param name="joinType">Type of the join</param>
        public TSource Join(QuerySourceTable source, string leftColumn, string rightColumn, IWhereCondition additionalCondition = null, JoinTypeEnum joinType = JoinTypeEnum.Inner)
        {
            var result = GetTypedQuery();

            // Get the full right expression
            result.TranslateSource(source);
            var rightExpression = source.GetFullExpression();

            // Move the source names
            result.LeftSourceName = result.RightSourceName;
            result.RightSourceName = source.GetName();

            var condition = result.GetJoinCondition(leftColumn, rightColumn);

            // Apply additional where condition if defined
            if (additionalCondition != null)
            {
                var completeCondition = new WhereCondition(condition).And(additionalCondition);
                condition = result.IncludeDataParameters(completeCondition.Parameters, completeCondition.WhereCondition);
            }

            result.SourceExpression = SqlHelper.GetJoin(result.SourceExpression, rightExpression, condition, joinType);

            return result;
        }


        /// <summary>
        /// Joins the given source with another
        /// </summary>
        /// <param name="sourceExpression">Source expression</param>
        /// <param name="condition">Join condition</param>
        public TSource LeftJoin(string sourceExpression, string condition = null)
        {
            return Join(sourceExpression, condition, JoinTypeEnum.LeftOuter);
        }


        /// <summary>
        /// Joins the given source with another
        /// </summary>
        /// <param name="sourceExpression">Source expression</param>
        /// <param name="condition">Join condition</param>
        public TSource LeftJoin(string sourceExpression, IWhereCondition condition)
        {
            return Join(sourceExpression, condition, JoinTypeEnum.LeftOuter);
        }


        /// <summary>
        /// Joins the given source with another
        /// </summary>
        /// <param name="source">Source table</param>
        /// <param name="leftColumn">Left column</param>
        /// <param name="rightColumn">Right column</param>
        /// <param name="additionalCondition">Additional JOIN condition, this will be added with AND operator to the base condition</param>
        public TSource LeftJoin(QuerySourceTable source, string leftColumn, string rightColumn, IWhereCondition additionalCondition = null)
        {
            return Join(source, leftColumn, rightColumn, additionalCondition, JoinTypeEnum.LeftOuter);
        }


        /// <summary>
        /// Joins the given source with another
        /// </summary>
        /// <param name="source">Source table</param>
        /// <param name="condition">Join condition</param>
        public TSource RightJoin(QuerySourceTable source, string condition = null)
        {
            return Join(source, condition, JoinTypeEnum.RightOuter);
        }


        /// <summary>
        /// Joins the given source with another
        /// </summary>
        /// <param name="source">Source table</param>
        /// <param name="condition">Join condition</param>
        public TSource RightJoin(QuerySourceTable source, IWhereCondition condition)
        {
            return Join(source, condition, JoinTypeEnum.RightOuter);
        }


        /// <summary>
        /// Joins the given source with another
        /// </summary>
        /// <param name="source">Source table</param>
        /// <param name="leftColumn">Left column</param>
        /// <param name="rightColumn">Right column</param>
        /// <param name="additionalCondition">Additional JOIN condition, this will be added with AND operator to the base condition</param>
        public TSource RightJoin(QuerySourceTable source, string leftColumn, string rightColumn, IWhereCondition additionalCondition = null)
        {
            return Join(source, leftColumn, rightColumn, additionalCondition, JoinTypeEnum.RightOuter);
        }


        /// <summary>
        /// Joins the given source with another
        /// </summary>
        /// <param name="source">Source table</param>
        /// <param name="condition">Join condition</param>
        public TSource InnerJoin(QuerySourceTable source, string condition = null)
        {
            return Join(source, condition);
        }


        /// <summary>
        /// Joins the given source with another
        /// </summary>
        /// <param name="source">Source table</param>
        /// <param name="condition">Join condition</param>
        public TSource InnerJoin(QuerySourceTable source, IWhereCondition condition)
        {
            return Join(source, condition);
        }


        /// <summary>
        /// Joins the given source with another
        /// </summary>
        /// <param name="source">Source table</param>
        /// <param name="leftColumn">Left column</param>
        /// <param name="rightColumn">Right column</param>
        public TSource InnerJoin(QuerySourceTable source, string leftColumn, string rightColumn)
        {
            return Join(source, leftColumn, rightColumn);
        }

        #endregion


        #region "Object joins"

        /// <summary>
        /// Joins the given source with another
        /// </summary>
        /// <param name="leftColumn">Left column</param>
        /// <param name="rightColumn">Right column</param>
        /// <param name="joinType">Type of the join</param>
        /// <param name="additionalCondition">Additional JOIN condition, this will be added with AND operator to the base condition</param>
        public TSource Join<TObject>(string leftColumn, string rightColumn, JoinTypeEnum joinType = JoinTypeEnum.Inner, IWhereCondition additionalCondition = null)
            where TObject : BaseInfo, new()
        {
            return Join(new ObjectSource<TObject>(), leftColumn, rightColumn, additionalCondition, joinType);
        }


        /// <summary>
        /// Joins the given source with another
        /// </summary>
        /// <param name="leftColumn">Left column</param>
        /// <param name="rightColumn">Right column</param>
        /// <param name="additionalCondition">Additional JOIN condition, this will be added with AND operator to the base condition</param>
        public TSource LeftJoin<TObject>(string leftColumn, string rightColumn, IWhereCondition additionalCondition = null)
            where TObject : BaseInfo, new()
        {
            return Join(new ObjectSource<TObject>(), leftColumn, rightColumn, additionalCondition, JoinTypeEnum.LeftOuter);
        }


        /// <summary>
        /// Joins the given source with another
        /// </summary>
        /// <param name="leftColumn">Left column</param>
        /// <param name="rightColumn">Right column</param>
        /// <param name="additionalCondition">Additional JOIN condition, this will be added with AND operator to the base condition</param>
        public TSource RightJoin<TObject>(string leftColumn, string rightColumn, IWhereCondition additionalCondition = null)
            where TObject : BaseInfo, new()
        {
            return Join(new ObjectSource<TObject>(), leftColumn, rightColumn, additionalCondition, JoinTypeEnum.RightOuter);
        }


        /// <summary>
        /// Joins the given source with another
        /// </summary>
        /// <param name="leftColumn">Left column</param>
        /// <param name="rightColumn">Right column</param>
        /// <param name="additionalCondition">Additional JOIN condition, this will be added with AND operator to the base condition</param>
        public TSource InnerJoin<TObject>(string leftColumn, string rightColumn, IWhereCondition additionalCondition = null)
            where TObject : BaseInfo, new()
        {
            return Join(new ObjectSource<TObject>(), leftColumn, rightColumn, additionalCondition);
        }

        #endregion


        #region "Operators"
        
        /// <summary>
        /// Returns the string representation of the expression, with possibility of expanding parameters
        /// </summary>
        /// <param name="expand">If true, the result is expanded with parameters so it can act as standalone value.</param>
        public override string ToString(bool expand)
        {
            var result = SourceExpression;

            if (expand)
            {
                result = Expand(result);
            }

            return result;
        }


        /// <summary>
        /// Implicit operator for conversion from query source to string expression
        /// </summary>
        /// <param name="source">Source object</param>
        public static implicit operator string(QuerySourceBase<TSource> source)
        {
            if (source == null)
            {
                return null;
            }

            return source.SourceExpression;
        }


        /// <summary>
        /// Implicit operator for conversion from query source to query source table expression
        /// </summary>
        /// <param name="source">Source object</param>
        public static implicit operator QuerySourceTable(QuerySourceBase<TSource> source)
        {
            if (source == null)
            {
                return null;
            }

            return source.SourceExpression;
        }

        #endregion
    }
}
