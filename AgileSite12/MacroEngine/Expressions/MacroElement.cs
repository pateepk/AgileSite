using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

using CMS.Helpers;
using CMS.Base;

namespace CMS.MacroEngine
{
    /// <summary>
    /// Macro expression element. Lexical analysis of the K# expression.
    /// </summary>
    public class MacroElement
    {
        #region "Variables"

        /// <summary>
        /// Replacement character for the backslash character
        /// </summary>
        private const string BACKSLASH_REPLACEMENT = "□";

        /// <summary>
        /// Replacements for special characters escapes [escaped -> unescaped]
        /// </summary>
        private static readonly Lazy<SafeDictionary<string, string>> mSpecialChars = new Lazy<SafeDictionary<string, string>>(GetSpecialChars);


        /// <summary>
        /// Set of all operators
        /// </summary>
        private static readonly Lazy<HashSet<string>> mOperators = new Lazy<HashSet<string>>(GetOperators);

        /// <summary>
        /// Set of all operator characters
        /// </summary>
        private static readonly Lazy<HashSet<char>> mOperatorChars = new Lazy<HashSet<char>>(GetOperatorChars);

        /// <summary>
        /// Set of all word operators
        /// </summary>
        private static readonly Lazy<HashSet<string>> mWordOperators = new Lazy<HashSet<string>>(GetWordOperators);

        #endregion


        #region "Properties"

        /// <summary>
        /// Element type.
        /// </summary>
        public ElementType Type
        {
            get;
            private set;
        }

        /// <summary>
        /// Element expression (as was parsed).
        /// </summary>
        public string Expression
        {
            get;
            private set;
        }

        /// <summary>
        /// Element value (of the element type).
        /// </summary>
        public object Value
        {
            get;
            private set;
        }


        /// <summary>
        /// Index of the expression start in the source expression.
        /// </summary>
        public int StartIndex
        {
            get;
            private set;
        }

        #endregion


        #region "Enumeration"

        /// <summary>
        /// Status of the parser.
        /// </summary>
        protected enum Status
        {
            /// <summary>
            /// Start (begining of parsing).
            /// </summary>
            Start,

            /// <summary>
            /// Identifier.
            /// </summary>
            Identifier,

            /// <summary>
            /// Integer.
            /// </summary>
            Integer,

            /// <summary>
            /// Dot.
            /// </summary>
            Dot,

            /// <summary>
            /// Comma.
            /// </summary>
            Comma,

            /// <summary>
            /// Double.
            /// </summary>
            Double,

            /// <summary>
            /// Error number.
            /// </summary>
            ErrorNumber,

            /// <summary>
            /// String read (quotes - beginning of the string).
            /// </summary>
            StringRead,

            /// <summary>
            /// Multiline string read (@ followed by quotes - beginning of the string).
            /// </summary>
            StringReadMultiline,

            /// <summary>
            /// String slash (for escaping character purposes).
            /// </summary>
            StringSlash,

            /// <summary>
            /// Operator.
            /// </summary>
            Operator,

            /// <summary>
            /// Parameter.
            /// </summary>
            Parameter,

            /// <summary>
            /// Unknown error.
            /// </summary>
            ErrorUnknown,

            /// <summary>
            /// Identifier error.
            /// </summary>
            ErrorIdentifier,

            /// <summary>
            /// Internal error.
            /// </summary>
            ErrorInternal,

            /// <summary>
            /// Operator error.
            /// </summary>
            ErrorOperator,

            /// <summary>
            /// Parameter error.
            /// </summary>
            ErrorParameter,

            /// <summary>
            /// Comment
            /// </summary>
            Comment,

            /// <summary>
            /// Multiline comment
            /// </summary>
            CommentMultiline
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="type">Element type</param>
        /// <param name="expression">Expression</param>
        /// <param name="value">Value</param>
        /// <param name="startIndex">Start index in the source string</param>
        public MacroElement(ElementType type, string expression, object value, int startIndex)
        {
            Value = value;
            Type = type;
            Expression = expression;
            StartIndex = startIndex;
        }

        #endregion


        #region "Lazy load methods"

        /// <summary>
        /// Gets the replacements for special characters escapes [escaped -> unescaped]
        /// </summary>
        private static SafeDictionary<string, string> GetSpecialChars()
        {
            return new SafeDictionary<string, string>
            {
                {"\\'", "\'"},
                {"\\\"", "\""},
                {"\\v", "\v"},
                {"\\0", "\0"},
                {"\\a", "\a"},
                {"\\b", "\b"},
                {"\\f", "\f"},
                {"\\n", "\n"},
                {"\\r", "\r"},
                {"\\t", "\t"},
                {"\\v", "\v"},
                {"%\\}", "%}"} // Special escaping (needed for macros in the string constants)
            };
        }


        /// <summary>
        /// Gets the set of all word operators
        /// </summary>
        private static HashSet<string> GetWordOperators()
        {
            return new HashSet<string>(StringComparer.InvariantCultureIgnoreCase)
            {
                "mod",
                "and",
                "or",
                "of",
                "in"   
            };
        }


        /// <summary>
        /// Gets the set of all operator characters
        /// </summary>
        private static HashSet<char> GetOperatorChars()
        {
            return new HashSet<char>
            {
                '-', 
                '+', 
                '*', 
                '/', 
                '%', 
                '>', 
                '<', 
                '!', 
                '=', 
                '&', 
                '|', 
                '~', 
                '^', 
                ':', 
                '?', 
                ';', 
            };
        }


        /// <summary>
        /// Gets the set of all operators
        /// </summary>
        private static HashSet<string> GetOperators()
        {
            return new HashSet<string>(StringComparer.InvariantCultureIgnoreCase)
            {
                "??",
                "of",
                "in",
                "u+",
                "u-",
                "p++",
                "p--",
                "=>",
                "=",
                "+",
                "-",
                "*",
                "/",
                "%",
                "++",
                "--",
                "==",
                "!=",
                ">",
                "<",
                ">=",
                "<=",
                "!",
                "&&",
                "||",
                "~",
                "&",
                "|",
                "^",
                "<<",
                ">>",
                "+=",
                "-=",
                "*=",
                "/=",
                "%=",
                "&=",
                "|=",
                "^=",
                "<<=",
                ">>=",
                ":",
                "?",
                ";"
            };
        }

        #endregion


        #region "Public parsing methods"

        /// <summary>
        /// Parses the expression.
        /// </summary>
        /// <param name="expression">Expression to parse</param>
        public static List<MacroElement> ParseExpression(string expression)
        {
            return ParseExpression(expression, false);
        }


        /// <summary>
        /// Parses the expression.
        /// </summary>
        /// <param name="expression">Expression to parse</param>
        /// <param name="supressError">If true no exceptions are thrown</param>
        public static List<MacroElement> ParseExpression(string expression, bool supressError)
        {
            List<MacroElement> result = new List<MacroElement>();

            bool lastWasParam = false;
            int index = 0;

            MacroElement lastElem = null;

            // Parse elements
            while (true)
            {
                MacroElement elem = ParseNext(expression, ref index, supressError, lastWasParam);
                if (elem != null)
                {
                    lastWasParam = (elem.Type == ElementType.Parameter);

                    result.Add(elem);
                    lastElem = elem;
                }
                else
                {
                    break;
                }
            }

            if ((lastElem != null) && (lastElem.Type == ElementType.Parameter))
            {
                // If the last lexem parsed is Parameter it means there was no explicit value assigned
                // We need to create manually an empty ParameterValue entry
                result.Add(new MacroElement(ElementType.ParameterValue, "", "", expression.Length - 1));
            }

            return result;
        }

        #endregion


        #region "Private parsing methods"

        /// <summary>
        /// Parses the next element in the given macro expression, starting at the given index. Adjusts the index to the beginning of the next element.
        /// </summary>
        /// <param name="expression">Macro expression</param>
        /// <param name="index">Start index of where to find nex lexem</param>
        /// <param name="supressError">If true no exceptions are thrown</param>
        /// <param name="isParamValue">If true, the expression is only read until next parameter</param>
        private static MacroElement ParseNext(string expression, ref int index, bool supressError, bool isParamValue)
        {
            // End of the expression
            if (index >= expression.Length)
            {
                return null;
            }

            // Status of the parsing process
            Status status = Status.Start;

            // Carries the actual lexem of parsing
            StringBuilder lex = new StringBuilder();

            // Carries the value of an integer constant (if the lexem is integer constant)
            int intvalue = 0;

            // Starting index of the lexem
            int startindex = index;

            // Continue parsing is a flag which is set to false whenever a parsing error occur. We stop parsing process to be able to tell where the error occured
            bool continueParsing = true;

            while (continueParsing && (index <= expression.Length))
            {
                // Prepare the current char and following char
                char ch = ' ';
                char nextch = ' ';
                if (index < expression.Length)
                {
                    ch = expression[index];
                    if (index < expression.Length - 1)
                    {
                        nextch = expression[index + 1];
                    }
                }

                if (!isParamValue)
                {
                    switch (status)
                    {
                        case Status.Start:
                            // Start of the lexem, can switch than to any lexem type
                            if (Char.IsWhiteSpace(ch))
                            {
                                // Ignore white space between lexems, move to the next character
                                startindex++;
                            }
                            else if (Char.IsLetter(ch) || (ch == '_'))
                            {
                                // Start of the identifier _ or letter
                                status = Status.Identifier;
                                lex.Append(ch);
                            }
                            else if (Char.IsDigit(ch))
                            {
                                // Number, first we expect integer
                                status = Status.Integer;
                                intvalue = ch - '0';
                                lex.Append(ch);
                            }
                            else if (IsOperatorChar(ch))
                            {
                                // Only unary operators are allowed at the beginning of a lexem
                                status = Status.Operator;
                                lex.Append(ch);
                            }
                            else
                            {
                                switch (ch)
                                {
                                    case '@':
                                        // Beginning of a multiline string constant
                                        if (nextch == '"')
                                        {
                                            status = Status.StringReadMultiline;
                                            lex.Append(ch);
                                            lex.Append('\"');
                                            index++;
                                        }
                                        else
                                        {
                                            status = Status.ErrorUnknown;
                                            continueParsing = false;
                                        }
                                        break;

                                    case '"':
                                        // Quotes, beginning of the string constant
                                        status = Status.StringRead;
                                        lex.Append(ch);
                                        break;

                                    case '(':
                                        // Left bracket
                                        lex.Append(ch);
                                        index++;
                                        return CreateElement(ElementType.LeftBracket, lex, startindex);

                                    case ')':
                                        // Right bracket
                                        lex.Append(ch);
                                        index++;
                                        return CreateElement(ElementType.RightBracket, lex, startindex);

                                    case '[':
                                        // Left indexer
                                        lex.Append(ch);
                                        index++;
                                        return CreateElement(ElementType.LeftIndexer, lex, startindex);

                                    case ']':
                                        // Right indexer
                                        lex.Append(ch);
                                        index++;
                                        return CreateElement(ElementType.RightIndexer, lex, startindex);

                                    case '{':
                                        // Left indexer
                                        lex.Append(ch);
                                        index++;
                                        return CreateElement(ElementType.BlockStart, lex, startindex);

                                    case '}':
                                        // Right indexer
                                        lex.Append(ch);
                                        index++;
                                        return CreateElement(ElementType.BlockEnd, lex, startindex);

                                    case '.':
                                        // Dot
                                        lex.Append(ch);
                                        index++;
                                        return CreateElement(ElementType.Dot, lex, startindex);

                                    case ',':
                                        // Comma
                                        lex.Append(ch);
                                        index++;
                                        return CreateElement(ElementType.Comma, lex, startindex);

                                    default:
                                        // Error, no other option as the beginning of the lexem is possible
                                        status = Status.ErrorUnknown;
                                        continueParsing = false;
                                        break;
                                }
                            }
                            break;

                        case Status.Identifier:
                            // Identifier
                            if (Char.IsLetterOrDigit(ch) || (ch == '_'))
                            {
                                // Next identifier letter
                                lex.Append(ch);
                            }
                            else
                            {
                                var l = lex.ToString();

                                // End of the identifier
                                if (IsWordOperator(l))
                                {
                                    return new MacroElement(ElementType.Operator, l, l, startindex);
                                }

                                if (IsBoolConst(l))
                                {
                                    return new MacroElement(ElementType.Boolean, l, ValidationHelper.GetBoolean(l, false), startindex);
                                }

                                if (!ValidationHelper.IsIdentifier(l))
                                {
                                    if (!supressError)
                                    {
                                        throw new LexicalAnalysisException(expression, index - l.Length);
                                    }

                                    return null;
                                }

                                return new MacroElement(ElementType.Identifier, l, l, startindex);
                            }
                            break;

                        case Status.Integer:
                            // Integer number
                            if (Char.IsDigit(ch))
                            {
                                // Next number
                                intvalue = intvalue * 10 + (ch - '0');
                                lex.Append(ch);
                            }
                            else if (ch == '.')
                            {
                                // Decimal dot
                                status = Status.Dot;
                                lex.Append(ch);
                            }
                            else if (Char.IsLetter(ch) || (ch == '_'))
                            {
                                // Invalid identifier starting with a digit
                                status = Status.Identifier;
                                lex.Append(ch);
                            }
                            else
                            {
                                // End of the integer
                                return new MacroElement(ElementType.Integer, lex.ToString(), intvalue, startindex);
                            }
                            break;

                        case Status.Dot:
                            // Decimal dot
                            if (Char.IsDigit(ch))
                            {
                                // Next letter after dot
                                status = Status.Double;
                                lex.Append(ch);
                            }
                            else if (Char.IsLetter(ch) || (ch == '_'))
                            {
                                // Go back to the dot
                                index--;

                                var l = lex.ToString();

                                // End of the integer
                                return new MacroElement(ElementType.Integer, l.Substring(0, l.Length - 1), intvalue, startindex);
                            }
                            else
                            {
                                // End of double number
                                var l = lex.ToString();

                                double value = double.Parse(l, NumberStyles.AllowDecimalPoint | NumberStyles.AllowExponent, CultureHelper.EnglishCulture.NumberFormat);

                                return new MacroElement(ElementType.Double, l, value, startindex);
                            }
                            break;

                        case Status.Double:
                            // Double number
                            if (Char.IsDigit(ch))
                            {
                                // Next double digit
                                lex.Append(ch);
                            }
                            else if ((ch == 'e') || (ch == 'E'))
                            {
                                // Append the exponent
                                if ((nextch == '-') || (nextch == '+'))
                                {
                                    // Append the negative exponent
                                    lex.Append(ch);
                                    lex.Append(nextch);
                                    index++;
                                }
                                else if (Char.IsDigit(nextch))
                                {
                                    // Append exponent 'e' sign if it's followed by the digit
                                    lex.Append(ch);
                                }
                                else
                                {
                                    status = Status.ErrorNumber;
                                    continueParsing = false;
                                }
                            }
                            else
                            {
                                // End of double number
                                try
                                {
                                    var l = lex.ToString();

                                    double value = double.Parse(l, NumberStyles.AllowDecimalPoint | NumberStyles.AllowExponent, CultureHelper.EnglishCulture.NumberFormat);

                                    return new MacroElement(ElementType.Double, l, value, startindex);
                                }
                                catch
                                {
                                    status = Status.ErrorNumber;
                                    continueParsing = false;
                                }
                            }
                            break;

                        case Status.StringRead:
                            // String - read
                            if (ch == '"')
                            {
                                // End of the string
                                lex.Append(ch);
                                index++;

                                var l = lex.ToString();

                                // Handle all the escaping sequences - make a string from the string literal
                                string value = UnescapeSpecialChars(l.Substring(1, l.Length - 2));

                                return new MacroElement(ElementType.String, l, value, startindex);
                            }

                            if (ch == '\\')
                            {
                                // Escaped character of the string
                                status = Status.StringSlash;
                                lex.Append(ch);
                            }
                            else
                            {
                                // String character
                                lex.Append(ch);
                            }
                            break;

                        case Status.StringReadMultiline:
                            if (ch == '"')
                            {
                                if (nextch == '"')
                                {
                                    // Double quotes mean a single quote inside multiline string
                                    lex.Append("\"\"");
                                    index++;
                                }
                                else
                                {
                                    // Single quote means end of multiline string literaly
                                    lex.Append(ch);
                                    index++;

                                    var l = lex.ToString();

                                    // Skip @ and quotes, do not unescape characters (just the double quotes)
                                    string value = l.Substring(2, l.Length - 3).Replace("\"\"", "\"");

                                    return new MacroElement(ElementType.String, l, value, startindex);
                                }
                            }
                            else
                            {
                                // In the middle of the string literal, read the character
                                lex.Append(ch);
                            }
                            break;

                        case Status.StringSlash:
                            // Reading the escaped value in the string
                            lex.Append(ch);
                            status = Status.StringRead;
                            break;

                        case Status.Operator:
                            {
                                // Operator - do not concatenate two operator strings into one operator if the first one is semicolon (it cannot lead to a valid operator)
                                // It's because of expressions like 2;-2. The second part is because of expressions like x==-1 and x=-1 The third condition (ch != ';') is because of expressions like x++;
                                var l = lex.ToString();

                                if (IsOperatorChar(ch) && (l != ";") && (l != "==") && ((l != "=") || (ch == '=') || (ch == '>')) && (ch != ';'))
                                {
                                    // Comment
                                    if ((index > 0) && (expression[index - 1] == '/'))
                                    {
                                        if (ch == '/')
                                        {
                                            status = Status.Comment;
                                        }
                                        else if (ch == '*')
                                        {
                                            status = Status.CommentMultiline;
                                        }
                                    }

                                    lex.Append(ch);
                                }
                                else
                                {
                                    // Check for start of the parameter
                                    if (l == "|")
                                    {
                                        bool isParam = true;
                                        if (ch == '(')
                                        {
                                            // Check the expression after whether it's only alpha characters
                                            for (int j = index + 1; j < expression.Length; j++)
                                            {
                                                if (expression[j] == ')')
                                                {
                                                    break;
                                                }
                                                if (!char.IsLetter(expression[j]))
                                                {
                                                    isParam = false;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            isParam = false;
                                        }

                                        if (isParam)
                                        {
                                            // Normal start of the parameter
                                            lex.Append(ch);
                                            status = Status.Parameter;
                                        }
                                        else
                                        {
                                            // Valid operator |
                                            return CreateElement(ElementType.Operator, lex, startindex);
                                        }
                                    }
                                    // End of the operator, validate the operator
                                    else if (IsValidOperator(l))
                                    {
                                        if (l == ";")
                                        {
                                            // Semicolon
                                            return CreateElement(ElementType.Semicolon, lex, startindex);
                                        }

                                        // Valid normal operator
                                        return CreateElement(ElementType.Operator, lex, startindex);
                                    }
                                    else
                                    {
                                        // Operator error
                                        status = Status.ErrorOperator;
                                    }
                                }
                            }
                            break;

                        case Status.Parameter:
                            // Parameter
                            lex.Append(ch);

                            if (ch == ')')
                            {
                                index++;

                                var l = lex.ToString();

                                // Valid parameter
                                return new MacroElement(ElementType.Parameter, l, l.Substring(2, l.Length - 3), startindex);
                            }

                            if (!Char.IsLetter(ch))
                            {
                                // Parameter error
                                status = Status.ErrorParameter;
                            }
                            break;

                        case Status.CommentMultiline:
                            // Multiline comment - ignore all the character until the end of the comment
                            if ((ch == '*') && (expression.Length > index + 1) && (expression[index + 1] == '/'))
                            {
                                // Skip the comment, reset the parser and start the next element
                                index++;

                                status = Status.Start;
                                lex.Clear();
                                intvalue = 0;
                                startindex = index;
                            }
                            break;

                        case Status.Comment:
                            // Comment - ignore all the character until the end of the line
                            if ((ch == '\n') || (ch == '\r'))
                            {
                                // Skip the comment, reset the parser and start the next element
                                status = Status.Start;
                                lex.Clear();
                                intvalue = 0;
                                startindex = index;
                            }
                            break;

                        default:
                            // Internal error
                            status = Status.ErrorInternal;
                            continueParsing = false;
                            break;
                    }
                }
                else
                {
                    // Everything until next 
                    if ((ch == '|') && (expression[index - 1] != '\\') || (index == expression.Length))
                    {
                        return CreateElement(ElementType.ParameterValue, lex, startindex);
                    }

                    // If we are handling \| skip the backslash (unescape potential nested macro parameter)
                    if ((ch != '\\') || (expression.Length <= index + 1) || (expression[index + 1] != '|'))
                    {
                        lex.Append(ch);
                    }
                }

                index++;
            }

            if ((status != Status.Start) && (status != Status.Comment) && (status != Status.CommentMultiline))
            {
                if (supressError)
                {
                    // String was not read to the end, consider it ok when errors are suppressed
                    if (status == Status.StringRead)
                    {
                        var l = lex.ToString();

                        string retval = UnescapeSpecialChars(l.Substring(1, l.Length - 1));

                        return new MacroElement(ElementType.String, l, retval, startindex);
                    }
                }
                else
                {
                    // Error in processing, throw a parsing expression
                    throw new LexicalAnalysisException(expression, index--);
                }

                return null;
            }

            // No element found
            return null;
        }


        /// <summary>
        /// Creates a macro element
        /// </summary>
        /// <param name="type">Element type</param>
        /// <param name="lex">Source lexem</param>
        /// <param name="startindex">Start index</param>
        private static MacroElement CreateElement(ElementType type, StringBuilder lex, int startindex)
        {
            var l = lex.ToString();

            return new MacroElement(type, l, l, startindex);
        }

        #endregion


        #region "Helper methods"

        /// <summary>
        /// Unescapes special characters in the string constant.
        /// </summary>
        /// <param name="text">Text of the string constant</param>
        public static string UnescapeSpecialChars(string text)
        {
            var result = text.Replace("\\\\", BACKSLASH_REPLACEMENT);

            // Process all special chars
            foreach (DictionaryEntry specialChar in mSpecialChars.Value)
            {
                result = result.Replace((string)specialChar.Key, (string)specialChar.Value);
            }

            result = result.Replace(BACKSLASH_REPLACEMENT, "\\");

            return result;
        }


        /// <summary>
        /// Escapes special characters in the string to create a string literal.
        /// </summary>
        /// <param name="text">String to create literal from</param>
        public static string EscapeSpecialChars(string text)
        {
            var result = text.Replace("\\", BACKSLASH_REPLACEMENT);

            // Process all special chars
            foreach (DictionaryEntry specialChar in mSpecialChars.Value)
            {
                result = result.Replace((string)specialChar.Value, (string)specialChar.Key);
            }

            result = result.Replace(BACKSLASH_REPLACEMENT, "\\\\");

            return result;
        }


        /// <summary>
        /// Returns true if the character is operator character.
        /// </summary>
        /// <param name="ch">Character to check</param>
        public static bool IsOperatorChar(char ch)
        {
            return mOperatorChars.Value.Contains(ch);
        }


        /// <summary>
        /// Returns true if the given string is a word operator.
        /// </summary>
        /// <param name="op">Operator to check</param>
        public static bool IsWordOperator(string op)
        {
            return mWordOperators.Value.Contains(op);
        }


        /// <summary>
        /// Returns true if the operator is valid.
        /// </summary>
        /// <param name="op">Operator to check</param>
        public static bool IsValidOperator(string op)
        {
            return mOperators.Value.Contains(op);
        }


        /// <summary>
        /// Returns true if the given string is a boolean constant "true" or "false".
        /// </summary>
        /// <param name="op">Operator to check</param>
        public static bool IsBoolConst(string op)
        {
            switch (op.ToLowerCSafe())
            {
                case "true":
                case "false":
                    return true;

                default:
                    return false;
            }
        }


        /// <summary>
        /// Returns the expression of the element.
        /// </summary>
        public override string ToString()
        {
            return Expression;
        }

        #endregion
    }
}