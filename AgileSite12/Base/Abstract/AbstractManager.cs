using System;

namespace CMS.Base
{
    /// <summary>
    /// Abstract manager
    /// </summary>
    public abstract class AbstractManager<ManagerType> : AbstractManager
        where ManagerType : AbstractManager<ManagerType>, new()
    {
        /// <summary>
        /// Changes the manager type to the given type
        /// </summary>
        /// <param name="newType">New manager type</param>
        public override void ChangeManagerTypeTo(Type newType)
        {
            ChangeManagerType<ManagerType>(newType);
        }


        /// <summary>
        /// Gets the instance of the helper.
        /// </summary>
        public static ManagerType GetInstance()
        {
            return LoadManager<ManagerType>();
        }
    }
    

    /// <summary>
    /// Abstract manager
    /// </summary>
    public abstract class AbstractManager
    {
        #region "Methods"

        /// <summary>
        /// Loads the manager.
        /// </summary>
        protected static ManagerType LoadManager<ManagerType>()
            where ManagerType : class, new()
        {
            return CMSExtensibilitySection.LoadManager<ManagerType>();
        }


        /// <summary>
        /// Changes the manager type to the given type
        /// </summary>
        /// <param name="newType">New manager type</param>
        public abstract void ChangeManagerTypeTo(Type newType);
        

        /// <summary>
        /// Changes the default manager type to the given type
        /// </summary>
        /// <param name="newType">New manager type</param>
        protected void ChangeManagerType<OriginalType>(Type newType)
            where OriginalType : class, new()
        {
            CMSExtensibilitySection.ChangeManagerType<OriginalType>(newType);
        }

        #endregion
    }
}