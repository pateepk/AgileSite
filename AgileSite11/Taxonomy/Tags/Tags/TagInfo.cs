using System;
using System.Data;

using CMS;
using CMS.Core;
using CMS.DataEngine;
using CMS.Taxonomy;

[assembly: RegisterObjectType(typeof(TagInfo), TagInfo.OBJECT_TYPE)]

namespace CMS.Taxonomy
{
    /// <summary>
    /// TagInfo data container class.
    /// </summary>
    public class TagInfo : AbstractInfo<TagInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "cms.tag";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(TagInfoProvider), OBJECT_TYPE, "CMS.Tag", "TagID", null, "TagGUID", null, "TagName", null, null, "TagGroupID", TagGroupInfo.OBJECT_TYPE)
        {
            SynchronizationSettings =
            {
                IncludeToSynchronizationParentDataSet = IncludeToParentEnum.None,
                LogSynchronization = SynchronizationTypeEnum.None,
            },
                                                  
            TouchCacheDependencies = true,

            AllowRestore = false,
            ModuleName = ModuleName.CONTENT,
            SupportsCloning = false,
            ImportExportSettings =
            {
                IncludeToExportParentDataSet = IncludeToParentEnum.None
            }
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Number of documents tagged with this tag.
        /// </summary>
        [DatabaseField]
        public virtual int TagCount
        {
            get
            {
                return GetIntegerValue("TagCount", 0);
            }
            set
            {
                SetValue("TagCount", value);
            }
        }


        /// <summary>
        /// Name of the tag.
        /// </summary>
        [DatabaseField]
        public virtual string TagName
        {
            get
            {
                return GetStringValue("TagName", "");
            }
            set
            {
                SetValue("TagName", value);
            }
        }


        /// <summary>
        /// ID of the tag group.
        /// </summary>
        [DatabaseField]
        public virtual int TagGroupID
        {
            get
            {
                return GetIntegerValue("TagGroupID", 0);
            }
            set
            {
                SetValue("TagGroupID", value);
            }
        }


        /// <summary>
        /// ID of the tag.
        /// </summary>
        [DatabaseField]
        public virtual int TagID
        {
            get
            {
                return GetIntegerValue("TagID", 0);
            }
            set
            {
                SetValue("TagID", value);
            }
        }

        /// <summary>
        /// Tag GUID
        /// </summary>
        [DatabaseField]
        public virtual Guid TagGUID
        {
            get
            {
                return GetGuidValue("TagGUID", Guid.Empty);
            }
            set
            {
                SetValue("TagGUID", value);
            }
        }
        
        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            TagInfoProvider.DeleteTagInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            TagInfoProvider.SetTagInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty TagInfo object.
        /// </summary>
        public TagInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new TagInfo object from the given DataRow.
        /// </summary>
        public TagInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}