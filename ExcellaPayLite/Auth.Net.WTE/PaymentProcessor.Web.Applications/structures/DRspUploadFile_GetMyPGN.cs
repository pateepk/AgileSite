using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;



namespace PaymentProcessor.Web.Applications 
{
	public class DRspUploadFile_GetMyPGN : TableReaderBase 
	{
		public class Columns 
		{ 
			public const string UploadFileID = "UploadFileID";
			public const string PGNFileID = "PGNFileID";
			public const string CEvent = "CEvent";
			public const string White = "White";
			public const string Black = "Black";
			public const string WhiteRating = "WhiteRating";
			public const string BlackRating = "BlackRating";
			public const string CRound = "CRound";
			public const string Result = "Result";
		}

		public DRspUploadFile_GetMyPGN(DataSet ds)
		{
			base.setData(ds);
		}

		public int UploadFileID(int index) 
		{
			return base.getValueInteger(index, Columns.UploadFileID);
		}

		public int PGNFileID(int index) 
		{
			return base.getValueInteger(index, Columns.PGNFileID);
		}

		public string CEvent(int index) 
		{
			return base.getValue(index, Columns.CEvent);
		}

		public string White(int index) 
		{
			return base.getValue(index, Columns.White);
		}

		public string Black(int index) 
		{
			return base.getValue(index, Columns.Black);
		}

		public int WhiteRating(int index) 
		{
			return base.getValueInteger(index, Columns.WhiteRating);
		}

		public int BlackRating(int index) 
		{
			return base.getValueInteger(index, Columns.BlackRating);
		}

		public int CRound(int index) 
		{
			return base.getValueInteger(index, Columns.CRound);
		}

		public string Result(int index) 
		{
			return base.getValue(index, Columns.Result);
		}

	}
}
