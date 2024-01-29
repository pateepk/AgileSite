using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;



namespace ssch.tools
{
	public class DRspScannedDocument_Insert : TableReaderBase 
	{
		public class Columns 
		{ 
			public const string ScannedDocumentID = "ScannedDocumentID";
		}

		public DRspScannedDocument_Insert(DataSet ds)
		{
			base.setData(ds);
		}

		public int ScannedDocumentID(int index) 
		{
			return base.getValueInteger(index, Columns.ScannedDocumentID);
		}

	}
}
