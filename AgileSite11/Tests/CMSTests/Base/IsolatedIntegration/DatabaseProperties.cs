using System;
using System.IO;

namespace CMS.Tests
{
    /// <summary>
    /// Represents properties of a database used in isolated integration tests.
    /// </summary>
    internal sealed class DatabaseProperties
    {
        #region "Public properties"

        /// <summary>
        /// Gets the name of the database.
        /// </summary>
        public string Name
        {
            get;
            private set;
        }


        /// <summary>
        /// Gets the name of the database file.
        /// </summary>
        public string FileName
        {
            get;
            private set;
        }


        /// <summary>
        /// Gets the name of the database log file.
        /// </summary>
        public string LogFileName
        {
            get;
            private set;
        }


        /// <summary>
        /// If true, the database has been already detached
        /// </summary>
        public bool Detached
        {
            get;
            set;
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Initializes a new instance of the <see cref="CMS.Tests.DatabaseProperties"/> class.
        /// </summary>
        /// <param name="name">The name of the database.</param>
        /// <param name="folderPath">The name of the database file.</param>
        /// <param name="fileName">The name of the database log file.</param>
        private DatabaseProperties(string name, string folderPath, string fileName)
        {
            Name = name;
            FileName = Path.Combine(folderPath, String.Format("{0}.mdf", fileName));
            LogFileName = Path.Combine(folderPath, String.Format("{0}_log.ldf", fileName));
        }


        /// <summary>
        /// Creates a new instance of the <see cref="CMS.Tests.DatabaseProperties"/> class that represents an instance database.
        /// </summary>
        /// <param name="name">The name of the database.</param>
        /// <param name="folderPath">The path to the folder with database files.</param>
        /// <returns>A new instance of the <see cref="CMS.Tests.DatabaseProperties"/> class that represents an instance database.</returns>
        public static DatabaseProperties CreateForInstance(string name, string folderPath)
        {
            return new DatabaseProperties(name, folderPath, name + "_INSTANCE");
        }


        /// <summary>
        /// Creates a new instance of the <see cref="CMS.Tests.DatabaseProperties"/> class that represents a master database.
        /// </summary>
        /// <param name="name">The name of the database.</param>
        /// <param name="folderPath">The path to the folder with database files.</param>
        /// <returns>A new instance of the <see cref="CMS.Tests.DatabaseProperties"/> class that represents a master database.</returns>
        public static DatabaseProperties CreateForMaster(string name, string folderPath)
        {
            return new DatabaseProperties(name, folderPath, name + "_MASTER");
        }

        #endregion
   }
}