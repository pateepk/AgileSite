using CMS.Core;
using System.Reflection;

namespace CMS.Tests
{
    /// <summary>
    /// Allows usage of more strict assembly discovery.
    /// </summary>
    public class TestsAssemblyDiscoveryHelper
    {
        private static AssemblyDiscovery originalAssemblyDiscovery;

        /// <summary>
        /// Uses system default assembly discovery instead of <see cref="TestsAssemblyDiscovery"/>.
        /// </summary>
        public static void UseSystemDefaultAssemblyDiscovery()
        {
            if (originalAssemblyDiscovery != null)
            {
                AssemblyDiscoveryHelper.AssemblyDiscovery = originalAssemblyDiscovery;
            }
        }


        /// <summary>
        /// Ensures that assembly discovery process is constrained only to <paramref name="assembly"/> and its references.
        /// </summary>
        /// <param name="assembly">Assembly the relevance of references are based on.</param>
        /// <remarks>It is required for environment where all tests are built into single output directory.</remarks>
        public static void EnsureTestsAssemblyDiscoveryUsed(Assembly assembly)
        {
            if (AssemblyDiscoveryHelper.AssemblyDiscovery is TestsAssemblyDiscovery)
            {
                return;
            }

            originalAssemblyDiscovery = AssemblyDiscoveryHelper.AssemblyDiscovery;

            AssemblyDiscoveryHelper.AssemblyDiscovery = new TestsAssemblyDiscovery(assembly);
        }
    }
}
