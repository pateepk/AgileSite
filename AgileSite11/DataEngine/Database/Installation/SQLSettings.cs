using System.Collections.Generic;
using System.Xml.Serialization;

namespace CMS.DataEngine
{
    /// <summary>
    /// Provides SQL settings for hotfixing databases.
    /// </summary>
    [XmlRoot("settings")]
    public class SQLSettings
    {
        #region "Variables"

        private List<SQLScript> mScripts = null;
        
        #endregion


        #region "Properties"

        /// <summary>
        /// Collection of SQL scripts.
        /// </summary>
        [XmlArray("scripts")]
        [XmlArrayItem("script")]
        public List<SQLScript> Scripts
        {
            get
            {
                return mScripts ?? (mScripts = new List<SQLScript>());
            }
            set
            {
                mScripts = value;
            }
        }

        #endregion
    }
}
