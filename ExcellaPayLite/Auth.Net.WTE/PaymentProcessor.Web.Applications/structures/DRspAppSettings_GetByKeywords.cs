namespace PaymentProcessor.Web.Applications
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Data;
    using System.Data.Sql;
    using System.Data.SqlClient;

	public class DRspAppSettings_GetByKeywords : TableReaderBase 
	{
		public class Columns 
		{ 
			public const string Name = "Name";
			public const string Value = "Value";
		}

        public DRspAppSettings_GetByKeywords(DataSet ds)
		{
			base.setData(ds);
		}

		public string Name(int index) 
		{
			return base.getValue(index, Columns.Name);
		}

		public string Value(int index) 
		{
			return base.getValue(index, Columns.Value);
		}

	}
}
