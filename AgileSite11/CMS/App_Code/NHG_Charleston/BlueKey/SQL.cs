using System.Data;

using CMS.DataEngine;

namespace NHG_C.BlueKey
{
    public class SQL
    {
        public static DataSet ExecuteQuery(string sql, QueryDataParameters p)
        {
            QueryParameters qp = new QueryParameters(sql, p, QueryTypeEnum.SQLQuery);

            return ConnectionHelper.ExecuteQuery(qp);
        }
    }
}