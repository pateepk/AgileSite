using CMS.DataEngine;

namespace CMS.ExternalAuthentication.Facebook
{

    /// <summary>
    /// Provides mapping of Facebook API entities to CMS objects.
    /// </summary>
    /// <typeparam name="TEntity">The type of Facebook API entity.</typeparam>
    /// <typeparam name="TInfo">The type of CMS object.</typeparam>
    public interface IEntityMapper<TEntity, TInfo> where TEntity : class where TInfo : BaseInfo
    {
        #region "Methods"

        /// <summary>
        /// Updates CMS object with data from Facebook API entity.
        /// </summary>
        /// <param name="entity">The Facebook API entity.</param>
        /// <param name="info">The CMS object.</param>
        /// <param name="mapping">The mapping between a Facebook API entity and a CMS object.</param>
        void Map(TEntity entity, TInfo info, EntityMapping mapping);

        #endregion
    }

}