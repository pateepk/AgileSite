using System;
using System.Xml.Serialization;

namespace CMS.Core
{
    /// <summary>
    /// Installation meta data about single installation module, shipped with module via NuGet.
    /// </summary>
    [XmlRoot("moduleInstallationMetaData")]
    [Serializable]
    public class ModuleInstallationMetaData
    {
        /// <summary>
        /// Name of the module.
        /// </summary>
        [XmlElement("name")]
        public string Name
        {
            get;
            set;
        }


        /// <summary>
        /// Version of the module (e.g. "1.2.3").
        /// </summary>
        [XmlElement("version")]
        public string Version
        {
            get;
            set;
        }
  
    }
}
