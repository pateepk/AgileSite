using System;
using System.Web.UI;

using CMS.DocumentEngine;


/// <summary>
/// Extendable Portal manager header panel interface.
/// </summary>
public interface IExtendableHeaderPanel
{
    /// <summary>
    /// Current node.
    /// </summary>
    TreeNode CurrentNode
    {
        get;
    }


    /// <summary>
    /// Adds aditional control to the HeaderPanel controls collection.
    /// </summary>
    /// <param name="control">Control to be added</param>
    void AddAdditionalControl(Control control);
}