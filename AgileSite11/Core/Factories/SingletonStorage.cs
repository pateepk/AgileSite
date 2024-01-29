namespace CMS.Core
{
    /// <summary>
    /// Storage for singleton objects
    /// </summary>
    internal class SingletonStorage<ParentType, SingletonType>
    {
        /// <summary>
        /// Singleton
        /// </summary>
        public static SingletonType Singleton 
        { 
            get; 
            set; 
        }


        /// <summary>
        /// Static constructor
        /// </summary>
        static SingletonStorage()
        {
            TypeManager.RegisterGenericType(typeof(SingletonStorage<ParentType, SingletonType>));
        }
    }
}
