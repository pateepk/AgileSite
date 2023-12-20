using System;
using System.Data;
using System.Runtime.Serialization;
using System.Collections.Generic;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Newsletters;

[assembly: RegisterObjectType(typeof(ClickedLinkInfo), ClickedLinkInfo.OBJECT_TYPE)]
namespace CMS.Newsletters
{
    /// <summary>
    /// ClickedLinkInfo data container class.
    /// </summary>
    [Serializable]
    public class ClickedLinkInfo : AbstractInfo<ClickedLinkInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "newsletter.clickedlink";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(ClickedLinkInfoProvider), OBJECT_TYPE, "Newsletter.ClickedLink", "ClickedLinkID", null, "ClickedLinkGuid", null, null, null, null, "ClickedLinkNewsletterLinkID", LinkInfo.OBJECT_TYPE)
        {
            ModuleName = "CMS.Newsletter",            
            SynchronizationSettings =
            {
                IncludeToSynchronizationParentDataSet = IncludeToParentEnum.None,
                LogSynchronization = SynchronizationTypeEnum.None
            }            
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Clicked link ID
        /// </summary>
        [DatabaseField]
        public virtual int ClickedLinkID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("ClickedLinkID"), 0);
            }
            set
            {
                SetValue("ClickedLinkID", value);
            }
        }


        /// <summary>
        /// Clicked link guid
        /// </summary>
        [DatabaseField]
        public virtual Guid ClickedLinkGuid
        {
            get
            {
                return ValidationHelper.GetGuid(GetValue("ClickedLinkGuid"), Guid.Empty);
            }
            set
            {
                SetValue("ClickedLinkGuid", value);
            }
        }


        /// <summary>
        /// Clicked link email
        /// </summary>
        [DatabaseField]
        public virtual string ClickedLinkEmail
        {
            get
            {
                return ValidationHelper.GetString(GetValue("ClickedLinkEmail"), String.Empty);
            }
            set
            {
                SetValue("ClickedLinkEmail", value);
            }
        }


        /// <summary>
        /// Clicked link newsletter link ID
        /// </summary>
        [DatabaseField]
        public virtual int ClickedLinkNewsletterLinkID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("ClickedLinkNewsletterLinkID"), 0);
            }
            set
            {
                SetValue("ClickedLinkNewsletterLinkID", value);
            }
        }


        /// <summary>
        /// Clicked link time
        /// </summary>
        [DatabaseField]
        public virtual DateTime ClickedLinkTime
        {
            get
            {
                return ValidationHelper.GetDateTime(GetValue("ClickedLinkTime"), DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("ClickedLinkTime", value, DateTimeHelper.ZERO_TIME);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            ClickedLinkInfoProvider.DeleteClickedLinkInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            ClickedLinkInfoProvider.SetClickedLinkInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor for de-serialization.
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Streaming context</param>
        public ClickedLinkInfo(SerializationInfo info, StreamingContext context)
            : base(info, context, TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates an empty ClickedLinkInfo object.
        /// </summary>
        public ClickedLinkInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new ClickedLinkInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data</param>
        public ClickedLinkInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}