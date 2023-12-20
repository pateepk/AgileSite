using System;
using System.Linq;
using System.Text;

namespace CMS.DataEngine
{
    /// <summary>
    /// Abstract object info class.
    /// </summary>
    public abstract partial class AbstractInfoBase<TInfo>
    {
        /// <summary>
        /// Info object wrapper for generalized access
        /// </summary>
        public new abstract class GeneralizedInfoWrapper : GeneralizedAbstractInfo
        {
            #region "General properties"

            /// <summary>
            /// Main object
            /// </summary>
            public new AbstractInfoBase<TInfo> MainObject
            {
                get;
                protected set;
            }

            #endregion


            #region "Properties"

            /// <summary>
            /// If true, version GUID of the object is updated when saved.
            /// </summary>
            public virtual bool UpdateVersionGUID
            {
                get
                {
                    return MainObject.UpdateVersionGUID;
                }
                set
                {
                    MainObject.UpdateVersionGUID = value;
                }
            }


            /// <summary>
            /// Indicates if all physical files should be deleted when object will be deleted.
            /// </summary>
            public virtual bool DeleteFiles
            {
                get
                {
                    return MainObject.DeleteFiles;
                }
                set
                {
                    MainObject.DeleteFiles = value;
                }
            }


            /// <summary>
            /// Returns the original object code name
            /// </summary>
            public override string OriginalObjectCodeName
            {
                get
                {
                    return MainObject.OriginalObjectCodeName;
                }
            }

            #endregion


            #region "Methods"

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="mainObj">Main object to wrap</param>
            protected GeneralizedInfoWrapper(AbstractInfoBase<TInfo> mainObj)
                : base(mainObj)
            {
                base.MainObject = mainObj;
                MainObject = mainObj;
            }


            /// <summary>
            /// Deletes the object from the database.
            /// </summary>
            public override void DeleteData()
            {
                MainObject.DeleteData();
            }


            /// <summary>
            /// Updates the object to the database.
            /// </summary>
            public override void UpdateData()
            {
                MainObject.UpdateData();
            }


            /// <summary>
            /// Inserts the object to the database.
            /// </summary>
            public override void InsertData()
            {
                MainObject.InsertData();
            }


            /// <summary>
            /// Updates or inserts the object to the database.
            /// </summary>
            public void SetData()
            {
                MainObject.SetData();
            }

            #endregion
        }
    }
}
