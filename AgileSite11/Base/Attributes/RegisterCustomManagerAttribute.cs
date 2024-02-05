using System;

using CMS.Base;
using CMS.Core;

namespace CMS
{
    /// <summary>
    /// Registers the custom manager within the system, replaces the default manager from which the defined one inherits.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public class RegisterCustomManagerAttribute : Attribute, IInitAttribute
    {
        #region "Properties"

        /// <summary>
        /// Gets the type used as manager implementation.
        /// </summary>
        public Type MarkedType 
        { 
            get; 
            protected set; 
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="type">Type used as manager implementation.</param>
        public RegisterCustomManagerAttribute(Type type)
        {
            MarkedType = type;
        }


        /// <summary>
        /// Applies the attribute
        /// </summary>
        public void Init()
        {
            var factory = new ObjectFactory(MarkedType);

            var managerObject = factory.CreateNewObject();

            // Check if the manager is of the correct type
            var manager = managerObject as AbstractManager;
            if (manager == null)
            {
                throw new NotSupportedException("Manager registered with attribute RegisterCustomManager must inherit AbstractManager.");
            }

            manager.ChangeManagerTypeTo(MarkedType);
        }

        #endregion
    }
}