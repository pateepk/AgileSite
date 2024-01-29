using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;


namespace CMS.Modules
{
    /// <summary>
    /// Thrown when module installation operation fails.
    /// </summary>
    [Serializable]
    public class ModuleInstallationException : Exception
    {
        /// <summary>
        /// Gets name of the module that caused the exception.
        /// </summary>
        public string ModuleName
        {
            get;
            protected set;
        }


        /// <summary>
        /// Gets version of the module that caused the exception.
        /// </summary>
        public string ModuleVersion
        {
            get;
            protected set;
        }


        /// <summary>
        /// Gets the name of installation operation that caused the exception.
        /// </summary>
        public string Operation
        {
            get;
            protected set;
        }


        /// <summary>
        /// Creates a new instance of ModuleInstallationException.
        /// </summary>
        public ModuleInstallationException()
        {

        }


        /// <summary>
        /// Creates a new instance of ModuleInstallationException.
        /// </summary>
        /// <param name="message">Exception's message.</param>
        /// <param name="moduleName">Name of the module that caused the exception.</param>
        /// <param name="operation">Name of the operation that caused the exception.</param>
        /// <param name="moduleVersion">Version of the module that caused the exception.</param>
        public ModuleInstallationException(string message, string moduleName, string operation, string moduleVersion = null)
            : base(message)
        {
            ModuleName = moduleName;
            Operation = operation;
            ModuleVersion = moduleVersion;
        }


        /// <summary>
        /// Creates a new instance of ModuleInstallationException.
        /// </summary>
        /// <param name="message">Exception's message.</param>
        /// <param name="moduleName">Name of the module that caused the exception.</param>
        /// <param name="operation">Name of the operation that caused the exception.</param>
        /// <param name="inner">Inner exception.</param>
        public ModuleInstallationException(string message, string moduleName, string operation, Exception inner)
            : base(message, inner)
        {
            ModuleName = moduleName;
            Operation = operation;
        }


        /// <summary>
        /// Creates a new instance of ModuleInstallationException.
        /// </summary>
        /// <param name="message">Exception's message.</param>
        /// <param name="moduleName">Name of the module that caused the exception.</param>
        /// <param name="operation">Name of the operation that caused the exception.</param>
        /// <param name="moduleVersion">Version of the module that caused the exception.</param>
        /// <param name="inner">Inner exception.</param>
        public ModuleInstallationException(string message, string moduleName, string operation, string moduleVersion, Exception inner)
            : base(message, inner)
        {
            ModuleName = moduleName;
            Operation = operation;
            ModuleVersion = moduleVersion;
        }


        /// <summary>
        /// Creates a new instance of ModuleInstallationException.
        /// </summary>
        /// <param name="info">SerializationInfo.</param>
        /// <param name="context">StreamingContext.</param>
        protected ModuleInstallationException(
            SerializationInfo info,
            StreamingContext context)
            : base(info, context)
        {
            ModuleName = info.GetString("ModuleName");
            ModuleVersion = info.GetString("ModuleVersion");
            Operation = info.GetString("Operation");
        }


        /// <summary>
        /// Populates a SerializationInfo with the data needed to serialize the target object.
        /// </summary>
        /// <param name="info">The SerializationInfo to populate with data. </param>
        /// <param name="context">The destination (see StreamingContext) for this serialization. </param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }

            info.AddValue("ModuleName", ModuleName);
            info.AddValue("ModuleVersion", ModuleVersion);
            info.AddValue("Operation", Operation);

            base.GetObjectData(info, context);
        }
    }
}
