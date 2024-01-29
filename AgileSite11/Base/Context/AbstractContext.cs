namespace CMS.Base
{
    /// <summary>
    /// Base context object class.
    /// </summary>
    public abstract class AbstractContext<TContext> : ContextContainer<TContext>, IContext, ICloneThreadItem
        where TContext : AbstractContext<TContext>, new()
    {
        /// <summary>
        /// Current request data
        /// </summary>
        protected static TContext CurrentContext
        {
            get
            {
                return Current;
            }
        }


        /// <summary>
        /// Clones the object for new thread
        /// </summary>
        public virtual object CloneForNewThread()
        {
            return MemberwiseClone();
        }
        

        /// <summary>
        /// Ensures that the given value is cached within a context.
        /// </summary>
        /// <param name="property">Property to ensure</param>
        protected static void Ensure(object property)
        {
            // No actions, 
        }
    }
}