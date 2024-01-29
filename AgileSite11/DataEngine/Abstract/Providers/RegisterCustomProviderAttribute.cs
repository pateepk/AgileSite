using System;

using CMS.Base;
using CMS.Core;

namespace CMS
{
    /// <summary>
    /// Registers the custom provider within the system, replaces the default provider from which the defined one inherits.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public class RegisterCustomProviderAttribute : Attribute, IInitAttribute
    {
        #region "Properties"

        /// <summary>
        /// Gets the type used as provider implementation
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
        /// <param name="type">Type used as helper implementation</param>
        public RegisterCustomProviderAttribute(Type type)
        {
            MarkedType = type;
        }


        /// <summary>
        /// Applies the attribute
        /// </summary>
        public void Init()
        {
            var factory = new ObjectFactory(MarkedType);

            var providerObject = factory.CreateNewObject();

            // Check if the provider is of the correct type
            var provider = providerObject as ICustomizableProvider;
            if (provider == null)
            {
                throw new NotSupportedException("Provider registered with attribute RegisterCustomProvider must implement interface ICustomizableProvider.");
            }

            provider.SetAsDefaultProvider();
        }

        #endregion
    }
}