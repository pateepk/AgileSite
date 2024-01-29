namespace CMS.Helpers.UniGraphConfig
{
    /// <summary>
    /// Summary description for NodeType.
    /// </summary>
    public enum NodeTypeEnum : int
    { 
        /// <summary>
        /// Standard node for general usage.
        /// </summary>
        Standard = 0,

        /// <summary>
        /// Node representing automatic action.
        /// </summary>
        Action = 1,

        /// <summary>
        /// Node representing IF/ELSE condition.
        /// </summary>
        Condition = 2,

        /// <summary>
        /// Node representing SWITCH{CASE,DEFAULT} construction.
        /// </summary>
        Multichoice = 3,

        /// <summary>
        /// Node representing branching by user choice.
        /// </summary>
        Userchoice = 4
    }
}
