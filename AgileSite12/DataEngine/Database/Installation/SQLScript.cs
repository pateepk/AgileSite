using System.Xml.Serialization;

namespace CMS.DataEngine
{
    /// <summary>
    /// SQL script to be applied during upgrade or hotfix.
    /// </summary>
    public class SQLScript
    {
        /// <summary>
        /// File name of the SQL script.
        /// </summary>
        [XmlAttribute("fileName")]
        public string SQLFileName
        {
            get;
            set;
        }


        /// <summary>
        /// Full path of SQL script.
        /// </summary>
        [XmlIgnore]
        public string SQLFilePath
        {
            get;
            set;
        }


        /// <summary>
        /// Name of connection string needed for script execution.
        /// </summary>
        [XmlAttribute("connectionStringName")]
        public string ConnectionStringName
        {
            get;
            set;
        }


        /// <summary>
        /// Connection string needed for script execution.
        /// </summary>
        [XmlIgnore]
        public string ConnectionString
        {
            get;
            set;
        }


        /// <summary>
        /// Order in which the script should be launched.
        /// </summary>
        [XmlAttribute("order")]
        public int SQLLaunchOrder
        {
            get;
            set;
        }
    }
}
