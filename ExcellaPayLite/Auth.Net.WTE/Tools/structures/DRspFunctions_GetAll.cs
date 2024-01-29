using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;


namespace ssch.tools
{
    public class DRspFunctions_GetAll : TableReaderBase
    {
        public class Columns
        {
            public const string ROUTINE_SCHEMA = "ROUTINE_SCHEMA";
            public const string ROUTINE_NAME = "ROUTINE_NAME";
            public const string CREATED = "CREATED";
            public const string LAST_ALTERED = "LAST_ALTERED";
        }

        public DRspFunctions_GetAll(DataSet ds)
        {
            base.setData(ds);
        }

        public string ROUTINE_SCHEMA(int index)
        {
            return base.getValue(index, Columns.ROUTINE_SCHEMA);
        }

        public string ROUTINE_NAME(int index)
        {
            return base.getValue(index, Columns.ROUTINE_NAME);
        }

        public DateTime CREATED(int index)
        {
            return base.getValueDate(index, Columns.CREATED);
        }

        public DateTime LAST_ALTERED(int index)
        {
            return base.getValueDate(index, Columns.LAST_ALTERED);
        }

    }
}
