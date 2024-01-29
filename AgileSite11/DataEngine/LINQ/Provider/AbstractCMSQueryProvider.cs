using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;

namespace CMS.DataEngine
{
    /// <summary>
    /// Abstract LINQ query provider for CMS objects
    /// </summary>
    internal abstract class AbstractCMSQueryProvider<ParentType> : DataQuerySettingsBase<ParentType>
        where ParentType : DataQuerySettingsBase<ParentType>, new()
    {
        #region "Expression methods"

        /// <summary>
        /// Processes the given expression.
        /// </summary>
        /// <param name="exp">Expression to process</param>
        /// <param name="sb">ExpressionBuilderSettings for processing of children</param>
        /// <param name="parentExp">Parent expression</param>
        protected virtual Expression ProcessExpression(Expression exp, ExpressionBuilderSettings sb, Expression parentExp)
        {
            if (exp == null)
            {
                return null;
            }

            if (IsUnary(exp.NodeType))
            {
                // Unary expressions
                return ProcessUnary((UnaryExpression)exp, sb);
            }
            else if (IsBinaryOperator(exp.NodeType))
            {
                // Binary expressions
                return ProcessBinary((BinaryExpression)exp, sb);
            }

            switch (exp.NodeType)
            {
                case ExpressionType.TypeIs:
                    // Type match
                    return ProcessTypeIs((TypeBinaryExpression)exp, sb);

                case ExpressionType.Conditional:
                    // Conditional expression
                    return ProcessConditional((ConditionalExpression)exp, sb);

                case ExpressionType.Constant:
                    // Constant
                    return ProcessConstant((ConstantExpression)exp, sb, parentExp);

                case ExpressionType.Parameter:
                    // Parameter expression
                    return ProcessParameter((ParameterExpression)exp, sb);

                case ExpressionType.MemberAccess:
                    // Member access
                    return ProcessMemberAccess((MemberExpression)exp, sb, parentExp);

                case ExpressionType.Call:
                    // Method call
                    return ProcessMethodCall((MethodCallExpression)exp, sb);

                case ExpressionType.Lambda:
                    // Lambda expression
                    return ProcessLambda((LambdaExpression)exp, sb);

                case ExpressionType.New:
                    // New expression
                    return ProcessNew((NewExpression)exp, sb);

                case ExpressionType.NewArrayInit:
                case ExpressionType.NewArrayBounds:
                    // Array expression
                    return ProcessNewArray((NewArrayExpression)exp, sb);

                case ExpressionType.Invoke:
                    // Invocation
                    return ProcessInvocation((InvocationExpression)exp, sb);

                case ExpressionType.MemberInit:
                    // Member initialization
                    return ProcessMemberInit((MemberInitExpression)exp, sb);

                case ExpressionType.ListInit:
                    // List initialization
                    return ProcessListInit((ListInitExpression)exp, sb);

                default:
                    // Not supported
                    throw new NotSupportedException("[AbstractQueryProvider.ProcessExpression]: Expression type '" + exp.NodeType + "' is not supported.");
            }
        }


        /// <summary>
        /// Returns true if the given type is an unary expression
        /// </summary>
        /// <param name="type">Type to check</param>
        protected bool IsUnary(ExpressionType type)
        {
            switch (type)
            {
                case ExpressionType.Negate:
                case ExpressionType.NegateChecked:
                case ExpressionType.Not:
                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
                case ExpressionType.ArrayLength:
                case ExpressionType.Quote:
                case ExpressionType.TypeAs:
                    return true;
            }

            return false;
        }


        /// <summary>
        /// Returns true if the given type is an unary expression
        /// </summary>
        /// <param name="type">Type to check</param>
        protected bool IsBinaryOperator(ExpressionType type)
        {
            return IsLogicalBinaryOperator(type) || IsComparisonBinaryOperator(type) || IsComputeBinaryOperator(type);
        }


        /// <summary>
        /// Returns true if the given type is an unary expression
        /// </summary>
        /// <param name="type">Type to check</param>
        protected bool IsComputeBinaryOperator(ExpressionType type)
        {
            switch (type)
            {
                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked:
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                case ExpressionType.Divide:
                case ExpressionType.Modulo:
                case ExpressionType.Coalesce:
                case ExpressionType.ArrayIndex:
                case ExpressionType.RightShift:
                case ExpressionType.LeftShift:
                    return true;
            }

            return false;
        }


        /// <summary>
        /// Returns true if the given type is an unary expression
        /// </summary>
        /// <param name="type">Type to check</param>
        protected bool IsLogicalBinaryOperator(ExpressionType type)
        {
            switch (type)
            {
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                case ExpressionType.ExclusiveOr:
                    return true;
            }

            return false;
        }


        /// <summary>
        /// Returns true if the given type is an unary expression
        /// </summary>
        /// <param name="type">Type to check</param>
        protected bool IsComparisonBinaryOperator(ExpressionType type)
        {
            switch (type)
            {
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                    return true;
            }

            return false;
        }


        /// <summary>
        /// Processes the member binding.
        /// </summary>
        /// <param name="binding">Binding to process</param>
        /// <param name="sb">ExpressionBuilderSettings for processing of children</param>
        /// <param name="parentExp">Parent expression</param>
        protected virtual
        MemberBinding ProcessBinding(MemberBinding binding, ExpressionBuilderSettings sb, Expression parentExp)
        {
            switch (binding.BindingType)
            {
                case MemberBindingType.Assignment:
                    return ProcessMemberAssignment((MemberAssignment)binding, sb, parentExp);

                case MemberBindingType.MemberBinding:
                    return ProcessMemberMemberBinding((MemberMemberBinding)binding, sb, parentExp);

                case MemberBindingType.ListBinding:
                    return ProcessMemberListBinding((MemberListBinding)binding, sb, parentExp);

                default:
                    // Not supported
                    throw new NotSupportedException("[AbstractQueryProvider.ProcessBinding]: Binding type '" + binding.BindingType + "' is not supported.");
            }
        }


        /// <summary>
        /// Processes the element initializer.
        /// </summary>
        /// <param name="initializer">Initializer to process</param>
        /// <param name="sb">ExpressionBuilderSettings for processing of children</param>
        /// <param name="parentExp">Parent expression</param>
        protected virtual ElementInit ProcessElementInitializer(ElementInit initializer, ExpressionBuilderSettings sb, Expression parentExp)
        {
            var arguments = ProcessExpressionList(initializer.Arguments, sb, parentExp);
            if (arguments != initializer.Arguments)
            {
                return Expression.ElementInit(initializer.AddMethod, arguments);
            }

            return initializer;
        }


        /// <summary>
        /// Processes the unary expression.
        /// </summary>
        /// <param name="u">Expression</param>
        /// <param name="sb">ExpressionBuilderSettings for processing of children</param>
        protected virtual Expression ProcessUnary(UnaryExpression u, ExpressionBuilderSettings sb)
        {
            var operand = ProcessExpression(u.Operand, sb, u);
            if (operand != u.Operand)
            {
                return Expression.MakeUnary(u.NodeType, operand, u.Type, u.Method);
            }

            return u;
        }


        /// <summary>
        /// Processes the binary expression.
        /// </summary>
        /// <param name="b">Expression</param>
        /// <param name="sb">ExpressionBuilderSettings for processing of children</param>
        protected virtual Expression ProcessBinary(BinaryExpression b, ExpressionBuilderSettings sb)
        {
            // Process all parts
            var left = ProcessExpression(b.Left, sb, b);
            var right = ProcessExpression(b.Right, sb, b);
            var conversion = ProcessExpression(b.Conversion, sb, b);

            if ((left != b.Left) || (right != b.Right) || (conversion != b.Conversion))
            {
                if (b.NodeType == ExpressionType.Coalesce)
                {
                    // Make coalescing operation
                    return Expression.Coalesce(left, right, conversion as LambdaExpression);
                }
                else
                {
                    // Make expression binary
                    return Expression.MakeBinary(b.NodeType, left, right, b.IsLiftedToNull, b.Method);
                }
            }

            return b;
        }


        /// <summary>
        /// Processes the type IDs.
        /// </summary>
        /// <param name="b">Expression</param>
        /// <param name="sb">ExpressionBuilderSettings for processing of children</param>
        protected virtual Expression ProcessTypeIs(TypeBinaryExpression b, ExpressionBuilderSettings sb)
        {
            var expr = ProcessExpression(b.Expression, sb, b);
            if (expr != b.Expression)
            {
                return Expression.TypeIs(expr, b.TypeOperand);
            }

            return b;
        }


        /// <summary>
        /// Constant expression.
        /// </summary>
        /// <param name="c">Expression</param>
        /// <param name="sb">ExpressionBuilderSettings for processing of children</param>
        /// <param name="parentExp">Parent expression</param>
        protected virtual Expression ProcessConstant(ConstantExpression c, ExpressionBuilderSettings sb, Expression parentExp)
        {
            return c;
        }


        /// <summary>
        /// Processes the conditional expression.
        /// </summary>
        /// <param name="c">Expression</param>
        /// <param name="sb">ExpressionBuilderSettings for processing of children</param>
        protected virtual Expression ProcessConditional(ConditionalExpression c, ExpressionBuilderSettings sb)
        {
            // Process all parts
            var test = ProcessExpression(c.Test, sb, c);
            var ifTrue = ProcessExpression(c.IfTrue, sb, c);
            var ifFalse = ProcessExpression(c.IfFalse, sb, c);

            if ((test != c.Test) || (ifTrue != c.IfTrue) || (ifFalse != c.IfFalse))
            {
                return Expression.Condition(test, ifTrue, ifFalse);
            }
            return c;
        }


        /// <summary>
        /// Processes the parameter.
        /// </summary>
        /// <param name="p">Expression</param>
        /// <param name="sb">ExpressionBuilderSettings for processing of children</param>
        protected virtual Expression ProcessParameter(ParameterExpression p, ExpressionBuilderSettings sb)
        {
            return p;
        }


        /// <summary>
        /// Processes the member access.
        /// </summary>
        /// <param name="m">Expression</param>
        /// <param name="sb">ExpressionBuilderSettings for processing of children</param>
        /// <param name="parentExp">Parent expression</param>
        protected virtual Expression ProcessMemberAccess(MemberExpression m, ExpressionBuilderSettings sb, Expression parentExp)
        {
            var exp = ProcessExpression(m.Expression, sb, parentExp);
            if (exp != m.Expression)
            {
                return Expression.MakeMemberAccess(exp, m.Member);
            }

            return m;
        }


        /// <summary>
        /// Processes the method call.
        /// </summary>
        /// <param name="m">Expression</param>
        /// <param name="sb">ExpressionBuilderSettings for processing of children</param>
        protected virtual Expression ProcessMethodCall(MethodCallExpression m, ExpressionBuilderSettings sb)
        {
            // Prepare the arguments
            var obj = ProcessExpression(m.Object, sb, m);
            var args = ProcessExpressionList(m.Arguments, sb, m);

            if ((obj != m.Object) || (args != m.Arguments))
            {
                return Expression.Call(obj, m.Method, args);
            }
            return m;
        }


        /// <summary>
        /// Processes the list of expressions.
        /// </summary>
        /// <param name="original">List</param>
        /// <param name="sb">ExpressionBuilderSettings for processing of children</param>
        /// <param name="parentExp">Parent expression</param>
        protected virtual ReadOnlyCollection<Expression> ProcessExpressionList(ReadOnlyCollection<Expression> original, ExpressionBuilderSettings sb, Expression parentExp)
        {
            List<Expression> list = null;

            for (int i = 0, n = original.Count; i < n; i++)
            {
                // Process each element as expression
                var p = ProcessExpression(original[i], sb, parentExp);
                if (list != null)
                {
                    list.Add(p);
                }
                else if (p != original[i])
                {
                    // Build new list if something changed
                    list = new List<Expression>(n);
                    for (int j = 0; j < i; j++)
                    {
                        list.Add(original[j]);
                    }
                    list.Add(p);
                }
            }

            if (list != null)
            {
                return list.AsReadOnly();
            }

            return original;
        }


        /// <summary>
        /// Processes the member assignment.
        /// </summary>
        /// <param name="assignment">Assignment</param>
        /// <param name="sb">ExpressionBuilderSettings for processing of children</param>
        /// <param name="parentExp">Parent expression</param>
        protected virtual MemberAssignment ProcessMemberAssignment(MemberAssignment assignment, ExpressionBuilderSettings sb, Expression parentExp)
        {
            var e = ProcessExpression(assignment.Expression, sb, parentExp);
            if (e != assignment.Expression)
            {
                return Expression.Bind(assignment.Member, e);
            }
            return assignment;
        }


        /// <summary>
        /// Processes the member member binding.
        /// </summary>
        /// <param name="binding">Binding</param>
        /// <param name="sb">ExpressionBuilderSettings for processing of children</param>
        /// <param name="parentExp">Parent expression</param>
        protected virtual MemberMemberBinding ProcessMemberMemberBinding(MemberMemberBinding binding, ExpressionBuilderSettings sb, Expression parentExp)
        {
            var bindings = ProcessBindingList(binding.Bindings, sb, parentExp);
            if (bindings != binding.Bindings)
            {
                return Expression.MemberBind(binding.Member, bindings);
            }
            return binding;
        }


        /// <summary>
        /// Processes the member list binding.
        /// </summary>
        /// <param name="binding">Binding</param>
        /// <param name="sb">ExpressionBuilderSettings for processing of children</param>
        /// <param name="parentExp">Parent expression</param>
        protected virtual MemberListBinding ProcessMemberListBinding(MemberListBinding binding, ExpressionBuilderSettings sb, Expression parentExp)
        {
            var initializers = ProcessElementInitializerList(binding.Initializers, sb, parentExp);
            if (initializers != binding.Initializers)
            {
                return Expression.ListBind(binding.Member, initializers);
            }
            return binding;
        }


        /// <summary>
        /// Processes the binding list.
        /// </summary>
        /// <param name="original">List</param>
        /// <param name="sb">ExpressionBuilderSettings for processing of children</param>
        /// <param name="parentExp">Parent expression</param>
        protected virtual IEnumerable<MemberBinding> ProcessBindingList(ReadOnlyCollection<MemberBinding> original, ExpressionBuilderSettings sb, Expression parentExp)
        {
            List<MemberBinding> list = null;
            for (int i = 0, n = original.Count; i < n; i++)
            {
                // Process each item as binding
                var b = ProcessBinding(original[i], sb, parentExp);
                if (list != null)
                {
                    list.Add(b);
                }
                else if (b != original[i])
                {
                    // Build new list if something changed
                    list = new List<MemberBinding>(n);
                    for (int j = 0; j < i; j++)
                    {
                        list.Add(original[j]);
                    }
                    list.Add(b);
                }
            }
            if (list != null)
            {
                return list;
            }
            return original;
        }


        /// <summary>
        /// Process list of element initializers.
        /// </summary>
        /// <param name="original">List</param>
        /// <param name="sb">ExpressionBuilderSettings for processing of children</param>
        /// <param name="parentExp">Parent expression</param>
        protected virtual IEnumerable<ElementInit> ProcessElementInitializerList(ReadOnlyCollection<ElementInit> original, ExpressionBuilderSettings sb, Expression parentExp)
        {
            List<ElementInit> list = null;
            for (int i = 0, n = original.Count; i < n; i++)
            {
                // Process all items as initializers
                var init = ProcessElementInitializer(original[i], sb, parentExp);
                if (list != null)
                {
                    list.Add(init);
                }
                else if (init != original[i])
                {
                    // If something changed, create new list
                    list = new List<ElementInit>(n);
                    for (int j = 0; j < i; j++)
                    {
                        list.Add(original[j]);
                    }
                    list.Add(init);
                }
            }
            if (list != null)
            {
                return list;
            }
            return original;
        }


        /// <summary>
        /// Processes the lambda expression.
        /// </summary>
        /// <param name="lambda">Expression</param>
        /// <param name="sb">ExpressionBuilderSettings for processing of children</param>
        protected virtual Expression ProcessLambda(LambdaExpression lambda, ExpressionBuilderSettings sb)
        {
            var body = ProcessExpression(lambda.Body, sb, lambda);
            if (body != lambda.Body)
            {
                return Expression.Lambda(lambda.Type, body, lambda.Parameters);
            }
            return lambda;
        }


        /// <summary>
        /// Processes the new expression.
        /// </summary>
        /// <param name="nex">Expression</param>
        /// <param name="sb">ExpressionBuilderSettings for processing of children</param>
        protected virtual NewExpression ProcessNew(NewExpression nex, ExpressionBuilderSettings sb)
        {
            var args = ProcessExpressionList(nex.Arguments, sb, nex);
            if (args != nex.Arguments)
            {
                // With members
                return Expression.New(nex.Constructor, args, nex.Members);
            }
            return nex;
        }


        /// <summary>
        /// Processes initialization of the member.
        /// </summary>
        /// <param name="init">Expression</param>
        /// <param name="sb">ExpressionBuilderSettings for processing of children</param>
        protected virtual Expression ProcessMemberInit(MemberInitExpression init, ExpressionBuilderSettings sb)
        {
            // Prepare the values
            var n = ProcessNew(init.NewExpression, sb);
            var bindings = ProcessBindingList(init.Bindings, sb, init);

            if ((n != init.NewExpression) || (bindings != init.Bindings))
            {
                return Expression.MemberInit(n, bindings);
            }
            return init;
        }


        /// <summary>
        /// Processes the initialization of the list.
        /// </summary>
        /// <param name="init">Expression</param>
        /// <param name="sb">ExpressionBuilderSettings for processing of children</param>
        protected virtual Expression ProcessListInit(ListInitExpression init, ExpressionBuilderSettings sb)
        {
            // Prepare the values
            var n = ProcessNew(init.NewExpression, sb);
            var initializers = ProcessElementInitializerList(init.Initializers, sb, init);

            if ((n != init.NewExpression) || (initializers != init.Initializers))
            {
                return Expression.ListInit(n, initializers);
            }
            return init;
        }


        /// <summary>
        /// Processes the array.
        /// </summary>
        /// <param name="na">Array</param>
        /// <param name="sb">ExpressionBuilderSettings for processing of children</param>
        protected virtual Expression ProcessNewArray(NewArrayExpression na, ExpressionBuilderSettings sb)
        {
            var exprs = ProcessExpressionList(na.Expressions, sb, na);
            if (exprs != na.Expressions)
            {
                if (na.NodeType == ExpressionType.NewArrayInit)
                {
                    // Init new array by elements
                    return Expression.NewArrayInit(na.Type.GetElementType(), exprs);
                }
                else
                {
                    // Init new array by size
                    return Expression.NewArrayBounds(na.Type.GetElementType(), exprs);
                }
            }
            return na;
        }


        /// <summary>
        /// Process the invocation of the method.
        /// </summary>
        /// <param name="iv">Expression</param>
        /// <param name="sb">ExpressionBuilderSettings for processing of children</param>
        protected virtual Expression ProcessInvocation(InvocationExpression iv, ExpressionBuilderSettings sb)
        {
            // Prepare the values
            var args = ProcessExpressionList(iv.Arguments, sb, iv);
            var expr = ProcessExpression(iv.Expression, sb, iv);

            if ((args != iv.Arguments) || (expr != iv.Expression))
            {
                return Expression.Invoke(expr, args);
            }
            return iv;
        }

        #endregion
    }
}
