using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;



namespace PaymentProcessor.Web.Applications 
{
	public class DRspWTE_excellalite_invoices_items_GetByInvoiceID : TableReaderBase 
	{
		public class Columns 
		{ 
			public const string invoiceItemID = "invoiceItemID";
			public const string invoiceID = "invoiceID";
			public const string Amount = "Amount";
			public const string CreatedDate = "CreatedDate";
			public const string UserID = "UserID";
			public const string InvoiceItemStatusID = "InvoiceItemStatusID";
			public const string UserID1 = "UserID1";
			public const string PaidDate = "PaidDate";
			public const string PaidUserID = "PaidUserID";
			public const string PaidNote = "PaidNote";
			public const string InvoiceItemStatus = "InvoiceItemStatus";
			public const string PaymentUser = "PaymentUser";
			public const string PaidUser = "PaidUser";
		}

		public DRspWTE_excellalite_invoices_items_GetByInvoiceID(DataSet ds)
		{
			base.setData(ds);
		}

		public int invoiceItemID(int index) 
		{
			return base.getValueInteger(index, Columns.invoiceItemID);
		}

		public int invoiceID(int index) 
		{
			return base.getValueInteger(index, Columns.invoiceID);
		}

		public decimal Amount(int index) 
		{
			return base.getValueDecimal(index, Columns.Amount);
		}

		public DateTime CreatedDate(int index) 
		{
			return base.getValueDate(index, Columns.CreatedDate);
		}

		public int UserID(int index) 
		{
			return base.getValueInteger(index, Columns.UserID);
		}

		public int InvoiceItemStatusID(int index) 
		{
			return base.getValueInteger(index, Columns.InvoiceItemStatusID);
		}

		public int UserID1(int index) 
		{
			return base.getValueInteger(index, Columns.UserID1);
		}

		public DateTime PaidDate(int index) 
		{
			return base.getValueDate(index, Columns.PaidDate);
		}

		public int PaidUserID(int index) 
		{
			return base.getValueInteger(index, Columns.PaidUserID);
		}

		public string PaidNote(int index) 
		{
			return base.getValue(index, Columns.PaidNote);
		}

		public string InvoiceItemStatus(int index) 
		{
			return base.getValue(index, Columns.InvoiceItemStatus);
		}

		public string PaymentUser(int index) 
		{
			return base.getValue(index, Columns.PaymentUser);
		}

		public string PaidUser(int index) 
		{
			return base.getValue(index, Columns.PaidUser);
		}

	}
}
