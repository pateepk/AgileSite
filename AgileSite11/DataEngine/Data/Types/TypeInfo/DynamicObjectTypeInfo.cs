using System;
using System.Linq;
using System.Text;

namespace CMS.DataEngine
{
    /// <summary>
    /// Dynamic object type info structure for objects which definition can be changed during the lifetime.
    /// </summary>
    public class DynamicObjectTypeInfo : ObjectTypeInfo
    {
        #region "Properties"

        /// <summary>
        /// Indicates if dynamic type info is valid and shouldn't be updated based on current state of definition
        /// </summary>
        public bool IsValid
        {
            get;
            set;
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="providerType">Provider type</param>
        /// <param name="objectType">Object type</param>
        /// <param name="objectClassName">Object class name</param>
        /// <param name="idColumn">ID column name</param>
        /// <param name="timeStampColumn">Time stamp column name</param>
        /// <param name="guidColumn">GUID column name</param>
        /// <param name="codeNameColumn">Code name column name</param>
        /// <param name="displayNameColumn">Display name column name</param>
        /// <param name="binaryColumn">Binary column name</param>
        /// <param name="siteIDColumn">Site ID column name</param>
        /// <param name="parentIDColumn">Parent ID column name</param>
        /// <param name="parentObjectType">Parent object type</param>
        public DynamicObjectTypeInfo(Type providerType, string objectType, string objectClassName, string idColumn, string timeStampColumn, string guidColumn, string codeNameColumn, string displayNameColumn, string binaryColumn, string siteIDColumn, string parentIDColumn, string parentObjectType) :
            base(providerType, objectType, objectClassName, idColumn, timeStampColumn, guidColumn, codeNameColumn, displayNameColumn, binaryColumn, siteIDColumn, parentIDColumn, parentObjectType)
        {
            IsValid = true;
        }

        #endregion
    }
}