namespace CMS.DataEngine
{
    /// <summary>
    /// Abstract info class provider.
    /// </summary>
    public abstract class AbstractInfoProvider<TInfo, TProvider> : AbstractInfoProvider<TInfo, TProvider, ObjectQuery<TInfo>>
        where TInfo : AbstractInfoBase<TInfo>, new()
        where TProvider : AbstractInfoProvider<TInfo, TProvider>, new()
    {

        /// <summary>
        /// Creates new instance of <see cref="AbstractInfoProvider{TInfo, TProvider}"/>.
        /// </summary>
        protected AbstractInfoProvider()
            : this(true)
        {
        }


        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="initialize">Indicates if provider together with hash tables should be initialized</param>
        protected AbstractInfoProvider(bool initialize)
            : base(initialize)
        {
        }


        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="settings">Hashtable settings</param>
        /// <param name="typeInfo">Object type information</param>
        protected AbstractInfoProvider(ObjectTypeInfo typeInfo, HashtableSettings settings = null)
            : base(typeInfo, settings)
        {
        }


        /// <summary>
        /// Gets the object query for the provider
        /// </summary>
        protected override ObjectQuery<TInfo> GetObjectQueryInternal()
        {
            var query = new ObjectQuery<TInfo>(TypeInfo.ObjectType, false)
            {
                ExecutingQueryProvider = new ClassNameExecutingQueryProvider(TypeInfo)
            };

            return query;
        }
    }
}