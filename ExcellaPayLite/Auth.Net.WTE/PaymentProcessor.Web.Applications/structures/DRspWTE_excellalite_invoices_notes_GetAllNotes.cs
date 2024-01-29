using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;



namespace PaymentProcessor.Web.Applications 
{
	public class DRspWTE_excellalite_invoices_notes_GetAllNotes : TableReaderBase 
	{
		public class Columns 
		{ 
			public const string invoiceNoteID = "invoiceNoteID";
			public const string invoiceID = "invoiceID";
			public const string invoiceItemID = "invoiceItemID";
			public const string CreatedDate = "CreatedDate";
			public const string NoteUserID = "NoteUserID";
			public const string invoiceNote = "invoiceNote";
			public const string FullName = "FullName";
		}

		public DRspWTE_excellalite_invoices_notes_GetAllNotes(DataSet ds)
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

		public DateTime CreatedDate(int index) 
		{
			return base.getValueDate(index, Columns.CreatedDate);
		}

		public int NoteUserID(int index) 
		{
			return base.getValueInteger(index, Columns.NoteUserID);
		}

		public string invoiceNote(int index) 
		{
			return base.getValue(index, Columns.invoiceNote);
		}

		public string FullName(int index) 
		{
			return base.getValue(index, Columns.FullName);
		}

	}
}
