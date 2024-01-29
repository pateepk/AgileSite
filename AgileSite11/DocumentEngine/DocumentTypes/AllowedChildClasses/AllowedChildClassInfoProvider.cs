using System;
using System.Linq;

using CMS.DataEngine;
using CMS.Base;

namespace CMS.DocumentEngine
{
    using TypedDataSet = InfoDataSet<AllowedChildClassInfo>;

    /// <summary>
    /// Class providing AllowedChildClassInfo management.
    /// </summary>
    public class AllowedChildClassInfoProvider : AbstractInfoProvider<AllowedChildClassInfo, AllowedChildClassInfoProvider>
    {
        #region "Variables"

        private static bool? mCMSFileHasChildClass = null;

        #endregion


        #region "Properties"

        /// <summary>
        /// Indicates if CMS File has allowed child classes.
        /// </summary>
        public static bool CMSFileHasChildClass
        {
            get
            {
                if (mCMSFileHasChildClass == null)
                {
                    DataClassInfo dci = DataClassInfoProvider.GetDataClassInfo(SystemDocumentTypes.File);
                    mCMSFileHasChildClass = dci != null && GetAllowedChildClasses().WhereEquals("ParentClassID", dci.ClassID).HasResults();
                }

                return mCMSFileHasChildClass.Value;
            }
        }

        #endregion


        #region "Public static methods"

        /// <summary>
        /// Returns all allowed child classes.
        /// </summary>
        public static ObjectQuery<AllowedChildClassInfo> GetAllowedChildClasses()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns allowed child classes for given parent class ID and site ID.
        /// </summary>
        public static ObjectQuery<DataClassInfo> GetAllowedChildClasses(int parentClassId, int siteId)
        {
            var q = DocumentTypeHelper.GetDocumentTypeClasses()
                .OnSite(siteId)
                .WhereIn("ClassID", 
                    GetAllowedChildClasses()
                        .Where("ParentClassID", QueryOperator.Equals, parentClassId).Column("ChildClassID")
                );

            return q;
        }


        /// <summary>
        /// Returns the DataSet of all the allowed child classes records.
        /// </summary>
        /// <param name="where">Where condition</param>
        /// <param name="orderBy">Order by expression</param>
        [Obsolete("Use method GetAllowedChildClasses() instead")]
        public static TypedDataSet GetAllowedChildClasses(string where, string orderBy)
        {
            return GetAllowedChildClasses().Where(where).OrderBy(orderBy).TypedResult;
        }


        /// <summary>
        /// Returns the AllowedChildClassInfo structure for the specified allowedChildClass.
        /// </summary>
        /// <param name="parentClassId">ParentClassID</param>
        /// <param name="childClassId">ChildClassID</param>
        public static AllowedChildClassInfo GetAllowedChildClassInfo(int parentClassId, int childClassId)
        {
            return ProviderObject.GetAllowedChildClassInfoInternal(parentClassId, childClassId);
        }


        /// <summary>
        /// Sets (updates or inserts) specified allowedChildClass.
        /// </summary>
        /// <param name="allowedChildClass">AllowedChildClass to set</param>
        public static void SetAllowedChildClassInfo(AllowedChildClassInfo allowedChildClass)
        {
            ProviderObject.SetAllowedChildClassInfoInternal(allowedChildClass);
        }


        /// <summary>
        /// Deletes specified allowedChildClass.
        /// </summary>
        /// <param name="infoObj">AllowedChildClass object</param>
        public static void DeleteAllowedChildClassInfo(AllowedChildClassInfo infoObj)
        {
            ProviderObject.DeleteInfo(infoObj);
        }


        /// <summary>
        /// Deletes specified allowedChildClass.
        /// </summary>
        /// <param name="parentClassId">ParentClassID</param>
        /// <param name="childClassId">ChildClassID</param>
        public static void RemoveAllowedChildClass(int parentClassId, int childClassId)
        {
            AllowedChildClassInfo infoObj = GetAllowedChildClassInfo(parentClassId, childClassId);
            DeleteAllowedChildClassInfo(infoObj);
        }


        /// <summary>
        /// Inserts new record to CMS_AllowedChildClasses table if no one exists.
        /// </summary>
        /// <param name="parentClassId">Parent ClassID</param>
        /// <param name="childClassId">Child ClassID</param>
        public static void AddAllowedChildClass(int parentClassId, int childClassId)
        {
            // Create new binding
            AllowedChildClassInfo infoObj = new AllowedChildClassInfo();
            infoObj.ParentClassID = parentClassId;
            infoObj.ChildClassID = childClassId;

            // Save to the database
            SetAllowedChildClassInfo(infoObj);
        }


        /// <summary>
        /// Returns true if child class is allowed within given parent class.
        /// </summary>
        /// <param name="parentClassId">Parent class ID</param>
        /// <param name="childClassId">Child class ID</param>
        public static bool IsChildClassAllowed(int parentClassId, int childClassId)
        {
            if ((parentClassId <= 0) || (childClassId <= 0))
            {
                return false;
            }

            AllowedChildClassInfo infoObj = GetAllowedChildClassInfo(parentClassId, childClassId);

            return (infoObj != null);
        }

        #endregion


        #region "Internal methods"

        /// <summary>
        /// Returns the AllowedChildClassInfo structure for the specified allowedChildClass.
        /// </summary>
        /// <param name="parentClassId">ParentClassID</param>
        /// <param name="childClassId">ChildClassID</param>
        protected virtual AllowedChildClassInfo GetAllowedChildClassInfoInternal(int parentClassId, int childClassId)
        {
            return GetAllowedChildClasses().TopN(1).WhereEquals("ParentClassID", parentClassId).And().WhereEquals("ChildClassID", childClassId).FirstOrDefault();
        }


        /// <summary>
        /// Sets (updates or inserts) specified allowedChildClass.
        /// </summary>
        /// <param name="allowedChildClass">AllowedChildClass to set</param>
        protected virtual void SetAllowedChildClassInfoInternal(AllowedChildClassInfo allowedChildClass)
        {
            if (allowedChildClass != null)
            {
                // Check IDs
                if ((allowedChildClass.ChildClassID <= 0) || (allowedChildClass.ParentClassID <= 0))
                {
                    throw new Exception("[AllowedChildClassInfoProvider.SetAllowedChildClassInfo]: Object IDs not set.");
                }

                // Get existing
                AllowedChildClassInfo existing = GetAllowedChildClassInfoInternal(allowedChildClass.ParentClassID, allowedChildClass.ChildClassID);
                if (existing != null)
                {
                    // Do nothing, item does not carry any data
                }
                else
                {
                    allowedChildClass.Generalized.InsertData();

                    // If parent is CMS File, invalidate variable
                    InvalidateFileHasChildClass(allowedChildClass.ParentClassID);
                }
            }
            else
            {
                throw new Exception("[AllowedChildClassInfoProvider.SetAllowedChildClassInfo]: No AllowedChildClassInfo object set.");
            }
        }


        /// <summary>
        /// Deletes the object to the database.
        /// </summary>
        /// <param name="info">Object to delete</param>
        protected override void DeleteInfo(AllowedChildClassInfo info)
        {
            if (info != null)
            {
                base.DeleteInfo(info);

                // If parent is CMS File, invalidate variable
                InvalidateFileHasChildClass(info.ParentClassID);
            }
        }


        /// <summary>
        /// Invalidates variable CMSFileHasChildClass
        /// </summary>
        /// <param name="classId">Class ID</param>
        private void InvalidateFileHasChildClass(int classId)
        {
            DataClassInfo dci = DataClassInfoProvider.GetDataClassInfo(classId);
            if (dci.ClassName.EqualsCSafe(SystemDocumentTypes.File, true))
            {
                mCMSFileHasChildClass = null;
            }
        }

        #endregion
    }
}