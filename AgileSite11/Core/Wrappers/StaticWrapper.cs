namespace CMS.Core
{
    /// <summary>
    /// Static wrapper for the implementation implementing the given interface
    /// </summary>
    public class StaticWrapper<InterfaceType> 
        where InterfaceType : class
    {
        /// <summary>
        /// Singleton object instance behind the wrapper
        /// </summary>
        protected static InterfaceType Implementation
        {
            get
            {
                return Service.Resolve<InterfaceType>();
            }
        }
    }
}
