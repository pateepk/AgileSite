using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;



namespace PaymentProcessor.Web.Applications 
{
	public class DRspWTE_excellalite_invoices_items_Fail : TableReaderBase 
	{
		public class Columns 
		{ 
			public const string InvoiceItemID = "InvoiceItemID";
			public const string RowUpdated = "RowUpdated";
			public const string UserID = "UserID";
            public const string InvoiceID = "InvoiceID";
            public const string CustNum = "CustNum";
		}

		public DRspWTE_excellalite_invoices_items_Fail(DataSet ds)
		{
			base.setData(ds);
		}

		public int InvoiceItemID(int index) 
		{
			return base.getValueInteger(index, Columns.InvoiceItemID);
		}

		public int RowUpdated(int index) 
		{
			return base.getValueInteger(index, Columns.RowUpdated);
		}

		public int UserID(int index) 
		{
			return base.getValueInteger(index, Columns.UserID);
		}

        public int InvoiceID(int index)
        {
            return base.getValueInteger(index, Columns.InvoiceID);
        }

        public string CustNum(int index)
        {
            return base.getValue(index, Columns.CustNum);
        }

	}
}
