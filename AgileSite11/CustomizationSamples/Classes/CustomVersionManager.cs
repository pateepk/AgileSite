using CMS;
using CMS.DocumentEngine;
using CMS.EventLog;

[assembly: RegisterCustomManager(typeof(CustomVersionManager))]

/// <summary>
/// Sample custom version manager, does log an event upon undo check-out action.
/// </summary>
public class CustomVersionManager : VersionManager
{
    /// <summary>
    /// Constructor
    /// </summary>
    public CustomVersionManager()
        : base(null)
    {
    }


    /// <summary>
    /// Undo all operations made during last checkout.
    /// </summary>
    /// <param name="node">Document node</param>
    protected override void UndoCheckOutInternal(TreeNode node)
    {
        base.UndoCheckOutInternal(node);

        // Log the event that the document checkout was undone
        EventLogProvider.LogEvent(EventType.INFORMATION, "CustomVersionManager", "UndoCheckOut", "The page '" + node.DocumentName + "' checkout was undone.", null);
    }
}
