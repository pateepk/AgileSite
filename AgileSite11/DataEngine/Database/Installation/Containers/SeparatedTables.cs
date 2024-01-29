using System;
using System.Collections.Generic;
using System.Text;

namespace CMS.DataEngine
{
    /// <summary>
    /// Class providing list of separated tables including SQL and XML definitions
    /// </summary>
    public class SeparatedTables
    {
        private string SeparationScriptsFolder
        {
            get;
            set;
        }
        

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="separationScriptsFolder">SQL scripts folder for separation</param>
        public SeparatedTables(string separationScriptsFolder)
        {
            if (separationScriptsFolder == null)
            {
                throw new ArgumentNullException(nameof(separationScriptsFolder));
            }

            SeparationScriptsFolder = separationScriptsFolder;
        }


        /// <summary>
        /// Get tables and schemas.
        /// </summary>
        internal List<TableAndClass> GetTablesAndSchemas()
        {
            List<TableAndClass> tableList = GetTableNames(false);
            GetTableSchemas(tableList);
            return tableList;
        }


        /// <summary>
        /// Gets list of table names.
        /// </summary>        
        /// <param name="inverted">Indicates if list should be inverted</param>
        /// <returns>Returns list of table names </returns>
        internal List<TableAndClass> GetTableNames(bool inverted)
        {
            var tableList = new List<TableAndClass>();
            foreach (var table in FileReader.ReadAndSplit(SeparationScriptsFolder, "tables.txt"))
            {
                if (inverted)
                {
                    tableList.Insert(0, new TableAndClass { TableName = table });
                }
                else
                {
                    tableList.Add(new TableAndClass { TableName = table });
                }
            }
            return tableList;
        }


        /// <summary>
        /// Returns string with table names.
        /// </summary>
        /// <param name="separator">Separator between table names</param>
        /// <returns>Returns string with separated tables names</returns>
        public string GetTableNames(string separator)
        {
            var str = new StringBuilder();
            foreach (var table in FileReader.ReadAndSplit(SeparationScriptsFolder, "tables.txt"))
            {
                str.Append(table);
                str.Append(separator);
            }
            string finalString = str.ToString();

            if (!String.IsNullOrEmpty(separator))
            {
                finalString = finalString.Substring(0, finalString.Length - separator.Length);
            }

            return finalString;
        }


        /// <summary>
        /// Get table schemas
        /// </summary>
        private void GetTableSchemas(IEnumerable<TableAndClass> tableList)
        {
            foreach (var table in tableList)
            {
                table.ClassInfo = DataClassInfoProvider.GetClasses()
                                                       .WhereEquals("ClassTableName", table.TableName)
                                                       .FirstObject;
            }
        }
    }
}
