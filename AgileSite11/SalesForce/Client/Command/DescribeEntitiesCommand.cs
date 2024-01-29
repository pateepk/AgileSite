using System.Collections.Generic;
using System.Linq;

namespace CMS.SalesForce
{

    internal sealed class DescribeEntitiesCommand : SalesForceClientCommand
    {

        #region "Constructors"

        public DescribeEntitiesCommand(Session session, WebServiceClient.Soap client) : base(session, client)
        {

        }

        #endregion

        #region "Methods"

        public EntityModel[] Execute(IEnumerable<string> names, SalesForceClientOptions options)
        {
            WebServiceClient.describeSObjectsRequest request = new WebServiceClient.describeSObjectsRequest();
            PrepareRequestHeaders(request, options);
            request.sObjectType = names.ToArray();
            WebServiceClient.describeSObjectsResponse response = mClient.describeSObjects(request);
            
            return response.result.Select(x => new EntityModel(x)).ToArray();
        }

        #endregion

    }

}