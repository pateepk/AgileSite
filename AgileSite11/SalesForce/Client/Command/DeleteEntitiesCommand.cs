using System.Collections.Generic;
using System.Linq;

namespace CMS.SalesForce
{

    internal sealed class DeleteEntitiesCommand : SalesForceClientCommand
    {

        #region "Constructors"

        public DeleteEntitiesCommand(Session session, WebServiceClient.Soap client) : base(session, client)
        {

        }

        #endregion

        #region "Methods"

        public DeleteEntityResult[] Execute(IEnumerable<string> entityIds, SalesForceClientOptions options)
        {
            WebServiceClient.deleteRequest request = new WebServiceClient.deleteRequest();
            PrepareRequestHeaders(request, options);
            request.ids = entityIds.ToArray();
            WebServiceClient.deleteResponse response = mClient.delete(request);
            
            return response.result.Select(x => new DeleteEntityResult(x)).ToArray();
        }

        #endregion

    }

}