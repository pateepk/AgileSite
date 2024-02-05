namespace PaymentProcessor.Web.Applications
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Data;
    using System.Data.Sql;
    using System.Data.SqlClient;

	public class DRspDocumentUserComment_GetByDocumentID : TableReaderBase 
	{
		public class Columns 
		{ 
			public const string DocumentUserCommentID = "DocumentUserCommentID";
			public const string DocumentID = "DocumentID";
			public const string UserID = "UserID";
			public const string CommentDate = "CommentDate";
			public const string Comment = "Comment";
            public const string CommentUser = "CommentUser";
		}

		public DRspDocumentUserComment_GetByDocumentID(DataSet ds)
		{
			base.setData(ds);
		}

		public Int64 DocumentUserCommentID(int index) 
		{
			return base.getValueInteger64(index, Columns.DocumentUserCommentID);
		}

		public Int64 DocumentID(int index) 
		{
			return base.getValueInteger64(index, Columns.DocumentID);
		}

		public int UserID(int index) 
		{
			return base.getValueInteger(index, Columns.UserID);
		}

		public DateTime CommentDate(int index) 
		{
			return base.getValueDate(index, Columns.CommentDate);
		}

		public string Comment(int index) 
		{
			return base.getValue(index, Columns.Comment);
		}

        public string CommentUser(int index)
        {
            return base.getValue(index, Columns.CommentUser);
        }

	}
}
