namespace PaymentProcessor.Web.Applications
{

    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Data;
    using System.Data.Sql;
    using System.Data.SqlClient;

	public class DRspWebErrorLogs_Select : TableReaderBase 
	{
		public class Columns 
		{ 
			public const string WebErrorID = "WebErrorID";
			public const string WebErrorDate = "WebErrorDate";
			public const string WebErrorMessage = "WebErrorMessage";
			public const string WebEnvironment = "WebEnvironment";
			public const string WebServer = "WebServer";
			public const string IPAddress = "IPAddress";
			public const string WebUser = "WebUser";
			public const string WebPage = "WebPage";
            public const string HttpMethod = "HttpMethod";
		}

		public DRspWebErrorLogs_Select(DataSet ds)
		{
			base.setData(ds);
		}

		public Int64 WebErrorID(int index) 
		{
			return base.getValueInteger64(index, Columns.WebErrorID);
		}

		public DateTime WebErrorDate(int index) 
		{
			return base.getValueDate(index, Columns.WebErrorDate);
		}

		public string WebErrorMessage(int index) 
		{
			return base.getValue(index, Columns.WebErrorMessage);
		}

		public string WebEnvironment(int index) 
		{
			return base.getValue(index, Columns.WebEnvironment);
		}

		public string WebServer(int index) 
		{
			return base.getValue(index, Columns.WebServer);
		}

		public string IPAddress(int index) 
		{
			return base.getValue(index, Columns.IPAddress);
		}

		public string WebUser(int index) 
		{
			return base.getValue(index, Columns.WebUser);
		}

		public string WebPage(int index) 
		{
			return base.getValue(index, Columns.WebPage);
		}

        public HttpMethods HttpMethod(int index)
        {
            int hm = base.getValueInteger(index, Columns.HttpMethod);
            return (HttpMethods)Enum.Parse(typeof(HttpMethods), hm.ToString()); 
        }

	}
}
