using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Data.SqlTypes;



namespace PaymentProcessor.Web.Applications 
{
	public class DRspTA_RecurringTransactions_GetByRecurringTransactionID : TableReaderBase 
	{
		public class Columns 
		{ 
			public const string RecurringTransactionID = "RecurringTransactionID";
			public const string IsDeleted = "IsDeleted";
			public const string PayTraceProfileRequestID = "PayTraceProfileRequestID";
			public const string PayTraceCreateRequestID = "PayTraceCreateRequestID";
			public const string PayTraceDeleteRequestID = "PayTraceDeleteRequestID";
			public const string UserGUID = "UserGUID";
			public const string CUSTID = "CUSTID";
			public const string CC = "CC";
			public const string START = "START";
			public const string AMOUNT = "AMOUNT";
			public const string RecurringFrequencyID = "RecurringFrequencyID";
			public const string TOTALCOUNT = "TOTALCOUNT";
			public const string TRANXTYPE = "TRANXTYPE";
			public const string CUSTRECEIPT = "CUSTRECEIPT";
			public const string LastCheckDate = "LastCheckDate";
			public const string PayTraceRECURID = "PayTraceRECURID";
			public const string PayTraceNEXT = "PayTraceNEXT";
			public const string PayTraceTOTALCOUNT = "PayTraceTOTALCOUNT";
			public const string PayTraceCURRENTCOUNT = "PayTraceCURRENTCOUNT";
			public const string PayTraceREPEAT = "PayTraceREPEAT";
			public const string BNAME = "BNAME";
		}

		public DRspTA_RecurringTransactions_GetByRecurringTransactionID(DataSet ds)
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

		public DateTime START(int index) 
		{
			return base.getValueDate(index, Columns.START);
		}

		public string AMOUNT(int index) 
		{
			return base.getValue(index, Columns.AMOUNT);
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

	}
}
