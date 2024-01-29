namespace PaymentProcessor.Web.Applications
{
    
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Data;
    using System.Data.Sql;
    using System.Data.SqlClient;

    public class DRspUSStates_Select : TableReaderBase
    {
        public class Columns
        {
            public const string StateID = "StateID";
            public const string Abbreviation = "Abbreviation";
            public const string Name = "Name";
        }

        public DRspUSStates_Select(DataSet ds)
        {
            base.setData(ds);
        }

        public int StateID(int index)
        {
            return base.getValueInteger(index, Columns.StateID);
        }

        public string Abbreviation(int index)
        {
            return base.getValue(index, Columns.Abbreviation);
        }

        public string Name(int index)
        {
            return base.getValue(index, Columns.Name);
        }

    }
}
