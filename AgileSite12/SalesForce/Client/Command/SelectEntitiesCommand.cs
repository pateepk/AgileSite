namespace CMS.SalesForce
{

    internal sealed class SelectEntitiesCommand : SalesForceClientCommand
    {

        #region "Constructors"

        public SelectEntitiesCommand(Session session, WebServiceClient.Soap client) : base(session, client)
        {

        }

        #endregion

        #region "Methods"

        public SelectEntitiesResult Execute(string statement, EntityModel model, SalesForceClientOptions options)
        {
            WebServiceClient.QueryResult result = null;
            if (options.IncludeDeleted)
            {
                WebServiceClient.queryAllRequest request = new WebServiceClient.queryAllRequest();
                request.queryString = statement;
                PrepareRequestHeaders(request, options);
                WebServiceClient.queryAllResponse response = mClient.queryAll(request);
                result = response.result;
            }
            else
            {
                WebServiceClient.queryRequest request = new WebServiceClient.queryRequest();
                request.queryString = statement;
                PrepareRequestHeaders(request, options);
                WebServiceClient.queryResponse response = mClient.query(request);
                result = response.result;
            }

            return new SelectEntitiesResult(result, model);
        }

        #endregion

    }

}