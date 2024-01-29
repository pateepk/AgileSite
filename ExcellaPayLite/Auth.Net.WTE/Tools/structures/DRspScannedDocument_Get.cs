using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;


namespace ssch.tools
{
	public class DRspScannedDocument_Get : TableReaderBase 
	{
		public class Columns 
		{ 
			public const string ScannedDocumentID = "ScannedDocumentID";
			public const string FullPath = "FullPath";
			public const string PrefixFolder = "PrefixFolder";
			public const string FirstName = "FirstName";
			public const string LastName = "LastName";
            public const string MID = "MID";
			public const string RecipientID = "RecipientID";
		}

		public DRspScannedDocument_Get(DataSet ds)
		{
			base.setData(ds);
		}

		public int ScannedDocumentID(int index) 
		{
			return base.getValueInteger(index, Columns.ScannedDocumentID);
		}

		public string FullPath(int index) 
		{
			return base.getValue(index, Columns.FullPath);
		}

		public string PrefixFolder(int index) 
		{
			return base.getValue(index, Columns.PrefixFolder);
		}

		public string FirstName(int index) 
		{
			return base.getValue(index, Columns.FirstName);
		}

		public string LastName(int index) 
		{
			return base.getValue(index, Columns.LastName);
		}

        public string MID(int index)
        {
            return base.getValue(index, Columns.MID);
        }

		public int RecipientID(int index) 
		{
			return base.getValueInteger(index, Columns.RecipientID);
		}

	}
}
