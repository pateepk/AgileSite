using System.Collections.Generic;
using System.Linq;

namespace CMS.SalesForce
{

    internal sealed class UpdateEntitiesCommand : SalesForceClientCommand
    {

        #region "Constructors"

        public UpdateEntitiesCommand(Session session, WebServiceClient.Soap client) : base(session, client)
        {

        }

        #endregion

        #region "Methods"

        public UpdateEntityResult[] Execute(IEnumerable<Entity> entities, SalesForceClientOptions options)
        {
            WebServiceClient.updateRequest request = new WebServiceClient.updateRequest();
            PrepareRequestHeaders(request, options);
            EntitySerializer serializer = new EntitySerializer();
            request.sObjects = entities.Select(x => serializer.Serialize(x)).ToArray();
            WebServiceClient.updateResponse response = mClient.update(request);
            if (response.result == null)
            {
                return new UpdateEntityResult[0];
            }

            return response.result.Select(x => new UpdateEntityResult(x)).ToArray();
        }

        #endregion

    }

}