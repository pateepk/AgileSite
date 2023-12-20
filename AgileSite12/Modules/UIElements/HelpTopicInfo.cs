using System;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Modules;

[assembly: RegisterObjectType(typeof(HelpTopicInfo), HelpTopicInfo.OBJECT_TYPE)]

namespace CMS.Modules
{
	/// <summary>
	/// HelpTopicInfo data container class.
	/// </summary>
	public class HelpTopicInfo : AbstractInfo<HelpTopicInfo>
	{
		#region "Type information"

		/// <summary>
		/// Object type
		/// </summary>
		public const string OBJECT_TYPE = "cms.helptopic";


		/// <summary>
		/// Type information.
		/// </summary>
		public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(HelpTopicInfoProvider), OBJECT_TYPE, "CMS.HelpTopic", "HelpTopicID", "HelpTopicLastModified", "HelpTopicGUID", null, "HelpTopicName", null, null, "HelpTopicUIElementID", UIElementInfo.OBJECT_TYPE)
		{
			SynchronizationSettings =
            {
                IncludeToSynchronizationParentDataSet = IncludeToParentEnum.Complete,
                LogSynchronization = SynchronizationTypeEnum.TouchParent,
            },
			TouchCacheDependencies = true,
			OrderColumn = "HelpTopicOrder",
			SupportsInvalidation = true,
			ImportExportSettings =
			{
				IncludeToExportParentDataSet = IncludeToParentEnum.Incremental,
			},
			DefaultData = new DefaultDataSettings(),
            ContinuousIntegrationSettings =
            {
                Enabled = true,
                ObjectFileNameFields = { "HelpTopicName" }
            }
		};

		#endregion


		#region "Properties"

		/// <summary>
		/// Help topic ID
		/// </summary>
		[DatabaseField]
		public virtual int HelpTopicID
		{
			get
			{
				return ValidationHelper.GetInteger(GetValue("HelpTopicID"), 0);
			}
			set
			{
				SetValue("HelpTopicID", value);
			}
		}


		/// <summary>
		/// Help topic UI element ID
		/// </summary>
		[DatabaseField]
		public virtual int HelpTopicUIElementID
		{
			get
			{
				return ValidationHelper.GetInteger(GetValue("HelpTopicUIElementID"), 0);
			}
			set
			{
				SetValue("HelpTopicUIElementID", value);
			}
		}


		/// <summary>
		/// Help topic name
		/// </summary>
		[DatabaseField]
		public virtual string HelpTopicName
		{
			get
			{
				return ValidationHelper.GetString(GetValue("HelpTopicName"), "");
			}
			set
			{
				SetValue("HelpTopicName", value);
			}
		}


		/// <summary>
		/// Help topic link
		/// </summary>
		[DatabaseField]
		public virtual string HelpTopicLink
		{
			get
			{
				return ValidationHelper.GetString(GetValue("HelpTopicLink"), "");
			}
			set
			{
				SetValue("HelpTopicLink", value);
			}
		}


        /// <summary>
        /// Sets a macro condition that must be fulfilled for the help topic to be visible in the system's user interface..
        /// </summary>
        [DatabaseField]
        public virtual string HelpTopicVisibilityCondition
        {
            get
            {
                return ValidationHelper.GetString(GetValue("HelpTopicVisibilityCondition"), "");
            }
            set
            {
                SetValue("HelpTopicVisibilityCondition", value, "");
            }
        }


        /// <summary>
        /// Help topic order.
        /// </summary>
        [DatabaseField]
		public virtual int HelpTopicOrder
		{
			get
			{
				return ValidationHelper.GetInteger(GetValue("HelpTopicOrder"), 0);
			}
			set
			{
				SetValue("HelpTopicOrder", value, 0);
			}
		}


		/// <summary>
		/// Help topic GUID.
		/// </summary>
		[DatabaseField]
		public virtual Guid HelpTopicGUID
		{
			get
			{
				return GetGuidValue("HelpTopicGUID", Guid.Empty);
			}
			set
			{
				SetValue("HelpTopicGUID", value);
			}
		}


		/// <summary>
		/// Help topic last modified
		/// </summary>
		[DatabaseField]
		public virtual DateTime HelpTopicLastModified
		{
			get
			{
				return ValidationHelper.GetDateTime(GetValue("HelpTopicLastModified"), DateTimeHelper.ZERO_TIME);
			}
			set
			{
				SetValue("HelpTopicLastModified", value);
			}
		}

		#endregion


		#region "Type based properties and methods"

		/// <summary>
		/// Deletes the object using appropriate provider.
		/// </summary>
		protected override void DeleteObject()
		{
			HelpTopicInfoProvider.DeleteHelpTopicInfo(this);
		}


		/// <summary>
		/// Updates the object using appropriate provider.
		/// </summary>
		protected override void SetObject()
		{
			HelpTopicInfoProvider.SetHelpTopicInfo(this);
		}

		#endregion


		#region "Constructors"

		/// <summary>
		/// Constructor - Creates an empty HelpTopicInfo object.
		/// </summary>
		public HelpTopicInfo()
			: base(TYPEINFO)
		{
		}


		/// <summary>
		/// Constructor - Creates a new HelpTopicInfo object from the given DataRow.
		/// </summary>
		/// <param name="dr">DataRow with the object data</param>
		public HelpTopicInfo(DataRow dr)
			: base(TYPEINFO, dr)
		{
		}

		#endregion
	}
}