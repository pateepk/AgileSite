using System;

using CMS.CustomTables;

namespace CMS
{
    /// <summary>
    /// Registers the custom table item class within the system or overrides the existing one.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public class RegisterCustomTableAttribute : Attribute, IInitAttribute
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
        /// Gets the custom table item implementation type
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
        /// <param name="type">Custom table item implementation type</param>
        public RegisterCustomTableAttribute(string className, Type type)
        {
            ClassName = className;
            MarkedType = type;
        }


        /// <summary>
        /// Initializes the attribute
        /// </summary>
        public void Init()
        {
            var factory = new CustomTableItemFactory(MarkedType);

            CustomTableItemGenerator.RegisterCustomTable(ClassName, factory);
        }

        #endregion
    }
}