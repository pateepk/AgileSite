using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CMS.Base;

namespace CMS.DataEngine
{
    /// <summary>
    /// Provides basic query column list functionality
    /// </summary>
    public abstract class QueryColumnListBase : IEnumerable<IQueryColumn>
    {
        #region "Variables"

        private List<IQueryColumn> mColumnsList = new List<IQueryColumn>();

        private HashSet<string> mPresentColumns;

        private string mColumns;

        #endregion


        #region "Properties"

        /// <summary>
        /// Returns column on specific index
        /// </summary>
        /// <param name="index">Index</param>
        public IQueryColumn this[int index]
        {
            get
            {
                return mColumnsList[index];
            }
            set
            {
                mColumnsList[index] = value;
            }
        }


        /// <summary>
        /// Parent query for the column list
        /// </summary>
        public IQueryObject Parent
        {
            get;
            internal set;
        }


        /// <summary>
        /// Indicates whether list contains all columns selector
        /// </summary>
        protected bool ContainsAllColumnsSelector
        {
            get
            {
                return PresentColumns.Contains(SqlHelper.COLUMNS_ALL);
            }
        }


        /// <summary>
        /// Indicates whether list represents an empty list with no columns, 
        /// no matter already existing columns or columns added after
        /// </summary>
        /// <remarks>
        /// If <see cref="SqlHelper.NO_COLUMNS"/> is explicitly added into an empty list, then this property is true
        /// and query does not return any columns only string representation of <see cref="SqlHelper.NO_COLUMNS"/>.
        /// </remarks>
        public bool NoColumns
        {
            get;
            protected set;
        }


        /// <summary>
        /// <para>There are three possible outputs.</para>
        /// <para>Firstly <see cref="SqlHelper.NO_COLUMNS"/>, there will be no column in query.</para>
        /// <para>Secondly list of columns represented as string, without occurrence of <see cref="SqlHelper.NO_COLUMNS"/>.</para>
        /// <para>Finally <see cref="String.Empty"/>, that resolves to * in query.</para>
        /// </summary>
        public string Columns
        {
            get
            {
                return mColumns ?? (mColumns = GetColumns());
            }
            protected set
            {
                mColumns = value;
            }
        }


        /// <summary>
        /// Returns true if any columns are defined within this column list
        /// </summary>
        public bool AnyColumnsDefined
        {
            get
            {
                return NoColumns || (Count > 0);
            }
        }


        /// <summary>
        /// Returns the number of defined columns
        /// </summary>
        public int Count
        {
            get
            {
                return mColumnsList.Count;
            }
        }


        /// <summary>
        /// Returns true if the columns contain all columns specification or columns do not contain definition of any columns
        /// </summary>
        public bool ReturnsAllColumns
        {
            get
            {
                return ContainsAllColumnsSelector || !AnyColumnsDefined;
            }
        }


        /// <summary>
        /// Returns true if the columns represent a single column
        /// </summary>
        public bool IsSingleColumn
        {
            get
            {
                return !NoColumns && !ContainsAllColumnsSelector && (Count == 1) && mColumnsList[0].IsSingleColumn;
            }
        }


        /// <summary>
        /// Gets the hash set of present columns for duplicity detection
        /// </summary>
        protected HashSet<string> PresentColumns
        {
            get
            {
                return mPresentColumns ?? (mPresentColumns = GetPresentColumns());
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="parent">Parent query parameters</param>
        protected QueryColumnListBase(IQueryObject parent = null)
        {
            Parent = parent;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Removes one explicitly defined column by <see cref="SelectQueryColumnBase{T}.Name"/> from the columns collection
        /// </summary>
        /// <remarks>
        /// See method <see cref="QueryColumn.FromExpression"/> and <seealso cref="QueryColumn"/> constructor for the difference how 
        /// <see cref="SelectQueryColumnBase{T}.Name"/> property can differ.
        /// </remarks>
        /// <param name="col">Column to remove</param>
        public void Remove(IQueryColumn col)
        {
            // Find the column by Name
            var columnToRemove = mColumnsList.Find(c => c.Name.EqualsCSafe(col.Name, true));
            bool removed = mColumnsList.Remove(columnToRemove);

            // If column removed and no other column exists, remove that column from set
            if (removed && !mColumnsList.Any(it => it.Name.EqualsCSafe(col.Name, true)))
            {
                PresentColumns.Remove(col.Name);
            }

            Changed();
        }


        /// <summary>
        /// Removes explicitly defined columns by <see cref="SelectQueryColumnBase{T}.Name"/> from the columns collection
        /// </summary>
        /// <remarks>
        /// See method <see cref="QueryColumn.FromExpression"/> and <seealso cref="QueryColumn"/> constructor for the difference how 
        /// <see cref="SelectQueryColumnBase{T}.Name"/> property can differ.
        /// </remarks>
        /// <param name="col">Column to remove</param>
        public void RemoveAll(IQueryColumn col)
        {
            mColumnsList.RemoveAll(c => c.Name.EqualsCSafe(col.Name, true));
            PresentColumns.Remove(col.Name);

            Changed();
        }


        /// <summary>
        /// Replaces the column with a given name with the new column definition
        /// </summary>
        /// <param name="originalName">Column name</param>
        /// <param name="newColumn">New column</param>
        public void ReplaceColumn(string originalName, IQueryColumn newColumn)
        {
            // Normalize column name
            originalName = SqlHelper.RemoveSquareBrackets(originalName);

            for (int i = 0; i < mColumnsList.Count; i++)
            {
                var col = mColumnsList[i];
                var colName = SqlHelper.RemoveSquareBrackets(col.Name);

                // If the column name matches, replace the column
                if (colName.EqualsCSafe(originalName, true))
                {
                    mColumnsList[i] = newColumn;

                    break;
                }
            }
        }


        /// <summary>
        /// Adds the given column to the column list
        /// </summary>
        /// <param name="col">Column to be added</param>
        protected void AddInternal(IQueryColumn col)
        {
            mColumnsList.Add(col);
            PresentColumns.Add(col.Name);

            NoColumns = false;

            Changed();
        }


        /// <summary>
        /// Adds the given column to the column list, only if not already present
        /// </summary>
        /// <param name="col">Column to be added</param>
        /// <param name="ignoreAllColumnsSelector">Indicates whether column will be added if all columns selector is present in the list</param>
        protected void AddUniqueInternal(IQueryColumn col, bool ignoreAllColumnsSelector = true)
        {
            // Check if the column is already present through * or specific alias
            if ((!ignoreAllColumnsSelector && !NoColumns && ContainsAllColumnsSelector) || PresentColumns.Contains(col.Name))
            {
                return;
            }

            AddInternal(col);
        }


        /// <summary>
        /// Ensures all columns flag if the list is empty
        /// </summary>
        public void EnsureAllColumns()
        {
            if (!NoColumns && (Count == 0))
            {
                AddUniqueInternal(QueryColumn.FromExpression(SqlHelper.COLUMNS_ALL));
            }
        }


        /// <summary>
        /// Gets list of columns represented as string
        /// </summary>
        /// <param name="parameters">Query parameters. If provided, the column parameters are included into the parameters and column expression is altered accordingly</param>
        /// <param name="expand">If true, the result expression is expanded with parameters</param>
        public string GetColumns(QueryDataParameters parameters = null, bool expand = false)
        {
            // No columns
            if (NoColumns)
            {
                return SqlHelper.NO_COLUMNS;
            }

            // Build the list of columns
            var sb = new StringBuilder();
            var first = true;

            // Add individual columns
            foreach (var column in this)
            {
                if (!first)
                {
                    sb.Append(", ");
                }
                else
                {
                    first = false;
                }

                var col = column.GetExpression();

                if (parameters != null)
                {
                    // Include the parameters and transform the column expression if necessary
                    col = parameters.IncludeDataParameters(column.Parameters, col);
                }
                else if (expand && (column.Parameters != null))
                {
                    // Expand the column expression using its parameters
                    col = column.Parameters.Expand(col);
                }

                sb.Append(col);
            }

            var result = sb.ToString();

            // Expand the whole result with parameters
            if (expand && (parameters != null))
            {
                result = parameters.Expand(result);
            }

            return result;
        }


        /// <summary>
        /// Gets the hash set of present columns for duplicity detection
        /// </summary>
        private HashSet<string> GetPresentColumns()
        {
            var cols = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);

            foreach (var queryColumn in mColumnsList)
            {
                cols.Add(queryColumn.Name);
            }

            return cols;
        }


        /// <summary>
        /// Clears the list of columns
        /// </summary>
        public void Clear()
        {
            mColumnsList.Clear();
            mPresentColumns = null;

            NoColumns = false;

            Changed();
        }


        /// <summary>
        /// Marks the list as changed and flushes all necessary caches
        /// </summary>
        protected void Changed()
        {
            mColumns = null;

            if (Parent != null)
            {
                Parent.Changed();
            }
        }

        /// <summary>
        /// Returns true if the object equals to another
        /// </summary>
        /// <param name="obj">Object to compare to</param>
        public override bool Equals(object obj)
        {
            // Only compare to column list
            var other = obj as QueryColumnList;
            if ((object)other == null)
            {
                return false;
            }

            return other.Columns == Columns;
        }


        /// <summary>
        /// Gets the object hash code
        /// </summary>
        public override int GetHashCode()
        {
            return Columns.GetHashCode();
        }


        /// <summary>
        /// Gets a string representation of the column list
        /// </summary>
        public override string ToString()
        {
            return Columns;
        }


        /// <summary>
        /// Compares two column lists
        /// </summary>
        /// <param name="cols1">First column list</param>
        /// <param name="cols2">Second columns list</param>
        public static bool operator ==(QueryColumnListBase cols1, QueryColumnListBase cols2)
        {
            if ((object)cols1 == null)
            {
                return ((object)cols2 == null);
            }

            return cols1.Equals(cols2);
        }


        /// <summary>
        /// Compares two column lists
        /// </summary>
        /// <param name="cols1">First column list</param>
        /// <param name="cols2">Second columns list</param>
        public static bool operator !=(QueryColumnListBase cols1, QueryColumnListBase cols2)
        {
            return !(cols1 == cols2);
        }


        /// <summary>
        /// Clones the column list
        /// </summary>
        /// <param name="newParent">New parent for the cloned object</param>>
        public QueryColumnList Clone(IQueryObject newParent = null)
        {
            var clone = new QueryColumnList(newParent);

            clone.mColumns = mColumns;
            clone.mColumnsList = new List<IQueryColumn>(mColumnsList);
            clone.NoColumns = NoColumns;

            return clone;
        }


        /// <summary>
        /// Gets the enumerator for the columns
        /// </summary>
        public IEnumerator<IQueryColumn> GetEnumerator()
        {
            return mColumnsList.GetEnumerator();
        }


        /// <summary>
        /// Gets the enumerator
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}
