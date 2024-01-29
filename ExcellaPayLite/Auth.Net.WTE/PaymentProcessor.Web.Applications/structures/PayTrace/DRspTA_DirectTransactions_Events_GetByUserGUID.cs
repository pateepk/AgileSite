using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Data.SqlTypes;



namespace PaymentProcessor.Web.Applications 
{
	public class DRspTA_DirectTransactions_Events_GetByUserGUID : TableReaderBase 
	{
		public class Columns 
		{ 
			public const string DirectTransactions_EventsID = "DirectTransactions_EventsID";
			public const string SiteID = "SiteID";
			public const string Egiving_EventsID = "Egiving_EventsID";
			public const string INVOICE = "INVOICE";
			public const string PayTraceRequestID = "PayTraceRequestID";
			public const string UserGUID = "UserGUID";
			public const string BNAME = "BNAME";
			public const string CC = "CC";
			public const string EXPMNTH = "EXPMNTH";
			public const string EXPYR = "EXPYR";
			public const string AMOUNT = "AMOUNT";
			public const string CSC = "CSC";
			public const string BADDRESS = "BADDRESS";
			public const string BZIP = "BZIP";
			public const string PayTraceTRANSACTIONID = "PayTraceTRANSACTIONID";
			public const string PayTraceAPPMSG = "PayTraceAPPMSG";
			public const string Address2 = "Address2";
			public const string City = "City";
			public const string State = "State";
			public const string Email = "Email";
			public const string Phone = "Phone";
			public const string ResponseString = "ResponseString";
			public const string TransactionDate = "TransactionDate";
			public const string EventName = "EventName";
			public const string EventDescription = "EventDescription";
		}

		public DRspTA_DirectTransactions_Events_GetByUserGUID(DataSet ds)
		{
			base.setData(ds);
		}

		public int DirectTransactions_EventsID(int index) 
		{
			return base.getValueInteger(index, Columns.DirectTransactions_EventsID);
		}

		public int SiteID(int index) 
		{
			return base.getValueInteger(index, Columns.SiteID);
		}

		public int Egiving_EventsID(int index) 
		{
			return base.getValueInteger(index, Columns.Egiving_EventsID);
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

		public string PayTraceTRANSACTIONID(int index) 
		{
			return base.getValue(index, Columns.PayTraceTRANSACTIONID);
		}

		public string PayTraceAPPMSG(int index) 
		{
			return base.getValue(index, Columns.PayTraceAPPMSG);
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

		public string ResponseString(int index) 
		{
			return base.getValue(index, Columns.ResponseString);
		}

		public DateTime TransactionDate(int index) 
		{
			return base.getValueDate(index, Columns.TransactionDate);
		}

		public string EventName(int index) 
		{
			return base.getValue(index, Columns.EventName);
		}

		public string EventDescription(int index) 
		{
			return base.getValue(index, Columns.EventDescription);
		}

	}
}
