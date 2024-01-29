using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Data.SqlTypes;



namespace PaymentProcessor.Web.Applications 
{
	public class DRspTA_CheckTransactions_GetByUserGUID : TableReaderBase 
	{
		public class Columns 
		{ 
			public const string CheckTransactionID = "CheckTransactionID";
			public const string SiteID = "SiteID";
			public const string EgivingsID = "EgivingsID";
			public const string INVOICE = "INVOICE";
			public const string PayTraceRequestID = "PayTraceRequestID";
			public const string UserGUID = "UserGUID";
			public const string BNAME = "BNAME";
			public const string DDA = "DDA";
			public const string TR = "TR";
			public const string AMOUNT = "AMOUNT";
			public const string BADDRESS = "BADDRESS";
			public const string BZIP = "BZIP";
			public const string PayTraceCHECKIDENTIFIER = "PayTraceCHECKIDENTIFIER";
			public const string PayTraceRESPONSE = "PayTraceRESPONSE";
			public const string Address2 = "Address2";
			public const string City = "City";
			public const string State = "State";
			public const string Email = "Email";
			public const string Phone = "Phone";
			public const string ManualNote = "ManualNote";
			public const string ResponseString = "ResponseString";
			public const string TransactionDate = "TransactionDate";
			public const string EgivingsName = "EgivingsName";
			public const string EgivingsDescription = "EgivingsDescription";
		}

		public DRspTA_CheckTransactions_GetByUserGUID(DataSet ds)
		{
			base.setData(ds);
		}

		public int CheckTransactionID(int index) 
		{
			return base.getValueInteger(index, Columns.CheckTransactionID);
		}

		public int SiteID(int index) 
		{
			return base.getValueInteger(index, Columns.SiteID);
		}

		public int EgivingsID(int index) 
		{
			return base.getValueInteger(index, Columns.EgivingsID);
		}

		public string INVOICE(int index) 
		{
			return base.getValue(index, Columns.INVOICE);
		}

		public int PayTraceRequestID(int index) 
		{
			return base.getValueInteger(index, Columns.PayTraceRequestID);
		}

        public SqlGuid UserGUID(int index)
        {
            return base.getSqlGuid(index, Columns.UserGUID);
        }

		public string BNAME(int index) 
		{
			return base.getValue(index, Columns.BNAME);
		}

		public string DDA(int index) 
		{
			return base.getValue(index, Columns.DDA);
		}

		public string TR(int index) 
		{
			return base.getValue(index, Columns.TR);
		}

		public string AMOUNT(int index) 
		{
			return base.getValue(index, Columns.AMOUNT);
		}

		public string BADDRESS(int index) 
		{
			return base.getValue(index, Columns.BADDRESS);
		}

		public string BZIP(int index) 
		{
			return base.getValue(index, Columns.BZIP);
		}

		public string PayTraceCHECKIDENTIFIER(int index) 
		{
			return base.getValue(index, Columns.PayTraceCHECKIDENTIFIER);
		}

		public string PayTraceRESPONSE(int index) 
		{
			return base.getValue(index, Columns.PayTraceRESPONSE);
		}

		public string Address2(int index) 
		{
			return base.getValue(index, Columns.Address2);
		}

		public string City(int index) 
		{
			return base.getValue(index, Columns.City);
		}

		public string State(int index) 
		{
			return base.getValue(index, Columns.State);
		}

		public string Email(int index) 
		{
			return base.getValue(index, Columns.Email);
		}

		public string Phone(int index) 
		{
			return base.getValue(index, Columns.Phone);
		}

		public string ManualNote(int index) 
		{
			return base.getValue(index, Columns.ManualNote);
		}

		public string ResponseString(int index) 
		{
			return base.getValue(index, Columns.ResponseString);
		}

		public DateTime TransactionDate(int index) 
		{
			return base.getValueDate(index, Columns.TransactionDate);
		}

		public string EgivingsName(int index) 
		{
			return base.getValue(index, Columns.EgivingsName);
		}

		public string EgivingsDescription(int index) 
		{
			return base.getValue(index, Columns.EgivingsDescription);
		}

	}
}
