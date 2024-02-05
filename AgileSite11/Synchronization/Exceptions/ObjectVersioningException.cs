using CMS.DataEngine;
using CMS.Base;
using System;

namespace CMS.Synchronization
{
    /// <summary>
    /// Base class for the exceptions raised during versioning methods (check in/out, etc.).
    /// </summary>
    public class ObjectVersioningException : Exception
    {
        /// <summary>
        /// Gets or sets the object settings info.
        /// </summary>
        public ObjectSettingsInfo Settings
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the name of the operation.
        /// </summary>
        public string Operation
        {
            get;
            set;
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="settings">Object settings</param>
        /// <param name="operation">Operation processed</param>
        public ObjectVersioningException(ObjectSettingsInfo settings, string operation)
        {
            Settings = settings;
            Operation = operation;
        }
    }
}