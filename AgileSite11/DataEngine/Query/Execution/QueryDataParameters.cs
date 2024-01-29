using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using CMS.Base;
using CMS.Helpers;

namespace CMS.DataEngine
{
    using Query;
    using ParametersDictionary = StringSafeDictionary<DataParameter>;

    /// <summary>
    /// Container that holds a list of query parameters.
    /// </summary>
    public class QueryDataParameters : IEnumerable<DataParameter>
    {
        #region "Variables"

        /// <summary>
        /// List of parameters defined locally in this instance
        /// </summary>
        private List<DataParameter> mLocalParameters;

        /// <summary>
        /// Table of existing parameters [name -> DataParameter]
        /// </summary>
        private ParametersDictionary mExistingLocalParameters;

        /// <summary>
        /// Extra query macros
        /// </summary>
        private StringSafeDictionary<string> mExtraLocalMacros;


        private DataSet mFillDataSet;

        private string mQueryBefore = String.Empty;
        private string mQueryAfter = String.Empty;
        private string mSource;

        /// <summary>
        /// Regular expression to get all parameters from the given expression
        /// </summary>
        private static readonly CMSRegex ParamNameRegex = new CMSRegex("@\\w+\\b", true);

        #endregion


        #region "Properties"

        /// <summary>
        /// Returns true if parameters are empty
        /// </summary>
        internal bool IsEmpty
        {
            get
            {
                return ((Count <= 0) && (MacroCount <= 0) && !String.IsNullOrEmpty(QueryAfter) && !String.IsNullOrEmpty(QueryBefore));
            }
        }


        /// <summary>
        /// Extra query macros
        /// </summary>
        protected StringSafeDictionary<string> ExtraLocalMacros
        {
            get
            {
                return mExtraLocalMacros ?? (mExtraLocalMacros = new StringSafeDictionary<string>());
            }
        }


        /// <summary>
        /// List of parameters defined locally in this instance
        /// </summary>
        protected List<DataParameter> LocalParameters
        {
            get
            {
                return mLocalParameters ?? (mLocalParameters = new List<DataParameter>());
            }
        }


        /// <summary>
        /// Table of existing parameters [name -> DataParameter]
        /// </summary>
        protected ParametersDictionary ExistingLocalParameters
        {
            get
            {
                return mExistingLocalParameters ?? (mExistingLocalParameters = new ParametersDictionary());
            }
        }


        /// <summary>
        /// Parent query parameters
        /// </summary>
        protected QueryDataParameters ParentParameters
        {
            get;
            set;
        }


        /// <summary>
        /// DataSet to be filled by the query. If not given, new DataSet is created
        /// </summary>
        public DataSet FillDataSet
        {
            get
            {
                var ds = mFillDataSet;
                if ((ds == null) && (ParentParameters != null))
                {
                    ds = ParentParameters.FillDataSet;
                }

                return ds;
            }
            set
            {
                mFillDataSet = value;
            }
        }


        /// <summary>
        /// Text included before the query
        /// </summary>
        public string QueryBefore
        {
            get
            {
                string query = null;

                // Add text from parent parameters
                if (ParentParameters != null)
                {
                    query += ParentParameters.QueryBefore;
                }

                query += mQueryBefore;

                return query;
            }
            set
            {
                mQueryBefore = value;
            }
        }


        /// <summary>
        /// Text included after the query
        /// </summary>
        public string QueryAfter
        {
            get
            {
                var query = mQueryAfter;

                // Add text from parent parameters
                if (ParentParameters != null)
                {
                    query += ParentParameters.QueryAfter;
                }

                return query;
            }
            set
            {
                mQueryAfter = value;
            }
        }


        /// <summary>
        /// Returns the parameter of the specified name.
        /// </summary>
        /// <param name="name">Parameter name</param>
        public DataParameter this[string name]
        {
            get
            {
                name = SqlHelper.GetParameterName(name);

                return GetParameter(name);
            }
        }


        /// <summary>
        /// Returns the parameter on specified index.
        /// </summary>
        /// <param name="index">Index</param>
        public DataParameter this[int index]
        {
            get
            {
                if (ParentParameters != null)
                {
                    // Get from parent parameters first
                    if (index < ParentParameters.Count)
                    {
                        return ParentParameters[index];
                    }

                    index -= ParentParameters.Count;
                }

                if (mLocalParameters != null)
                {
                    return mLocalParameters[index];
                }

                return null;
            }
        }


        /// <summary>
        /// Number of the registered parameters
        /// </summary>
        public int Count
        {
            get
            {
                // Get the number of local parameters if exist
                var count = (mLocalParameters != null) ? mLocalParameters.Count : 0;

                if (ParentParameters != null)
                {
                    count += ParentParameters.Count;
                }

                return count;
            }
        }


        /// <summary>
        /// Number of macros in the data parameters
        /// </summary>
        public int MacroCount
        {
            get
            {
                // Get the number of local macros if exist
                var count = (mExtraLocalMacros != null) ? mExtraLocalMacros.Count : 0;

                if (ParentParameters != null)
                {
                    count += ParentParameters.MacroCount;
                }

                return count;
            }
        }


        /// <summary>
        /// Source of the data
        /// </summary>
        public string Source
        {
            get
            {
                var source = mSource;

                // In case of empty source, try parent parameters
                if (String.IsNullOrEmpty(source) && (ParentParameters != null))
                {
                    source = ParentParameters.Source;
                }

                return source;
            }
            set
            {
                mSource = value;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="parentParameters">Parent query parameters</param>
        public QueryDataParameters(QueryDataParameters parentParameters = null)
        {
            ParentParameters = parentParameters;
        }


        /// <summary>
        /// Gets a parameter by its name
        /// </summary>
        /// <param name="name">Parameter name</param>
        private DataParameter GetParameter(string name)
        {
            // Try local parameters first
            var existing = (mExistingLocalParameters != null) ? mExistingLocalParameters[name] : null;

            if ((existing == null) && (ParentParameters != null))
            {
                // Try parent parameters
                return ParentParameters[name];
            }

            return existing;
        }


        /// <summary>
        /// Adds the date time parameter into the list (if the date time is DateTime.MinValue, adds DBNull.Value).
        /// </summary>
        /// <param name="name">Parameter name</param>
        /// <param name="value">Parameter value</param>
        public DataParameter AddDateTime(string name, DateTime value)
        {
            return Add(name, value, (value != DateTime.MinValue));
        }


        /// <summary>
        /// Adds the guid parameter into the list (if the guid id Guid.Empty, adds DBNull.Value).
        /// </summary>
        /// <param name="name">Parameter name</param>
        /// <param name="value">Parameter value</param>
        public DataParameter AddGuid(string name, Guid value)
        {
            return Add(name, value, (value != Guid.Empty));
        }


        /// <summary>
        /// Adds the ID parameter into the list (if the ID is not larger than 0, adds DBNull.Value).
        /// </summary>
        /// <param name="name">Parameter name</param>
        /// <param name="value">Parameter value</param>
        public DataParameter AddId(string name, int value)
        {
            return Add(name, value, value > 0);
        }


        /// <summary>
        /// Adds the parameter into the list.
        /// </summary>
        /// <param name="name">Parameter name</param>
        /// <param name="value">Parameter value</param>
        /// <param name="condition">Condition, if false, the NULL value is set</param>
        public DataParameter Add(string name, object value, bool condition)
        {
            return Add(name, condition ? value : DBNull.Value);
        }


        /// <summary>
        /// Adds the parameter into the list, if the parameter already exists, defines a unique name for it.
        /// </summary>
        /// <param name="name">Parameter name</param>
        /// <param name="value">Parameter value</param>
        /// <param name="mergeSameParameters">If true, the process merges the parameters with same name and value into one</param>
        public DataParameter AddUnique(string name, object value, bool mergeSameParameters = true)
        {
            if (value == null)
            {
                value = DBNull.Value;
            }

            name = SqlHelper.GetParameterName(name);

            // Create new parameter
            var newParam = new DataParameter(name, value);

            return AddUnique(newParam, mergeSameParameters);
        }


        /// <summary>
        /// Adds the parameter into the list, if the parameter already exists, defines a unique name for it.
        /// </summary>
        /// <param name="par">Parameter to add</param>
        /// <param name="mergeSameParameters">If true, the process merges the parameters with same name and value into one</param>
        public DataParameter AddUnique(DataParameter par, bool mergeSameParameters = true)
        {
            if (mergeSameParameters)
            {
                // Try to merge with existing
                var existing = GetParameter(par.Name);

                if ((existing != null) && existing.Equals(par))
                {
                    return existing;
                }
            }

            Add(par, true);

            return par;
        }


        /// <summary>
        /// Adds the parameter into the list.
        /// </summary>
        /// <param name="name">Parameter name</param>
        /// <param name="value">Parameter value</param>
        /// <param name="type">Type of the parameter</param>
        public DataParameter Add(string name, object value, Type type = null)
        {
            if (value == null)
            {
                value = DBNull.Value;
            }

            // Create parameter
            var param = new DataParameter(name, value);

            // Set type
            if (type != null)
            {
                param.Type = type;
            }

            Add(param);

            return param;
        }


        /// <summary>
        /// Adds the parameter into the list.
        /// </summary>
        /// <param name="param">Parameter to add</param>
        public void Add(DataParameter param)
        {
            Add(param, false);
        }


        /// <summary>
        /// Adds the parameter into the list.
        /// </summary>
        /// <param name="param">Parameter to add</param>
        /// <param name="unique">If true, the parameter will be added with a unique name if already present</param>
        protected virtual void Add(DataParameter param, bool unique)
        {
            string name = param.Name;

            // Check for macro - Not supported anymore
            if (name.StartsWith("##", StringComparison.Ordinal))
            {
                throw new NotSupportedException("[QueryDataParameters.Add]: Method Add does not supports query macros. Use method AddMacro to add query macro to the parameters.");
            }

            var existing = GetParameter(name);
            if (existing != null)
            {
                if (unique)
                {
                    // Ensure a unique parameter name
                    name = GetUniqueName(name, false);
                    param.Name = name;
                }
                else
                {
                    // Check existing type
                    if (param.Type != existing.Type)
                    {
                        throw new Exception(String.Format("[QueryDataParameters.Add]: The collection already contains parameter '{0}' of a different type.", param.Name));
                    }

                    // Replace with a new value and name
                    existing.Value = param.Value;
                    existing.Name = param.Name;

                    return;
                }
            }

            // Add new parameter if not exists
            LocalParameters.Add(param);

            ExistingLocalParameters[name] = param;
        }


        /// <summary>
        /// Gets a unique parameter name using the base name
        /// </summary>
        /// <param name="name">Base name</param>
        /// <param name="checkBaseName">If true, given base name is also checked for uniqueness</param>
        private string GetUniqueName(string name, bool checkBaseName = true)
        {
            // Check base name
            if (checkBaseName && (GetParameter(name) == null))
            {
                return name;
            }

            // Find unique index
            int index = 0;
            string tryName;

            // Trim existing digits
            name = TextHelper.TrimNumberSuffix(name);

            do
            {
                index++;
                tryName = name + index;
            }
            while (GetParameter(tryName) != null);

            // Adjust the parameter name with the index to make it unique
            name += index;

            return name;
        }


        /// <summary>
        /// Adds the data parameters to the current query parameters
        /// </summary>
        /// <param name="addParameters">Parameters to add</param>
        /// <param name="expression">Expression which refers to the parameters</param>
        /// <param name="mergeSameParameters">If true, the process merges the parameters with same name and value into one</param>
        public string IncludeDataParameters(QueryDataParameters addParameters, string expression, bool mergeSameParameters = true)
        {
            if (addParameters == null)
            {
                return expression;
            }

            // Include parameters
            expression = IncludeParameters(addParameters, expression, mergeSameParameters);

            // Include macros
            var macros = addParameters.GetMacros();

            foreach (DictionaryEntry macro in macros)
            {
                AddMacro((string)macro.Key, (string)macro.Value);
            }

            // Merge before / after texts
            var before = addParameters.QueryBefore;
            if (!String.IsNullOrEmpty(before) && (mQueryBefore != before))
            {
                mQueryBefore += before;
            }

            var after = addParameters.QueryAfter;

            if (!String.IsNullOrEmpty(after) && (mQueryAfter != after))
            {
                mQueryAfter = after + mQueryAfter;
            }

            return expression;
        }


        /// <summary>
        /// Adds the data parameters to the current query parameters
        /// </summary>
        /// <param name="addParameters">Parameters to add</param>
        /// <param name="expression">Expression which refers to the parameters</param>
        /// <param name="mergeSameParameters">If true, the process merges the parameters with same name and value into one</param>
        private string IncludeParameters(QueryDataParameters addParameters, string expression, bool mergeSameParameters)
        {
            if ((addParameters == null) || (addParameters.Count <= 0))
            {
                return expression;
            }

            StringSafeDictionary<string> changedNames = null;

            // Process all parameters
            foreach (var par in addParameters)
            {
                if (mergeSameParameters && ContainsSameParameter(par))
                {
                    // Skip all parameters that are already present
                    continue;
                }

                string name = par.Name;

                // Add the data parameter as unique
                var newPar = AddUnique(par.Clone(), mergeSameParameters);
                var newName = newPar.Name;

                if ((newName != name) && !String.IsNullOrEmpty(expression))
                {
                    // Remember all changed names
                    if (changedNames == null)
                    {
                        changedNames = new StringSafeDictionary<string>();
                    }

                    changedNames[name] = newName;
                }
            }

            if (changedNames != null)
            {
                // Replace all changed names
                expression = ParamNameRegex.Replace(expression, m =>
                {
                    var name = m.Value;

                    return changedNames[name] ?? name;
                });
            }

            return expression;
        }


        /// <summary>
        /// Returns true, if parameters contain the same parameter as the given one
        /// </summary>
        /// <param name="par">Parameter</param>
        private bool ContainsSameParameter(DataParameter par)
        {
            var existing = GetParameter(par.Name);
            if (existing == null)
            {
                return false;
            }

            return (existing.Value == par.Value);
        }


        /// <summary>
        /// Expands the expression by replacing parameters with their values
        /// </summary>
        /// <param name="expression">Expression to expand</param>
        public string Expand(string expression)
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
            // Do not expand empty expression
            if (String.IsNullOrEmpty(expression))
            {
                return expression;
            }

            // Expand all parameters
            foreach (var par in this)
            {
                expression = par.Expand(expression, getValue);
            }

            return expression;
        }
        

        /// <summary>
        /// Gets the complete query text updated with the parameters
        /// </summary>
        /// <param name="queryText">Query text template</param>
        public string GetCompleteQueryText(string queryText)
        {
            return AddBeforeAfter(ResolveMacros(queryText));
        }


        /// <summary>
        /// Adds the before and after query text
        /// </summary>
        /// <param name="queryText">Query text</param>
        internal string AddBeforeAfter(string queryText)
        {
            return QueryBefore + queryText + QueryAfter;
        }


        /// <summary>
        /// Resolves macros within the given query text
        /// </summary>
        /// <param name="queryText">Query text template</param>
        public string ResolveMacros(string queryText)
        {
            // Resolve source macro first, as a native query macro. It may contain other extra macros inside.
            if (!String.IsNullOrEmpty(Source))
            {
                queryText = ResolveMacro(queryText, "##SOURCE##", Source);
            }

            // Resolve macros from parent parameters first, as they may contain other more local macros
            if (ParentParameters != null)
            {
                queryText = ParentParameters.ResolveMacros(queryText);
            }

            // Resolve extra local macros
            if (mExtraLocalMacros != null)
            {
                foreach (var macro in mExtraLocalMacros.TypedKeys)
                {
                    queryText = ResolveMacro(queryText, macro, mExtraLocalMacros[macro]);
                }
            }

            return queryText;
        }


        /// <summary>
        /// Resolves the given macro within the given query text
        /// </summary>
        /// <param name="queryText">Query text</param>
        /// <param name="name">Macro name</param>
        /// <param name="value">Macro value</param>
        private static string ResolveMacro(string queryText, string name, string value)
        {
            var re = RegexHelper.GetRegex(name, true);

            queryText = re.Replace(queryText, value);

            return queryText;
        }


        /// <summary>
        /// Adds the macro to the query data parameters
        /// </summary>
        /// <param name="name">Macro name</param>
        /// <param name="value">Macro value</param>
        /// <param name="addHashes">If true, hash chars are added to the macro automatically, e.g. "TABLE" is converted to "##TABLE##"</param>
        public void AddMacro(string name, string value, bool addHashes = false)
        {
            if (addHashes)
            {
                name = String.Format("##{0}##", name);
            }

            ExtraLocalMacros[name] = value;
        }


        /// <summary>
        /// Converts the data parameter to string
        /// </summary>
        public override string ToString()
        {
            return String.Join(", ", this);
        }


        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        protected IEnumerable<DictionaryEntry> GetMacros()
        {
            // Enumerate first the parent parameters
            if (ParentParameters != null)
            {
                var parentMacros = ParentParameters.GetMacros();

                foreach (var macro in parentMacros)
                {
                    yield return macro;
                }
            }

            // Enumerate local parameters
            if (mExtraLocalMacros != null)
            {
                foreach (DictionaryEntry macro in mExtraLocalMacros)
                {
                    yield return macro;
                }
            }
        }



        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        public IEnumerator<DataParameter> GetEnumerator()
        {
            // Enumerate first the parent parameters
            if (ParentParameters != null)
            {
                foreach (var p in ParentParameters)
                {
                    yield return p;
                }
            }

            // Enumerate local parameters
            if (mLocalParameters != null)
            {
                foreach (var p in mLocalParameters)
                {
                    yield return p;
                }
            }
        }


        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }


        /// <summary>
        /// Returns true if the object equals another object
        /// </summary>
        /// <param name="obj">Object to compare to</param>
        public override bool Equals(object obj)
        {
            var another = obj as QueryDataParameters;

            // Check fields
            if ((another == null) ||
                (Count != another.Count) ||
                (MacroCount != another.MacroCount) ||
                (QueryBefore != another.QueryBefore) ||
                (QueryAfter != another.QueryAfter) ||
                (FillDataSet != another.FillDataSet))
            {
                return false;
            }

            // Check all parameters
            for (int i = 0; i < Count; i++)
            {
                if (!this[i].Equals(another[i]))
                {
                    return false;
                }
            }

            // Check all macros
            var macroEnum = GetMacros().GetEnumerator();
            var otherMacroEnum = another.GetMacros().GetEnumerator();

            while (macroEnum.MoveNext())
            {
                // Move the other as well
                if (!otherMacroEnum.MoveNext())
                {
                    return false;
                }

                // Compare current items
                var current = macroEnum.Current;
                var otherCurrent = otherMacroEnum.Current;

                if ((current.Key != otherCurrent.Key) || (current.Value != otherCurrent.Value))
                {
                    return false;
                }
            }

            return true;
        }


        /// <summary>
        /// Provides a hashcode for the object
        /// </summary>
        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }


        /// <summary>
        /// Gets the declaration of parameters
        /// </summary>
        internal string GetDeclaration()
        {
            return String.Join(Environment.NewLine, this.Select(param => param.GetDeclaration()));
        }


        /// <summary>
        /// Includes the given value to the parameters if necessary
        /// </summary>
        /// <param name="name">Parameter name</param>
        /// <param name="value">Value to include</param>
        /// <param name="parameters">Parameters</param>
        /// <returns>Returns the expression to represent the value in a query</returns>
        internal static string IncludeValue(string name, object value, ref QueryDataParameters parameters)
        {
            string result;

            // Convert data parameters to value object
            if (value is DataParameter)
            {
                value = value.AsValue();
            }

            // Try to process as query expression
            var expr = value as IQueryObjectWithValue;
            if (expr != null)
            {
                IQueryExpression expression = expr.AsValue();

                // Include the object parameters and update expression
                if (expr.Parameters != null)
                {
                    parameters = parameters ?? new QueryDataParameters();

                    expression.Expression = parameters.IncludeDataParameters(expression.Parameters, expression.Expression);
                }
                
                // Get the expression as a result
                result = expression.GetExpression();
            }
            else
            {
                parameters = parameters ?? new QueryDataParameters();

                // Add parameter value
                var param = parameters.AddUnique(name, value);

                result = param.Name;
            }

            return result;
        }

        #endregion
    }
}