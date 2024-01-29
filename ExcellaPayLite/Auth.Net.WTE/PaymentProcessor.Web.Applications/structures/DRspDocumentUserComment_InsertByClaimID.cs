namespace PaymentProcessor.Web.Applications
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Data;
    using System.Data.Sql;
    using System.Data.SqlClient;

	public class DRspDocumentUserComment_InsertByClaimID : TableReaderBase 
	{
		public class Columns 
		{ 
			public const string DocumentUserCommentID = "DocumentUserCommentID";
		}

		public DRspDocumentUserComment_InsertByClaimID(DataSet ds)
		{
			base.setData(ds);
		}

		public Int64 DocumentUserCommentID(int index) 
		{
			return base.getValueInteger64(index, Columns.DocumentUserCommentID);
		}

	}
}
