namespace CMS.MacroEngine
{
    /// <summary>
    /// Expression type.
    /// </summary>
    public enum ExpressionType : int
    {
        /// <summary>
        /// Unparsed.
        /// </summary>
        Unparsed = 0,

        /// <summary>
        /// Standalone method call, typically in format Something(...).
        /// </summary>
        MethodCall = 1,

        /// <summary>
        /// Data member (simple identifier), typically in format ABC.
        /// </summary>
        DataMember = 2,

        /// <summary>
        /// Specific value (any type), typically in format 0.5 or "something".
        /// </summary>
        Value = 3,

        /// <summary>
        /// Empty command.
        /// </summary>
        Empty = 4,

        /// <summary>
        /// Property of result, typically in format ABC.Something.
        /// </summary>
        Property = 5,

        /// <summary>
        /// Subexpression defined by parenthesis, typically in format (Something + SomethingElse) > XXX
        /// </summary>
        SubExpression = 6,

        /// <summary>
        /// Parameter value (backward compatibility).
        /// </summary>
        ParameterValue = 7,

        /// <summary>
        /// Block of expressions
        /// </summary>
        Block = 8,

        /// <summary>
        /// Special command (break, continue, ...)
        /// </summary>
        Command = 9
    }
}