using System;
using System.IO;

namespace CMS.Tests
{
    internal class TestsDirectoryHelper
    {
        public static void DeleteDirectoryIgnoreIOErrors(string path)
        {
            if ((path != null) && Directory.Exists(path))
            {
                try
                {
                    Directory.Delete(path, true);
                }
                catch (Exception ex)
                {
                    // Do nothing if resources in the directory still in use
                    if (!(ex is IOException || ex is UnauthorizedAccessException))
                    {
                        throw;
                    }
                }
            }
        }
    }
}
