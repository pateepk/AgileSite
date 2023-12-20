using System;

using CMS.Base;
using CMS.DataEngine;

namespace CMS.OnlineForms
{
    /// <summary>
    /// Type info for the BizForm items
    /// </summary>
    public class BizFormTypeInfo : DynamicObjectTypeInfo
    {
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
        public BizFormTypeInfo(Type providerType, string objectType, string objectClassName, string idColumn, string timeStampColumn, string guidColumn, string codeNameColumn, string displayNameColumn, string binaryColumn, string siteIDColumn, string parentIDColumn, string parentObjectType)
            : base(providerType, objectType, objectClassName, idColumn, timeStampColumn, guidColumn, codeNameColumn, displayNameColumn, binaryColumn, siteIDColumn, parentIDColumn, parentObjectType)
        {
        }


        /// <summary>
        /// Gets the nice object type name for this type
        /// </summary>
        public override string GetNiceObjectTypeName()
        {
            return TypeHelper.GetNiceObjectTypeName(BizFormItemProvider.BIZFORM_ITEM_PREFIX.Trim('.'));
        }


        /// <summary>
        /// Copies the event's hooks from current ObjectTypeInfo to specified one.
        /// </summary>
        /// <param name="info">Target.</param>
        internal void CopyBizFormTypeInfoEventsTo(ObjectTypeInfo info)
        {
            CopyEventsTo(info);
        }
    }
}
