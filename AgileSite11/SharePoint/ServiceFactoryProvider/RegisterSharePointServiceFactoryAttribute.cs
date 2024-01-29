using System;
using System.Linq;
using System.Reflection;
using System.Text;

namespace CMS.SharePoint
{

    /// <summary>
    /// Attribute for registering custom SharePoint service factory for certain SharePoint version.
    /// The registration is performed on current SharePoint service factory provider.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public class RegisterSharePointServiceFactoryAttribute : Attribute, IInitAttribute
    {
        #region "Fields"

        private Type mMarkedType;
        private string mSharePointVersion;

        #endregion


        #region "Properties"

        /// <summary>
        /// SharePoint service factory.
        /// </summary>
        /// <exception cref="SharePointServiceFactoryNotSupportedException">Thrown when MarkedType does not implement ISharePointServiceFactory.</exception>
        public Type MarkedType
        {
            get
            {
                return mMarkedType;
            }
            protected set
            {
                if (!typeof(ISharePointServiceFactory).IsAssignableFrom(value))
                {
                    throw new SharePointServiceFactoryNotSupportedException(String.Format("[RegisterSharePointServiceFactoryAttribute.MarkedType]: Given type '{0}' does not implement ISharePointServiceFactory.", value));
                }
                mMarkedType = value;
            }
        }


        /// <summary>
        /// SharePoint version.
        /// </summary>
        /// <exception cref="NullReferenceException">Thrown when sharePointVersion is null.</exception>
        public string SharePointVersion
        {
            get
            {
                return mSharePointVersion;
            }
            set
            {
                if (value == null)
                {
                    throw new NullReferenceException("[RegisterSharePointServiceFactoryAttribute.SharePointVersion]: SharePoint version can not be null.");
                }
                mSharePointVersion = value;
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Registers new implementation of ISharePointServiceFactory for given SharePoint version.
        /// </summary>
        /// <param name="sharePointVersion">SharePoint version</param>
        /// <param name="serviceFactory">Class implementing ISharePointServiceFactory</param>
        /// <exception cref="SharePointServiceFactoryNotSupportedException">Thrown when serviceFactory does not implement ISharePointServiceFactory.</exception>
        /// <exception cref="ArgumentNullException">Thrown when sharePointVersion is null.</exception>
        public RegisterSharePointServiceFactoryAttribute(string sharePointVersion, Type serviceFactory)
        {
            if (sharePointVersion == null)
            {
                throw new ArgumentNullException("sharePointVersion", "[RegisterSharePointServiceFactoryAttribute.RegisterSharePointServiceFactoryAttribute]: SharePoint version can not be null.");
            }
            if (!typeof (ISharePointServiceFactory).IsAssignableFrom(serviceFactory))
            {
                throw new SharePointServiceFactoryNotSupportedException(String.Format("[RegisterSharePointServiceFactoryAttribute.RegisterSharePointServiceFactoryAttribute]: Given type '{0}' does not implement ISharePointServiceFactory.", serviceFactory));
            }

            MarkedType = serviceFactory;
            SharePointVersion = sharePointVersion;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Registers SharePoint service factory in current service factory provider.
        /// </summary>
        public void Init()
        {
            ISharePointServiceFactoryProvider serviceFactoryProvider = SharePointServiceFactoryProvider.Current;
            MethodInfo method = serviceFactoryProvider.GetType().GetMethod("RegisterFactory");
            MethodInfo genericMethod = method.MakeGenericMethod(MarkedType);

            genericMethod.Invoke(serviceFactoryProvider, new object[]
            {
                SharePointVersion
            });
        }

        #endregion
    }
}
