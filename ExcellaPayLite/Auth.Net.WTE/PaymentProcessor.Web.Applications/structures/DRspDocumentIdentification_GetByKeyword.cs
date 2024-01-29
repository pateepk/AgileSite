namespace PaymentProcessor.Web.Applications
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Data;
    using System.Data.Sql;
    using System.Data.SqlClient;
    using System.Xml;

	public class DRspDocumentIdentification_GetByKeyword : TableReaderBase 
	{
		public class Columns 
		{ 
			public const string DocumentID = "DocumentID";
			public const string InsertDate = "InsertDate";
			public const string DocumentType = "DocumentType";
            public const string DocumentTypeID = "DocumentTypeID";
			public const string RecipientID = "RecipientID";
			public const string RecipientName = "RecipientName";
			public const string MID = "MID";
			public const string OtherMIDs = "OtherMIDs";
            public const string XMLRelatedDocuments = "XMLRelatedDocuments";
            public const string IncomingDocumentID = "IncomingDocumentID";
		}

        public class Tags
        {
            public const string Documents = "Documents";
            public const string DocumentTypeID = "DocumentTypeID";
            public const string DocumentID = "DocumentID";
        }

		public DRspDocumentIdentification_GetByKeyword(DataSet ds)
		{
			base.setData(ds);
		}

		public Int64 DocumentID(int index) 
		{
			return base.getValueInteger64(index, Columns.DocumentID);
		}

		public DateTime InsertDate(int index) 
		{
			return base.getValueDate(index, Columns.InsertDate);
		}

		public string DocumentType(int index) 
		{
			return base.getValue(index, Columns.DocumentType);
		}

        public int DocumentTypeID(int index)
        {
            return base.getValueInteger(index, Columns.DocumentTypeID);
        }

		public int RecipientID(int index) 
		{
			return base.getValueInteger(index, Columns.RecipientID);
		}

		public string RecipientName(int index) 
		{
			return base.getValue(index, Columns.RecipientName);
		}

		public string MID(int index) 
		{
			return base.getValue(index, Columns.MID);
		}

		public string OtherMIDs(int index) 
		{
			return base.getValue(index, Columns.OtherMIDs);
		}

        public XmlDocument XMLRelatedDocuments(int index)
        {
            return base.getXmlDocument(index, Columns.XMLRelatedDocuments);
        }

        public Int64 IncomingDocumentID(int index)
        {
            return base.getValueInteger64(index, Columns.IncomingDocumentID);
        }


	}
}
