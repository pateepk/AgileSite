using System;
using CMS.DataEngine;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Collection settings for Document collection
    /// </summary>
    public class TreeNodeCollectionSettings : BaseCollectionSettings
    {
        /// <summary>
        /// Lower name of the collection for indexation
        /// </summary>
        public string LowerName
        {
            get;
            set;
        }


        /// <summary>
        /// Class name
        /// </summary>
        public string ClassName
        {
            get;
            set;
        }


        /// <summary>
        /// Object type
        /// </summary>
        protected override string ObjectTypeInternal
        {
            get
            {
                return TreeNodeProvider.GetObjectType(ClassName);
            }
        }
        

        /// <summary>
        /// Alias path
        /// </summary>
        public string AliasPath
        {
            get;
            set;
        }


        /// <summary>
        /// Culture code
        /// </summary>
        public string CultureCode
        {
            get;
            set;
        }


        /// <summary>
        /// Combine the documents with default culture
        /// </summary>
        public bool CombineWithDefaultCulture
        {
            get;
            set;
        }


        /// <summary>
        /// Class names of the documents to get
        /// </summary>
        public string ClassNames
        {
            get;
            set;
        }


        /// <summary>
        /// Maximum relative level
        /// </summary>
        public int MaxRelativeLevel
        {
            get;
            set;
        }


        /// <summary>
        /// Select only published documents
        /// </summary>
        public bool SelectOnlyPublished
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if coupled data should be included
        /// </summary>
        public bool SelectAllData
        {
            get;
            set;
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Collection name</param>
        public TreeNodeCollectionSettings(string name)
            : base(name)
        {
            AliasPath = TreeProvider.ALL_DOCUMENTS;
            MaxRelativeLevel = TreeProvider.ALL_LEVELS;
            CultureCode = TreeProvider.ALL_CULTURES;
        }
    }
}