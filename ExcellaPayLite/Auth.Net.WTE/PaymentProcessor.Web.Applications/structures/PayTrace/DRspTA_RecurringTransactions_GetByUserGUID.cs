using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Data.SqlTypes;



namespace PaymentProcessor.Web.Applications 
{
	public class DRspTA_RecurringTransactions_GetByUserGUID : TableReaderBase 
	{
		public class Columns 
		{ 
			public const string RecurringTransactionID = "RecurringTransactionID";
			public const string IsDeleted = "IsDeleted";
			public const string PayTraceProfileRequestID = "PayTraceProfileRequestID";
			public const string PayTraceCreateRequestID = "PayTraceCreateRequestID";
			public const string PayTraceDeleteRequestID = "PayTraceDeleteRequestID";
			public const string SiteID = "SiteID";
			public const string EgivingsID = "EgivingsID";
			public const string UserGUID = "UserGUID";
			public const string CUSTID = "CUSTID";
			public const string CC = "CC";
			public const string EXPMNTH = "EXPMNTH";
			public const string EXPYR = "EXPYR";
			public const string AMOUNT = "AMOUNT";
			public const string CSC = "CSC";
			public const string BADDRESS = "BADDRESS";
			public const string BZIP = "BZIP";
			public const string START = "START";
			public const string RecurringFrequencyID = "RecurringFrequencyID";
			public const string TOTALCOUNT = "TOTALCOUNT";
			public const string TRANXTYPE = "TRANXTYPE";
			public const string CUSTRECEIPT = "CUSTRECEIPT";
			public const string Address2 = "Address2";
			public const string City = "City";
			public const string State = "State";
			public const string Email = "Email";
			public const string Phone = "Phone";
			public const string LastCheckDate = "LastCheckDate";
			public const string PayTraceRECURID = "PayTraceRECURID";
			public const string PayTraceNEXT = "PayTraceNEXT";
			public const string PayTraceTOTALCOUNT = "PayTraceTOTALCOUNT";
			public const string PayTraceCURRENTCOUNT = "PayTraceCURRENTCOUNT";
			public const string PayTraceREPEAT = "PayTraceREPEAT";
			public const string BNAME = "BNAME";
			public const string ResponseString = "ResponseString";
			public const string TransactionDate = "TransactionDate";
			public const string EgivingsName = "EgivingsName";
			public const string EgivingsDescription = "EgivingsDescription";
			public const string RecurringFrequency = "RecurringFrequency";
		}

		public DRspTA_RecurringTransactions_GetByUserGUID(DataSet ds)
		{
			base.setData(ds);
		}

		public int RecurringTransactionID(int index) 
		{
			return base.getValueInteger(index, Columns.RecurringTransactionID);
		}

		public bool IsDeleted(int index) 
		{
			return base.getValueBool(index, Columns.IsDeleted);
		}

		public int PayTraceProfileRequestID(int index) 
		{
			return base.getValueInteger(index, Columns.PayTraceProfileRequestID);
		}

		public int PayTraceCreateRequestID(int index) 
		{
			return base.getValueInteger(index, Columns.PayTraceCreateRequestID);
		}

		public int PayTraceDeleteRequestID(int index) 
		{
			return base.getValueInteger(index, Columns.PayTraceDeleteRequestID);
		}

		public int SiteID(int index) 
		{
			return base.getValueInteger(index, Columns.SiteID);
		}

		public int EgivingsID(int index) 
		{
			return base.getValueInteger(index, Columns.EgivingsID);
		}

        public SqlGuid UserGUID(int index)
        {
            return base.getSqlGuid(index, Columns.UserGUID);
        }

		public string CUSTID(int index) 
		{
			return base.getValue(index, Columns.CUSTID);
		}

		public string CC(int index) 
		{
			return base.getValue(index, Columns.CC);
		}

		public string EXPMNTH(int index) 
		{
			return base.getValue(index, Columns.EXPMNTH);
		}

		public string EXPYR(int index) 
		{
			return base.getValue(index, Columns.EXPYR);
		}

		public string AMOUNT(int index) 
		{
			return base.getValue(index, Columns.AMOUNT);
		}

		public string CSC(int index) 
		{
			return base.getValue(index, Columns.CSC);
		}

		public string BADDRESS(int index) 
		{
			return base.getValue(index, Columns.BADDRESS);
		}

		public string BZIP(int index) 
		{
			return base.getValue(index, Columns.BZIP);
		}

		public DateTime START(int index) 
		{
			return base.getValueDate(index, Columns.START);
		}

		public string RecurringFrequencyID(int index) 
		{
			return base.getValue(index, Columns.RecurringFrequencyID);
		}

		public string TOTALCOUNT(int index) 
		{
			return base.getValue(index, Columns.TOTALCOUNT);
		}

		public string TRANXTYPE(int index) 
		{
			return base.getValue(index, Columns.TRANXTYPE);
		}

		public string CUSTRECEIPT(int index) 
		{
			return base.getValue(index, Columns.CUSTRECEIPT);
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

		public DateTime LastCheckDate(int index) 
		{
			return base.getValueDate(index, Columns.LastCheckDate);
		}

		public string PayTraceRECURID(int index) 
		{
			return base.getValue(index, Columns.PayTraceRECURID);
		}

		public DateTime PayTraceNEXT(int index) 
		{
			return base.getValueDate(index, Columns.PayTraceNEXT);
		}

		public string PayTraceTOTALCOUNT(int index) 
		{
			return base.getValue(index, Columns.PayTraceTOTALCOUNT);
		}

		public string PayTraceCURRENTCOUNT(int index) 
		{
			return base.getValue(index, Columns.PayTraceCURRENTCOUNT);
		}

		public string PayTraceREPEAT(int index) 
		{
			return base.getValue(index, Columns.PayTraceREPEAT);
		}

		public string BNAME(int index) 
		{
			return base.getValue(index, Columns.BNAME);
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

		public string RecurringFrequency(int index) 
		{
			return base.getValue(index, Columns.RecurringFrequency);
		}

	}
}
