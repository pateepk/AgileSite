using System;

namespace CMS.DataEngine
{
    /// <summary>
    /// Base info object interface for abstract info
    /// </summary>
    public abstract class GeneralizedAbstractInfo : GeneralizedInfo
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="mainObj">Main object</param>
        protected GeneralizedAbstractInfo(BaseInfo mainObj)
            : base(mainObj, null)
        {
        }
    }


    /// <summary>
    /// Info object interface for abstract info
    /// </summary>
    public class GeneralizedAbstractInfo<TInfo> : AbstractInfoBase<TInfo>.GeneralizedInfoWrapper
        where TInfo : AbstractInfoBase<TInfo>, new()
    {
        #region "Constructors"

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="mainObj">Main object</param>
        /// <param name="dummy">Dummy object to separate the protected constructor</param>
        protected GeneralizedAbstractInfo(AbstractInfoBase<TInfo> mainObj, object dummy)
            : base(mainObj)
        {
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="mainObj">Main object</param>
        internal GeneralizedAbstractInfo(AbstractInfoBase<TInfo> mainObj)
            : base(mainObj)
        {
        }

        #endregion


        #region "Overriden properties"

        /// <summary>
        /// Returns true if the object has changed.
        /// </summary>
        public override bool HasChanged
        {
            get
            {
                return MainObject.HasChanged;
            }
        }


        /// <summary>
        /// Returns true if the object is complete (has all columns).
        /// </summary>
        public override bool IsComplete
        {
            get
            {
                return MainObject.IsComplete;
            }
        }

        #endregion


        #region "Overridden methods"

        /// <summary>
        /// Reverts the object changes to the original values.
        /// </summary>
        public override void RevertChanges()
        {
            MainObject.RevertChanges();
        }


        /// <summary>
        /// Resets the object changes and keeps the new values as unchanged.
        /// </summary>
        public override void ResetChanges()
        {
            MainObject.ResetChanges();
        }


        /// <summary>
        /// Returns the original value of column.
        /// </summary>
        /// <param name="columnName">Column name</param>
        public override object GetOriginalValue(string columnName)
        {
            return MainObject.GetOriginalValue(columnName);
        }


        /// <summary>
        /// Gets the column type.
        /// </summary>
        /// <param name="columnName">Column name</param>
        public override Type GetColumnType(string columnName)
        {
            return MainObject.TypeInfo.ClassStructureInfo.GetColumnType(columnName);
        }

        #endregion
    }
}