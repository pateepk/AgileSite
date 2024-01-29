using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;



namespace PaymentProcessor.Web.Applications 
{
	public class DRspWTE_excellalite_invoices_items_Delete : TableReaderBase 
	{
		public class Columns 
		{ 
			public const string InvoiceItemID = "InvoiceItemID";
			public const string RowDeleted = "RowDeleted";
			public const string UserID = "UserID";
		}

		public DRspWTE_excellalite_invoices_items_Delete(DataSet ds)
		{
			base.setData(ds);
		}

		public int InvoiceItemID(int index) 
		{
			return base.getValueInteger(index, Columns.InvoiceItemID);
		}

		public int RowDeleted(int index) 
		{
			return base.getValueInteger(index, Columns.RowDeleted);
		}

		public int UserID(int index) 
		{
			return base.getValueInteger(index, Columns.UserID);
		}

	}
}
