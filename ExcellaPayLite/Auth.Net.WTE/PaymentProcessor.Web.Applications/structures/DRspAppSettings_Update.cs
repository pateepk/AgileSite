namespace PaymentProcessor.Web.Applications
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Data;
    using System.Data.Sql;
    using System.Data.SqlClient;

	public class DRspAppSettings_Update : TableReaderBase 
	{
		public class Columns 
		{ 
			public const string RowUpdated = "RowUpdated";
		}

		public DRspAppSettings_Update(DataSet ds)
		{
			base.setData(ds);
		}

		public int RowUpdated(int index) 
		{
			return base.getValueInteger(index, Columns.RowUpdated);
		}

	}
}