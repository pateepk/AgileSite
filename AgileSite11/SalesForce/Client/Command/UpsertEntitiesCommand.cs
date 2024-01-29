using System.Collections.Generic;
using System.Linq;

namespace CMS.SalesForce
{

    internal sealed class UpsertEntitiesCommand : SalesForceClientCommand
    {

        #region "Constructors"

        public UpsertEntitiesCommand(Session session, WebServiceClient.Soap client) : base(session, client)
        {

        }

        #endregion

        #region "Methods"

        public UpsertEntityResult[] Execute(IEnumerable<Entity> entities, string externalAttributeName, SalesForceClientOptions options)
        {
            WebServiceClient.upsertRequest request = new WebServiceClient.upsertRequest();
            PrepareRequestHeaders(request, options);
            EntitySerializer serializer = new EntitySerializer();
            request.sObjects = entities.Select(x => serializer.Serialize(x)).ToArray();
            request.externalIDFieldName = externalAttributeName;
            WebServiceClient.upsertResponse response = mClient.upsert(request);
            if (response.result == null)
            {
                return new UpsertEntityResult[0];
            }

            return response.result.Select(x => new UpsertEntityResult(x)).ToArray();
        }

        #endregion

    }

}