using System;
using System.Data;
using System.Runtime.Serialization;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Search.Azure;

[assembly: RegisterObjectType(typeof(SearchTaskAzureInfo), SearchTaskAzureInfo.OBJECT_TYPE)]

namespace CMS.Search.Azure
{
    /// <summary>
    /// Data container class for <see cref="SearchTaskAzureInfo"/>.
    /// </summary>
	[Serializable]
    public class SearchTaskAzureInfo : AbstractInfo<SearchTaskAzureInfo>
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public const string OBJECT_TYPE = "cms.azuresearchtask";


        /// <summary>
        /// Type information.
        /// </summary>
        public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(SearchTaskAzureInfoProvider), OBJECT_TYPE, "CMS.SearchTaskAzure", nameof(SearchTaskAzureID), null, null, null, null, null, null, null, null)
        {
            SupportsVersioning = false,
            LogIntegration = false,
            SupportsCloning = false,
            ContainsMacros = false
        };


        /// <summary>
        /// Search task Azure ID.
        /// </summary>
		[DatabaseField]
        public virtual int SearchTaskAzureID
        {
            get
            {
                return GetIntegerValue(nameof(SearchTaskAzureID), 0);
            }
            set
            {
                SetValue(nameof(SearchTaskAzureID), value);
            }
        }


        /// <summary>
        /// Type of the Azure Search task type e.g. <see cref="SearchTaskTypeEnum.Rebuild"/>.
        /// </summary>
		[DatabaseField(ColumnName = nameof(SearchTaskAzureType), ValueType = typeof(string))]
        public virtual SearchTaskTypeEnum SearchTaskAzureType
        {
            get
            {
                return GetStringValue(nameof(SearchTaskAzureType), "").ToEnum<SearchTaskTypeEnum>();
            }
            set
            {
                SetValue(nameof(SearchTaskAzureType), value.ToStringRepresentation());
            }
        }


        /// <summary>
        /// Object type of the object defined by ID in <see cref="SearchTaskAzureInitiatorObjectID"/> property.
        /// </summary>
		[DatabaseField]
        public virtual string SearchTaskAzureObjectType
        {
            get
            {
                return GetStringValue(nameof(SearchTaskAzureObjectType), String.Empty);
            }
            set
            {
                SetValue(nameof(SearchTaskAzureObjectType), value, String.Empty);
            }
        }


        /// <summary>
        /// Value of this field describes in what form to expect data inside <see cref="SearchTaskAzureAdditionalData"/> property.
        /// </summary>
		[DatabaseField]
        public virtual string SearchTaskAzureMetadata
        {
            get
            {
                return GetStringValue(nameof(SearchTaskAzureMetadata), String.Empty);
            }
            set
            {
                SetValue(nameof(SearchTaskAzureMetadata), value);
            }
        }


        /// <summary>
        /// Additional data needed to process search task e.g. alias path of the moved document.
        /// </summary>
		[DatabaseField]
        public virtual string SearchTaskAzureAdditionalData
        {
            get
            {
                return GetStringValue(nameof(SearchTaskAzureAdditionalData), String.Empty);
            }
            set
            {
                SetValue(nameof(SearchTaskAzureAdditionalData), value);
            }
        }


        /// <summary>
        /// ID of the object that initiated creation of this search task e.g. update task if object got modified. 
        /// </summary>
        /// <remarks>ID of document that had it's content changed, ID of an modified object or class ID of the custom table or page type.</remarks>
		[DatabaseField]
        public virtual int SearchTaskAzureInitiatorObjectID
        {
            get
            {
                return GetIntegerValue(nameof(SearchTaskAzureInitiatorObjectID), 0);
            }
            set
            {
                SetValue(nameof(SearchTaskAzureInitiatorObjectID), value, 0);
            }
        }


        /// <summary>
        /// Contains error message if error occurred while processing the task. 
        /// </summary>
        [DatabaseField]
        public virtual string SearchTaskAzureErrorMessage
        {
            get
            {
                return GetStringValue(nameof(SearchTaskAzureErrorMessage), String.Empty);
            }
            set
            {
                SetValue(nameof(SearchTaskAzureErrorMessage), value, String.Empty);
            }
        }


        /// <summary>
        /// Determines which task should be processed earlier, higher value = higher priority.
        /// </summary>
        [DatabaseField]
        public virtual int SearchTaskAzurePriority
        {
            get
            {
                return GetIntegerValue(nameof(SearchTaskAzurePriority), 0);
            }
            set
            {
                SetValue(nameof(SearchTaskAzurePriority), value);
            }
        }


        /// <summary>
        /// Gets the task creation time.
        /// </summary>
		[DatabaseField]
        public virtual DateTime SearchTaskAzureCreated
        {
            get
            {
                return GetDateTimeValue(nameof(SearchTaskAzureCreated), DateTimeHelper.ZERO_TIME);
            }
        }


        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            SearchTaskAzureInfoProvider.DeleteSearchTaskAzureInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            SearchTaskAzureInfoProvider.SetSearchTaskAzureInfo(this);
        }


        /// <summary>
        /// Constructor for de-serialization.
        /// </summary>
        /// <param name="info">Serialization info.</param>
        /// <param name="context">Streaming context.</param>
        protected SearchTaskAzureInfo(SerializationInfo info, StreamingContext context)
            : base(info, context, TYPEINFO)
        {
        }


        /// <summary>
        /// Creates an empty instance of the <see cref="SearchTaskAzureInfo"/> class.
        /// </summary>
        public SearchTaskAzureInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Creates a new instances of the <see cref="SearchTaskAzureInfo"/> class from the given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public SearchTaskAzureInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }
    }
}