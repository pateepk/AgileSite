using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.Newsletters;

[assembly: RegisterObjectType(typeof(LinkInfo), LinkInfo.OBJECT_TYPE)]

namespace CMS.Newsletters
{
    /// <summary>
    /// LinkInfo data container class.
    /// </summary>
    public class LinkInfo : AbstractInfo<LinkInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "newsletter.link";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(LinkInfoProvider), OBJECT_TYPE, "Newsletter.Link", "LinkID", null, "LinkGUID", null, "LinkTarget", null, null, "LinkIssueID", IssueInfo.OBJECT_TYPE)
        {
            SynchronizationSettings =
            {
                IncludeToSynchronizationParentDataSet = IncludeToParentEnum.None,
                LogSynchronization = SynchronizationTypeEnum.None
            },
            LogEvents = false,
            RegisterAsChildToObjectTypes = new List<string>() { IssueInfo.OBJECT_TYPE, IssueInfo.OBJECT_TYPE_VARIANT },
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets or sets the ID of this LinkInfo object.
        /// </summary>
        /// <value>
        /// ID of this LinkInfo object or 0 if not present.
        /// </value>
        public virtual int LinkID
        {
            get
            {
                return GetIntegerValue("LinkID", 0);
            }
            set
            {
                SetValue("LinkID", value);
            }
        }


        /// <summary>
        /// Gets or sets ID of the newsletter's issue.
        /// </summary>
        /// <value>
        /// Newsletter issue's ID, or 0 if not found.
        /// </value>
        public virtual int LinkIssueID
        {
            get
            {
                return GetIntegerValue("LinkIssueID", 0);
            }
            set
            {
                SetValue("LinkIssueID", value);
            }
        }


        /// <summary>
        /// Gets or sets the unique identifier of this link.
        /// </summary>
        /// <value>
        /// Link's unique identifier or empty unique identifier if not present.
        /// </value>
        public virtual Guid LinkGUID
        {
            get
            {
                return GetGuidValue("LinkGUID", Guid.Empty);
            }
            set
            {
                SetValue("LinkGUID", value);
            }
        }


        /// <summary>
        /// Gets or sets the original link URL.
        /// </summary>
        /// <value>
        /// The original URL of the link, or empty string if not present.
        /// </value>
        public virtual string LinkTarget
        {
            get
            {
                return GetStringValue("LinkTarget", string.Empty);
            }
            set
            {
                SetValue("LinkTarget", value);
            }
        }


        /// <summary>
        /// Gets or sets the description of this link.
        /// </summary>
        /// <value>
        /// The description contains text from <em>title</em> parameter of the link.
        /// </value>
        public virtual string LinkDescription
        {
            get
            {
                return GetStringValue("LinkDescription", "");
            }
            set
            {
                SetValue("LinkDescription", value);
            }
        }

        
        /// <summary>
        /// Object full name if defined
        /// </summary>
        protected override string ObjectFullName => ObjectHelper.BuildFullName(LinkIssueID.ToString(), LinkTarget);

        #endregion


        #region "Constructors"

        /// <summary>
        /// Creates a new LinkInfo object.
        /// </summary>
        public LinkInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Creates a new LinkInfo object from the specified DataRow.
        /// </summary>
        /// <param name="linkInfoRow">Raw values from DB table that represent this object</param>
        public LinkInfo(DataRow linkInfoRow)
            : base(TYPEINFO, linkInfoRow)
        {
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Deletes this LinkInfo object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            LinkInfoProvider.DeleteLinkInfo(this);
        }


        /// <summary>
        /// Updates this LinkInfo the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            LinkInfoProvider.SetLinkInfo(this);
        }

        #endregion
    }
}