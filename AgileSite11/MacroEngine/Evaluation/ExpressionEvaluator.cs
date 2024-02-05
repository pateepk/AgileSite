using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Base;

namespace CMS.MacroEngine
{
    /// <summary>
    /// Class used for MacroExpression evaluation.
    /// </summary>
    public class ExpressionEvaluator
    {
        #region "Properties"

        /// <summary>
        /// Expression to evaluate.
        /// </summary>
        public MacroExpression Expression
        {
            get;
            protected set;
        }


        /// <summary>
        /// Evaluation context.
        /// </summary>
        public EvaluationContext Context
        {
            get;
            protected set;
        }


        /// <summary>
        /// Returns the expression type.
        /// </summary>
        protected ExpressionType Type
        {
            get
            {
                return Expression.Type;
            }
        }


        /// <summary>
        /// Name of the data member or method call.
        /// </summary>
        protected string Name
        {
            get
            {
                return Expression.Name;
            }
        }


        /// <summary>
        /// Value of the value expression (is null for expression types like method, property, etc.).
        /// </summary>
        protected object Value
        {
            get
            {
                return Expression.Value;
            }
        }


        /// <summary>
        /// Parent expression of the expression.
        /// </summary>
        protected MacroExpression Parent
        {
            get
            {
                return Expression.Parent;
            }
        }


        /// <summary>
        /// Child expressions of the expression.
        /// </summary>
        protected List<MacroExpression> Children
        {
            get
            {
                return Expression.Children;
            }
        }

        #endregion


        #region  "Constructors"

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="expr">Expression to evaluate</param>
        /// <param name="context">Evaluation context</param>
        public ExpressionEvaluator(MacroExpression expr, EvaluationContext context)
        {
            Expression = expr;
            Context = context;
        }

        #endregion


        #region "Evaluation methods"

        /// <summary>
        /// Evaluates the expression and returns the result. Sets the result of security check into MacroEvalParameters object.
        /// </summary>
        public EvaluationResult Evaluate()
        {
            // Load evaluation context from in-line parameters and do final initialization of the context
            UpdateEvaluationContextFromParameters();

            if (!string.IsNullOrEmpty(Context.ResolverName) && !Context.Resolver.ResolverName.EqualsCSafe(Context.ResolverName, true))
            {
                // The name of the resolver which is used for resolving the macro does not correspond to the required resolver for this macro, abort the evaluation
                return new EvaluationResult
                {
                    Result = null,
                    Match = false,
                    Skipped = true,
                    SecurityPassed = true,
                    ContextUsed = Context
                };
            }

            // Evaluate the expression
            bool match = true;
            bool securityPassed = true;

            using (new CMSActionContext
            {
                // Disable license error when macro evaluation tries to load objects that are not included in current license
                EmptyDataForInvalidLicense = true,

                // Set evaluation context culture to the thread for macro methods
                ThreadCulture = Context.CultureInfo
            })
            {
                object result = EvaluateInternal(ref match, ref securityPassed);

                // Handle special results
                if (result is MacroCommand)
                {
                    MacroCommand command = (MacroCommand)result;
                    if (command.Name == "return")
                    {
                        result = command.Value ?? Context.ConsoleOutput;
                    }
                }

                return new EvaluationResult
                {
                    Result = result,
                    Match = match,
                    Skipped = false,
                    SecurityPassed = securityPassed,
                    ContextUsed = Context
                };
            }
        }


        /// <summary>
        /// Checks whether the evaluation time did not exceed the specified limit.
        /// </summary>
        protected void CheckForTimeout()
        {
            if (!SystemContext.IsWebProjectDebug && Context.CheckForTimeout())
            {
                // Abort evaluation
                throw new EvaluationTimeoutException(Context.OriginalExpression, Context.TimeoutOverTime);
            }
        }


        /// <summary>
        /// Reads all the in-line macro parameters and modifies the default evaluation context accordingly.
        /// </summary>
        protected void UpdateEvaluationContextFromParameters()
        {
            // Set the evaluation started (for measuring time to abort too demanding expressions)
            Context.EvaluationStarted = DateTime.Now;

            // Load evaluation context modifiers from in-line parameters
            if (Expression.Parameters != null)
            {
                var parameters = new EvaluationParameters(Expression.Parameters);
                Context.LoadParameters(parameters);
            }
        }


        /// <summary>
        /// Evaluates the child expression (uses the same context, does not create a child context).
        /// </summary>
        /// <param name="childExpr">Child expression to evaluate</param>
        /// <param name="match">Returns true if all data members used were known (could have been null, but must have been registered)</param>
        /// <param name="securityPassed">Returns true if the security of the evaluated macro was OK (both integrity and permissions)</param>
        protected object EvaluateChild(MacroExpression childExpr, ref bool match, ref bool securityPassed)
        {
            var evaluator = new ExpressionEvaluator(childExpr, Context);

            return evaluator.EvaluateInternal(ref match, ref securityPassed);
        }


        /// <summary>
        /// Evaluates the expression and returns the result.
        /// </summary>
        /// <param name="match">Returns true if all data members used were known (could have been null, but must have been registered)</param>
        /// <param name="securityPassed">Returns true if the security of the evaluated macro was OK (both integrity and permissions)</param>
        protected object EvaluateInternal(ref bool match, ref bool securityPassed)
        {
            // Check for the evaluation timeout every time any subexpression is evaluated.
            CheckForTimeout();

            object result = null;
            DataRow drLogItem = null;

            // Do not log simple values and compound expression root
            bool logItem = (Context.DetailedDebug) && ((Type == ExpressionType.Property) || IsMethodWithName(";") || (Type == ExpressionType.DataMember));
            if (logItem)
            {
                drLogItem = MacroDebug.ReserveMacroLogItem();
            }

            // Set evaluation started
            DateTime evaluationStarted = Context.EvaluationStarted;

            switch (Type)
            {
                case ExpressionType.MethodCall:
                    {
                        switch (Name.ToLowerCSafe())
                        {
                            case "=>":
                                result = EvaluateLambdaExpr();
                                break;

                            case "println":
                            case "print":
                                result = PrintToConsole(ref match, ref securityPassed);
                                break;

                            case "if":
                                result = EvaluateCondition(ref match, ref securityPassed, drLogItem, evaluationStarted);
                                break;

                            case "while":
                                result = EvaluateWhileLoop(ref match, ref securityPassed);
                                break;

                            case "for":
                                result = EvaluateForLoop(ref match, ref securityPassed);
                                break;

                            case "foreach":
                                result = EvaluateForEachLoop(ref match, ref securityPassed);
                                break;

                            case ";":
                                result = EvaluateCompoundExpression(ref match, ref securityPassed);
                                break;

                            case "??":
                                result = EvaluateIsNullOperator(ref match, ref securityPassed);
                                break;

                            case "=":
                            case "+=":
                            case "-=":
                            case "*=":
                            case "/=":
                            case "%=":
                            case "&=":
                            case "|=":
                            case "^=":
                            case "<<=":
                            case ">>=":
                            case "++":
                            case "--":
                            case "p++":
                            case "p--":
                                result = EvaluateAssignment(ref match, ref securityPassed);
                                break;

                            case "[]":
                                result = EvaluateIndexer(ref match, ref securityPassed);
                                break;

                            case "?":
                                result = EvaluateTernaryOperator(ref match, ref securityPassed);
                                break;

                            case "equals":
                            case "==":
                                result = EvaluateEquality(ref match, ref securityPassed);
                                break;

                            case "notequals":
                            case "!=":
                                result = !EvaluateEquality(ref match, ref securityPassed);
                                break;

                            // Default method execution
                            default:
                                result = EvaluateMethodCall(ref match, ref securityPassed);
                                break;
                        }
                        break;
                    }

                case ExpressionType.Command:
                    result = EvaluateCommand(ref match, ref securityPassed);
                    break;

                case ExpressionType.DataMember:
                    result = EvaluateDataMember(ref match);
                    break;

                case ExpressionType.Property:
                    result = EvaluateProperty(ref match, ref securityPassed);
                    break;

                case ExpressionType.Value:
                    // Simple constant, just return the value
                    result = Value;
                    break;

                case ExpressionType.SubExpression:
                case ExpressionType.Block:
                    // Block of expressions or subexpression (has to have exactly one child - the inner expression)
                    result = EvaluateChild(Children[0], ref match, ref securityPassed);
                    break;

                case ExpressionType.Empty:
                    // Result of an empty command is null
                    break;
            }

            // If in a sub-expression check for timeout after evaluation
            if (Parent != null)
            {
                CheckForTimeout();
            }

            // Ensure correct result of the macro - if it is the root of the macro, check the console output
            object consoleOutput = Context.ConsoleOutput;
            if ((Parent == null) && (consoleOutput != null) && !(result is MacroCommand))
            {
                result = consoleOutput;
            }

            // Log debug information
            if (logItem && (drLogItem != null))
            {
                // If statement was logged separately - special case
                if (!IsMethodWithName("if"))
                {
                    object itemToLog = (result ?? "null");
                    MacroDebug.SetLogItemData(drLogItem, Expression.ToString(true), itemToLog, MacroDebug.CurrentLogIndent + GetDebugIndent(), DateTime.Now.Subtract(evaluationStarted).TotalSeconds);
                }
            }

            // Check security
            var objectToCheck = result as IMacroSecurityCheckPermissions;
            if (objectToCheck != null)
            {
                bool checkCollections = (result is IInfoObjectCollection);

                if (!MacroSecurityProcessor.CheckSecurity(Context, objectToCheck.GetObjectToCheck(), checkCollections))
                {
                    securityPassed = false;
                }
            }

            return MacroResolver.EncapsulateObject(result, Context);
        }


        /// <summary>
        /// Evaluates general method call.
        /// </summary>
        /// <param name="match">Determines whether all the necessary objects for evaluation were present</param>
        /// <param name="securityPassed">Returns true if the security of the evaluated macro was OK (both integrity and permissions)</param>
        private object EvaluateMethodCall(ref bool match, ref bool securityPassed)
        {
            // First, check if there is a lambda expression of that name
            var lambda = Context.Resolver.GetDynamicParameter(Name);

            var lambdaExp = lambda.Result as MacroLambdaExpression;
            if (lambdaExp != null)
            {
                if (Children.Count != lambdaExp.Variables.Count)
                {
                    throw new EvaluationException(Context.OriginalExpression, "Lambda expression " + Name + " doesn't take " + Children.Count + " parameters.");
                }

                return EvaluateLambdaMethod(ref match, ref securityPassed, lambdaExp);
            }

            // Get the first child to get the list of supported methods
            IMacroMethod method = null;
            object firstChild = null;

            if (Children.Count > 0)
            {
                // Prefer method from system name space with first parameter lazy if found (solves the Cache method problem)
                method = MacroMethodContainer.GetMethodForObject(SystemNamespace.Instance, Name);
                if ((method == null) || !FirstParameterLazy(method))
                {
                    firstChild = EvaluateChild(Children[0], ref match, ref securityPassed);
                    method = MacroMethodContainer.GetMethodForObject(firstChild, Name);
                }
            }

            // No specific method found, look into the default name spaces
            if (method == null)
            {
                var innerSources = Context.GetInnerSources();

                foreach (var item in innerSources)
                {
                    method = MacroMethodContainer.GetMethodForObject(item, Name);
                    if (method != null)
                    {
                        break;
                    }
                }
            }

            // No specific method found, look into the static storage (backward compatibility)
            if (method == null)
            {
                method = MacroMethods.GetMethod(Name);
            }

            if (method == null)
            {
                // Try simple method registrations
                if (firstChild != null)
                {
                    var prop = firstChild.GetType().StaticProperty<IMacroMethod>(Name);
                    if (prop != null)
                    {
                        method = prop.Value;
                    }
                }
                else
                {
                    // Type of the first child is not known, look for the first method of this name
                    var extensions = Extension<MacroMethodContainer>.GetExtensions();

                    foreach (var ext in extensions)
                    {
                        var container = ext.Value;

                        method = container.GetMethod(Name);
                        if (method != null)
                        {
                            break;
                        }
                    }
                }
            }

            if (method == null)
            {
                ThrowMethodNotFound(firstChild);
            }

            // Prepare method parameters
            var parameters = PrepareParameters(method, firstChild, ref match, ref securityPassed);

            return ExecuteMethod(method, parameters);
        }


        /// <summary>
        /// Returns true if the given method has a first parameter that is lazy (AsExpression)
        /// </summary>
        /// <param name="method">Method to check</param>
        private bool FirstParameterLazy(IMacroMethod method)
        {
            var parameters = method.Parameters;

            if ((parameters == null) || (parameters.Count < 1))
            {
                return false;
            }

            return parameters[0].AsExpression;
        }


        /// <summary>
        /// Prepares parameters for the given method
        /// </summary>
        /// <param name="method">Method to execute</param>
        /// <param name="firstChild">First method child</param>
        /// <param name="match">Returns true if all data members used were known (could have been null, but must have been registered)</param>
        /// <param name="securityPassed">Returns true if the security of the evaluated macro was OK (both integrity and permissions)</param>
        private IEnumerable<object> PrepareParameters(IMacroMethod method, object firstChild, ref bool match, ref bool securityPassed)
        {
            // Handle special parameters
            var specialParameters = method.SpecialParameters;

            int specialParametersCount = ((specialParameters == null) ? 0 : specialParameters.Length);

            var parameters = new List<object>();

            if (specialParameters != null)
            {
                // Add special parameters as the first parameters
                for (int i = 0; i < specialParametersCount; i++)
                {
                    // Get the special parameter as data member
                    object specParameter;

                    if (specialParameters[i].ToLowerCSafe() == "childresolver")
                    {
                        specParameter = Context.Resolver;
                    }
                    else
                    {
                        var r = Context.Resolver.CheckDataSources(specialParameters[i], Context);

                        specParameter = r.Result;
                    }

                    parameters.Add(specParameter);
                }
            }

            int paramIndex = 0;

            // Add all other parameters
            foreach (var child in Children)
            {
                object addParameter;

                // Skip name space parameters
                if ((method.Parameters.Count > paramIndex) && method.Parameters[paramIndex].AsExpression)
                {
                    // Single parameter as expression (not evaluated, passed as is)
                    if (IsNamespace(child))
                    {
                        firstChild = null;
                        continue;
                    }

                    addParameter = child;
                }
                else
                {
                    if ((paramIndex == 0) && (firstChild != null))
                    {
                        // Do not compute first parameter again
                        addParameter = firstChild;
                    }
                    else
                    {
                        // Evaluate the child
                        addParameter = EvaluateChild(child, ref match, ref securityPassed);
                    }
                }

                // Skip name space as the first child, never use it as a first parameter
                if ((paramIndex == 0) && (addParameter is IMacroNamespace))
                {
                    firstChild = null;
                    continue;
                }

                parameters.Add(addParameter);

                paramIndex++;
            }

            return parameters;
        }


        /// <summary>
        /// Returns true if the given expression is a name space
        /// </summary>
        /// <param name="expr">Expression to check</param>
        private bool IsNamespace(MacroExpression expr)
        {
            if (expr.Type != ExpressionType.DataMember)
            {
                return false;
            }

            // Get the named source data
            var namedSource = Context.Resolver.GetNamedSourceData(expr.Name);
            if (namedSource is IMacroNamespace)
            {
                return true;
            }

            return false;
        }


        /// <summary>
        /// Throws an exception that the method was not found
        /// </summary>
        /// <param name="callingObject">Object calling the method</param>
        private void ThrowMethodNotFound(object callingObject)
        {
            if (callingObject != null)
            {
                throw new EvaluationException(Context.OriginalExpression, String.Format("Method '{0}' for object of type '{1}' not found, please check the macro or the method declaration.", Name, callingObject.GetType().FullName));
            }

            throw new EvaluationException(Context.OriginalExpression, String.Format("Method '{0}' not found, please check the macro or the method declaration.", Name));
        }


        /// <summary>
        /// Executes the given macro method
        /// </summary>
        /// <param name="method">Method to execute</param>
        /// <param name="parameters">Method parameters</param>
        private object ExecuteMethod(IMacroMethod method, IEnumerable<object> parameters)
        {
            // Execute method
            try
            {
                // Try to execute method with context
                var macroMethod = method as MacroMethod;
                if (macroMethod != null)
                {
                    return macroMethod.ExecuteMethod(Context.CreateChildContext(false), parameters.ToArray());
                }

                // Older version for methods without context
                return method.ExecuteMethod(parameters.ToArray());
            }
            catch (NotSupportedException ex)
            {
                throw new EvaluationException(Context.OriginalExpression,(String.IsNullOrEmpty(ex.Message) ? "Method " + Name + " has invalid number of arguments." : ex.Message), ex);
            }
        }


        /// <summary>
        /// Evaluates call of given lambda expression.
        /// </summary>
        /// <param name="match">Determines whether all the necessary objects for evaluation were present</param>
        /// <param name="securityPassed">Returns true if the security of the evaluated macro was OK (both integrity and permissions)</param>
        /// <param name="lambdaExp">Lambda expression to evaluate</param>
        private object EvaluateLambdaMethod(ref bool match, ref bool securityPassed, MacroLambdaExpression lambdaExp)
        {
            // Initialize new resolver to evaluate the lambda expression with given parameters
            var lambdaContext = Context.CreateChildContext();
            var lambdaResolver = lambdaContext.Resolver;

            int j = 0;

            // Prepare parameters for the lambda method
            foreach (string childVar in lambdaExp.Variables)
            {
                lambdaResolver.SetDynamicParameter(childVar, EvaluateChild(Children[j++], ref match, ref securityPassed));
            }

            // Evaluate the method
            var evalResult = new ExpressionEvaluator(lambdaExp.Expression, lambdaContext).Evaluate();
            if (!evalResult.Match)
            {
                match = false;
            }

            if (!evalResult.SecurityPassed)
            {
                securityPassed = false;
            }

            return evalResult.Result;
        }


        /// <summary>
        /// Evaluates equality operators.
        /// </summary>
        /// <param name="match">Determines whether all the necessary objects for evaluation were present</param>
        /// <param name="securityPassed">Returns true if the security of the evaluated macro was OK (both integrity and permissions)</param>
        private bool EvaluateEquality(ref bool match, ref bool securityPassed)
        {
            object objToCompare1 = EvaluateChild(Children[0], ref match, ref securityPassed);
            object objToCompare2 = EvaluateChild(Children[1], ref match, ref securityPassed);
            return IsEqual(objToCompare1, objToCompare2, Context);
        }


        /// <summary>
        /// Evaluates compound expressions (separated with semicolon).
        /// </summary>
        /// <param name="match">Determines whether all the necessary objects for evaluation were present</param>
        /// <param name="securityPassed">Returns true if the security of the evaluated macro was OK (both integrity and permissions)</param>
        private object EvaluateCompoundExpression(ref bool match, ref bool securityPassed)
        {
            object result = null;
            for (int i = 0; i < Children.Count; i++)
            {
                object obj = EvaluateChild(Children[i], ref match, ref securityPassed);
                if (obj is MacroCommand)
                {
                    // If the result is macro command (break, continue, etc.) than it is the immediate result
                    result = obj;
                    break;
                }
                if (i == (Children.Count - 1))
                {
                    // As a result return last child
                    result = obj;
                    break;
                }
            }
            return result;
        }


        /// <summary>
        /// Evaluates for each loop.
        /// </summary>
        /// <param name="match">Determines whether all the necessary objects for evaluation were present</param>
        /// <param name="securityPassed">Returns true if the security of the evaluated macro was OK (both integrity and permissions)</param>
        private object EvaluateForEachLoop(ref bool match, ref bool securityPassed)
        {
            MacroExpression loopBody = Children[1];
            MacroExpression inOperator = Children[0];
            MacroExpression forEachVar = inOperator.Children[0];
            MacroExpression enumerable = inOperator.Children[1];

            // Get the enumerable object
            object enumEval = EvaluateChild(enumerable, ref match, ref securityPassed);
            if (!(enumEval is IEnumerable))
            {
                throw new EvaluationException(Expression.ToString(true), "Enumerable object expected within foreach loop.");
            }



            // Save the current value of the used variable (variable used to index elements in the list could be used before
            // To recover its value after the loop, it has to be saved
            EvaluationResult r = Context.Resolver.GetDynamicParameter(forEachVar.Name);
            object lastVarValue = r.Result;

            ArrayList forResult = new ArrayList();
            object result = null;

            IEnumerable enumObj = (IEnumerable)enumEval;

            foreach (object o in enumObj)
            {
                // Save the variable
                ProcessAssignment("=", forEachVar.Name, o);

                // Check the special commands
                object bodyResult = EvaluateChild(loopBody, ref match, ref securityPassed);
                if (bodyResult is MacroCommand)
                {
                    MacroCommand command = (MacroCommand)bodyResult;
                    if (command.Name == "continue")
                    {
                        continue;
                    }

                    if (command.Name == "break")
                    {
                        break;
                    }

                    if (command.Name == "return")
                    {
                        // Propagate return further
                        result = command;
                        break;
                    }
                }

                // Process the body
                forResult.Add(bodyResult);
            }

            // Result of the for statement is list of results of the for body
            if (result == null)
            {
                result = forResult;
            }

            // Recover the value of the variable
            Context.Resolver.SetDynamicParameter(forEachVar.Name, lastVarValue);

            return result;
        }


        /// <summary>
        /// Evaluates for loop.
        /// </summary>
        /// <param name="match">Determines whether all the necessary objects for evaluation were present</param>
        /// <param name="securityPassed">Returns true if the security of the evaluated macro was OK (both integrity and permissions)</param>
        private object EvaluateForLoop(ref bool match, ref bool securityPassed)
        {
            var forLoopBody = Children[1];
            var forParameters = Children[0];
            var initialization = forParameters.Children[0];
            var condition = forParameters.Children[1];
            var forExpression = forParameters.Children[2];

            // Process the initialization command
            EvaluateChild(initialization, ref match, ref securityPassed);

            bool forCondition = GetBoolConditionResult(EvaluateChild(condition, ref match, ref securityPassed));

            var forResult = new ArrayList();
            object result = null;

            // Process the if statement
            while (forCondition)
            {
                // Check the special commands
                object bodyResult = EvaluateChild(forLoopBody, ref match, ref securityPassed);
                if (bodyResult is MacroCommand)
                {
                    MacroCommand command = (MacroCommand)bodyResult;
                    if (command.Name == "continue")
                    {
                        // Evaluate for expression (i++ etc.)
                        EvaluateChild(forExpression, ref match, ref securityPassed);

                        // Reevaluate the condition
                        forCondition = GetBoolConditionResult(EvaluateChild(condition, ref match, ref securityPassed));

                        continue;
                    }

                    if (command.Name == "break")
                    {
                        break;
                    }

                    if (command.Name == "return")
                    {
                        // Propagate return further
                        result = command;
                        break;
                    }
                }

                // Add the result
                forResult.Add(bodyResult);

                // Evaluate for expression (i++ etc.)
                EvaluateChild(forExpression, ref match, ref securityPassed);

                // Reevaluate the condition
                forCondition = GetBoolConditionResult(EvaluateChild(condition, ref match, ref securityPassed));
            }

            // Result of the for statement is list of results of the for body
            return result ?? forResult;
        }


        /// <summary>
        /// Evaluates ?? operator.
        /// </summary>
        /// <param name="match">Determines whether all the necessary objects for evaluation were present</param>
        /// <param name="securityPassed">Returns true if the security of the evaluated macro was OK (both integrity and permissions)</param>
        private object EvaluateIsNullOperator(ref bool match, ref bool securityPassed)
        {
            return EvaluateChild(Children[0], ref match, ref securityPassed) ?? EvaluateChild(Children[1], ref match, ref securityPassed);
        }


        /// <summary>
        /// Evaluates ternary operator.
        /// </summary>
        /// <param name="match">Determines whether all the necessary objects for evaluation were present</param>
        /// <param name="securityPassed">Returns true if the security of the evaluated macro was OK (both integrity and permissions)</param>
        private object EvaluateTernaryOperator(ref bool match, ref bool securityPassed)
        {
            bool resultBool = ValidationHelper.GetBoolean(EvaluateChild(Children[0], ref match, ref securityPassed), false);
            List<MacroExpression> children = Children[1].Children;

            if (resultBool)
            {
                return EvaluateChild(children[0], ref match, ref securityPassed);
            }
            else
            {
                return EvaluateChild(children[1], ref match, ref securityPassed);
            }
        }


        /// <summary>
        /// Evaluates indexer operator [].
        /// </summary>
        /// <param name="match">Determines whether all the necessary objects for evaluation were present</param>
        /// <param name="securityPassed">Returns true if the security of the evaluated macro was OK (both integrity and permissions)</param>
        private object EvaluateIndexer(ref bool match, ref bool securityPassed)
        {
            object parentObject = EvaluateChild(Children[0], ref match, ref securityPassed);
            object indexingValue = EvaluateChild(Children[1], ref match, ref securityPassed);
            if ((parentObject == null) || (indexingValue == null))
            {
                return null;
            }
            else
            {
                EvaluationResult r;

                if (ValidationHelper.IsInteger(indexingValue))
                {
                    // Indexing by integer
                    int index = ValidationHelper.GetInteger(indexingValue, 0);
                    r = Context.Resolver.GetObjectValue(parentObject, index, Context);
                }
                else
                {
                    // Indexing by text value
                    string index = indexingValue.ToString();
                    r = Context.Resolver.GetObjectValue(parentObject, index, Context);
                }

                if (r != null)
                {
                    // TreeNode has to be explicitly checked when accessed because it has individual ACLs and also objects
                    // from combined collections have to be checked as the permissions cannot be checked for the whole collection
                    if (r.Result is ITreeNode || parentObject is CombinedInfoObjectCollection)
                    {
                        if (!MacroSecurityProcessor.CheckSecurity(Context, r.Result, false))
                        {
                            securityPassed = false;
                        }
                    }

                    return r.Result;
                }

                return null;
            }
        }


        /// <summary>
        /// Evaluates assignment operators (+=, =, -=, etc.).
        /// </summary>
        /// <param name="match">Determines whether all the necessary objects for evaluation were present</param>
        /// <param name="securityPassed">Returns true if the security of the evaluated macro was OK (both integrity and permissions)</param>
        private object EvaluateAssignment(ref bool match, ref bool securityPassed)
        {
            object result = null;

            MacroExpression variableExpr = Children[0];
            if ((Name == "=") && (variableExpr.Type != ExpressionType.DataMember))
            {
                result = EvaluateEquality(ref match, ref securityPassed);
            }
            else
            {
                // Left side is considered as key for NamedDataSource, therefore it has to be in identifier format
                string variableName = variableExpr.Name;

                // Get the value to be assigned
                object rightSideValue = null;
                if (Children.Count > 1)
                {
                    rightSideValue = EvaluateChild(Children[1], ref match, ref securityPassed);
                }

                // New assignment, just overwrite if data source already exists
                result = ProcessAssignment(Name, variableName, rightSideValue);
            }
            return result;
        }


        /// <summary>
        /// Evaluates the while loop.
        /// </summary>
        /// <param name="match">Determines whether all the necessary objects for evaluation were present</param>
        /// <param name="securityPassed">Returns true if the security of the evaluated macro was OK (both integrity and permissions)</param>
        private object EvaluateWhileLoop(ref bool match, ref bool securityPassed)
        {
            bool whileConditionResult = GetBoolConditionResult(EvaluateChild(Children[0], ref match, ref securityPassed));

            // Process the while loop
            ArrayList whileResult = new ArrayList();
            object result = null;

            // Process the if statement
            while (whileConditionResult)
            {
                // Check the special commands
                object bodyResult = EvaluateChild(Children[1], ref match, ref securityPassed);
                if (bodyResult is MacroCommand)
                {
                    MacroCommand command = (MacroCommand)bodyResult;
                    if (command.Name == "continue")
                    {
                        continue;
                    }
                    else if (command.Name == "break")
                    {
                        break;
                    }
                    else if (command.Name == "return")
                    {
                        // Propagate return further
                        result = command;
                        break;
                    }
                }

                whileResult.Add(bodyResult);
                whileConditionResult = GetBoolConditionResult(EvaluateChild(Children[0], ref match, ref securityPassed));
            }

            // Result of the while statement is list of results of the while body
            if (result == null)
            {
                result = whileResult;
            }

            return result;
        }


        /// <summary>
        /// Evaluates the if condition.
        /// </summary>
        /// <param name="match">Determines whether all the necessary objects for evaluation were present</param>
        /// <param name="securityPassed">Returns true if the security of the evaluated macro was OK (both integrity and permissions)</param>
        /// <param name="drLogItem">Data row with debug log data</param>
        /// <param name="evaluationStarted">DateTime when the evaluation was started (Eval method was called)</param>
        private object EvaluateCondition(ref bool match, ref bool securityPassed, DataRow drLogItem, DateTime evaluationStarted)
        {
            object result = null;

            // Evaluate the condition
            bool boolConditionResult = GetBoolConditionResult(EvaluateChild(Children[0], ref match, ref securityPassed));

            string logItemString = "";

            var logItem = (drLogItem != null);

            // Process the if statement
            if (boolConditionResult)
            {
                // Result of the if statement is null if condition doesn't hold or the result of the if body
                result = EvaluateChild(Children[1], ref match, ref securityPassed);

                if (logItem)
                {
                    logItemString = "if (" + Children[0].ToString(true) + ") { ";
                }
            }
            else
            {
                // Evaluate else part
                if (Children.Count == 3)
                {
                    // Else part (we know that Children[2] is the "else" method and it's first child is block of code of that else
                    result = EvaluateChild(Children[2].Children[0], ref match, ref securityPassed);

                    if (logItem)
                    {
                        logItemString = "if (" + Children[0].ToString(true) + ") { ... } else { ";
                    }
                }
                else
                {
                    // No else part, just log the if condition
                    if (logItem)
                    {
                        logItemString = "if (" + Children[0].ToString(true) + ") { ... }";
                    }
                }
            }

            // Log the condition to debug
            if (logItem)
            {
                double duration = DateTime.Now.Subtract(evaluationStarted).TotalSeconds;

                MacroDebug.SetLogItemData(drLogItem, logItemString, boolConditionResult, MacroDebug.CurrentLogIndent + GetDebugIndent(), 0);
                if (!(result is MacroCommand))
                {
                    MacroDebug.LogMacroOperation("Result of if statement", result, MacroDebug.CurrentLogIndent + GetDebugIndent(), duration);
                }
                MacroDebug.LogMacroOperation("}", "", MacroDebug.CurrentLogIndent + GetDebugIndent(), duration);
            }

            return result;
        }


        /// <summary>
        /// Evaluates the print and println commands.
        /// </summary>
        /// <param name="match">Determines whether all the necessary objects for evaluation were present</param>
        /// <param name="securityPassed">Returns true if the security of the evaluated macro was OK (both integrity and permissions)</param>
        private object PrintToConsole(ref bool match, ref bool securityPassed)
        {
            string newLine = (Name.ToLowerCSafe() == "println" ? Environment.NewLine : "");
            string console = Context.ConsoleOutput;
            foreach (MacroExpression child in Children)
            {
                if (!string.IsNullOrEmpty(console))
                {
                    console += newLine;
                }
                console += ValidationHelper.GetString(EvaluateChild(child, ref match, ref securityPassed), "");
            }

            Context.ConsoleOutput = console;

            // Result of the print action is the state of the console.
            return console;
        }


        /// <summary>
        /// Evaluates the expression as a lambda expression ('=>' operator).
        /// </summary>
        private object EvaluateLambdaExpr()
        {
            var varList = new List<string>();

            var lambdaExpression = Children[1];
            var lambdaVar = Children[0];

            if (lambdaVar.Type == ExpressionType.SubExpression)
            {
                var variables = lambdaVar.Children;

                // Handle "() => expression" syntax
                if ((variables.Count == 1) && (variables[0].Name == null))
                {
                    // No parameters of lambda
                }
                else
                {
                    // Handle "(var1, var2, ..., varN) => expression" syntax
                    foreach (MacroExpression variable in variables)
                    {
                        varList.Add(variable.Name);
                    }
                }
            }
            else
            {
                // Handle "var1 => expression" syntax
                varList.Add(lambdaVar.Name);
            }

            return new MacroLambdaExpression(varList, lambdaExpression);
        }


        /// <summary>
        /// Evaluates the expression as a command.
        /// </summary>
        /// <param name="match">Determines whether all the necessary objects for evaluation were present</param>
        /// <param name="securityPassed">Returns true if the security of the evaluated macro was OK (both integrity and permissions)</param>
        private object EvaluateCommand(ref bool match, ref bool securityPassed)
        {
            object result;

            if (HasNumberOfChildren(1))
            {
                // Evaluate the expression after command
                object retval = EvaluateChild(Children[0], ref match, ref securityPassed);

                // Special case for unresolved value
                object retvalToLog = retval;
                if (retval == MacroResolver.UNRESOLVED_RETURN_VALUE)
                {
                    retvalToLog = "unresolved";
                    match = false;
                }

                MacroDebug.LogMacroOperation(Name + " " + Children[0].ToString(true), retvalToLog, MacroDebug.CurrentLogIndent + GetDebugIndent());
                result = new MacroCommand(Name, retval);
            }
            else
            {
                MacroDebug.LogMacroOperation(Name, null, MacroDebug.CurrentLogIndent + GetDebugIndent());
                result = new MacroCommand(Name, null);
            }
            return result;
        }


        /// <summary>
        /// Evaluates the expression as a data member.
        /// </summary>
        /// <param name="match">Determines whether all the necessary objects for evaluation were present</param>
        private object EvaluateDataMember(ref bool match)
        {
            if (Name.ToLowerCSafe() == "unresolved")
            {
                // Unresolved keyword
                return MacroResolver.UNRESOLVED_RETURN_VALUE;
            }
            else
            {
                // Try to find data member in one of the sources given
                // (special values, named data source, source tables, source data, source parameters)
                EvaluationResult result = Context.Resolver.CheckDataSources(Name, Context);
                if (!result.Match)
                {
                    // Data source does not exist
                    match = false;
                }

                return result.Result;
            }
        }


        /// <summary>
        /// Evaluates the expression as a property
        /// </summary>
        /// <param name="match">Determines whether all the necessary objects for evaluation were present</param>
        /// <param name="securityPassed">Returns true if the security of the evaluated macro was OK (both integrity and permissions)</param>
        private object EvaluateProperty(ref bool match, ref bool securityPassed)
        {
            object result = null;

            // Order with properties is switched. The identifier after last dot gets to this method first
            // Since root expressions are called data members, a property has to have a parent (at least the data member, or method call or other property)
            // datamember.property1.property2 (property2 goes to this method first)
            if (HasNumberOfChildren(1))
            {
                // First get the object to which the property belongs (turn off the macro object encapsulation for evaluating data members of the property getting purposes)
                bool origEncapsulation = Context.EncapsulateMacroObjects;
                Context.EncapsulateMacroObjects = false;

                object parentObject = EvaluateChild(Children[0], ref match, ref securityPassed);

                Context.EncapsulateMacroObjects = origEncapsulation;

                if (parentObject != null)
                {
                    // Get the property of the parent object
                    EvaluationResult r = Context.Resolver.GetObjectValue(parentObject, Name, Context);
                    if (!r.Match)
                    {
                        // Property does not exist
                        match = false;
                    }

                    result = r.Result;

                    // If the property ends with custom data and the result is string, wrap it into CustomData object
                    if ((result is string) && Name.EndsWithCSafe("customdata", true))
                    {
                        CustomData data = new CustomData();
                        data.LoadData((string)result);
                        result = data;
                    }

                    // Check the security if we are accessing the properties of the object (unless it's context object)
                    if (!(parentObject is IContext))
                    {
                        if (!MacroSecurityProcessor.CheckSecurity(Context, result, false))
                        {
                            securityPassed = false;
                        }
                    }
                }
            }
            return result;
        }


        /// <summary>
        /// Returns boolean result from object (used for if statements to handle even non boolean inputs similarly as JavaScript does).
        /// </summary>
        /// <param name="ifConditionResult">Result of if condition expression (not strongly typed)</param>
        private static bool GetBoolConditionResult(object ifConditionResult)
        {
            bool result;

            if (ifConditionResult is bool)
            {
                result = (bool)ifConditionResult;
            }
            else
            {
                // For integer values the condition is true if result does not equal to zero
                if ((ifConditionResult is int) || (ifConditionResult is short) || (ifConditionResult is long) || (ifConditionResult is float) || (ifConditionResult is double))
                {
                    result = ValidationHelper.GetDouble(ifConditionResult, 0) != 0;
                }
                else
                {
                    // For object values it has to be different from null
                    result = (ifConditionResult != null);
                }
            }

            return result;
        }


        /// <summary>
        /// Processes the assignment operators. Uses dynamic parameters for storing those values.
        /// </summary>
        /// <param name="op">Assignment operator (=, +=, *=, etc.)</param>
        /// <param name="variableName">Name of the variable (will be used as key of dynamic parameter)</param>
        /// <param name="rightSideValue">Value to be assigned</param>
        private object ProcessAssignment(string op, string variableName, object rightSideValue)
        {
            if (op == "=")
            {
                Context.Resolver.SetDynamicParameter(variableName, rightSideValue);
                return rightSideValue;
            }

            EvaluationResult r = Context.Resolver.GetDynamicParameter(variableName);
            object data = r.Result;
            if (data == null)
            {
                // Variable was not created yet, create and assign the value
                Context.Resolver.SetDynamicParameter(variableName, rightSideValue);
                return rightSideValue;
            }

            object newVal = null;

            switch (op)
            {
                case "+=":
                    newVal = SystemMethods.Add(Context, data, rightSideValue);
                    break;

                case "-=":
                    newVal = SystemMethods.Subtract(Context, data, rightSideValue);
                    break;

                case "p++":
                    newVal = SystemMethods.Add(Context, data, 1);
                    break;

                case "p--":
                    newVal = SystemMethods.Subtract(Context, data, 1);
                    break;

                case "++":
                    newVal = SystemMethods.Add(Context, data, 1);
                    Context.Resolver.SetDynamicParameter(variableName, newVal);
                    return data;

                case "--":
                    newVal = SystemMethods.Subtract(Context, data, 1);
                    Context.Resolver.SetDynamicParameter(variableName, newVal);
                    return data;

                case "*=":
                    newVal = SystemMethods.Multiply(Context, data, rightSideValue);
                    break;

                case "/=":
                    newVal = SystemMethods.Divide(Context, data, rightSideValue);
                    break;

                case "%=":
                    newVal = SystemMethods.Modulo(Context, data, rightSideValue);
                    break;

                case "&=":
                    newVal = SystemMethods.LogicalAnd(Context, data, rightSideValue);
                    break;

                case "|=":
                    newVal = SystemMethods.LogicalOr(Context, data, rightSideValue);
                    break;

                case "^=":
                    newVal = SystemMethods.LogicalXor(Context, data, rightSideValue);
                    break;

                case "<<=":
                    newVal = SystemMethods.LeftShift(Context, data, rightSideValue);
                    break;

                case ">>=":
                    newVal = SystemMethods.RightShift(Context, data, rightSideValue);
                    break;
            }

            // Set the left side of the expression
            Context.Resolver.SetDynamicParameter(variableName, newVal);

            return newVal;
        }

        #endregion


        #region "Equality methods"

        /// <summary>
        /// Returns true if first parameter is equal to the second. Handles several specialties: 
        /// 1) GUID is equal also to string representation of GUID (case insensitive regardless the context setting). 
        /// 2) Simple data types are equal to their ToString representation. 
        /// 3) InfoObject is equal also to string constant if it's either its display name or code name. 
        /// 4) Two info objects are equal when they have same object type and same ID. 
        /// 5) Empty string is equal to null. 
        /// </summary>
        /// <param name="param1">First parameter to compare</param>
        /// <param name="param2">Second parameter to compare</param>
        /// <param name="context">Evaluation context</param>
        public static bool IsEqual(object param1, object param2, EvaluationContext context)
        {
            // Special case for enumerations
            if (param1 is Enum)
            {
                return IsEnumObjectEqual((Enum)param1, param2, context);
            }

            if (param2 is Enum)
            {
                return IsEnumObjectEqual((Enum)param2, param1, context);
            }

            if (param1 is ObjectProperty)
            {
                param1 = ((ObjectProperty)param1).Value;
            }
            if (param2 is ObjectProperty)
            {
                param2 = ((ObjectProperty)param2).Value;
            }

            if (ValidationHelper.IsDouble(param1, context.Culture) && ValidationHelper.IsDouble(param2, context.Culture))
            {
                // Both are numbers, compare as numbers
                double number1 = ValidationHelper.GetDouble(param1, Double.NaN, context.Culture);
                double number2 = ValidationHelper.GetDouble(param2, Double.NaN, context.Culture);

                // Skip checking for invalid double values
                if (!Double.IsNaN(number1) && !Double.IsNaN(number2))
                {
                    return (number1 == number2);
                }
            }

            if (ValidationHelper.IsDateTime(param1, context.CultureInfo) && ValidationHelper.IsDateTime(param2, context.CultureInfo))
            {
                DateTime date1 = ValidationHelper.GetDateTime(param1, DateTimeHelper.ZERO_TIME, context.CultureInfo);
                DateTime date2 = ValidationHelper.GetDateTime(param2, DateTimeHelper.ZERO_TIME, context.CultureInfo);

                return DateTime.Compare(date1, date2) == 0;
            }

            if ((param1 is Guid) || (param2 is Guid))
            {
                // Compare GUIDs as case-insensitive strings (regardless of context case-sensitivity settings)
                if ((param1 != null) && (param2 != null))
                {
                    return param1.ToString().EqualsCSafe(param2.ToString(), true);
                }
            }
            else if ((param1 is BaseInfo) && (param2 is BaseInfo))
            {
                return IsInfoInfoEqual((BaseInfo)param1, (BaseInfo)param2);
            }
            else if ((param1 is BaseInfo) && (param2 is string))
            {
                return IsObjectStringEqual((BaseInfo)param1, (string)param2, context);
            }
            else if ((param2 is BaseInfo) && (param1 is string))
            {
                return IsObjectStringEqual((BaseInfo)param2, (string)param1, context);
            }
            else if ((param2 is string) && (param1 is string))
            {
                return IsStringStringEqual((string)param1, (string)param2, context);
            }
            else
            {
                // Handle null vs empty string equality
                if (param1 == null)
                {
                    return (param2 == null) || string.Empty.EqualsCSafe(param2);
                }

                if (param2 == null)
                {
                    return string.Empty.EqualsCSafe(param1);
                }

                return param1.Equals(param2);
            }

            return false;
        }


        /// <summary>
        /// Returns true if two Info objects are equal (have same object type and ID).
        /// </summary>
        /// <param name="info1">First info to compare</param>
        /// <param name="info2">Second info to compare</param>
        protected static bool IsInfoInfoEqual(BaseInfo info1, BaseInfo info2)
        {
            if ((info1 != null) && (info2 != null))
            {
                // Two InfoObjects are same when they are of same type and have same IDs
                return ((info1.Generalized.ObjectID == info2.Generalized.ObjectID) && (info1.TypeInfo.ObjectType.EqualsCSafe(info2.TypeInfo.ObjectType, true)));
            }

            return info1 == info2;
        }


        /// <summary>
        /// Returns true if given object has code name or display name equal to specified name.
        /// </summary>
        /// <param name="info">BaseInfo object</param>
        /// <param name="name">Name to compare with</param>
        /// <param name="context">Evaluation context</param>
        protected static bool IsObjectStringEqual(BaseInfo info, string name, EvaluationContext context)
        {
            if (info != null)
            {
                if (info.Generalized.ObjectCodeName.EqualsCSafe(name, !context.CaseSensitive))
                {
                    return true;
                }
                if (info.Generalized.ObjectDisplayName.EqualsCSafe(name, !context.CaseSensitive))
                {
                    return true;
                }
            }
            return false;
        }


        /// <summary>
        /// Compares the given enumeration object with any object. If the object is not enumeration of the same type, then compares it according its values (integer enumeration with integer, otherwise as text constants).
        /// </summary>
        /// <param name="enumObj">enumeration obj to compare</param>
        /// <param name="obj2">Object to compare with</param>
        /// <param name="context">Evaluation context</param>
        protected static bool IsEnumObjectEqual(Enum enumObj, object obj2, EvaluationContext context)
        {
            if (enumObj == null)
            {
                return obj2 == null;
            }

            if (obj2 is Enum)
            {
                return enumObj.Equals(obj2);
            }
            else if ((enumObj.GetTypeCode() == TypeCode.Int32) && ValidationHelper.IsInteger(obj2))
            {
                // Integer based enumerations
                int num = ValidationHelper.GetInteger(obj2, 0);
                return enumObj.Equals(Enum.ToObject(enumObj.GetType(), num));
            }
            else
            {
                if (obj2 != null)
                {
                    return enumObj.ToString().EqualsCSafe(obj2.ToString(), !context.CaseSensitive);
                }
            }
            return false;
        }


        /// <summary>
        /// Returns true if the two strings are equal. Empty string is equal to null.
        /// </summary>
        /// <param name="text1">First text to compare</param>
        /// <param name="text2">Second text to compare</param>
        /// <param name="context">Evaluation context</param>
        protected static bool IsStringStringEqual(string text1, string text2, EvaluationContext context)
        {
            if (text1 == null)
            {
                text1 = string.Empty;
            }
            if (text2 == null)
            {
                text2 = string.Empty;
            }
            return text1.EqualsCSafe(text2, !context.CaseSensitive);
        }

        #endregion


        #region "Debug methods"

        /// <summary>
        /// Returns the indentation of the debug item (counts deepness withing blocks {...})
        /// </summary>
        protected int GetDebugIndent()
        {
            int indent = -1;

            MacroExpression current = Expression;
            while (current != null)
            {
                if ((current.Type == ExpressionType.Property) || ((Type == ExpressionType.MethodCall) && Name.EqualsCSafe(";")) || (current.Type == ExpressionType.DataMember) || (current.Type == ExpressionType.Command))
                {
                    indent++;
                }
                current = current.Parent;
            }

            return indent;
        }

        #endregion


        #region "Helper methods"

        /// <summary>
        /// Returns true if the type of current expression is MethodCall with specified name.
        /// </summary>
        /// <param name="name">Name of the method</param>
        protected bool IsMethodWithName(string name)
        {
            return (Type == ExpressionType.MethodCall) && (Name.EqualsCSafe(name, true));
        }


        /// <summary>
        /// Returns true if the current expression node has exactly specified number of children.
        /// </summary>
        /// <param name="number">Number of children</param>
        protected bool HasNumberOfChildren(int number)
        {
            if (number == 0)
            {
                return (Children == null) || (Children.Count == 0);
            }
            return (Children != null) && (Children.Count == number);
        }

        #endregion
    }
}