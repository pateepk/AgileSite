namespace PaymentProcessor.Web.Applications
{

    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Data;
    using System.Data.Sql;
    using System.Data.SqlClient;

	public class DRspUsersRoles_InsertByLoginID : TableReaderBase 
	{
		public class Columns 
		{ 
			public const string UserRoleID = "UserRoleID";
		}

		public DRspUsersRoles_InsertByLoginID(DataSet ds)
		{
			base.setData(ds);
		}

		public int UserRoleID(int index) 
		{
			return base.getValueInteger(index, Columns.UserRoleID);
		}

	}
}
