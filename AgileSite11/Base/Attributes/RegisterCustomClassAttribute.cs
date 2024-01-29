using System;

using CMS.Base;
using CMS.Core;

namespace CMS
{
    /// <summary>
    /// Registers the custom class within the system or overrides the existing one.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public class RegisterCustomClassAttribute : Attribute, IPreInitAttribute
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
        /// Class assembly name
        /// </summary>
        protected string AssemblyName
        {
            get;
            set;
        }


        /// <summary>
        /// Document factory
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
        /// <param name="type">Object type</param>
        public RegisterCustomClassAttribute(string className, Type type)
        {
            ClassName = className;
            MarkedType = type;
        }


        /// <summary>
        /// Initializes the attribute
        /// </summary>
        public void PreInit()
        {
            var factory = new ObjectFactory(MarkedType);

            ClassHelper.RegisterCustomClass(AssemblyName, ClassName, factory);
        }

        #endregion
    }
}