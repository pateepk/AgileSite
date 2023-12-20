using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

using CMS.Helpers;
using CMS.Base;
using CMS.Core;

namespace CMS.DataEngine
{
    /// <summary>
    /// LINQ query provider for CMS objects
    /// </summary>
    internal class CMSQueryProvider<TObject> : AbstractCMSQueryProvider<CMSQueryProvider<TObject>>, IQueryProvider
    {
        #region "Properties"

        /// <summary>
        /// Data source object
        /// </summary>
        protected IQueryable Source
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if fallback to LINQ to object is required
        /// </summary>
        protected bool RequireLinqFallback
        {
            get;
            set;
        }

        public Guid InstanceGUID = Guid.NewGuid();

        #endregion


        #region "Methods"

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="source">Source of the data</param>
        public CMSQueryProvider(IQueryable source)
        {
            Source = source;
        }


        /// <summary>
        /// Constructor
        /// </summary>
        public CMSQueryProvider()
        {
        }

        #endregion


        #region "IQueryProvider Members"

        /// <summary>
        /// Creates the query from given expression.
        /// </summary>
        /// <param name="expression">Expression to parse</param>
        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            return CreateQueryInternal<TElement>(expression);
        }


        /// <summary>
        /// Creates the query from given expression.
        /// </summary>
        /// <param name="expression">Expression to parse</param>
        public IQueryable CreateQuery(Expression expression)
        {
            return CreateQuery<TObject>(expression);
        }


        /// <summary>
        /// Executes the query.
        /// </summary>
        /// <param name="expression">Query expression</param>
        public TResult Execute<TResult>(Expression expression)
        {
            return (TResult)ExecuteInternal(expression);
        }


        /// <summary>
        /// Executes the query.
        /// </summary>
        /// <param name="expression">Query expression</param>
        public object Execute(Expression expression)
        {
            return ExecuteInternal(expression);
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Creates the query from given expression.
        /// </summary>
        /// <param name="expression">Expression to parse</param>
        protected virtual IQueryable<TElement> CreateQueryInternal<TElement>(Expression expression)
        {
            var src = Source;

            IQueryable<TElement> res;

            var cmsSrc = src as ICMSQueryable;
            if (cmsSrc == null)
            {
                // For different type of source, use LINQ to objects directly
                LinqToObjectsFallback(expression, src, out res);

                return res;
            }
            else
            {
                var mc = expression as MethodCallExpression;
                if (mc != null)
                {
                    // Cast directly, do not attempt to create child
                    var lowerName = mc.Method.Name.ToLowerCSafe();

                    switch (lowerName)
                    {
                        case "cast":
                            return (IQueryable<TElement>)cmsSrc;
                    }
                }
            }

            try
            {
                if (cmsSrc.IsOffline)
                {
                    // LINQ to objects on offline collection
                    LinqToObjectsFallback(expression, src, out res);
                }
                else
                {
                    // Process through SQL query as a cloned collection
                    LoadExpression(expression);

                    var columnsChildQuery = ((ICMSQueryable<TObject>)cmsSrc).CreateChild(this);

                    // Return queryable if types match
                    res = columnsChildQuery as IQueryable<TElement>;

                    // Perform fallback to LINQ to objects if necessary
                    // Variable res is null when query changed type (TObject != TElement)
                    if ((res == null) || RequireLinqFallback)
                    {
                        // Fallback to LINQ to objects with modified query.                       
                        LinqToObjectsFallback(expression, src, out res, columnsChildQuery);
                    }
                }

                return res;
            }
            catch (Exception ex)
            {
                // Fallback to LINQ to objects with original query
                if (LinqToObjectsFallback(expression, src, out res))
                {
                    if (SystemContext.DiagnosticLogging)
                    {
                        CoreServices.EventLog.LogException("CMSQueryProvider", "CREATEQUERY", new NotImplementedException("LINQ fallback used for expression '" + expression.ToString() + "'", ex));
                    }
                    return res;
                }

                throw;
            }
        }


        /// <summary>
        /// Fallback to LINQ to objects
        /// </summary>
        /// <param name="expression">Expression to execute</param>
        /// <param name="source">Source data</param>
        /// <param name="result">Returning the result</param>
        /// <param name="newSource">New source data to be injected instead of original source</param>
        protected virtual bool LinqToObjectsFallback<TElement>(Expression expression, IEnumerable source, out IQueryable<TElement> result, IEnumerable newSource = null)
        {
            var wrapper = new EnumerableWrapper<TObject>(newSource ?? source).AsQueryable();

            var ex = new ExpressionTreeModifier(source, wrapper).CopyAndModify(expression);
            ex = Expression.Convert(ex, typeof(IEnumerable<TElement>));

            var enumRes = (IEnumerable<TElement>)wrapper.Provider.Execute(ex);
            result = enumRes.AsQueryable();

            return true;
        }


        /// <summary>
        /// Gets the result of the expression
        /// </summary>
        /// <param name="expression">Expression to execute</param>
        protected virtual object ExecuteInternal(Expression expression)
        {
            var constExp = expression as ConstantExpression;
            if (constExp != null)
            {
                // Constant
                return constExp.Value;
            }

            var mc = expression as MethodCallExpression;
            if (mc != null)
            {
                // Method call
                var lowerName = mc.Method.Name.ToLowerCSafe();

                switch (lowerName)
                {
                    case "count":
                        {
                            // Do the where filtering using LINQ to objects
                            Expression<Func<TObject, bool>> lambda = null;
                            if (mc.Arguments.Count == 2)
                            {
                                var argument = (UnaryExpression)mc.Arguments[1];

                                lambda = (Expression<Func<TObject, bool>>)argument.Operand;
                            }

                            if (lambda != null)
                            {
                                // Count with lambda
                                return ((IEnumerable<TObject>)Source).Count(lambda.Compile());
                            }

                            var col = Source as ICMSQueryable<TObject>;
                            if (col != null)
                            {
                                return col.Count;
                            }

                            return ((IEnumerable<TObject>)Source).Count();
                        }

                    default:
                        // Fallback to LINQ to objects for all other operations
                        {
                            var col = Source as IEnumerable<TObject>;
                            var wrapper = new EnumerableWrapper<TObject>(col).AsQueryable();

                            var ex = new ExpressionTreeModifier(col, wrapper).CopyAndModify(mc);

                            return wrapper.Provider.Execute(ex);
                        }
                }
            }

            return null;
        }

        #endregion


        #region "QueryProvider methods"

        /// <summary>
        /// Loads the given expression.
        /// </summary>
        /// <param name="ex">Expression to load</param>
        protected virtual void LoadExpression(Expression ex)
        {
            var sb = new ExpressionBuilderSettings(null);

            // Process the expression
            ProcessExpression(ex, sb, null);
        }


        /// <summary>
        /// Processes the unary expression.
        /// </summary>
        /// <param name="u">Expression</param>
        /// <param name="sb">ExpressionBuilderSettings for processing of children</param>
        protected override Expression ProcessUnary(UnaryExpression u, ExpressionBuilderSettings sb)
        {
            switch (u.NodeType)
            {
                case ExpressionType.Not:
                    // Not
                    sb.Result.Append("NOT ");
                    ProcessExpression(u.Operand, sb, u);
                    break;

                default:
                    throw new NotSupportedException("[CMSQueryProvider.ProcessUnary]: The unary operator '" + u.NodeType + "' is not supported");
            }
            return u;
        }


        /// <summary>
        /// Processes the binary expression.
        /// </summary>
        /// <param name="b">Expression</param>
        /// <param name="s">ExpressionBuilderSettings for processing of children</param>
        protected override Expression ProcessBinary(BinaryExpression b, ExpressionBuilderSettings s)
        {
            var sb = s.Result;

            sb.Append("(");

            // Process left side
            ProcessExpression(b.Left, s, b);

            sb.Append(" ", WhereBuilder.GetBinaryOperator(b.NodeType, b.Right), " ");

            // Process right side
            ProcessExpression(b.Right, s, b);

            sb.Append(")");

            return b;
        }


        /// <summary>
        /// Constant expression.
        /// </summary>
        /// <param name="c">Expression</param>
        /// <param name="s">ExpressionBuilderSettings for processing of children</param>
        /// <param name="parentExp">Parent expression</param>
        protected override Expression ProcessConstant(ConstantExpression c, ExpressionBuilderSettings s, Expression parentExp)
        {
            var sb = s.Result;

            // Constants are not included into columns
            if (s.IsColumnList)
            {
                RequireLinqFallback = true;
                return c;
            }

            IQueryable q = c.Value as IQueryable;
            if (q != null)
            {
                // Ignore collection
            }
            else if (c.Value == null)
            {
                // NULL value
                sb.Append(WhereBuilder.NULL);
            }
            else
            {
                // Process type code
                switch (Type.GetTypeCode(c.Value.GetType()))
                {
                    case TypeCode.Object:
                        // General objects
                        throw new NotSupportedException("[CMSQueryProvider.ProcessConstant]: The constant for '" + c.Value + "' is not supported");

                    default:
                        // Other values
                        // Ensure proper operator for a single boolean property evaluation
                        if (s.IsWhereCondition && (c.Value is bool) && ((parentExp == null) || !IsComparisonBinaryOperator(parentExp.NodeType)))
                        {
                            sb.Append((bool)c.Value ? "(1 = 1)" : "(0 = 1)");
                            break;
                        }

                        sb.Append(WhereBuilder.GetParameter("p", c.Value, ref mParameters));
                        break;
                }
            }

            return c;
        }


        /// <summary>
        /// Processes the member access.
        /// </summary>
        /// <param name="m">Expression</param>
        /// <param name="s">ExpressionBuilderSettings for processing of children</param>
        /// <param name="parentExp"></param>
        protected override Expression ProcessMemberAccess(MemberExpression m, ExpressionBuilderSettings s, Expression parentExp)
        {
            if (m.Expression == null)
            {
                throw new NotSupportedException("[CMSQueryProvider.ProcessMemberAccess]: The member '" + m.Member.Name + "' is not supported.");
            }

            var sb = s.Result;

            // Add separator
            if (sb.Length > 0)
            {
                sb.Append(s.MemberSeparator);
            }

            // Use property as SQL column name
            if (m.Member is PropertyInfo)
            {
                ProcessPropertyMemberAccess(m, s, parentExp);
                return m;
            }

            if (m.Member is System.Reflection.FieldInfo)
            {
                // Accessed member that cannot be resolved into Select clause.
                // Fallback is required but column list is valid and can be used in SELECT clause. 
                if (s.IsColumnList)
                {
                    RequireLinqFallback = true;
                    return m;
                }

                ProcessFieldMemberAccess(m, s, parentExp);
                return m;
            }

            throw new NotSupportedException("[CMSQueryProvider.ProcessMemberAccess]: The member '" + m.Member.Name + "' is not supported.");
        }


        private void ProcessPropertyMemberAccess(MemberExpression m, ExpressionBuilderSettings s, Expression parentExp)
        {
            var property = m.Expression.Type.GetProperty(m.Member.Name);

            // If property is not property of lambda parameter or is not of supported type, then fallback to LINQ to objects.
            if ((m.Expression.NodeType != ExpressionType.Parameter) || !property.PropertyType.IsValueType && (property.PropertyType != typeof(string)))
            {
                throw new NotSupportedException("[CMSQueryProvider.ProcessMemberAccess]: The member '" + property.Name + "' is not supported.");
            }

            string column = null;

            // Find the DatabaseMappingAttribute information
            var attributes = Attribute.GetCustomAttributes(property, typeof(DatabaseMappingAttribute), true);
            if (attributes.Length > 0)
            {
                var map = (DatabaseMappingAttribute)attributes[0];

                // Boolean property with DatabaseMappingAttribute can only be used in WHERE clause. In other clauses it's not supported. 
                // Also properties that can't be evaluated in DB or are declared without expression are not supported and requires fallback to LINQ to object.
                if ((!s.IsWhereCondition && (property.PropertyType == typeof(bool))) || !map.ExecuteInDB || (map.Expression == null))
                {
                    throw new NotSupportedException("[CMSQueryProvider.ProcessPropertyMemberAccess]: Property '" + property.Name + "' is not supported.");
                }

                // Bool properties with mapping expression require translation from boolean to bit for comparing compatibility with values in table columns. 
                if ((property.PropertyType == typeof(bool)))
                {
                    column = "(CASE WHEN " + map.Expression + " THEN 1 ELSE 0 END)";
                }
                else
                {
                    column = SqlHelper.AddSquareBrackets(map.Expression);
                }
            }
            else
            {
                // Find the DatabaseFieldAttribute information
                attributes = Attribute.GetCustomAttributes(property, typeof(DatabaseFieldAttribute), true);
                if (attributes.Length > 0)
                {
                    var map = (DatabaseFieldAttribute)attributes[0];

                    // Use specific column name if defined
                    column = SqlHelper.AddSquareBrackets(map.ColumnName ?? property.Name);
                }
            }

            // No attribute found
            if (column == null)
            {
                // Check if column exists
                var typeInfo = ObjectTypeManager.GetTypeInfos(typeof(TObject)).FirstOrDefault();
                if (typeInfo != null)
                {
                    var obj = ModuleManager.GetReadOnlyObject(typeInfo.ObjectType, true);
                    if (!obj.ContainsColumn(property.Name))
                    {
                        throw new NotSupportedException("[CMSQueryProvider.ProcessMemberAccess]: Property '" + m.Member.Name + "' is not supported.");
                    }
                }

                column = SqlHelper.AddSquareBrackets(property.Name);
            }


            // Ensure proper value for unary (single) boolean property in WHERE clause (bit to boolean conversion).
            if (s.IsWhereCondition && (property.PropertyType == typeof(bool)) && ((parentExp == null) || !IsComparisonBinaryOperator(parentExp.NodeType)))
            {
                column = String.Format("({0} = 1)", column);
            }

            if (!string.IsNullOrEmpty(column))
            {
                // Add member name
                s.Result.Append(column);
            }
        }


        private void ProcessFieldMemberAccess(MemberExpression m, ExpressionBuilderSettings s, Expression parentExp)
        {
            // Get value
            var objectMember = Expression.Convert(m, typeof(object));
            var getterLambda = Expression.Lambda<Func<object>>(objectMember);
            var getter = getterLambda.Compile();
            var value = getter();

            if (value == null)
            {
                // NULL value
                s.Result.Append(WhereBuilder.NULL);
            }
            else
            {
                // Process type code
                switch (Type.GetTypeCode(value.GetType()))
                {
                    case TypeCode.Object:
                        // General objects
                        throw new NotSupportedException("[CMSQueryProvider.ProcessFieldMemberAccess]: The field '" + value + "' is not supported");

                    default:
                        // Other values
                        // Ensure proper operator for a single boolean property evaluation
                        if (s.IsWhereCondition && (value is bool) && ((parentExp == null) || !IsComparisonBinaryOperator(parentExp.NodeType)))
                        {
                            s.Result.Append((bool)value ? "(1 = 1)" : "(0 = 1)");
                            break;
                        }

                        s.Result.Append(WhereBuilder.GetParameter("p", value, ref mParameters));
                        break;
                }
            }
        }


        /// <summary>
        /// Processes the method call.
        /// </summary>
        /// <param name="m">Expression</param>
        /// <param name="settings">ExpressionBuilderSettings for processing of children</param>
        protected override Expression ProcessMethodCall(MethodCallExpression m, ExpressionBuilderSettings settings)
        {
            string lowerName = m.Method.Name.ToLowerCSafe();

            // Check for SQL representation of the method
            var attr = AttributeHelper.GetCustomAttributes(m.Method, typeof(SqlRepresentationAttribute));
            if (attr.Length > 0)
            {
                var sql = attr[0] as SqlRepresentationAttribute;
                if (sql != null)
                {
                    // Build arguments
                    var arguments = new ArrayList();

                    // Add the calling object argument as the first one
                    if (!m.Method.IsStatic)
                    {
                        var s = new ExpressionBuilderSettings(settings);
                        ProcessExpression(m.Object, s, m);

                        arguments.Add(s.Result.ToString());
                    }

                    foreach (var arg in m.Arguments)
                    {
                        var s = new ExpressionBuilderSettings(settings);
                        ProcessExpression(arg, s, m);

                        arguments.Add(s.Result.ToString());
                    }

                    var sb = settings.Result;

                    sb.AppendFormat(sql.Format, arguments.ToArray());

                    return m;
                }
            }

            // Queryable selection methods
            if (m.Method.DeclaringType == typeof(Queryable))
            {
                switch (lowerName)
                {
                    case "where":
                        {
                            // Where condition
                            var s = new ExpressionBuilderSettings(null) { IsWhereCondition = true };

                            // Process the children
                            var lambda = (LambdaExpression)StripQuotes(m.Arguments[1]);
                            ProcessExpression(lambda.Body, s, lambda);

                            // Add the where condition
                            WhereCondition = SqlHelper.AddWhereCondition(WhereCondition, s.Result.ToString());

                            return m;
                        }

                    case "orderby":
                    case "orderbydescending":
                    case "thenby":
                    case "thenbydescending":
                        {
                            // Order by expression
                            var s = new ExpressionBuilderSettings(null) { IsColumnList = true };

                            // Process the children
                            var lambda = (LambdaExpression)StripQuotes(m.Arguments[1]);
                            ProcessExpression(lambda.Body, s, lambda);

                            var order = lowerName.Contains("descending") ? OrderDirection.Descending : OrderDirection.Ascending;
                            var sourceOrder = lowerName.Contains("then") ? ((ICMSQueryable<TObject>)Source).OrderByColumns : String.Empty;
                            OrderByColumns = SqlHelper.AddOrderBy(sourceOrder, s.Result.ToString(), order);

                            return m;
                        }

                    case "take":
                        {
                            // TopN
                            var number = (ConstantExpression)StripQuotes(m.Arguments[1]);

                            TopNRecords = ValidationHelper.GetInteger(number.Value, 0);

                            return m;
                        }
                    case "select":
                        {
                            // Process the children
                            var lambda = (LambdaExpression)StripQuotes(m.Arguments[1]);
                            var s = new ExpressionBuilderSettings(null)
                            {
                                MemberSeparator = ", ",
                                IsColumnList = true
                            };

                            ProcessExpression(lambda.Body, s, lambda);

                            // Set up the columns
                            var columns = s.Result.ToString();

                            SelectColumnsList.Load(columns);

                            return m;
                        }

                    case "cast":
                        {
                            // Cast - let through
                            return m;
                        }
                }
            }

            throw new NotSupportedException("[CMSQueryProvider.ProcessMethodCall]: The method '" + m.Method.Name + "' is not supported");
        }


        /// <summary>
        /// Removes the quotes from the expression.
        /// </summary>
        /// <param name="e">Expression</param>
        private static Expression StripQuotes(Expression e)
        {
            while (e.NodeType == ExpressionType.Quote)
            {
                e = ((UnaryExpression)e).Operand;
            }
            return e;
        }

        #endregion
    }
}
