using System;

using CMS.DocumentEngine;

namespace CMS
{
    /// <summary>
    /// Registers the document type class within the system or overrides the existing one.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public class RegisterDocumentTypeAttribute : Attribute, IPreInitAttribute
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
            set; 
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="className">Class name</param>
        /// <param name="type">Object type</param>
        public RegisterDocumentTypeAttribute(string className, Type type)
        {
            ClassName = className;
            MarkedType = type;
        }


        /// <summary>
        /// Initializes the attribute
        /// </summary>
        public void PreInit()
        {
            var factory = new DocumentFactory<TreeNode>(MarkedType);

            DocumentGenerator.RegisterDocumentType(ClassName, factory);
        }

        #endregion
    }
}