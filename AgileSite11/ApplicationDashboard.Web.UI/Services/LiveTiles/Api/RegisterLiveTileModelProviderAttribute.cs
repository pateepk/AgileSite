using System;
using System.Linq;
using System.Text;

namespace CMS.ApplicationDashboard.Web.UI
{
    /// <summary>
    /// Registers live model provider for given resource and element name.
    /// Live model provider is used when obtaining model for live tiles on the dashboard.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public sealed class RegisterLiveTileModelProviderAttribute : Attribute, IPreInitAttribute
    {
        /// <summary>
        /// Name of the resource the provider is registered to.
        /// </summary>
        private readonly string mResourceName;


        /// <summary>
        /// Name of the UI element the provider is registered to.
        /// </summary>
        private readonly string mElementName;


        /// <summary>
        /// Gets the type of the live model provider.
        /// </summary>
        public Type MarkedType
        {
            get;
            private set;
        }


        /// <summary>
        /// Attribute constructor.
        /// </summary>
        /// <param name="resourceName">Name of the resource</param>
        /// <param name="elementName">Name of the UI element</param>
        /// <param name="liveTileModelProvider">Type of live model provider. The type has to be implementing <see cref="ILiveTileModelProvider"/> interface</param>
        /// <exception cref="ArgumentException"><paramref name="resourceName"/> or <paramref name="elementName"/> or <paramref name="liveTileModelProvider"/> is null</exception>
        /// <exception cref="ArgumentNullException">Given type does not implement <see cref="ILiveTileModelProvider"/> interface</exception>
        public RegisterLiveTileModelProviderAttribute(string resourceName, string elementName, Type liveTileModelProvider)
        {
            if (resourceName == null)
            {
                throw new ArgumentNullException("resourceName");
            }

            if (elementName == null)
            {
                throw new ArgumentNullException("elementName");
            }
            
            if (liveTileModelProvider == null)
            {
                throw new ArgumentNullException("liveTileModelProvider");
            }
            
            if (!typeof (ILiveTileModelProvider).IsAssignableFrom(liveTileModelProvider))
            {
                throw new ArgumentException("Provided type does not implement ILiveTileModelProvider interface.", "liveTileModelProvider");
            }

            mResourceName = resourceName;
            mElementName = elementName;
            MarkedType = liveTileModelProvider;
        }


        /// <summary>
        /// Registers the live model provider factory with the Live model provider container.
        /// </summary>
        public void PreInit()
        {
            LiveTileModelProviderContainer.RegisterProviderFactory(mResourceName, mElementName, () => (ILiveTileModelProvider)Activator.CreateInstance(MarkedType));
        }
    }
}
