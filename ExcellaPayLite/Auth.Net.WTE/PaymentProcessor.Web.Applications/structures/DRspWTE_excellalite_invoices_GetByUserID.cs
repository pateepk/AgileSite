using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;



namespace PaymentProcessor.Web.Applications 
{
	public class DRspWTE_excellalite_invoices_GetByUserID : TableReaderBase 
	{
		public class Columns 
		{ 
			public const string invoiceID = "invoiceID";
			public const string CustNum = "CustNum";
			public const string Invoice = "Invoice";
			public const string Project = "Project";
			public const string DateDue = "DateDue";
			public const string Terms = "Terms";
            public const string AmountDue = "AmountDue";
            public const string AmountPaid = "AmountPaid";
            public const string AmountInTransit = "AmountInTransit";
			public const string docname = "docname";
			public const string invoiceDate = "invoiceDate";
			public const string InvoiceTotal = "InvoiceTotal";
            public const string InvoiceStatusID = "InvoiceStatusID";
            public const string InvoiceStatus = "InvoiceStatus";
		}

		public DRspWTE_excellalite_invoices_GetByUserID(DataSet ds)
		{
			base.setData(ds);
		}

		public int invoiceID(int index) 
		{
			return base.getValueInteger(index, Columns.invoiceID);
		}

		public string CustNum(int index) 
		{
			return base.getValue(index, Columns.CustNum);
		}

		public string Invoice(int index) 
		{
			return base.getValue(index, Columns.Invoice);
		}

		public string Project(int index) 
		{
			return base.getValue(index, Columns.Project);
		}

		public DateTime DateDue(int index) 
		{
			return base.getValueDate(index, Columns.DateDue);
		}

		public int Terms(int index) 
		{
			return base.getValueInteger(index, Columns.Terms);
		}

        public decimal AmountDue(int index)
        {
            return base.getValueDecimal(index, Columns.AmountDue);
        }

        public decimal AmountPaid(int index) 
		{
			return base.getValueDecimal(index, Columns.AmountPaid);
		}

        public decimal AmountInTransit(int index)
        {
            return base.getValueDecimal(index, Columns.AmountInTransit);
        }

		public string docname(int index) 
		{
			return base.getValue(index, Columns.docname);
		}

		public DateTime invoiceDate(int index) 
		{
			return base.getValueDate(index, Columns.invoiceDate);
		}

		public decimal InvoiceTotal(int index) 
		{
			return base.getValueDecimal(index, Columns.InvoiceTotal);
		}

        public int InvoiceStatusID(int index)
        {
            return base.getValueInteger(index, Columns.InvoiceStatusID);
        }

        public string InvoiceStatus(int index)
        {
            return base.getValue(index, Columns.InvoiceStatus);
        }

	}
}
