using System;


namespace CMS.DataEngine
{
    /// <summary>
    /// Generic variant of the abstract query object, provides fluent syntax
    /// </summary>
    public abstract class QueryParametersBase<TParent> : AbstractQueryObject, IQueryParameters<TParent>
        where TParent : QueryParametersBase<TParent>, new()
    {
        #region "Variables"

        /// <summary>
        /// Query data parameters
        /// </summary>
        protected QueryDataParameters mParameters = null;

        #endregion


        #region "Properties"

        /// <summary>
        /// If true, this object instance is immutable, and next subsequent modification starts with a clone of the object.
        /// </summary>
        protected bool IsImmutable
        {
            get;
            set;
        }


        /// <summary>
        /// Returns specifically typed current instance
        /// </summary>
        private TParent TypedThis
        {
            get
            {
                return (TParent)this;
            }
        }


        /// <summary>
        /// Query data parameters
        /// </summary>
        public override QueryDataParameters Parameters
        {
            get
            {
                return mParameters;
            }
            set
            {
                mParameters = value;
                Changed();
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Applies this where condition to the target object
        /// </summary>
        /// <param name="target">Target object defining parameters</param>
        public override void ApplyParametersTo(IQueryObject target)
        {
            var t = target as IQueryParameters;
            if (t != null)
            {
                // Combine where conditions
                t.IncludeDataParameters(Parameters, null);
            }
        }


        /// <summary>
        /// Copies all the object properties to the given target class
        /// </summary>
        /// <param name="target">Target class</param>
        public virtual void CopyPropertiesTo(IQueryObject target)
        {
            var t = target as IQueryParameters;
            if (t != null)
            {
                t.Parameters = new QueryDataParameters(Parameters);
            }
        }


        /// <summary>
        /// Creates the clone of the object.
        /// </summary>
        public override IQueryObject CloneObject()
        {
            return Clone();
        }


        /// <summary>
        /// Creates the clone of the object.
        /// </summary>
        public virtual TParent Clone()
        {
            // Create new instance and copy over the properties
            var result = new TParent();
            CopyPropertiesTo(result);

            return result;
        }


        /// <summary>
        /// Ensures data parameters for the given query
        /// </summary>
        public QueryDataParameters EnsureParameters()
        {
            return Parameters ?? (Parameters = new QueryDataParameters());
        }


        /// <summary>
        /// Adds the data parameters to the current query parameters
        /// </summary>
        /// <param name="addParameters">Parameters to add</param>
        /// <param name="expression">Expression which refers to the parameters</param>
        public sealed override string IncludeDataParameters(QueryDataParameters addParameters, string expression)
        {
            if ((addParameters == null) || addParameters.IsEmpty)
            {
                return expression;
            }

            var par = EnsureParameters();

            return par.IncludeDataParameters(addParameters, expression);
        }


        /// <summary>
        /// Makes this object instance is immutable, and next subsequent modification starts with a clone of the object.
        /// </summary>
        public TParent Immutable()
        {
            IsImmutable = true;

            return TypedThis;
        }


        /// <summary>
        /// Creates an immutable snapshot of the current query as a base for further evaluation. Doesn't modify the original query in any way.
        /// </summary>
        public TParent Snapshot()
        {
            var result = Clone();

            return result.Immutable();
        }


        /// <summary>
        /// Returns specifically typed instance of current Query object to allow user to compose Query in fluent syntax. If current Query is immutable then its clone is returned.
        /// </summary>
        /// <remarks>Use this method in case of creating custom parameterization method for <see cref="DataQuery" /> and its derivatives to obtain specifically typed instance of the Query and to reflect its immutability. 
        /// Invoke this method before any manipulation is done on the Query object.
        /// </remarks>
        public TParent GetTypedQuery()
        {
            if (IsImmutable)
            {
                return Clone();
            }

            return TypedThis;
        }


        /// <summary>
        /// Expands the expression by replacing parameters with their values
        /// </summary>
        /// <param name="expression">Expression to expand</param>
        public virtual string Expand(string expression)
        {
            return Expand(expression, null);
        }


        /// <summary>
        /// Expands the expression by replacing parameters with their values
        /// </summary>
        /// <param name="expression">Expression to expand</param>
        /// <param name="getValue">Custom function for getting the value</param>
        internal string Expand(string expression, Func<DataParameter, string> getValue)
        {
            if (Parameters != null)
            {
                return Parameters.Expand(expression, getValue);
            }

            return expression;
        }


        /// <summary>
        /// Returns the string representation of the expression
        /// </summary>
        public sealed override string ToString()
        {
            return ToString(false);
        }


        /// <summary>
        /// Returns the string representation of the expression, with possibility of expanding parameters
        /// </summary>
        /// <param name="expand">If true, the result is expanded with parameters so it can act as standalone value.</param>
        public abstract string ToString(bool expand);

        #endregion
    }
}