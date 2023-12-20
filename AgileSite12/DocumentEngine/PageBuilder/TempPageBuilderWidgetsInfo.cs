using System;
using System.Data;
using System.Runtime.Serialization;

using CMS;
using CMS.Core;
using CMS.DataEngine;
using CMS.DocumentEngine.PageBuilder;
using CMS.Helpers;

[assembly: RegisterObjectType(typeof(TempPageBuilderWidgetsInfo), TempPageBuilderWidgetsInfo.OBJECT_TYPE)]

namespace CMS.DocumentEngine.PageBuilder
{
    /// <summary>
    /// Data container class for <see cref="TempPageBuilderWidgetsInfo"/>.
    /// </summary>
    [Serializable]
    public class TempPageBuilderWidgetsInfo : AbstractInfo<TempPageBuilderWidgetsInfo>
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public const string OBJECT_TYPE = "temp.pagebuilderwidgets";


        /// <summary>
        /// Type information.
        /// </summary>
        public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(TempPageBuilderWidgetsInfoProvider), OBJECT_TYPE, "Temp.PageBuilderWidgets", "PageBuilderWidgetsID", "PageBuilderWidgetsLastModified", null, null, null, null, null, null, null)
        {
            ModuleName = ModuleName.CONTENT,
            MacroCollectionName = "CMS.TempPageBuilderWidgets",
            ContainsMacros = false
        };


        /// <summary>
        /// Page builder widgets ID.
        /// </summary>
        [DatabaseField]
        public virtual int PageBuilderWidgetsID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("PageBuilderWidgetsID"), 0);
            }
            set
            {
                SetValue("PageBuilderWidgetsID", value);
            }
        }


        /// <summary>
        /// Page builder widgets configuration.
        /// </summary>
        [DatabaseField]
        public virtual string PageBuilderWidgetsConfiguration
        {
            get
            {
                return ValidationHelper.GetString(GetValue("PageBuilderWidgetsConfiguration"), String.Empty);
            }
            set
            {
                SetValue("PageBuilderWidgetsConfiguration", value, String.Empty);
            }
        }


        /// <summary>
        /// Page builder page template configuration.
        /// </summary>
        [DatabaseField]
        public virtual string PageBuilderTemplateConfiguration
        {
            get
            {
                return ValidationHelper.GetString(GetValue("PageBuilderTemplateConfiguration"), String.Empty);
            }
            set
            {
                SetValue("PageBuilderTemplateConfiguration", value, String.Empty);
            }
        }


        /// <summary>
        /// Page builder widgets guid.
        /// </summary>
        [DatabaseField]
        public virtual Guid PageBuilderWidgetsGuid
        {
            get
            {
                return ValidationHelper.GetGuid(GetValue("PageBuilderWidgetsGuid"), Guid.Empty);
            }
            set
            {
                SetValue("PageBuilderWidgetsGuid", value);
            }
        }


        /// <summary>
        /// Page builder widgets last modified.
        /// </summary>
        [DatabaseField]
        public virtual DateTime PageBuilderWidgetsLastModified
        {
            get
            {
                return ValidationHelper.GetDateTime(GetValue("PageBuilderWidgetsLastModified"), DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("PageBuilderWidgetsLastModified", value);
            }
        }


        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            TempPageBuilderWidgetsInfoProvider.DeletePageBuilderWidgetsInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            TempPageBuilderWidgetsInfoProvider.SetPageBuilderWidgetsInfo(this);
        }


        /// <summary>
        /// Constructor for de-serialization.
        /// </summary>
        /// <param name="info">Serialization info.</param>
        /// <param name="context">Streaming context.</param>
        protected TempPageBuilderWidgetsInfo(SerializationInfo info, StreamingContext context)
            : base(info, context, TYPEINFO)
        {
        }


        /// <summary>
        /// Creates an empty instance of the <see cref="TempPageBuilderWidgetsInfo"/> class.
        /// </summary>
        public TempPageBuilderWidgetsInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Creates a new instances of the <see cref="TempPageBuilderWidgetsInfo"/> class from the given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public TempPageBuilderWidgetsInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }
    }
}