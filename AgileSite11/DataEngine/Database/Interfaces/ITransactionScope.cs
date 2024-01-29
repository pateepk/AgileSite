using System;

namespace CMS.DataEngine
{
    /// <summary>
    /// Transaction scope interface
    /// </summary>
    public interface ITransactionScope : IDisposable
    {
        /// <summary>
        /// Commits the transaction (does the same as Complete).
        /// </summary>
        void Commit();
    }
}
