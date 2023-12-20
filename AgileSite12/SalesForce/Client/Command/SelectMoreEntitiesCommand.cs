namespace CMS.SalesForce
{

    internal sealed class SelectMoreEntitiesCommand : SalesForceClientCommand
    {

        #region "Constructors"

        public SelectMoreEntitiesCommand(Session session, WebServiceClient.Soap client) : base(session, client)
        {

        }

        #endregion

        #region "Methods"

        public SelectEntitiesResult Execute(string locator, EntityModel model, SalesForceClientOptions options)
        {
            WebServiceClient.queryMoreRequest request = new WebServiceClient.queryMoreRequest();
            PrepareRequestHeaders(request, options);
            request.queryLocator = locator;
            WebServiceClient.queryMoreResponse response = mClient.queryMore(request);

            return new SelectEntitiesResult(response.result, model);
        }

        #endregion

    }

}