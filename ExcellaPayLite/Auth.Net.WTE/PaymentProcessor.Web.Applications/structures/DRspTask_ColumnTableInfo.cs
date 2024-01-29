using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;



namespace PaymentProcessor.Web.Applications 
{
	public class DRspTask_ColumnTableInfo : TableReaderBase 
	{
		public class Columns 
		{ 
			public const string AllColumns = "AllColumns";
		}

		public DRspTask_ColumnTableInfo(DataSet ds)
		{
			base.setData(ds);
		}

		public string AllColumns(int index) 
		{
			return base.getValue(index, Columns.AllColumns);
		}

	}
}
