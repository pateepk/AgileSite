using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Data.SqlTypes;



namespace PaymentProcessor.Web.Applications 
{
	public class DRspTA_custom_egiving_Events_Get : TableReaderBase 
	{
		public class Columns 
		{ 
			public const string Egiving_EventsID = "Egiving_EventsID";
			public const string EventName = "EventName";
			public const string EventDate = "EventDate";
			public const string EventDescription = "EventDescription";
			public const string SellTicketStart = "SellTicketStart";
			public const string SellTicketStop = "SellTicketStop";
			public const string Number_of_Tickets = "Number_of_Tickets";
			public const string Ticket1Name = "Ticket1Name";
			public const string Ticket1Price = "Ticket1Price";
			public const string Ticket2Name = "Ticket2Name";
			public const string Ticket2Price = "Ticket2Price";
			public const string Ticket3Name = "Ticket3Name";
			public const string Ticket3Price = "Ticket3Price";
		}

		public DRspTA_custom_egiving_Events_Get(DataSet ds)
		{
			base.setData(ds);
		}

		public int Egiving_EventsID(int index) 
		{
			return base.getValueInteger(index, Columns.Egiving_EventsID);
		}

		public string EventName(int index) 
		{
			return base.getValue(index, Columns.EventName);
		}

		public DateTime EventDate(int index) 
		{
			return base.getValueDate(index, Columns.EventDate);
		}

		public string EventDescription(int index) 
		{
			return base.getValue(index, Columns.EventDescription);
		}

		public DateTime SellTicketStart(int index) 
		{
			return base.getValueDate(index, Columns.SellTicketStart);
		}

		public DateTime SellTicketStop(int index) 
		{
			return base.getValueDate(index, Columns.SellTicketStop);
		}

		public int Number_of_Tickets(int index) 
		{
			return base.getValueInteger(index, Columns.Number_of_Tickets);
		}

		public string Ticket1Name(int index) 
		{
			return base.getValue(index, Columns.Ticket1Name);
		}

		public decimal Ticket1Price(int index) 
		{
			return base.getValueDecimal(index, Columns.Ticket1Price);
		}

		public string Ticket2Name(int index) 
		{
			return base.getValue(index, Columns.Ticket2Name);
		}

		public decimal Ticket2Price(int index) 
		{
			return base.getValueDecimal(index, Columns.Ticket2Price);
		}

		public string Ticket3Name(int index) 
		{
			return base.getValue(index, Columns.Ticket3Name);
		}

		public decimal Ticket3Price(int index) 
		{
			return base.getValueDecimal(index, Columns.Ticket3Price);
		}

	}
}
