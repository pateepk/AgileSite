using System;

using CMS.Base;

namespace CMS
{
    /// <summary>
    /// Specifies the extender in the assembly being attributed.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public class RegisterExtenderAttribute : RegisterExtensionAttribute
    {
        /// <summary>
        /// Initializes a new instance of the RegisterExtenderAttribute class with the specified extender type.
        /// </summary>
        /// <param name="extenderType">The type of the extender class.</param>
        public RegisterExtenderAttribute(Type extenderType)
            : base(extenderType, null)
        {
            if (extenderType == null)
            {
                throw new ArgumentNullException("extenderType");
            }

            var baseType = extenderType.BaseType;
            while (baseType != null && baseType.GetGenericTypeDefinition() != typeof(Extender<>))
            {
                baseType = baseType.BaseType;
            }

            if (baseType == null)
            {
                throw new ArgumentException("The specified type does not derive from the Extender<T> class.", "extenderType");
            }

            MarkedType = baseType.GetGenericArguments()[0];
        }
    }
}