using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using CMS;
using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.Forums;
using CMS.Helpers;
using CMS.Membership;
using CMS.Search;

[assembly: RegisterObjectType(typeof(ForumPostInfo), ForumPostInfo.OBJECT_TYPE)]

namespace CMS.Forums
{
    /// <summary>
    /// ForumPostInfo data container class.
    /// </summary>
    public class ForumPostInfo : AbstractInfo<ForumPostInfo>, ISearchable
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = PredefinedObjectType.FORUMPOST;


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(ForumPostInfoProvider), OBJECT_TYPE, "Forums.ForumPost", "PostID", "PostLastModified", "PostGUID", null, "PostSubject", null, "PostSiteID", "PostForumID", ForumInfo.OBJECT_TYPE)
        {
            DependsOn = new List<ObjectDependency>
            {
                new ObjectDependency("PostParentID", OBJECT_TYPE, ObjectDependencyEnum.NotRequired),
                new ObjectDependency("PostApprovedByUserID", UserInfo.OBJECT_TYPE, ObjectDependencyEnum.NotRequired),
                new ObjectDependency("PostUserID", UserInfo.OBJECT_TYPE, ObjectDependencyEnum.NotRequired)
            },
            SynchronizationSettings =
            {
                IncludeToSynchronizationParentDataSet = IncludeToParentEnum.None,
                LogSynchronization = SynchronizationTypeEnum.None,
            },
            ContinuousIntegrationSettings =
            {
                Enabled = true
            },
            SerializationSettings =
            {
                ExcludedFieldNames =
                {
                    "PostIDPath",
                    "PostLevel",
                    "PostViews",
                    "PostAttachmentCount",
                    "PostThreadPosts",
                    "PostThreadPostsAbsolute",
                    "PostThreadLastPostTime",
                    "PostThreadLastPostUserName",
                    "PostThreadLastPostTimeAbsolute",
                    "PostThreadLastPostUserNameAbsolute"
                }
            },
            LogEvents = true,
            TouchCacheDependencies = true,
            SupportsVersioning = false,
            AllowRestore = false,
            ModuleName = ModuleName.FORUMS,
            SupportsCloning = false,
            ObjectIDPathColumn = "PostIDPath",
            ObjectLevelColumn = "PostLevel",
            OrderColumn = "PostStickOrder",
            RegisterAsChildToObjectTypes = new List<string>
            {
                ForumInfo.OBJECT_TYPE, ForumInfo.OBJECT_TYPE_GROUP
            },
            ImportExportSettings =
            {
                IncludeToExportParentDataSet = IncludeToParentEnum.None,
                LogExport = false,
                LogProgress = false
            },
            ContainsMacros = false
        };

        #endregion


        #region "Variables"

        private UserDataInfo mPostInfo = null;

        #endregion


        #region "Properties"

        /// <summary>
        /// Post IP, and agent values.
        /// </summary>
        public virtual UserDataInfo PostInfo
        {
            get
            {
                if (mPostInfo == null)
                {
                    // Load the xml data
                    mPostInfo = new UserDataInfo();
                    mPostInfo.LoadData(ValidationHelper.GetString(GetValue("PostInfo"), ""));
                }
                return mPostInfo;
            }
        }


        /// <summary>
        /// Gets or sets the SiteID of the post
        /// </summary>
        public virtual int SiteId
        {
            get
            {
                return GetIntegerValue("PostSiteID", -1);
            }
            set
            {
                SetValue("PostSiteID", value);
            }
        }


        /// <summary>
        /// Gets or sets the type of the post (0=Normal, 1=Answer).
        /// </summary>
        public virtual int PostType
        {
            get
            {
                return GetIntegerValue("PostType", 0);
            }
            set
            {
                SetValue("PostType", value);
            }
        }


        /// <summary>
        /// Gets or sets the date of last edit.
        /// </summary>
        public virtual DateTime PostLastEdit
        {
            get
            {
                return GetDateTimeValue("PostLastEdit", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("PostLastEdit", value);
            }
        }


        /// <summary>
        /// Gets or sets the value that indicates how many times thread was displayed.
        /// </summary>
        public virtual int PostViews
        {
            get
            {
                return GetIntegerValue("PostViews", 0);
            }
            set
            {
                SetValue("PostViews", value);
            }
        }


        /// <summary>
        /// Gets or sets the value that indicates whether the current post is sticked and in which order.
        /// </summary>
        public virtual int PostStickOrder
        {
            get
            {
                return GetIntegerValue("PostStickOrder", 0);
            }
            set
            {
                SetValue("PostStickOrder", value);
            }
        }


        /// <summary>
        /// Gets or sets the value that indicates how many users set this post answer.
        /// </summary>
        public virtual int PostIsAnswer
        {
            get
            {
                return GetIntegerValue("PostIsAnswer", 0);
            }
            set
            {
                SetValue("PostIsAnswer", value);
            }
        }


        /// <summary>
        /// Gets or sets the value that indicates how many users set this post as not answer.
        /// </summary>
        public virtual int PostIsNotAnswer
        {
            get
            {
                return GetIntegerValue("PostIsNotAnswer", 0);
            }
            set
            {
                SetValue("PostIsNotAnswer", value);
            }
        }


        /// <summary>
        /// Indicates whether current thread (if it's question) is solved - i.e. there is at least one
        /// post marked as solution in the thread.
        /// </summary>
        public virtual bool PostQuestionSolved
        {
            get
            {
                return GetBooleanValue("PostQuestionSolved", false);
            }
            set
            {
                SetValue("PostQuestionSolved", value);
            }
        }


        /// <summary>
        /// Indicates whether current post is locked.
        /// </summary>
        public virtual bool PostIsLocked
        {
            get
            {
                return GetBooleanValue("PostIsLocked", false);
            }
            set
            {
                SetValue("PostIsLocked", value);
            }
        }


        /// <summary>
        /// Post user signature.
        /// </summary>
        public virtual string PostUserSignature
        {
            get
            {
                return GetStringValue("PostUserSignature", "");
            }
            set
            {
                SetValue("PostUserSignature", value);
            }
        }


        /// <summary>
        /// Number of thread posts.
        /// </summary>
        public virtual int PostThreadPosts
        {
            get
            {
                return GetIntegerValue("PostThreadPosts", 0);
            }
            set
            {
                SetValue("PostThreadPosts", value);
            }
        }


        /// <summary>
        /// Last post user name.
        /// </summary>
        public virtual string PostThreadLastPostUserName
        {
            get
            {
                return GetStringValue("PostThreadLastPostUserName", "");
            }
            set
            {
                SetValue("PostThreadLastPostUserName", value);
            }
        }


        /// <summary>
        /// Time of last post of thread.
        /// </summary>
        public virtual DateTime PostThreadLastPostTime
        {
            get
            {
                return GetDateTimeValue("PostThreadLastPostTime", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("PostThreadLastPostTime", value);
            }
        }


        /// <summary>
        /// Post ID.
        /// </summary>
        public virtual int PostId
        {
            get
            {
                return GetIntegerValue("PostID", 0);
            }
            set
            {
                SetValue("PostID", value);
            }
        }


        /// <summary>
        /// Post time.
        /// </summary>
        public virtual DateTime PostTime
        {
            get
            {
                return GetDateTimeValue("PostTime", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("PostTime", value);
            }
        }


        /// <summary>
        /// Post level.
        /// </summary>
        public virtual int PostLevel
        {
            get
            {
                return GetIntegerValue("PostLevel", 0);
            }
            set
            {
                SetValue("PostLevel", value);
            }
        }


        /// <summary>
        /// User mail.
        /// </summary>
        public virtual string PostUserMail
        {
            get
            {
                return GetStringValue("PostUserMail", "");
            }
            set
            {
                SetValue("PostUserMail", value);
            }
        }


        /// <summary>
        /// Post forum ID.
        /// </summary>
        public virtual int PostForumID
        {
            get
            {
                return GetIntegerValue("PostForumID", 0);
            }
            set
            {
                SetValue("PostForumID", value);
            }
        }


        /// <summary>
        /// Post text.
        /// </summary>
        public virtual string PostText
        {
            get
            {
                return GetStringValue("PostText", "");
            }
            set
            {
                SetValue("PostText", value);
            }
        }


        /// <summary>
        /// Approved by user ID.
        /// </summary>
        public virtual int PostApprovedByUserID
        {
            get
            {
                return GetIntegerValue("PostApprovedByUserID", 0);
            }
            set
            {
                if (value <= 0)
                {
                    SetValue("PostApprovedByUserID", null);
                }
                else
                {
                    SetValue("PostApprovedByUserID", value);
                }
            }
        }


        /// <summary>
        /// Parent post ID.
        /// </summary>
        public virtual int PostParentID
        {
            get
            {
                return GetIntegerValue("PostParentID", 0);
            }
            set
            {
                if (value <= 0)
                {
                    SetValue("PostParentID", null);
                }
                else
                {
                    SetValue("PostParentID", value);
                }
            }
        }


        /// <summary>
        /// User ID of user which added post.
        /// </summary>
        public virtual int PostUserID
        {
            get
            {
                return GetIntegerValue("PostUserID", 0);
            }
            set
            {
                if (value <= 0)
                {
                    SetValue("PostUserID", null);
                }
                else
                {
                    SetValue("PostUserID", value);
                }
            }
        }


        /// <summary>
        /// Post ID path - unique path (for tree purposes).
        /// </summary>
        public virtual string PostIDPath
        {
            get
            {
                return GetStringValue("PostIDPath", "");
            }
            set
            {
                SetValue("PostIDPath", value);
            }
        }


        /// <summary>
        /// Indicates whether current post is approved, approver is specified by PostApprovedByuserID property.
        /// </summary>
        public virtual bool PostApproved
        {
            get
            {
                return GetBooleanValue("PostApproved", false);
            }
            set
            {
                SetValue("PostApproved", value);
            }
        }


        /// <summary>
        /// Post subject.
        /// </summary>
        public virtual string PostSubject
        {
            get
            {
                return GetStringValue("PostSubject", "");
            }
            set
            {
                SetValue("PostSubject", value);
            }
        }


        /// <summary>
        /// Post user name.
        /// </summary>
        public virtual string PostUserName
        {
            get
            {
                return GetStringValue("PostUserName", "");
            }
            set
            {
                SetValue("PostUserName", value);
            }
        }


        /// <summary>
        /// Post GUID.
        /// </summary>
        public virtual Guid PostGUID
        {
            get
            {
                return GetGuidValue("PostGUID", Guid.Empty);
            }
            set
            {
                SetValue("PostGUID", value, Guid.Empty);
            }
        }


        /// <summary>
        /// Object last modified.
        /// </summary>
        public virtual DateTime PostLastModified
        {
            get
            {
                return GetDateTimeValue("PostLastModified", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("PostLastModified", value, DateTimeHelper.ZERO_TIME);
            }
        }


        /// <summary>
        /// Post attachment count.
        /// </summary>
        public virtual int PostAttachmentCount
        {
            get
            {
                return GetIntegerValue("PostAttachmentCount", 0);
            }
            set
            {
                SetValue("PostAttachmentCount", value);
            }
        }


        /// <summary>
        /// Number of all posts.
        /// </summary>
        public virtual int PostThreadPostsAbsolute
        {
            get
            {
                return GetIntegerValue("PostThreadPostsAbsolute", 0);
            }
            set
            {
                SetValue("PostThreadPostsAbsolute", value);
            }
        }


        /// <summary>
        /// User name of last post.
        /// </summary>
        public virtual string PostThreadLastPostUserNameAbsolute
        {
            get
            {
                return GetStringValue("PostThreadLastPostUserNameAbsolute", null);
            }
            set
            {
                SetValue("PostThreadLastPostUserNameAbsolute", value);
            }
        }


        /// <summary>
        /// Forum thread last modified time.
        /// </summary>
        public virtual DateTime PostThreadLastPostTimeAbsolute
        {
            get
            {
                return GetDateTimeValue("PostThreadLastPostTimeAbsolute", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("PostThreadLastPostTimeAbsolute", value, DateTimeHelper.ZERO_TIME);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            ForumPostInfoProvider.DeleteForumPostInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            ForumPostInfoProvider.SetForumPostInfo(this);
        }


        /// <summary>
        /// Gets the field value.
        /// </summary>
        /// <param name="columnName">Column name</param>
        public override object GetValue(string columnName)
        {
            switch (columnName.ToLowerCSafe())
            {
                // Returns forum name for current forum
                case "postforumname":
                    ForumInfo fi = ForumInfoProvider.GetForumInfo(PostForumID);
                    if (fi != null)
                    {
                        return fi.ForumName;
                    }
                    return String.Empty;

                // Returns true if current forum is not adHoc and is not community
                case "issearchable":
                    // Get relevant forum info
                    fi = ForumInfoProvider.GetForumInfo(PostForumID);
                    if (fi != null)
                    {
                        // If forum is adHoc => is not searchable
                        if (fi.ForumDocumentID > 0)
                        {
                            return false;
                        }

                        // Get relevant forum group
                        ForumGroupInfo fgi = ForumGroupInfoProvider.GetForumGroupInfo(fi.ForumGroupID);
                        if (fgi != null)
                        {
                            // If forum is community => is not searchable
                            return (fgi.GroupGroupID == 0);
                        }
                    }
                    return false;

                // Returns site id relevant to the current post
                case "postsiteid":
                    // Get relevant forum info
                    fi = ForumInfoProvider.GetForumInfo(PostForumID);
                    if (fi != null)
                    {
                        return fi.ForumSiteID;
                    }
                    return 0;
            }

            return base.GetValue(columnName);
        }


        /// <summary>
        /// Creates where condition according to Parent, Group and Site settings.
        /// </summary>
        protected override WhereCondition GetSiblingsWhereCondition()
        {
            return base.GetSiblingsWhereCondition().WhereGreaterThan("PostStickOrder", 0);
        }


        /// <summary>
        /// Builds the path from the given column.
        /// </summary>
        /// <param name="parentColumName">Column of the parent ID</param>
        /// <param name="pathColumnName">Column name to build the path from</param>
        /// <param name="levelColumnName">Column name of the level</param>
        /// <param name="level">Level of the object within the tree hierarchy</param>
        /// <param name="pathPartColumn">Name of the column which creates the path (IDColumn for IDPath, CodeNameColumn for name path)</param>
        protected override string BuildObjectPath(string parentColumName, string pathColumnName, string levelColumnName, string pathPartColumn, out int level)
        {
            BaseInfo parent = Generalized.GetObject(GetIntegerValue(parentColumName, 0));
            if (parent != null)
            {
                level = parent.GetIntegerValue(levelColumnName, 0) + 1;
                return parent.GetStringValue(pathColumnName, "").TrimEnd('/') + "/" + GetCurrentObjectPathPart(pathPartColumn);
            }
            else
            {
                level = 0;
                return "/" + GetCurrentObjectPathPart(pathPartColumn);
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Approves this post by current user and immediately saves it to DB.
        /// </summary>
        public void Approve()
        {
            if (MembershipContext.AuthenticatedUser != null)
            {
                Approve(MembershipContext.AuthenticatedUser.UserID, false);
            }
        }


        /// <summary>
        /// Approves this post by specified user id and optionally all its sub-posts.
        /// </summary>
        /// <param name="userId">The user id</param>
        /// <param name="withSubtree">If set to <c>true</c> all sub-posts of this post are approved too</param>
        public void Approve(int userId, bool withSubtree)
        {
            PostApproved = true;
            PostApprovedByUserID = userId;
            ForumPostInfoProvider.SetForumPostInfo(this);

            if (withSubtree)
            {
                // Get all sub-posts
                DataSet ds = ForumPostInfoProvider.GetChildPosts(PostId);

                if (!DataHelper.DataSourceIsEmpty(ds))
                {
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        var fpi = new ForumPostInfo(dr);
                        if (!fpi.PostApproved)
                        {
                            fpi.PostApproved = true;
                            fpi.PostApprovedByUserID = userId;

                            ForumPostInfoProvider.SetForumPostInfo(fpi);
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Rejects this post and immediately saves it to DB.
        /// </summary>
        public void Reject()
        {
            Reject(false);
        }


        /// <summary>
        /// Rejects this post and optionally all its sub-posts.
        /// </summary>        
        /// <param name="withSubtree">If set to <c>true</c> all sub-posts of this post are rejected too</param>
        public void Reject(bool withSubtree)
        {
            PostApproved = false;
            PostApprovedByUserID = 0;
            ForumPostInfoProvider.SetForumPostInfo(this);

            if (withSubtree)
            {
                // Get all sub-posts
                DataSet ds = ForumPostInfoProvider.GetChildPosts(PostId);

                if (!DataHelper.DataSourceIsEmpty(ds))
                {
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        ForumPostInfo fpi = new ForumPostInfo(dr);
                        if (fpi.PostApproved)
                        {
                            fpi.PostApproved = false;
                            fpi.PostApprovedByUserID = 0;
                            ForumPostInfoProvider.SetForumPostInfo(fpi);
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Inserts cloned object to DB.
        /// </summary>
        /// <param name="settings">Cloning settings</param>
        /// <param name="result">Cloning result</param>
        /// <param name="originalObject">Original source BaseInfo (object being cloned)</param>
        protected override void InsertAsCloneInternal(CloneSettings settings, CloneResult result, BaseInfo originalObject)
        {
            // Keep PostSubject same as for original object (have to overwrite this, because general procedure made the subject (= display name) unique
            PostSubject = originalObject.GetStringValue("PostSubject", "");

            Insert();

            // Copy all posts under this post
            DataSet ds = ForumPostInfoProvider.GetForumPosts().WhereEquals("PostParentID", originalObject.GetIntegerValue("PostID", 0));
            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    ForumPostInfo post = new ForumPostInfo(dr);
                    post.PostParentID = ObjectID;

                    post.InsertAsClone(settings, result);
                }
            }
        }


        /// <summary>
        /// Removes object dependencies.
        /// </summary>
        protected override void RemoveObjectDependencies(bool deleteAll = false, bool clearHashtables = true)
        {
            // Delete attachments related to the current post
            ForumAttachmentInfoProvider.DeleteFiles("AttachmentPostID = " + PostId, ForumInfoProvider.GetForumSiteName(PostForumID));

            base.RemoveObjectDependencies(deleteAll, clearHashtables);
        }


        /// <summary>
        /// Sets the post's siteID to the same SiteID as to post's forum.
        /// </summary>
        internal virtual void SetSiteIdFromForum()
        {
            ForumInfo forum = ForumInfoProvider.GetForumInfo(PostForumID);
            int? siteID = null;
            if (forum != null)
            {
                siteID = forum.ForumSiteID;
            }
            SetValue("PostSiteID", siteID);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty ForumPostInfo object.
        /// </summary>
        public ForumPostInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new ForumPostInfo object from the given DataRow.
        /// </summary>
        public ForumPostInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion


        #region "ISearchable Members"

        /// <summary>
        /// Gets the type of current object.
        /// </summary>
        public override string SearchType
        {
            get
            {
                return ForumInfo.OBJECT_TYPE;
            }
        }


        /// <summary>
        /// Returns search fields collection. When existing collection is passed as argument, fields will be added to that collection.
        /// When collection is not passed, new collection will be created and return. 
        /// Collection will contain field values only when collection with StoreValues property set to true is passed to the method.
        /// When method creates new collection, it is created with StoreValues property set to false.
        /// </summary>
        /// <param name="index">Search index</param>
        /// <param name="searchFields">Search fields collection</param>
        public override ISearchFields GetSearchFields(ISearchIndexInfo index, ISearchFields searchFields = null)
        {
            // If search fields colletion is not given, create new collection that doesn't store values.
            searchFields = searchFields ?? Service.Resolve<ISearchFields>();

            // Get data class
            var dataClass = DataClassInfoProvider.GetDataClassInfo(TypeInfo.ObjectClassName);
            SearchSettings fieldsSettings;

            // When DataClass or fields search settings are null return empty list
            if (dataClass == null || ((fieldsSettings = dataClass.ClassSearchSettingsInfos) == null))
            {
                return searchFields;
            }

            ForumInfo forum = null;
            var searchFieldFactory = SearchFieldFactory.Instance;

            // Add ID column name
            searchFields.Add(searchFieldFactory.Create(SearchFieldsConstants.IDCOLUMNNAME, typeof(string), CreateSearchFieldOption.SearchableAndRetrievable), () => TypeInfo.IDColumn);

            // Add empty content field to ensure that content field will be created
            searchFields.EnsureContentField();
            
            // Add classname field
            searchFields.Add(searchFieldFactory.Create("_classname", typeof(string), CreateSearchFieldOption.SearchableAndRetrievable), () => SearchHelper.INVARIANT_FIELD_VALUE);
            
            // Add forum id
            searchFields.Add(searchFieldFactory.Create("_postforumid", typeof(int), CreateSearchFieldOption.SearchableAndRetrievable), () =>
            {
                forum = forum ?? ForumInfoProvider.GetForumInfo(PostForumID);
                return forum != null ? (object)forum.ForumID : null;
            });

            // Add forum group id
            searchFields.Add(searchFieldFactory.Create("_postforumgroupid", typeof(int), CreateSearchFieldOption.SearchableAndRetrievable), () =>
            {
                return forum != null ? (object)forum.ForumGroupID : null;
            });

            // Loop through all general columns
            foreach (var setting in fieldsSettings)
            {
                if (SearchFieldsHelper.Instance.IsContentField(index, setting))
                {
                    // Set special field values for post text
                    if ((setting.Name.ToLowerInvariant() == "posttext"))
                    {
                        searchFields.AddToContentField(() =>
                        {
                            // Strip BB Tags
                            DiscussionMacroResolver resolver = new DiscussionMacroResolver();
                            resolver.ResolveToPlainText = true;
                            return searchFields.PrepareContentValue(resolver.ResolveMacros(PostText), forum == null || forum.ForumHTMLEditor);
                        });
                    }
                    // Standard case
                    else
                    {
                        searchFields.AddContentField(this, index, setting);
                    }
                }

                searchFields.AddIndexField(this, index, setting, dataClass.GetSearchColumnType(setting.Name));
            }

            return searchFields;
        }


        /// <summary>
        /// Returns index document for current object.
        /// </summary>
        /// <param name="index">Search index info object</param>
        public override SearchDocument GetSearchDocument(ISearchIndexInfo index)
        {
            // Get data class
            DataClassInfo dataClass = DataClassInfoProvider.GetDataClassInfo(TypeInfo.ObjectClassName);

            // Return null when data class cannot be retrieved
            if (dataClass == null)
            {
                return null;
            }

            // Create search document
            var documentParameters = new SearchDocumentParameters
            {
                Index = index,
                Type = SearchType,
                Id = PostId.ToString(),
                Created = PostTime,
                SiteName = ForumInfoProvider.GetForumSiteName(PostForumID),
            };

            var doc = SearchHelper.CreateDocument(documentParameters);

            // Create search fields collection 
            var searchFields = Service.Resolve<ISearchFields>();
            searchFields.StoreValues = true;

            foreach (var field in GetSearchFields(index, searchFields).Items)
            {
                // When field is content, raise event
                if (field.FieldName == SearchFieldsConstants.CONTENT)
                {
                    // Get content
                    string content = field.Value.ToString();

                    // Raise the custom event
                    TypeInfo.Events.GetContent.StartEvent(this, ref content);
                    field.Value = content;
                }

                // Add field to document
                doc.AddSearchField(field);
            }

            // Return document
            return doc;
        }

        #endregion
    }
}