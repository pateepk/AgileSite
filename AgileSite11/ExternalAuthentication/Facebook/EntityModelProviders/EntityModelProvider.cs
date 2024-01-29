using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CMS.ExternalAuthentication.Facebook
{
    /// <summary>
    /// Provides models of Facebook API entities.
    /// </summary>
    public sealed class EntityModelProvider
    {
        #region "Public methods"

        /// <summary>
        /// Creates a model of the specified Facebook API entity, and returns it.
        /// </summary>
        /// <typeparam name="T">A type of Facebook API entity.</typeparam>
        /// <returns>A model of the specified Facebook API entity.</returns>
        public EntityModel GetEntityModel<T>()
        {
            IEnumerable<EntityAttributeModel> source = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance).Select(x => new EntityAttributeModel(x));

            return new EntityModel(source);
        }

        #endregion
    }

}