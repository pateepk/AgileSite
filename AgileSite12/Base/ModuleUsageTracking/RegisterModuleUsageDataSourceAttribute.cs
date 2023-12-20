using System;
using System.Linq;
using System.Text;

using CMS.Core;

namespace CMS.Base
{
    /// <summary>
    /// Registers class to be a source of a module statistical data.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public sealed class RegisterModuleUsageDataSourceAttribute : Attribute, IInitAttribute
    {
        // Type of the module data provider.
        private Type mMarkType;


        /// <summary>
        /// Gets the type of the module data provider.
        /// </summary>
        /// <exception cref="System.ArgumentNullException">Thrown when value to set is null</exception>
        /// <exception cref="System.ArgumentException">Thrown when value to set is not implementing <see cref="CMS.Base.IModuleUsageDataSource"/> interface</exception>
        public Type MarkedType
        {
            get
            {
                return mMarkType;
            }
            private set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                if (!typeof(IModuleUsageDataSource).IsAssignableFrom(value))
                {
                    throw new ArgumentException("Provided type does not implement IModuleDataProvider interface.", "value");
                }

                mMarkType = value;
            }
        }


        /// <summary>
        /// Registers class to be a source of a module statistical data.
        /// </summary>
        /// <param name="moduleDataProviderType">Type of registered provider.</param>
        /// <exception cref="System.ArgumentException">Some of given attributes is invalid.</exception>
        /// <exception cref="System.ArgumentNullException">Some of given attributes is null.</exception>
        public RegisterModuleUsageDataSourceAttribute(Type moduleDataProviderType)
        {
            if (moduleDataProviderType == null)
            {
                throw new ArgumentNullException("moduleDataProviderType");
            }

            if (!typeof(IModuleUsageDataSource).IsAssignableFrom(moduleDataProviderType))
            {
                throw new ArgumentException("Provided type does not implement IModuleDataProvider interface.", "moduleDataProviderType");
            }

            mMarkType = moduleDataProviderType;
        }


        /// <summary>
        /// Registers given data provider to module usage provider.
        /// </summary>
        public void Init()
        {
            // Get module usage tracking provider
            var provider = ObjectFactory<IModuleUsageDataSourceContainer>.StaticSingleton();
            
            // Register data provider
            provider.RegisterDataSource(this);
        }
    }
}
