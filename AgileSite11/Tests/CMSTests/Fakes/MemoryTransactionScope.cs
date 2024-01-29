using CMS.DataEngine;

namespace CMS.Tests
{
    internal class MemoryTransactionScope : ITransactionScope
    {
        public void Dispose()
        {
            // Rollback in memory is not supported
        }


        public void Commit()
        {
            // Changes are performed directly to the memory, commit doesn't need to do anything.
        }
    }
}
