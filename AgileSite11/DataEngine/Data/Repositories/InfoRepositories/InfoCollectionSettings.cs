using CMS.Core;

namespace CMS.DataEngine
{
    /// <summary>
    /// Settings for the info object collection
    /// </summary>
    public class InfoCollectionSettings : BaseCollectionSettings
    {
        #region "Properties"

        /// <summary>
        /// Object type
        /// </summary>
        public string ObjectType
        {
            get;
            set;
        }


        /// <summary>
        /// Object type
        /// </summary>
        protected internal override string ObjectTypeInternal
        {
            get
            {
                return ObjectType;
            }
        }
        

        /// <summary>
        /// Factory to provide new collection object
        /// </summary>
        public IObjectFactory CollectionFactory
        {
            get;
            set;
        }

        #endregion


        #region "Constructor"

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Collection name</param>
        /// <param name="objectType">Object type</param>
        public InfoCollectionSettings(string name, string objectType)
            : base(name ?? objectType)
        {
            ObjectType = objectType;
        }

        #endregion
    }
}