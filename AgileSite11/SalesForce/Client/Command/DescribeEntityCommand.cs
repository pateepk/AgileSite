namespace CMS.SalesForce
{

    internal sealed class DescribeEntityCommand : SalesForceClientCommand
    {

        #region "Constructors"

        public DescribeEntityCommand(Session session, WebServiceClient.Soap client) : base(session, client)
        {

        }

        #endregion

        #region "Methods"

        public EntityModel Execute(string name, SalesForceClientOptions options)
        {
            WebServiceClient.describeSObjectRequest request = new WebServiceClient.describeSObjectRequest();
            PrepareRequestHeaders(request, options);
            request.sObjectType = name;
            WebServiceClient.describeSObjectResponse response = mClient.describeSObject(request);

            return new EntityModel(response.result);
        }

        #endregion

    }

}