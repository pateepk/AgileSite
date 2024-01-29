namespace PaymentProcessor.Web.Applications
{

    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Data;
    using System.Data.Sql;
    using System.Data.SqlClient;

	public class DRspWebsites_SetUpgrade : TableReaderBase 
	{
		public class Columns 
		{ 
			public const string IsCompleted = "IsCompleted";
		}

		public DRspWebsites_SetUpgrade(DataSet ds)
		{
			base.setData(ds);
		}

		public bool IsCompleted(int index) 
		{
			return base.getValueBool(index, Columns.IsCompleted);
		}

	}
}
