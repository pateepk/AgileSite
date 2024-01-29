namespace CMS.MacroEngine
{
    /// <summary>
    /// Element type.
    /// </summary>
    public enum ElementType : int
    {
        /// <summary>
        /// Unknown.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Identifier, e.g. Something.
        /// </summary>
        Identifier = 1,

        /// <summary>
        /// Integer, e.g. 4.
        /// </summary>
        Integer = 2,

        /// <summary>
        /// Double, e.g. 4.0.
        /// </summary>
        Double = 3,

        /// <summary>
        /// String, e.g. "something".
        /// </summary>
        String = 4,

        /// <summary>
        /// Boolean, e.g. true.
        /// </summary>
        Boolean = 5,

        /// <summary>
        /// Left bracket.
        /// </summary>
        LeftBracket = 6,

        /// <summary>
        /// Right bracket.
        /// </summary>
        RightBracket = 7,

        /// <summary>
        /// Operator.
        /// </summary>
        Operator = 8,

        /// <summary>
        /// Dot.
        /// </summary>
        Dot = 9,

        /// <summary>
        /// Comma.
        /// </summary>
        Comma = 10,

        /// <summary>
        /// Left indexer.
        /// </summary>
        LeftIndexer = 11,

        /// <summary>
        /// Right indexer.
        /// </summary>
        RightIndexer = 12,

        /// <summary>
        /// Parameter.
        /// </summary>
        Parameter = 13,

        /// <summary>
        /// Parameter value.
        /// </summary>
        ParameterValue = 14,

        /// <summary>
        /// Start of the block.
        /// </summary>
        BlockStart = 15,

        /// <summary>
        /// End of the block.
        /// </summary>
        BlockEnd = 16,

        /// <summary>
        /// Semicolon for compound expressions.
        /// </summary>
        Semicolon = 17
    }
}