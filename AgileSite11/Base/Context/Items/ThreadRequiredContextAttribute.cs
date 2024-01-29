using System;

using CMS.Core;

namespace CMS.Base
{
    /// <summary>
    /// Indicates whether the marked class should be ensured before the new <see cref="CMSThread"/> is started to be available with current values within the <see cref="CMSThread"/> run.
    /// </summary>
    /// <remarks>
    /// Marked class must implement <see cref="IContextContainer"/> interface.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public sealed class ThreadRequiredContextAttribute : Attribute, IPreInitAttribute
    {
        /// <summary>
        /// Type of the context.
        /// </summary>
        /// <remarks>
        /// Must implements <see cref="IContextContainer"/>.
        /// </remarks>
        public Type MarkedType
        {
            get;
            private set;
        }


        /// <summary>
        /// Creates new instance of <see cref="ThreadRequiredContextAttribute"/>.
        /// </summary>
        /// <param name="markedType"><see cref="IContextContainer"/> context type.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="markedType"/> is not set.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="markedType"/> is not implementing <see cref="IContextContainer"/>.</exception>
        public ThreadRequiredContextAttribute(Type markedType)
        {
            if (markedType == null)
            {
                throw new ArgumentNullException("markedType");
            }

            if (!typeof(IContextContainer).IsAssignableFrom(markedType))
            {
                throw new ArgumentException("Provided type does not implement IContextContainer interface.", "markedType");
            }

            MarkedType = markedType;
        }


        /// <summary>
        /// Registers context of <see cref="MarkedType"/> type.
        /// </summary>
        /// <exception cref="NullReferenceException">Thrown when <see cref="MarkedType"/> is not defined.</exception>
        public void PreInit()
        {
            IContextContainer contextContainer = new ObjectFactory(MarkedType).CreateNewObject() as IContextContainer;
            
            if (contextContainer == null)
            {
                throw new NullReferenceException("Provided type in MarkedType property does not implement IContextContainer interface.");
            }

            ThreadRequiredContexts.Register(contextContainer);
        }
    }
}
