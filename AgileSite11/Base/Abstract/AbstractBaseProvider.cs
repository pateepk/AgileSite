using CMS.Core;

namespace CMS.Base
{
    /// <summary>
    /// Abstract class for providers.
    /// </summary>
    public abstract class AbstractBaseProvider<ProviderType> : ICustomizableProvider
        where ProviderType : AbstractBaseProvider<ProviderType>, new()
    {
        /// <summary>
        /// Provider object.
        /// </summary>
        private static ProviderType mProviderObject;

        private static readonly object lockObject = new object();


        /// <summary>
        /// Static constructor
        /// </summary>
        static AbstractBaseProvider()
        {
            TypeManager.RegisterGenericType(typeof(AbstractBaseProvider<ProviderType>));
        }
        

        /// <summary>
        /// Provider object.
        /// </summary>
        public static ProviderType ProviderObject
        {
            get
            {
                return LockHelper.Ensure(ref mProviderObject, CMSExtensibilitySection.LoadProvider<ProviderType>, lockObject);
            }
            set
            {
                mProviderObject = value;
            }
        }


        /// <summary>
        /// Sets this object as the default provider
        /// </summary>
        public void SetAsDefaultProvider()
        {
            ProviderObject = (ProviderType)this;
        }
    }
}