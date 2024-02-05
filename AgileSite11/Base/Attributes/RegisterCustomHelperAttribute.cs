using System;

using CMS.Base;
using CMS.Core;

namespace CMS
{
    /// <summary>
    /// Registers the custom helper within the system, replaces the default helper from which the defined one inherits.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public class RegisterCustomHelperAttribute : Attribute, IInitAttribute
    {
        #region "Properties"

        /// <summary>
        /// Gets the type used as helper implementation.
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
        /// <param name="type">Type used as helper implementation.</param>
        public RegisterCustomHelperAttribute(Type type)
        {
            MarkedType = type;
        }


        /// <summary>
        /// Applies the attribute
        /// </summary>
        public void Init()
        {
            var factory = new ObjectFactory(MarkedType);

            var helperObject = factory.CreateNewObject();

            // Check if the helper is of the correct type
            var helper = helperObject as AbstractHelper;
            if (helper == null)
            {
                throw new NotSupportedException("Helper registered with attribute RegisterCustomHelper must inherit AbstractHelper.");
            }

            helper.SetAsDefaultHelper();
        }

        #endregion
    }
}