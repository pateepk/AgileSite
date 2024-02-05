using System.Linq;

namespace CMS.SalesForce
{

    /// <summary>
    /// Represents a result of the select entities command.
    /// </summary>
    public sealed class SelectEntitiesResult
    {

        #region "Private members"

        private WebServiceClient.QueryResult mSource;
        private Entity[] mEntities;

        #endregion

        #region "Public properties"

        /// <summary>
        /// Gets the locator of the next set of results.
        /// </summary>
        public string NextResultLocator
        {
            get
            {
                return mSource.queryLocator;
            }
        }

        /// <summary>
        /// Gets the value indicating whether this is the last set of entities.
        /// </summary>
        public bool IsComplete
        {
            get
            {
                return mSource.done;
            }
        }

        /// <summary>
        /// Gets the total number of entities that the query returned.
        /// </summary>
        public int TotalEntityCount
        {
            get
            {
                return mSource.size;
            }
        }

        /// <summary>
        /// Gets the list of entities in this set.
        /// </summary>
        public Entity[] Entities
        {
            get
            {
                return mEntities;
            }
        }

        #endregion

        #region "Constructors"

        internal SelectEntitiesResult(WebServiceClient.QueryResult source, EntityModel model)
        {
            mSource = source;
            if (source.records == null)
            {
                mEntities = new Entity[0];
            }
            else
            {
                EntitySerializer serializer = new EntitySerializer();
                mEntities = source.records.Select(x => serializer.Deserialize(x, model)).ToArray();
            }
        }

        #endregion

    }

}