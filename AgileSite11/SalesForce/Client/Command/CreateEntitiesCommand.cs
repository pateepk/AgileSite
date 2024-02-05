using System.Collections.Generic;
using System.Linq;

namespace CMS.SalesForce
{

    internal sealed class CreateEntitiesCommand : SalesForceClientCommand
    {

        #region "Constructors"

        public CreateEntitiesCommand(Session session, WebServiceClient.Soap client) : base(session, client)
        {

        }

        #endregion

        #region "Methods"

        public CreateEntityResult[] Execute(IEnumerable<Entity> entities, SalesForceClientOptions options)
        {
            WebServiceClient.createRequest request = new WebServiceClient.createRequest();
            PrepareRequestHeaders(request, options);
            EntitySerializer serializer = new EntitySerializer();
            request.sObjects = entities.Select(x => serializer.Serialize(x)).ToArray();
            WebServiceClient.createResponse response = mClient.create(request);
            if (response.result == null)
            {
                return new CreateEntityResult[0];
            }

            return response.result.Select(x => new CreateEntityResult(x)).ToArray();
        }

        #endregion

    }

}