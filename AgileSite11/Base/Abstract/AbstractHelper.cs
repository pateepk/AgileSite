using CMS.Core;

namespace CMS.Base
{
    /// <summary>
    /// Abstract helper.
    /// </summary>
    public abstract class AbstractHelper<HelperType> : AbstractHelper
        where HelperType : AbstractHelper<HelperType>, new()    
    {
        #region "Provider object methods"

        /// <summary>
        /// Helper object.
        /// </summary> 
        private static HelperType mHelperObject;


        /// <summary>
        /// Helper object.
        /// </summary>
        public static HelperType HelperObject
        {
            get
            {
                return mHelperObject ?? (mHelperObject = CMSExtensibilitySection.LoadHelper<HelperType>());
            }
            set
            {
                mHelperObject = value;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Static constructor
        /// </summary>
        static AbstractHelper()
        {
            TypeManager.RegisterGenericType(typeof (AbstractHelper<HelperType>));
        }


        /// <summary>
        /// Sets this object as the default helper
        /// </summary>
        public override void SetAsDefaultHelper()
        {
            HelperObject = (HelperType)this;
        }

        #endregion
    }


    /// <summary>
    /// Abstract helper.
    /// </summary>
    public abstract class AbstractHelper : CoreMethods
    {
        #region "Methods"
               
        /// <summary>
        /// Sets this object as the default helper
        /// </summary>
        public abstract void SetAsDefaultHelper();

        #endregion
    }
}