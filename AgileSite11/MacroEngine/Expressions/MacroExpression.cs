using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CMS.Base;
using CMS.Core;
using CMS.Helpers;

namespace CMS.MacroEngine
{
    /// <summary>
    /// Macro expression - represents a syntactic tree of the macro expression.
    /// </summary>
    public class MacroExpression
    {
        #region "Variables"

        /// <summary>
        /// Priority of the method.
        /// </summary>
        protected int mPriority = 0;

        /// <summary>
        /// Children expressions (parameters of method or subexpression).
        /// </summary>
        protected List<MacroExpression> mChildren = null;

        /// <summary>
        /// Parameters for the given expression (backward compatibility).
        /// </summary>
        protected List<MacroExpression> mParameters = null;


        /// <summary>
        /// Hash table with cached parsed expressions to speedup the macro evaluation.
        /// </summary>
        protected static SafeDictionary<string, MacroExpression> mSyntacticTreeTable = null;

        /// <summary>
        /// Dictionary of operator priorities [operator] => [priority]
        /// </summary>
        private static readonly Lazy<StringSafeDictionary<int>> mOperatorPriorities = new Lazy<StringSafeDictionary<int>>(GetOperatorPriorities);

        /// <summary>
        /// Set of left folding operators (e.g. arithmetic or bitwise operators)
        /// </summary>
        private static readonly Lazy<SafeHashSet<string>> mLeftFoldedOperators = new Lazy<SafeHashSet<string>>(GetLeftFoldedOperators);

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets or sets the value that indicates whether syntax error was detected during a parsing
        /// </summary>
        /// <remarks>This property is used for checking within the parsing with suppressed errors</remarks>
        private bool SyntaxErrorDetected
        {
            get;
            set;
        }


        /// <summary>
        /// Returns the expression type.
        /// </summary>
        public ExpressionType Type
        {
            get;
            protected set;
        }


        /// <summary>
        /// Name of the data member or method call.
        /// </summary>
        public string Name
        {
            get;
            protected set;
        }


        /// <summary>
        /// Value of the value expression (is null for expression types like method, property, etc.).
        /// </summary>
        public object Value
        {
            get;
            protected set;
        }


        /// <summary>
        /// Parameters for the given expression; i.e. |(paramname)value expressions.
        /// </summary>
        public List<MacroExpression> Parameters
        {
            get
            {
                if (mParameters == null)
                {
                    if (Parent != null)
                    {
                        return Parent.Parameters;
                    }
                }
                return mParameters;
            }
        }


        /// <summary>
        /// Parent expression.
        /// </summary>
        public MacroExpression Parent
        {
            get;
            set;
        }


        /// <summary>
        /// Child expressions of the expression.
        /// </summary>
        public List<MacroExpression> Children => mChildren ?? (mChildren = new List<MacroExpression>());


        /// <summary>
        /// Hashtable with cached parsed expressions to speedup the macro evaluation.
        /// </summary>
        private static SafeDictionary<string, MacroExpression> SyntacticTreeTable => mSyntacticTreeTable ?? (mSyntacticTreeTable = new SafeDictionary<string, MacroExpression>());

        #endregion


        #region "Protected properties"

        /// <summary>
        /// Next expression to evaluate.
        /// </summary>
        protected MacroExpression Next
        {
            get;
            set;
        }


        /// <summary>
        /// Previous expression (the expression from which this expression is the next).
        /// </summary>
        protected MacroExpression Previous
        {
            get;
            set;
        }


        /// <summary>
        /// Elements from the source expression.
        /// </summary>
        protected List<MacroElement> SourceElements
        {
            get;
            set;
        }


        /// <summary>
        /// Starting index of the expression in the source.
        /// </summary>
        protected int StartIndex
        {
            get;
            set;
        }


        /// <summary>
        /// Ending index of the expression in the source.
        /// </summary>
        protected int EndIndex
        {
            get;
            set;
        }


        /// <summary>
        /// Gets the element priority.
        /// </summary>
        protected int Priority
        {
            get
            {
                if (mPriority <= 0)
                {
                    // Get the priority for the method call
                    if (Type == ExpressionType.MethodCall)
                    {
                        mPriority = GetPriority(Name);
                    }
                }

                return mPriority;
            }
        }

        #endregion


        #region  "Constructors"

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="elements">Elements of the source expression</param>
        protected MacroExpression(List<MacroElement> elements)
            : this(elements, 0, elements.Count - 1)
        {
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="elements">Elements of the source expression</param>
        /// <param name="startIndex">Index of the first item of this expression</param>
        /// <param name="endIndex">Index of the last item of this expression</param>
        protected MacroExpression(List<MacroElement> elements, int startIndex, int endIndex)
        {
            SourceElements = elements;
            StartIndex = startIndex;
            EndIndex = endIndex;
        }

        #endregion


        #region "Lazy load methods"

        /// <summary>
        /// Gets the dictionary of operator priorities [operator] => [priority]
        /// </summary>
        private static StringSafeDictionary<int> GetOperatorPriorities()
        {
            return new StringSafeDictionary<int>
            {
                { "++", 17 },
                { "--", 17 },
                { "%", 17 },

                { "p++", 16 },
                { "p--", 16 },
                { "u+", 16 },
                { "u-", 16 },
                { "!", 16 },
                { "~", 16 },

                { "of", 15 },
                { "*", 15 },
                { "/", 15 },
                { "mod", 15 },

                { "+", 14 },
                { "-", 14 },

                { "<<", 13 },
                { ">>", 13 },

                { "<", 12 },
                { "<=", 12 },
                { ">", 12 },
                { ">=", 12 },

                { "==", 11 },
                { "!=", 11 },
                    
                { "&", 10 },

                { "^", 9 },

                { "|", 8 },
                    
                { "and", 7 },
                { "&&", 7 },

                { "or", 6 },
                { "||", 6 },

                { "??", 5 },

                { "?", 4 },
                { ":", 4 },

                { "in", 3 },
                { "=>", 3 },
                { "=", 3 },
                { "+=", 3 },
                { "-=", 3 },
                { "/=", 3 },
                { "%=", 3 },
                { "*=", 3 },
                { "<<=", 3 },
                { ">>=", 3 },
                { "&=", 3 },
                { "^=", 3 },
                { "|=", 3 },

                { ";", 1 },
            };
        }


        /// <summary>
        /// Gets set of left folding operators (e.g. arithmetic or bitwise operators)
        /// </summary>
        private static SafeHashSet<string> GetLeftFoldedOperators()
        {
            return new SafeHashSet<string>()
            {
                "++",
                "--",
                "%",

                "p++",
                "p--",
                "u+",
                "u-",
                "!",
                "~",

                "of",
                "*",
                "/",
                "mod",

                "+",
                "-",

                "<<",
                ">>",

                "+=",
                "-=",
                "/=",
                "%=",
                "*=",

                "<<=",
                ">>=",
                "&=",
                "^=",
                "|=",
            };
        }

        #endregion


        #region "Parsing methods"

        /// <summary>
        /// Gets the element on specific index of this expression.
        /// </summary>
        /// <param name="index">Index to get, starting with 0</param>
        protected MacroElement GetElement(int index)
        {
            // Check if the index is within the current expression
            if (index > EndIndex)
            {
                return null;
            }

            // Check if the index is within the whole source expression
            if (SourceElements.Count > index)
            {
                return SourceElements[index];
            }

            return null;
        }


        /// <summary>
        /// Parses the expression.
        /// </summary>
        /// <param name="startIndex">Starting index for the parsing (within the expression, starts with 0)</param>
        protected void Parse(int startIndex)
        {
            Parse(startIndex, false);
        }


        /// <summary>
        /// Parses the expression.
        /// </summary>
        /// <param name="startIndex">Starting index for the parsing (within the expression, starts with 0)</param>
        /// <param name="supressError">If true no exceptions are thrown</param>
        protected void Parse(int startIndex, bool supressError)
        {
            // Already parsed
            if ((Type != ExpressionType.Unparsed))
            {
                return;
            }

            var endIndex = EndIndex;

            // Empty command, ignore it
            if (StartIndex > endIndex)
            {
                Type = ExpressionType.Empty;
                return;
            }

            // First split the expression into parts (according to semicolons)
            var subEpressions = new List<MacroExpression>();

            int lastIndex = startIndex;
            int blocks = 0;
            int brackets = 0;
            int indexers = 0;

            for (int i = startIndex; i <= endIndex; i++)
            {
                MacroElement el = GetElement(i);
                if (el != null)
                {
                    switch (el.Type)
                    {
                        case ElementType.LeftBracket:
                            brackets++;
                            break;

                        case ElementType.RightBracket:
                            brackets--;
                            break;

                        case ElementType.LeftIndexer:
                            indexers++;
                            break;

                        case ElementType.RightIndexer:
                            indexers--;
                            break;

                        case ElementType.BlockStart:
                            blocks++;
                            break;

                        case ElementType.BlockEnd:
                            {
                                blocks--;

                                if ((blocks == 0) && (indexers == 0) && (brackets == 0) && (i < endIndex))
                                {
                                    MacroElement next = GetElement(i + 1);
                                    if (next != null)
                                    {
                                        if ((next.Type == ElementType.Semicolon) || (next.Expression.ToLowerCSafe() == "else"))
                                        {
                                            break;
                                        }
                                    }
                                    MacroExpression exp = new MacroExpression(SourceElements, lastIndex, i);
                                    exp.Parent = this;
                                    subEpressions.Add(exp);
                                    lastIndex = i + 1;
                                }
                            }
                            break;

                        case ElementType.Semicolon:
                            if ((blocks == 0) && (indexers == 0) && (brackets == 0))
                            {
                                MacroExpression exp = new MacroExpression(SourceElements, lastIndex, i - 1);
                                exp.Parent = this;
                                subEpressions.Add(exp);
                                lastIndex = i + 1;
                            }
                            break;
                    }
                }
            }

            // If it is an compound expression, collection is not empty
            if (subEpressions.Count > 0)
            {
                // Add the last one and create compound expression
                if (lastIndex <= endIndex)
                {
                    var exp = new MacroExpression(SourceElements, lastIndex, endIndex);

                    exp.Parent = this;
                    subEpressions.Add(exp);
                }

                // Create main root of the expression
                Name = ";";
                Type = ExpressionType.MethodCall;
                mChildren = new List<MacroExpression>();
                mChildren.AddRange(subEpressions);
                Previous = null;
                Next = null;

                return;
            }


            // Analyze first element
            int index = startIndex;
            bool onResult = false;

            MacroElement firstElement = GetElement(index);
            MacroElement nextElement = GetElement(index + 1);

            // Handle the call on the result
            if ((firstElement != null) && (firstElement.Type == ElementType.Dot) &&
                (nextElement != null) && (nextElement.Type == ElementType.Identifier))
            {
                index++;

                firstElement = nextElement;
                nextElement = GetElement(index + 1);

                onResult = true;
            }

            switch (firstElement.Type)
            {
                // Identifier - method call or data member
                case ElementType.Identifier:
                    {
                        var lowerExpression = firstElement.Expression.ToLowerCSafe();

                        // Special command - return
                        if (lowerExpression == "return")
                        {
                            // Return statement
                            if (startIndex < endIndex)
                            {
                                MacroExpression child = new MacroExpression(SourceElements, startIndex + 1, endIndex);
                                child.Parent = this;
                                mChildren = new List<MacroExpression>();
                                mChildren.Add(child);
                            }

                            Name = "return";
                            Type = ExpressionType.Command;
                            Previous = null;
                            Next = null;
                            return;
                        }

                        if ((nextElement != null) && (nextElement.Type == ElementType.LeftBracket))
                        {
                            Name = firstElement.Expression;
                            Type = ExpressionType.MethodCall;

                            // Method call
                            if (onResult)
                            {
                                // Make result the child
                                InitMethodOnResult(true);
                            }

                            // Parse the method parameters
                            index += 2;
                            ParseParameters(ref index, ElementType.LeftBracket, ElementType.RightBracket);
                        }
                        else
                        {
                            // Data member or special command
                            if (onResult)
                            {
                                Type = ExpressionType.Property;
                                InitPropertyOnDataMember();
                            }
                            else
                            {
                                if (lowerExpression == "null")
                                {
                                    // Special case for null value
                                    Type = ExpressionType.Value;
                                    Value = null;
                                }
                                else
                                {
                                    if (IsSpecialCommand(firstElement.Expression))
                                    {
                                        Type = ExpressionType.Command;
                                    }
                                    else if (lowerExpression == "else")
                                    {
                                        Type = ExpressionType.MethodCall;

                                        // Append else block as the child of if
                                        if (!String.IsNullOrEmpty(Previous?.Parent?.Name) && Previous.Parent.Name.Equals("if", StringComparison.OrdinalIgnoreCase))
                                        {
                                            Parent = Previous.Parent;
                                            Parent.Children.Add(this);
                                            Previous.Next = null;
                                        }
                                    }
                                    else
                                    {
                                        Type = ExpressionType.DataMember;
                                    }
                                }
                            }
                            Name = firstElement.Expression;
                            index++;
                        }
                    }
                    break;

                // Operator
                case ElementType.Operator:
                    {
                        bool addparam = true;
                        bool useresult = true;

                        // Convert operator to method call
                        Type = ExpressionType.MethodCall;
                        Name = firstElement.Expression;

                        // Unary and prefix operators
                        if (Previous == null)
                        {
                            useresult = false;

                            switch (Name)
                            {
                                case "++":
                                case "--":
                                    // Prefix operator
                                    Name = "p" + Name;
                                    break;

                                case "+":
                                case "-":
                                    // Unary operators + and -
                                    Name = "u" + Name;
                                    break;

                                case "!":
                                case "~":
                                    // Unary operators ! and ~
                                    break;

                                default:
                                    // Not allowed unary operators
                                    SyntaxError(index, supressError);
                                    return;
                            }
                        }

                        if (useresult)
                        {
                            switch (Name)
                            {
                                case "++":
                                case "--":
                                case "%":
                                    // Suffix operators are applied on result
                                    InitMethodOnResult(true);
                                    addparam = false;
                                    break;

                                default:
                                    // Alter the end index to the current index
                                    InitMethodOnResult(false);
                                    break;
                            }
                        }

                        // Move to next element
                        index++;

                        if (addparam)
                        {
                            // The rest of the expression is considered the parameter of the operator
                            if (index <= endIndex)
                            {
                                var paramExp = new MacroExpression(SourceElements, index, endIndex);
                                paramExp.Parent = this;

                                if (mChildren == null)
                                {
                                    mChildren = new List<MacroExpression>();
                                }

                                mChildren.Add(paramExp);
                            }

                            index = endIndex + 1;
                        }
                    }
                    break;

                // Indexer
                case ElementType.LeftIndexer:
                    {
                        // Indexer call - convert to method call
                        Type = ExpressionType.MethodCall;
                        Name = "[]";

                        // Make the current result the first child
                        InitMethodOnResult(true);

                        // Parse the indexer parameters
                        index++;
                        ParseParameters(ref index, ElementType.LeftIndexer, ElementType.RightIndexer);
                    }
                    break;

                // Subexpression
                case ElementType.BlockStart:
                case ElementType.LeftBracket:
                    {
                        int parenthesis = 1;

                        index++;
                        int start = index;
                        int end = -1;
                        int lastEnd = start;

                        MacroElement current;

                        ElementType openingType = firstElement.Type;
                        ElementType closingType = (firstElement.Type == ElementType.LeftBracket ? ElementType.RightBracket : ElementType.BlockEnd);

                        List<MacroExpression> subExpressions = new List<MacroExpression>();

                        do
                        {
                            // Get next element
                            current = GetElement(index);
                            if (current != null)
                            {
                                end = index;

                                // End of one subexpression
                                if (openingType == ElementType.LeftBracket)
                                {
                                    if ((current.Type == ElementType.Comma) && (parenthesis - 1 == 0))
                                    {
                                        subExpressions.Add(new MacroExpression(SourceElements, lastEnd, end - 1));
                                        lastEnd = end + 1;
                                    }
                                }

                                if (current.Type == openingType)
                                {
                                    // Open parenthesis
                                    parenthesis++;
                                }
                                else if (current.Type == closingType)
                                {
                                    // Close parenthesis
                                    parenthesis--;
                                    if (parenthesis <= 0)
                                    {
                                        end--;
                                    }
                                }
                            }

                            index++;
                        } while ((parenthesis > 0) && (current != null));

                        // Add the last subexpression
                        subExpressions.Add(new MacroExpression(SourceElements, lastEnd, end));

                        if (end >= 0)
                        {
                            // Subexpression
                            if (openingType == ElementType.BlockStart)
                            {
                                Type = ExpressionType.Block;

                                MakeChildOfPrevious();
                            }
                            else
                            {
                                Type = ExpressionType.SubExpression;
                            }

                            mChildren = new List<MacroExpression>();
                            foreach (MacroExpression subExpression in subExpressions)
                            {
                                subExpression.Parent = this;
                                mChildren.Add(subExpression);
                            }
                        }
                        else
                        {
                            // No content in the brackets - syntax error
                            SyntaxError(index, supressError);
                            return;
                        }
                    }
                    break;

                // Literal
                case ElementType.Integer:
                    {
                        // Specific value
                        Type = ExpressionType.Value;

                        // All the numbers are treated as double
                        Value = Convert.ToDouble(firstElement.Value);

                        // If number exceeds max int value log warning
                        if (!ValidationHelper.IsInteger(firstElement.Expression))
                        {
                            CoreServices.EventLog.LogEvent("W", "MacroResolver", "NUMBEROVERFLOW", $"The given number '{firstElement.Expression}' is too large and resolves into '{Value}'. This is caused by integer overflow. If you want to display the original value as a string literal, encapsulate the original value with apostrophes.");
                        }

                        index++;
                    }
                    break;

                case ElementType.Double:
                case ElementType.String:
                case ElementType.Boolean:
                    {
                        // Specific value
                        Type = ExpressionType.Value;
                        Value = firstElement.Value;

                        index++;
                    }
                    break;

                // Parameters
                case ElementType.Parameter:
                    {
                        // Break the link between parameters and standard part of an expression
                        MacroExpression root = Previous;
                        if (root == null)
                        {
                            root = Parent;

                            // If parameter is as a child of some expression, remove it from the children collection
                            root?.mChildren.Remove(this);
                        }

                        if (root != null)
                        {
                            // Get the root of whole expression
                            root = root.GetRoot();

                            // Break the connections with parameters
                            if (Previous != null)
                            {
                                Previous.Next = null;
                            }
                            Previous = null;

                            // Parse the parameters
                            string paramName = firstElement.Value.ToString();
                            string paramValue = "";
                            int start = index;

                            if (root.mParameters == null)
                            {
                                root.mParameters = new List<MacroExpression>();
                            }

                            MacroElement current = GetElement(++index);
                            while (current != null)
                            {
                                if (current.Type == ElementType.Parameter)
                                {
                                    int end = index;

                                    // Append the parameter
                                    MacroExpression parameter = new MacroExpression(SourceElements, start, end)
                                    {
                                        Type = ExpressionType.ParameterValue,
                                        Name = paramName,
                                        Value = paramValue
                                    };
                                    root.mParameters.Add(parameter);

                                    // Reset the values
                                    start = index;
                                    paramName = current.Value.ToString();
                                    paramValue = "";
                                }
                                else
                                {
                                    paramValue += current.Expression;
                                }
                                current = GetElement(++index);
                            }

                            // Append the last parameter to the root
                            MacroExpression lastParam = new MacroExpression(SourceElements, start, SourceElements.Count - 1)
                            {
                                Type = ExpressionType.ParameterValue,
                                Name = paramName,
                                Value = paramValue
                            };
                            root.mParameters.Add(lastParam);
                        }
                    }
                    // Parameters are the last thing to process
                    return;

                // Unexpected element = syntax error
                default:
                    SyntaxError(index, supressError);
                    return;
                    
            }

            // Link the next expression if available
            if (index <= endIndex)
            {
                MacroExpression nextExpression = new MacroExpression(SourceElements, index, endIndex);

                // Update the end index so that the two linked expression don't overlap
                EndIndex = index - 1;
                nextExpression.Previous = this;

                Next = nextExpression;
            }
        }


        /// <summary>
        /// Makes current element child of the previous element if exists.
        /// </summary>
        protected void MakeChildOfPrevious()
        {
            if (Previous != null)
            {
                Parent = Previous;
                if (Parent.mChildren == null)
                {
                    Parent.mChildren = new List<MacroExpression>();
                }
                Parent.Children.Add(this);
                Previous.Next = null;
                Previous = null;
            }
        }


        /// <summary>
        /// Determines whether the operators/methods priority requires different folding (left-hand rather than right-hand)
        /// </summary>
        /// <param name="childPriority">Priority of child (current) operator/method</param>
        /// <param name="parentPriority">Priority of parent operator/method</param>
        /// <param name="parentName">Name of parent operator/method</param>
        /// <returns>True if opposite folding is required</returns>
        private static bool IsWronglyFolded(int childPriority, int parentPriority, string parentName)
        {
            return (parentPriority > childPriority) || ((parentPriority == childPriority) && mLeftFoldedOperators.Value.Contains(parentName));
        }


        /// <summary>
        /// Flips given expression so it folded left-hand rather than right-hand
        /// </summary>
        /// <remarks>
        /// A→B←(CDE) becomes (A→B←C)→D←E instead of A→B←(C→D←E)
        /// </remarks>
        /// <param name="expression">Macro expression that is wrongly folded</param>
        /// <returns>Flipped macro expression</returns>
        private static MacroExpression FlipFolding(MacroExpression expression)
        {
            return expression.Parent;
        }


        /// <summary>
        /// Transfers the current result to a first child (for the parameter of method).
        /// </summary>
        /// <param name="isCalledOnResult">If true, method is called on data member (i.e. x.method() or postfix operator such as ++), otherwise the call is infix (i.e. method(x) or infix operator such as +)</param>
        protected void InitMethodOnResult(bool isCalledOnResult)
        {
            mChildren = new List<MacroExpression>();

            MacroExpression resultExpression = Previous;

            while (true)
            {
                // Get the starting expression of the result
                while (resultExpression.Previous != null)
                {
                    resultExpression = resultExpression.Previous;
                }

                // If the parent expression has higher priority, move it lower
                // There is an exception for the block element, because you cannot go outside of the block
                if ((resultExpression.Parent != null) && (resultExpression.Parent.Type != ExpressionType.Block))
                {
                    int parentPriority = resultExpression.Parent.Priority;
                    int priority = Priority;
                    if (isCalledOnResult)
                    {
                        if (priority == 0)
                        {
                            priority = 20;
                        }
                    }

                    if (IsWronglyFolded(priority, parentPriority, resultExpression.Parent.Name))
                    {
                        resultExpression = FlipFolding(resultExpression);
                        continue;
                    }
                }

                break;
            }

            // Break the existing connections
            Previous.Next = null;
            Previous = null;

            // Swap the parent relationship
            resultExpression.Parent?.ReplaceChild(resultExpression, this);

            // Add as the child
            resultExpression.Parent = this;
            mChildren.Add(resultExpression);
        }


        /// <summary>
        /// Transfers the current result to a first child.
        /// </summary>
        protected void InitPropertyOnDataMember()
        {
            mChildren = new List<MacroExpression>();

            MacroExpression resultExpression = Previous;

            // Get the starting expression of the result
            while (resultExpression.Previous != null)
            {
                resultExpression = resultExpression.Previous;
            }

            // Break the existing connections
            Previous.Next = null;
            Previous = null;

            // Swap the parent relationship
            resultExpression.Parent?.ReplaceChild(resultExpression, this);

            // Add as the child
            resultExpression.Parent = this;
            mChildren.Add(resultExpression);
        }


        /// <summary>
        /// Replaces the given child expression with the given one.
        /// </summary>
        /// <param name="oldExp">Old child expression</param>
        /// <param name="newExp">New child expression</param>
        protected void ReplaceChild(MacroExpression oldExp, MacroExpression newExp)
        {
            // No children, no replace
            if (mChildren == null)
            {
                return;
            }

            int index = mChildren.IndexOf(oldExp);
            if (index >= 0)
            {
                oldExp.Parent = null;
                newExp.Parent = this;

                mChildren[index] = newExp;
            }
        }


        /// <summary>
        /// Parses the method or indexer parameters.
        /// </summary>
        /// <param name="index">Index in the expression, adjusted to after the end element</param>
        /// <param name="openParenthesis">Opening parenthesis for these parameters</param>
        /// <param name="closeParenthesis">Closing parenthesis for these parameters</param>
        protected void ParseParameters(ref int index, ElementType openParenthesis, ElementType closeParenthesis)
        {
            int startIndex = -1;
            int parenthesis = 1;

            MacroElement current;

            // New list of children
            if (mChildren == null)
            {
                mChildren = new List<MacroExpression>();
            }

            do
            {
                // Initialize the start index
                if (startIndex < 0)
                {
                    startIndex = index;
                }

                // Analyze next element
                current = GetElement(index);
                if (current != null)
                {
                    if (current.Type == openParenthesis)
                    {
                        // Open parenthesis
                        parenthesis++;
                    }
                    else if (current.Type == closeParenthesis)
                    {
                        // Close parenthesis
                        parenthesis--;
                    }
                }

                // Check end of the current subexpression
                if ((current == null) || (parenthesis <= 0) || ((current.Type == ElementType.Comma) && (parenthesis == 1)))
                {
                    int endIndex = ((current != null) ? index - 1 : index);

                    // Add the subexpression
                    if (endIndex >= startIndex)
                    {
                        MacroExpression subExpression = new MacroExpression(SourceElements, startIndex, endIndex);
                        subExpression.Parent = this;

                        mChildren.Add(subExpression);
                    }

                    // Reset the start index for initialization of next parameter
                    startIndex = -1;
                }

                index++;
            } while ((parenthesis > 0) && (current != null));
        }


        /// <summary>
        /// Checks the syntactic tree of parsed expression if everything is OK and ready for correct evaluation.
        /// </summary>
        /// <param name="originalExpression">Expression which was parsed</param>
        protected void CheckSyntax(string originalExpression)
        {
            switch (Type)
            {
                case ExpressionType.MethodCall:
                    switch (Name.ToLowerInvariant())
                    {
                        case "=>":
                            if (!HasNumberOfChildren(2))
                            {
                                throw new SyntacticAnalysisException(originalExpression, "Wrong syntax of lambda expression; (<variable1>, <variable2>, ..., <variablen>) => <expression> expected.");
                            }
                            break;

                        case "if":
                            if (!(HasNumberOfChildren(2) || HasNumberOfChildren(3)))
                            {
                                throw new SyntacticAnalysisException(originalExpression, "Wrong syntax of if structure; if (<condition>) { <expression> } [else { <expression> }] expected.");
                            }
                            break;

                        case "else":
                            if (!HasNumberOfChildren(1))
                            {
                                throw new SyntacticAnalysisException(originalExpression, "Wrong syntax of if structure; if (<condition>) { <expression> } [else { <expression> }] expected.");
                            }
                            break;

                        case "while":
                            if (!HasNumberOfChildren(2))
                            {
                                throw new SyntacticAnalysisException(originalExpression, "Wrong syntax of while structure; while (<condition>) { <expression> } expected.");
                            }
                            break;

                        case "for":
                            {
                                if (!HasNumberOfChildren(2))
                                {
                                    throw new SyntacticAnalysisException(originalExpression, "Wrong syntax of loop; for (<variable> = <initialvalue>; <condition>; <expression>) { <expression> } expected.");
                                }

                                if (Children[1].Type != ExpressionType.Block)
                                {
                                    throw new SyntacticAnalysisException(originalExpression, "There has to be block of code { ... } after for loop declaration.");
                                }

                                if (!Children[0].IsMethodWithName(";") || (Children[0].Children.Count != 3))
                                {
                                    throw new SyntacticAnalysisException(originalExpression, "Wrong syntax of loop; for (<variable> = <initialvalue>; <condition>; <expression>) { <expression> } expected.");
                                }
                            }
                            break;

                        case "foreach":
                            {
                                string syntaxError = "Wrong syntax of loop; foreach (<variable> in <enumerable>) { <expression> } expected.";

                                if (!HasNumberOfChildren(2))
                                {
                                    throw new SyntacticAnalysisException(originalExpression, syntaxError);
                                }
                                MacroExpression inOperator = Children[0];
                                if (!inOperator.IsMethodWithName("in") || !inOperator.HasNumberOfChildren(2))
                                {
                                    throw new SyntacticAnalysisException(originalExpression, syntaxError);
                                }
                                if (Children[1].Type != ExpressionType.Block)
                                {
                                    throw new SyntacticAnalysisException(originalExpression, "There has to be block of code { ... } after foreach declaration.");
                                }
                                if (inOperator.Children[0].Type != ExpressionType.DataMember)
                                {
                                    throw new SyntacticAnalysisException(originalExpression, "Identifier expected before in operator within foreach loop.");
                                }
                            }
                            break;

                        case "[]":
                            if (!HasNumberOfChildren(2))
                            {
                                // Check is for two parameters, cause first parameter is the data member on which the indexer is called
                                throw new SyntacticAnalysisException(originalExpression, "Indexer can have only one parameter.");
                            }
                            break;

                        case "?":
                            {
                                if (!HasNumberOfChildren(2))
                                {
                                    throw new SyntacticAnalysisException(originalExpression, "Wrong syntax of (<bool> ? <true_expr> : <false_expr>) expression.");
                                }
                                MacroExpression innerParam = Children[1];
                                if ((innerParam.Children.Count != 2) || (innerParam.Name != ":"))
                                {
                                    throw new SyntacticAnalysisException(originalExpression, "Wrong syntax of (<bool> ? <true_expr> : <false_expr>) expression.");
                                }
                            }
                            break;

                        case "==":
                            if (!HasNumberOfChildren(2))
                            {
                                throw new SyntacticAnalysisException(originalExpression, "Operator == can have only two parameters.");
                            }
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
                            {
                                if (!HasNumberOfChildren(2))
                                {
                                    throw new SyntacticAnalysisException(originalExpression, "Wrong syntax near '" + Name + "' operator.");
                                }
                                if (!ValidationHelper.IsIdentifier(Children[0].Name))
                                {
                                    throw new SyntacticAnalysisException(originalExpression, "Wrong use of operator '" + Name + "'. Left side of the operator has to be in identifier format");
                                }
                            }
                            break;

                        case "p++":
                        case "p--":
                            if (!HasNumberOfChildren(1))
                            {
                                throw new SyntacticAnalysisException(originalExpression, "Operator == can have only two parameters.");
                            }
                            break;
                    }
                    break;

                case ExpressionType.Block:
                case ExpressionType.SubExpression:
                    if (!HasNumberOfChildren(1))
                    {
                        throw new SyntacticAnalysisException(originalExpression, "Block / Subexpression has to have only 1 child.");
                    }
                    break;
            }
        }


        /// <summary>
        /// Returns the root of parsed expression. Uses cache of the parsed expressions to speedup the process.
        /// </summary>
        /// <param name="expression">Expression string to parse</param>
        /// <param name="supressError">If true no exceptions are thrown</param>
        /// <exception cref="SyntacticAnalysisException">Syntax error if <paramref name="supressError"/> is false</exception>
        /// <exception cref="LexicalAnalysisException">Lexical analysis error if <paramref name="supressError"/> is false</exception>
        public static MacroExpression ParseExpression(string expression, bool supressError = false)
        {
            // Try to find the cached result first
            MacroExpression cached = SyntacticTreeTable[expression];
            if (cached == null)
            {
                List<MacroElement> elements = MacroElement.ParseExpression(expression, supressError);
                MacroExpression expr = new MacroExpression(elements).ParseAll(supressError);

                // Check that the syntax tree was built correctly
                if (!supressError)
                {
                    expr.CheckSyntax(expression);

                    // Cache the result only if the errors were not suppressed, we don't want to cache wrongly parsed expressions
                    SyntacticTreeTable[expression] = expr;
                }

                return expr;
            }
            return cached;
        }


        /// <summary>
        /// Parses the whole expression tree.
        /// </summary>
        /// <param name="supressError">If true no exceptions are thrown</param>
        protected MacroExpression ParseAll(bool supressError)
        {
            // Parse this
            Parse(StartIndex, supressError);

            // Parse the children
            if (mChildren != null)
            {
                // Parsing the children may introduce new children, do not use for each here
                for (int i = 0; i < mChildren.Count; ++i)
                {
                    // Parse the child
                    mChildren[i].ParseAll(supressError);
                }
            }

            // Parse the next expression
            if ((Next != null) && !SyntaxErrorDetected)
            {
                Next.ParseAll(supressError);
            }

            // Append value of the return statement
            if ((Previous != null) && (Previous.Type == ExpressionType.Command) && (Previous.Name.ToLowerInvariant() == "return"))
            {
                MakeChildOfPrevious();
            }

            return GetRoot();
        }


        /// <summary>
        /// Gets the root of the expression.
        /// </summary>
        protected MacroExpression GetRoot()
        {
            // If parent exists, parent leads to root
            if (Parent != null)
            {
                return Parent.GetRoot();
            }

            // If previous exists, previous leads to root
            if (Previous != null)
            {
                return Previous.GetRoot();
            }

            // If no parent or previous, this expression is the root
            return this;
        }


        /// <summary>
        /// Gets the priority of the evaluation for the given method.
        /// </summary>
        /// <param name="methodName">Method name</param>
        protected int GetPriority(string methodName)
        {
            return mOperatorPriorities.Value[methodName];
        }


        /// <summary>
        /// Returns true for special commands such as break or continue, otherwise false.
        /// </summary>
        /// <param name="name">Name of the command</param>
        protected bool IsSpecialCommand(string name)
        {
            switch (name.ToLowerInvariant())
            {
                case "return":
                case "break":
                case "continue":
                    return true;
            }
            return false;
        }

        #endregion


        #region "Exceptional states handling"

        /// <summary>
        /// Aborts the parsing of the expression due to a syntactic error
        /// </summary>
        /// <param name="lexemIndex">Position within the lexeme list</param>
        /// <param name="supressError">If true no exceptions are thrown</param>
        /// <exception cref="SyntacticAnalysisException">Syntax error if <paramref name="supressError" /> is false</exception>
        protected void SyntaxError(int lexemIndex, bool supressError = false)
        {
            if (!supressError)
            {
                // Get the root expression which was being parsed to combine the whole expression.
                var root = GetRoot() ?? this;

                // Construct the original expression from the lexeme list
                int position = 0;
                int i = 0;

                var sb = new StringBuilder();

                foreach (var item in root.SourceElements)
                {
                    sb.Append(item.Expression);

                    if (i < lexemIndex)
                    {
                        position += item.Expression.Length;
                    }

                    i++;
                }

                throw new SyntacticAnalysisException(sb.ToString(), position);
            }

            // Stop processing on error (special case for lightweight parsing 
            // e.g. Editor IntelliSense cannot throw an exception during a typing
            Next = null;
            Previous = null;
            SyntaxErrorDetected = true;
        }

        #endregion


        #region "ToString representation"

        /// <summary>
        /// Returns string representation of this MacroExpression.
        /// </summary>
        public override string ToString()
        {
            return ToString(false);
        }


        /// <summary>
        /// Returns string representation of this MacroExpression.
        /// </summary>
        /// <param name="debugMode">If true, debug mode is on</param>
        public string ToString(bool debugMode)
        {
            switch (Type)
            {
                case ExpressionType.MethodCall:
                    {
                        var lowerName = Name.ToLowerCSafe();

                        if (debugMode && (lowerName == "rule"))
                        {
                            // Rule method in debug mode should be displayed without XML definition
                            return String.Concat("Rule(", Children[0].ToString(true), ", \"...\")");
                        }

                        switch (lowerName)
                        {
                            case "if":
                            case "foreach":
                            case "for":
                            case "while":
                                return ToStringControlFlowStatements(debugMode);

                            case "[]":
                                return ToStringIndexer(debugMode);

                            default:
                                return ToStringMethod(debugMode);
                        }
                    }

                case ExpressionType.Block:
                    return ToStringBlock(debugMode);

                case ExpressionType.Property:
                    return ToStringProperty(debugMode);

                case ExpressionType.DataMember:
                    return Name;

                case ExpressionType.Value:
                    return ToStringConstant();

                case ExpressionType.Command:
                    return ToStringCommand(debugMode);
            }

            // Default ToString representation is simply concatenation of lexemes of the expression.
            var sb = new StringBuilder();
            for (int i = StartIndex; i <= EndIndex; i++)
            {
                if (IsSpecialCommand(SourceElements[i].Expression))
                {
                    sb.Append(" ", SourceElements[i].Expression, " ");
                }
                else
                {
                    sb.Append(SourceElements[i].Expression);
                }
            }
            return sb.ToString();
        }


        /// <summary>
        /// Handles indexer expressions.
        /// </summary>
        /// <param name="debugMode">Determines whether the string representation is made for debug purposes</param>
        protected string ToStringIndexer(bool debugMode)
        {
            if ((Children != null) && (Children.Count == 2))
            {
                return String.Concat(Children[0].ToString(debugMode), "[", Children[1].ToString(debugMode), "]");
            }
            return "";
        }


        /// <summary>
        /// Handles constants (string literal, numbers, booleans, etc.).
        /// </summary>
        protected string ToStringConstant()
        {
            if (Value is string)
            {
                return String.Concat("\"", MacroElement.EscapeSpecialChars(Value.ToString()), "\"");
            }

            if (Value is bool)
            {
                return Value.ToString().ToLowerCSafe();
            }

            if (Value == null)
            {
                return "null";
            }

            return Value.ToString();
        }


        /// <summary>
        /// Handles properties.
        /// </summary>
        /// <param name="debugMode">Determines whether the string representation is made for debug purposes</param>
        protected string ToStringProperty(bool debugMode)
        {
            var result = new StringBuilder();
            if ((Children != null) && (Children.Count == 1))
            {
                // Append child properties
                MacroExpression child = Children[0];
                if ((child.Type == ExpressionType.DataMember) || (child.Type == ExpressionType.Property))
                {
                    result.Append(child.ToString(debugMode));
                    result.Append(".");
                }
            }

            result.Append(Name);

            return result.ToString();
        }


        /// <summary>
        /// Handles special commands like return, break, etc.
        /// </summary>
        /// <param name="debugMode">Determines whether the string representation is made for debug purposes</param>
        protected string ToStringCommand(bool debugMode)
        {
            var result = new StringBuilder();
            result.Append(" ", Name, " ");

            if (Children.Count > 0)
            {
                result.Append(Children[0].ToString(debugMode));
            }

            return result.ToString();
        }


        /// <summary>
        /// Handles block of code: { [some code] }
        /// </summary>
        /// <param name="debugMode">Determines whether the string representation is made for debug purposes</param>
        protected string ToStringBlock(bool debugMode)
        {
            if ((Children != null) && (Children.Count == 1))
            {
                return String.Concat("{ ", Children[0].ToString(debugMode), " }");
            }
            return "";
        }


        /// <summary>
        /// Handles structures like [commandname] { [some code] }
        /// </summary>
        /// <param name="debugMode">Determines whether the string representation is made for debug purposes</param>
        protected string ToStringControlFlowStatements(bool debugMode)
        {
            if (Children != null)
            {
                switch (Children.Count)
                {
                    case 2:
                        return String.Concat(Name.ToLowerCSafe(), " (", Children[0].ToString(debugMode), ") ", (debugMode ? "{ ... }" : Children[1].ToString()));

                    case 3:
                        if (debugMode)
                        {
                            // Short version of if-else statement
                            return String.Concat(Name.ToLowerCSafe(), " (", Children[0].ToString(true), ") { ... } else { ... }");
                        }
                        else
                        {
                            // Standards version of if-else statement
                            return String.Concat(Name.ToLowerCSafe(), " (", Children[0].ToString(), ") ", Children[1].ToString(), " else ", Children[2].Children[0].ToString());
                        }
                }
            }
            return "";
        }


        /// <summary>
        /// Handles classic method calls.
        /// </summary>
        /// <param name="debugMode">Determines whether the string representation is made for debug purposes</param>
        protected string ToStringMethod(bool debugMode)
        {
            // Default handling of the methods
            if (MacroElement.IsValidOperator(Name))
            {
                // Operators
                if (Name == ";")
                {
                    // Composition of commands
                    return string.Join("; ", Children.Select(t => t.ToString(debugMode)));
                }

                if (Children != null)
                {
                    if (Children.Count == 2)
                    {
                        return String.Concat(Children[0].ToString(debugMode), " ", Name, " ", Children[1].ToString(debugMode));
                    }

                    if (Children.Count == 1)
                    {
                        // Unary operators
                        if (Name.StartsWithCSafe("p") || Name.StartsWithCSafe("u"))
                        {
                            return Name.Substring(1) + Children[0].ToString(debugMode);
                        }

                        return Children[0].ToString(debugMode) + Name;
                    }

                    var result = new StringBuilder();
                    bool addOp = false;
                    foreach (var ch in Children)
                    {
                        if (addOp)
                        {
                            result.Append(" ", Name, " ");
                        }
                        result.Append(ch.ToString(debugMode));
                        addOp = true;
                    }

                    return result.ToString();
                }
            }
            else
            {
                // Classical methods
                if ((Children != null) && (Children.Count > 0) && ValidationHelper.IsIdentifier(Children[0]))
                {
                    return GetPrefixMethodCall(debugMode);
                }

                return GetInfixMethodCall(debugMode);
            }

            return "";
        }


        /// <summary>
        /// Returns string representation of the method in format MyMethod(First, Second, Third).
        /// </summary>
        /// <param name="debugMode">Debug mode</param>
        protected string GetInfixMethodCall(bool debugMode)
        {
            return String.Concat(Name, "(", string.Join(", ", Children.Select(t => t.ToString(debugMode))), ")");
        }


        /// <summary>
        /// Returns string representation of the method in format First.MyMethod(Second, Third).
        /// </summary>
        /// <param name="debugMode">Debug mode</param>
        protected string GetPrefixMethodCall(bool debugMode)
        {
            if ((Children != null) && (Children.Count > 0))
            {
                string result = String.Concat(Children[0], ".", Name, "(");
                result += string.Join(", ", Children.Skip(1).Select(t => t.ToString(debugMode))) + ")";
                return result;
            }

            return Name + "()";
        }

        #endregion


        #region "Structure processing methods"

        /// <summary>
        /// Extracts specified parameter of Rule expression.
        /// </summary>
        /// <param name="expression">Expression with rule method</param>
        /// <param name="methodName">Name of the method to extract parameter of</param>
        /// <param name="parameter">Index of the parameter</param>
        public static MacroExpression ExtractParameter(string expression, string methodName, int parameter)
        {
            MacroExpression expr = ParseExpression(expression, true);
            if (expr != null)
            {
                if (expr.IsMethodWithName(methodName))
                {
                    if (expr.Children.Count > parameter)
                    {
                        return expr.Children[parameter];
                    }
                }
            }
            return null;
        }


        /// <summary>
        /// Modifies the syntactic tree of the expression to the flat structure. Changes expressions like "(a || b) || c" to "a || b || c", etc.
        /// </summary>
        public void MakeFlatter()
        {
            for (int i = Children.Count - 1; i >= 0; i--)
            {
                Children[i].MakeFlatter();
            }

            if ((Type == ExpressionType.MethodCall) && IsFlatOperator(Name))
            {
                if (Parent != null)
                {
                    if (Parent.IsMethodWithName(Name))
                    {
                        // Move to the parent
                        Parent.Children.Remove(this);

                        foreach (MacroExpression child in Children)
                        {
                            child.Parent = Parent;
                            Parent.Children.Add(child);
                        }

                        Parent = null;
                        mChildren = null;
                    }
                }
            }
        }


        /// <summary>
        /// Returns true for operators which can be made flat - without subexpressions (||, &amp;&amp;, +, etc.)
        /// </summary>
        /// <param name="op">Operator to check</param>
        protected bool IsFlatOperator(string op)
        {
            switch (op)
            {
                case "||":
                case "&&":
                case "+":
                    return true;
            }
            return false;
        }

        #endregion


        #region "Helper methods"

        /// <summary>
        /// Returns true if the type of current expression is MethodCall with specified name.
        /// </summary>
        /// <param name="name">Name of the method</param>
        protected bool IsMethodWithName(string name)
        {
            return (Type == ExpressionType.MethodCall) && Name.Equals(name, StringComparison.InvariantCultureIgnoreCase);
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