using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;



namespace PaymentProcessor.Web.Applications 
{
	public class DRspWTE_excellalite_invoices_notes_InsertItem : TableReaderBase 
	{
		public class Columns 
		{ 
			public const string invoiceNoteID = "invoiceNoteID";
			public const string invoiceID = "invoiceID";
			public const string invoiceItemID = "invoiceItemID";
		}

		public DRspWTE_excellalite_invoices_notes_InsertItem(DataSet ds)
		{
			base.setData(ds);
		}

		public Int64 invoiceNoteID(int index) 
		{
			return base.getValueInteger64(index, Columns.invoiceNoteID);
		}

		public int invoiceID(int index) 
		{
			return base.getValueInteger(index, Columns.invoiceID);
		}

		public int invoiceItemID(int index) 
		{
			return base.getValueInteger(index, Columns.invoiceItemID);
		}

	}
}
