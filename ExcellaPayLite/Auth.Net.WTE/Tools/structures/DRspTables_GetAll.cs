using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;


namespace ssch.tools
{
    public class DRspTables_GetAll : TableReaderBase
    {
        private class Columns
        {
            public const string name = "name";
            public const string create_date = "create_date";
            public const string modify_date = "modify_date";
            public const string schema = "schema";
        }

        public DRspTables_GetAll(DataSet ds)
        {
            base.setData(ds);
        }

        public string name(int index)
        {
            return base.getValue(index, Columns.name);
        }

        public DateTime create_date(int index)
        {
            return base.getValueDate(index, Columns.create_date);
        }

        public DateTime modify_date(int index)
        {
            return base.getValueDate(index, Columns.modify_date);
        }

        public string schema(int index)
        {
            return base.getValue(index, Columns.schema);
        }

    }
}
