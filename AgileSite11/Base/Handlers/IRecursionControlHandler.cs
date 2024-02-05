using System;

namespace CMS.Base
{
    /// <summary>
    /// Interface that defines the recursion control for the given handler
    /// </summary>
    public interface IRecursionControlHandler<TArgs>
        where TArgs : EventArgs
    {
        /// <summary>
        /// Gets the recursion key of the class to identify recursion
        /// </summary>
        /// <param name="e">Event arguments</param>
        string GetRecursionKey(TArgs e);
    }
}
