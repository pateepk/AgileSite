using System;

using CMS.OnlineForms;

namespace CMS
{
    /// <summary>
    /// Registers the BizForm item class within the system or overrides the existing one.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public class RegisterBizFormAttribute : Attribute, IInitAttribute
    {
        #region "Properties"

        /// <summary>
        /// Class name
        /// </summary>
        public string ClassName 
        { 
            get; 
            protected set; 
        }


        /// <summary>
        /// Gets the BizForm item implementation type
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
        /// <param name="className">Class name</param>
        /// <param name="type">BizForm item implementation type</param>
        public RegisterBizFormAttribute(string className, Type type)
        {
            ClassName = className;
            MarkedType = type;
        }


        /// <summary>
        /// Initializes the attribute
        /// </summary>
        public void Init()
        {
            var factory = new BizFormItemFactory(MarkedType);

            BizFormItemGenerator.RegisterBizForm(ClassName, factory);
        }

        #endregion
    }
}