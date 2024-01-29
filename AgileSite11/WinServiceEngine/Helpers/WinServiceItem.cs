using System;
using System.Xml;

using CMS.Base;
using CMS.Helpers;

namespace CMS.WinServiceEngine
{
    /// <summary>
    /// Class representing windows service definition item.
    /// </summary>
    public class WinServiceItem
    {
        #region "Properties"

        /// <summary>
        /// Base name of service in definition file.
        /// </summary>
        public string BaseName
        {
            get;
            set;
        }


        /// <summary>
        /// Service name in definition file.
        /// </summary>
        public string Name
        {
            get;
            set;
        }


        /// <summary>
        /// Display name of service in definition file.
        /// </summary>
        public string DisplayName
        {
            get;
            set;
        }


        /// <summary>
        /// Description of the service in definition file.
        /// </summary>
        public string Description
        {
            get;
            set;
        }


        /// <summary>
        /// Assembly name of service.
        /// </summary>
        public string AssemblyName
        {
            get;
            set;
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Creates instance and initializes properties from xml node.
        /// </summary>
        /// <param name="node">Node</param>
        public WinServiceItem(XmlNode node)
        {
            foreach (XmlNode child in node.ChildNodes)
            {
                switch (child.Name.ToLowerCSafe())
                {
                    case "basename":
                        BaseName = child.InnerText;
                        break;

                    case "name":
                        Name = child.InnerText;
                        break;

                    case "displayname":
                        DisplayName = child.InnerText;
                        break;

                    case "description":
                        Description = child.InnerText;
                        break;

                    case "assemblyname":
                        AssemblyName = child.InnerText;
                        break;
                }
            }

            // Check data integrity
            if (string.IsNullOrEmpty(BaseName) || string.IsNullOrEmpty(Name) || string.IsNullOrEmpty(DisplayName) || string.IsNullOrEmpty(AssemblyName))
            {
                throw new Exception("Wrong services configuration. Missing either BaseName, Name, DisplayName or AssemblyName attribute is in wrong format.");
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Gets service name.
        /// </summary>
        public string GetServiceName()
        {
            return WinServiceHelper.FormatServiceName(Name, SystemHelper.ApplicationName, SystemHelper.ApplicationIdentifier);
        }


        /// <summary>
        /// Gets service display name.
        /// </summary>
        public string GetServiceDisplayName()
        {
            return WinServiceHelper.FormatServiceDisplayName(DisplayName, SystemHelper.ApplicationName);
        }

        #endregion
    }
}