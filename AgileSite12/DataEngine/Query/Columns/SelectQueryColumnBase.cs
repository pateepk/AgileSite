using System;

namespace CMS.DataEngine
{
    /// <summary>
    /// Query column
    /// </summary>
    public abstract class SelectQueryColumnBase<TColumn> : QueryColumnBase<TColumn>
        where TColumn : SelectQueryColumnBase<TColumn>, new()
    {
        #region "Variables"

        private bool mEnsureBracketsForAlias = true;

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets the column name
        /// </summary>
        public override string Name
        {
            get
            {
                return ColumnAlias ?? base.Name;
            }
        }


        /// <summary>
        /// Column alias
        /// </summary>
        public string ColumnAlias
        {
            get;
            set;
        }


        /// <summary>
        /// If true, brackets for the column alias are ensured
        /// </summary>
        protected bool EnsureBracketsForAlias
        {
            get
            {
                return mEnsureBracketsForAlias;
            }
            set
            {
                mEnsureBracketsForAlias = value;
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="expression">Expression (column name)</param>
        protected SelectQueryColumnBase(string expression)
            : base(expression)
        {
        }
        
        #endregion


        #region "Methods"
        
        /// <summary>
        /// Sets the alias to the column
        /// </summary>
        /// <param name="alias">Column alias</param>
        public TColumn As(string alias)
        {
            var result = GetTypedQuery();
            result.ColumnAlias = alias;

            return TypedThis;
        }


        /// <summary>
        /// Gets the value expression of the column
        /// </summary>
        protected internal virtual string GetValueExpression()
        {
            return base.GetExpression();
        }


        /// <summary>
        /// Gets the expression for the column data
        /// </summary>
        public override string GetExpression()
        {
            string result = GetValueExpression();

            // Apply the alias
            if (!String.IsNullOrEmpty(ColumnAlias))
            {
                result = SqlHelper.AddColumnAlias(result, ColumnAlias, EnsureBracketsForAlias);
            }

            return result;
        }
        
        #endregion
    }
}
