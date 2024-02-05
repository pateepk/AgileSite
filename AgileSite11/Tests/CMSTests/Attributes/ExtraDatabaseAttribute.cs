using System;

namespace CMS.Tests
{
    /// <summary>
    /// Provides an extra database within the given test
    /// The extra database can be use by a block of code using method ExecuteWithExtraDatabase
    /// You can alternatively use method <see cref="AutomatedTestsWithLocalDB.EnsureExtraDatabase" /> to initialize an extra database on-the-fly
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class ExtraDatabaseAttribute : Attribute
    {
        /// <summary>
        /// Database name
        /// </summary>
        public string Name
        {
            get;
            private set;
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Database name</param>
        public ExtraDatabaseAttribute(string name)
        {
            Name = name;
        }
    }
}
