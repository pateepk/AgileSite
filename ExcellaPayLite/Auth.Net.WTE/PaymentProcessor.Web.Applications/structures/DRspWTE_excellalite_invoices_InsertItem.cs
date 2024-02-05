using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;



namespace PaymentProcessor.Web.Applications 
{
	public class DRspWTE_excellalite_invoices_InsertItem : TableReaderBase 
	{
		public class Columns 
		{ 
			public const string invoiceItemID = "invoiceItemID";
		}

		public DRspWTE_excellalite_invoices_InsertItem(DataSet ds)
		{
			base.setData(ds);
		}

		public int invoiceItemID(int index) 
		{
			return base.getValueInteger(index, Columns.invoiceItemID);
		}

	}
}
