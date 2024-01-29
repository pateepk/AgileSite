using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading;

using CMS.SalesForce.WebServiceClient;

namespace CMS.SalesForce
{

    /// <summary>
    /// Provides access to SalesForce organization using the integration API.
    /// </summary>
    public sealed class SalesForceClient
    {

        #region "Constants"

        private const string API_VERSION = "23.0";

        #endregion

        #region "Private members"

        private Session mSession;
        private SalesForceClientOptions mOptions;
        private WebServiceClient.Soap mClient;

        #endregion

        #region "Public properties"

        /// <summary>
        /// Gets the current command options.
        /// </summary>
        public SalesForceClientOptions Options
        {
            get
            {
                return mOptions;
            }
        }

        #endregion

        #region "Constructors"

        /// <summary>
        /// Initializes a new instance of the SalesForceClient class with the specified session.
        /// </summary>
        /// <param name="session">A SalesForce communication session.</param>
        public SalesForceClient(Session session)
        {
            mSession = session;
            mOptions = CreateDefaultOptions();
            mClient = CreateWebServiceClient();
        }

        #endregion

        #region "Public methods"

        /// <summary>
        /// Retrieves an entity model with the specified name, and returns it.
        /// </summary>
        /// <param name="name">The name of the entity model.</param>
        /// <returns>An entity model with the specified name.</returns>
        public EntityModel DescribeEntity(string name)
        {
            return DescribeEntity(name, mOptions);
        }

        /// <summary>
        /// Retrieves an entity model with the specified name, and returns it.
        /// </summary>
        /// <param name="name">The name of the entity model.</param>
        /// <param name="options">The command options that override the default ones.</param>
        /// <returns>An entity model with the specified name.</returns>
        public EntityModel DescribeEntity(string name, SalesForceClientOptions options)
        {
            DescribeEntityCommand command = new DescribeEntityCommand(mSession, mClient);
            try
            {
                return command.Execute(name, options);
            }
            catch (FaultException<InvalidSObjectFault> exception)
            {
                throw new SalesForceClientException(exception.Detail, exception);
            }
            catch (FaultException<UnexpectedErrorFault> exception)
            {
                throw new SalesForceClientException(exception.Detail, exception);
            }
        }

        /// <summary>
        /// Retrieves an array of entity models with the specified names, and returns it.
        /// </summary>
        /// <param name="names">The collection of entity model names.</param>
        /// <returns>An array of entity models with the specified names.</returns>
        public EntityModel[] DescribeEntities(IEnumerable<string> names)
        {
            return DescribeEntities(names, mOptions);
        }

        /// <summary>
        /// Retrieves an array of entity models with the specified names, and returns it.
        /// </summary>
        /// <param name="names">The collection of entity model names.</param>
        /// <param name="options">The command options that override the default ones.</param>
        /// <returns>An array of entity models with the specified names.</returns>
        public EntityModel[] DescribeEntities(IEnumerable<string> names, SalesForceClientOptions options)
        {
            DescribeEntitiesCommand command = new DescribeEntitiesCommand(mSession, mClient);
            try
            {
                return command.Execute(names, options);
            }
            catch (FaultException<InvalidSObjectFault> exception)
            {
                throw new SalesForceClientException(exception.Detail, exception);
            }
            catch (FaultException<UnexpectedErrorFault> exception)
            {
                throw new SalesForceClientException(exception.Detail, exception);
            }
        }

        /// <summary>
        /// Executes the specified SOQL query and returns the result.
        /// </summary>
        /// <param name="statement">The SOQL query.</param>
        /// <param name="model">The model of the entities in the result.</param>
        /// <returns>The result of the SOQL query.</returns>
        public SelectEntitiesResult SelectEntities(string statement, EntityModel model)
        {
            return SelectEntities(statement, model, mOptions);
        }

        /// <summary>
        /// Executes the specified SOQL query and returns the result.
        /// </summary>
        /// <param name="statement">The SOQL query.</param>
        /// <param name="model">The model of the entities in the result.</param>
        /// <param name="options">The command options that override the default ones.</param>
        /// <returns>The result of the SOQL query.</returns>
        public SelectEntitiesResult SelectEntities(string statement, EntityModel model, SalesForceClientOptions options)
        {
            SelectEntitiesCommand command = new SelectEntitiesCommand(mSession, mClient);
            try
            {
                return command.Execute(statement, model, options);
            }
            catch (FaultException<MalformedQueryFault> exception)
            {
                throw new SalesForceClientException(exception.Detail, exception);
            }
            catch (FaultException<InvalidSObjectFault> exception)
            {
                throw new SalesForceClientException(exception.Detail, exception);
            }
            catch (FaultException<InvalidFieldFault> exception)
            {
                throw new SalesForceClientException(exception.Detail, exception);
            }
            catch (FaultException<UnexpectedErrorFault> exception)
            {
                throw new SalesForceClientException(exception.Detail, exception);
            }
        }

        /// <summary>
        /// Returns the next set of entities from the SOQL query result.
        /// </summary>
        /// <param name="locator">The identifier of the set of entities.</param>
        /// <param name="model">The model of the entities in the result.</param>
        /// <returns>The next set of entities from the SOQL query result.</returns>
        public SelectEntitiesResult SelectMoreEntities(string locator, EntityModel model)
        {
            return SelectMoreEntities(locator, model, mOptions);
        }

        /// <summary>
        /// Returns the next set of entities from the SOQL query result.
        /// </summary>
        /// <param name="locator">The identifier of the set of entities.</param>
        /// <param name="model">The model of the entities in the result.</param>
        /// <param name="options">The command options that override the default ones.</param>
        /// <returns>The next set of entities from the SOQL query result.</returns>
        public SelectEntitiesResult SelectMoreEntities(string locator, EntityModel model, SalesForceClientOptions options)
        {
            SelectMoreEntitiesCommand command = new SelectMoreEntitiesCommand(mSession, mClient);
            try
            {
                return command.Execute(locator, model, options);
            }
            catch (FaultException<MalformedQueryFault> exception)
            {
                throw new SalesForceClientException(exception.Detail, exception);
            }
            catch (FaultException<InvalidSObjectFault> exception)
            {
                throw new SalesForceClientException(exception.Detail, exception);
            }
            catch (FaultException<InvalidFieldFault> exception)
            {
                throw new SalesForceClientException(exception.Detail, exception);
            }
            catch (FaultException<UnexpectedErrorFault> exception)
            {
                throw new SalesForceClientException(exception.Detail, exception);
            }
        }

        /// <summary>
        /// Creates new entities and returns the result.
        /// </summary>
        /// <param name="entities">The collection of entities to create.</param>
        /// <returns>An array of operation results.</returns>
        public CreateEntityResult[] CreateEntities(IEnumerable<Entity> entities)
        {
            return CreateEntities(entities, mOptions);
        }

        /// <summary>
        /// Creates new entities and returns the result.
        /// </summary>
        /// <param name="entities">The collection of entities to create.</param>
        /// <param name="options">The command options that override the default ones.</param>
        /// <returns>An array of operation results.</returns>
        public CreateEntityResult[] CreateEntities(IEnumerable<Entity> entities, SalesForceClientOptions options)
        {
            CreateEntitiesCommand command = new CreateEntitiesCommand(mSession, mClient);
            try
            {
                return command.Execute(entities, options);
            }
            catch (FaultException<InvalidSObjectFault> exception)
            {
                throw new SalesForceClientException(exception.Detail, exception);
            }
            catch (FaultException<UnexpectedErrorFault> exception)
            {
                throw new SalesForceClientException(exception.Detail, exception);
            }
        }

        /// <summary>
        /// Updates entities and returns the result.
        /// </summary>
        /// <param name="entities">The collection of entities to update.</param>
        /// <returns>An array of operation results.</returns>
        public UpdateEntityResult[] UpdateEntities(IEnumerable<Entity> entities)
        {
            return UpdateEntities(entities, mOptions);
        }

        /// <summary>
        /// Updates entities and returns the result.
        /// </summary>
        /// <param name="entities">The collection of entities to update.</param>
        /// <param name="options">The command options that override the default ones.</param>
        /// <returns>An array of operation results.</returns>
        public UpdateEntityResult[] UpdateEntities(IEnumerable<Entity> entities, SalesForceClientOptions options)
        {
            UpdateEntitiesCommand command = new UpdateEntitiesCommand(mSession, mClient);
            try
            {
                return command.Execute(entities, options);
            }
            catch (FaultException<InvalidSObjectFault> exception)
            {
                throw new SalesForceClientException(exception.Detail, exception);
            }
            catch (FaultException<UnexpectedErrorFault> exception)
            {
                throw new SalesForceClientException(exception.Detail, exception);
            }
        }

        /// <summary>
        /// Upserts entities and returns the result.
        /// </summary>
        /// <param name="entities">The collection of entities to upsert.</param>
        /// <param name="externalAttributeName">The name of the attribute whose value determines whether to create a new entity or update an existing one.</param>
        /// <returns>An array of operation results.</returns>
        public UpsertEntityResult[] UpsertEntities(IEnumerable<Entity> entities, string externalAttributeName)
        {
            return UpsertEntities(entities, externalAttributeName, mOptions);
        }

        /// <summary>
        /// Upserts entities and returns the result.
        /// </summary>
        /// <param name="entities">The collection of entities to upsert.</param>
        /// <param name="externalAttributeName">The name of the attribute whose value determines whether to create a new entity or update an existing one.</param>
        /// <param name="options">The command options that override the default ones.</param>
        /// <returns>An array of operation results.</returns>
        public UpsertEntityResult[] UpsertEntities(IEnumerable<Entity> entities, string externalAttributeName, SalesForceClientOptions options)
        {
            UpsertEntitiesCommand command = new UpsertEntitiesCommand(mSession, mClient);
            try
            {
                return command.Execute(entities, externalAttributeName, options);
            }
            catch (FaultException<InvalidSObjectFault> exception)
            {
                throw new SalesForceClientException(exception.Detail, exception);
            }
            catch (FaultException<UnexpectedErrorFault> exception)
            {
                throw new SalesForceClientException(exception.Detail, exception);
            }
        }

        /// <summary>
        /// Deletes entities with the specified identifiers and returns the result.
        /// </summary>
        /// <param name="entityIds">The collection of entity identifiers.</param>
        /// <returns>An array of operation results.</returns>
        public DeleteEntityResult[] DeleteEntities(IEnumerable<string> entityIds)
        {
            return DeleteEntities(entityIds, mOptions);
        }

        /// <summary>
        /// Deletes entities with the specified identifiers and returns the result.
        /// </summary>
        /// <param name="entityIds">The collection of entity identifiers.</param>
        /// <param name="options">The command options that override the default ones.</param>
        /// <returns>An array of operation results.</returns>
        public DeleteEntityResult[] DeleteEntities(IEnumerable<string> entityIds, SalesForceClientOptions options)
        {
            DeleteEntitiesCommand command = new DeleteEntitiesCommand(mSession, mClient);
            try
            {
                return command.Execute(entityIds, options);
            }
            catch (FaultException<InvalidSObjectFault> exception)
            {
                throw new SalesForceClientException(exception.Detail, exception);
            }
            catch (FaultException<UnexpectedErrorFault> exception)
            {
                throw new SalesForceClientException(exception.Detail, exception);
            }
        }

        #endregion

        #region "Private methods"

        private WebServiceClient.Soap CreateWebServiceClient()
        {
            string url = mSession.PartnerEndpointUrlFormat.Replace("{version}", API_VERSION);

            return new WebServiceClient.SoapClient("SalesForceClient", url);
        }

        private SalesForceClientOptions CreateDefaultOptions()
        {
            return new SalesForceClientOptions();
        }

        #endregion

    }

}