using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;



namespace PaymentProcessor.Web.Applications 
{
	public class DRspWTE_excellalite_invoices_GetCustNum : TableReaderBase 
	{
		public class Columns 
		{ 
			public const string CustNum = "CustNum";
			public const string Total = "Total";
		}

		public DRspWTE_excellalite_invoices_GetCustNum(DataSet ds)
		{
			base.setData(ds);
		}

		public string CustNum(int index) 
		{
			return base.getValue(index, Columns.CustNum);
		}

		public int Total(int index) 
		{
			return base.getValueInteger(index, Columns.Total);
		}

	}
}
