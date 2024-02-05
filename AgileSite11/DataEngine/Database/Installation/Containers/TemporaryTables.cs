using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

using CMS.Helpers;
using CMS.IO;

namespace CMS.DataEngine
{
    internal static class TemporaryTables
    {
        #region "Constants"

        /// <summary>
        /// Key for persistent storage helper containing FK's deleted during DB separation.
        /// </summary>
        private const string DELETED_FKS = "DBSeparationFKs";

        #endregion


        #region "Public methods"

        /// <summary>
        /// Creates all required temporary tables for DB separation.
        /// </summary>
        /// <param name="scriptsFolder">Scripts folder</param>
        /// <returns>Returns list of temporary table names.</returns>
        public static List<string> CreateTemporaryTables(string scriptsFolder)
        {
            var tables = FileReader.ReadAndSplit(scriptsFolder, "temporary_tables.txt");
            foreach (var table in tables)
            {
                CreateTemporaryTable(table);
            }
            return tables;
        }


        /// <summary>
        /// Removes temporary tables.
        /// </summary>
        /// <param name="temporaryTables">List of temporary tables</param>
        public static void RemoveTemporaryTables(List<string> temporaryTables)
        {
            RemoveFKs(temporaryTables);
            DeleteTemporaryTables(temporaryTables);
        }


        /// <summary>
        /// Recreates additional FK constraints which were stored during DB separation.
        /// </summary>
        public static void CreateAdditionalConstraints()
        {
            foreach (DeletedFKs deletedFK in GetConstraints())
            {
                ConnectionHelper.ExecuteQuery(
@"IF (EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '" + deletedFK.ReferencingTable + "' AND COLUMN_NAME = '" + deletedFK.ReferencingColumn + @"') 
AND EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '" + deletedFK.PKTable + "' AND COLUMN_NAME = '" + deletedFK.FKName + @"')) 
BEGIN	
    IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[" + deletedFK.FKName + @"]') AND parent_object_id = OBJECT_ID(N'[" + deletedFK.ReferencingTable + @"]'))
    BEGIN
        ALTER TABLE [" + deletedFK.ReferencingTable + "] ADD CONSTRAINT [" + deletedFK.FKName + @"]
            FOREIGN KEY ([" + deletedFK.ReferencingColumn + "]) REFERENCES [" + deletedFK.PKTable + @"]
                (" + deletedFK.PKColumn + @")
    END
END", null, QueryTypeEnum.SQLQuery);
            }
            PersistentStorageHelper.RemoveValue(DELETED_FKS);
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Creates temporary table.
        /// </summary>
        private static void CreateTemporaryTable(string table)
        {
            string[] tableColumn = table.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            string queryText = "CREATE TABLE  [" + tableColumn[0] + "] ([" + tableColumn[1] + "] [int] IDENTITY(1, 1) NOT NULL)";
            ConnectionHelper.ExecuteQuery(queryText, null, QueryTypeEnum.SQLQuery);
            queryText = "ALTER TABLE  [" + tableColumn[0] + "] ADD CONSTRAINT [PK_" + tableColumn[1] + "] PRIMARY KEY CLUSTERED ([" + tableColumn[1] + "])";
            ConnectionHelper.ExecuteQuery(queryText, null, QueryTypeEnum.SQLQuery);
        }


        /// <summary>
        /// Remove FKs referencing temporary tables.
        /// </summary>
        private static void RemoveFKs(IEnumerable<string> temporaryTables)
        {
            foreach (var temporaryTable in temporaryTables)
            {
                string[] tableColumn = temporaryTable.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                string queryText =
@"
select t.name as TableWithForeignKey, fk.name as FKname, col.name as FKcolumn, pkt.name as PKTable, ccu.COLUMN_NAME as PKColumn
from sys.foreign_key_columns as c
inner join sys.tables as t on c.parent_object_id = t.object_id
inner join sys.objects as fk on  c.constraint_object_id = fk.object_id
inner join sys.columns as col on c.parent_object_id = col.object_id and c.parent_column_id = col.column_id
inner join sys.tables as pkt on pkt.object_id =  c.referenced_object_id
inner join INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc on tc.TABLE_NAME = pkt.name AND tc.CONSTRAINT_TYPE = 'PRIMARY KEY'
inner join INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE ccu ON tc.CONSTRAINT_NAME = ccu.Constraint_name
where pkt.name = '" + tableColumn[0] + @"'";
                DataSet fks = ConnectionHelper.ExecuteQuery(queryText, null, QueryTypeEnum.SQLQuery);
                DeleteFKs(fks);
            }
        }


        /// <summary>
        /// Deletes FKs.
        /// </summary>
        private static void DeleteFKs(DataSet fks)
        {
            if (!DataHelper.DataSourceIsEmpty(fks))
            {
                var deletedFKs = new List<DeletedFKs>();
                foreach (DataRow row in fks.Tables[0].Rows)
                {
                    ConnectionHelper.ExecuteQuery("alter table [" + row[0] + "] drop [" + row[1] + "]", null, QueryTypeEnum.SQLQuery);
                    InitializeFK(deletedFKs, row);
                }
                StoreRemovedFKs(deletedFKs);
            }
        }


        /// <summary>
        /// Adds new FK reference to list.
        /// </summary>
        private static void InitializeFK(List<DeletedFKs> deletedFKs, DataRow row)
        {
            var fk = new DeletedFKs();
            fk.ReferencingTable = ValidationHelper.GetString(row[0], null);
            fk.FKName = ValidationHelper.GetString(row[1], null);
            fk.ReferencingColumn = ValidationHelper.GetString(row[2], null);
            fk.PKTable = ValidationHelper.GetString(row[3], null);
            fk.PKColumn = ValidationHelper.GetString(row[4], null);
            deletedFKs.Add(fk);
        }


        /// <summary>
        /// Stores deleted FKs.
        /// </summary>
        private static void StoreRemovedFKs(List<DeletedFKs> deletedFKS)
        {
            var ser = new XmlSerializer(typeof(List<DeletedFKs>));
            var sb = new StringBuilder();
            var writer = new StringWriter(sb);
            ser.Serialize(writer, deletedFKS);
            PersistentStorageHelper.SetValue(DELETED_FKS, sb.ToString());
        }


        /// <summary>
        /// Deletes temporary tables.
        /// </summary>
        private static void DeleteTemporaryTables(IEnumerable<string> temporaryTables)
        {
            foreach (var temporaryTable in temporaryTables)
            {
                string[] tableColumn = temporaryTable.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                ConnectionHelper.ExecuteQuery(@"IF OBJECT_ID('" + tableColumn[0] + @"','U') IS NOT NULL 
                                                        DROP TABLE [" + tableColumn[0] + "]", null, QueryTypeEnum.SQLQuery);
            }
        }


        /// <summary>
        /// Gets constraints from persistent storage helper.
        /// </summary>
        private static IEnumerable<DeletedFKs> GetConstraints()
        {
            try
            {
                var doc = new XmlDocument();
                doc.LoadXml(ValidationHelper.GetString(PersistentStorageHelper.GetValue(DELETED_FKS), String.Empty));
                if (doc.DocumentElement != null)
                {
                    var reader = new XmlNodeReader(doc.DocumentElement);
                    var ser = new XmlSerializer(typeof(List<DeletedFKs>));
                    return (List<DeletedFKs>)ser.Deserialize(reader);
                }
                return new List<DeletedFKs>();
            }
            catch
            {
                return new List<DeletedFKs>();
            }
        }

        #endregion
    }
}
