using System;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Data;

using CMS.CustomTables;
using CMS.EventLog;
using CMS.Base.Web.UI;
using CMS.Helpers;
using CMS.Base;
using CMS.SiteProvider;
using CMS.DataEngine;
using CMS.PortalEngine.Web.UI;
using CMS.PortalEngine;

namespace CMS.DocumentEngine.Web.UI
{
    /// <summary>
    /// Base transformation class.
    /// </summary>
    public class CMSAbstractTransformation : AbstractUserControl, IDataItemContainer, IRelatedData
    {
        #region "Variables"

        /// <summary>
        /// Editable items (document content).
        /// </summary>
        protected EditableItems mEditableItems;

        /// <summary>
        /// Node custom data.
        /// </summary>
        protected CustomData mNodeCustomData;

        /// <summary>
        /// Document custom data.
        /// </summary>
        protected CustomData mDocumentCustomData;


        /// <summary>
        /// Parent data control.
        /// </summary>
        protected IDataControl mDataControl;

        /// <summary>
        /// Returns true if the parent data control was already searched
        /// </summary>
        protected bool mDataControlSearched;


        /// <summary>
        /// Related data is loaded.
        /// </summary>
        protected bool mRelatedDataLoaded;

        /// <summary>
        /// Custom data connected to the object.
        /// </summary>
        protected object mRelatedData;


        /// <summary>
        /// Object containing the data
        /// </summary>
        protected BaseInfo mObject;

        /// <summary>
        /// Flag whether the object was loaded or not
        /// </summary>
        protected bool mObjectLoaded;


        /// <summary>
        /// Regular expression for CDATA.
        /// </summary>
        protected static Regex mCDATARegExp;

        /// <summary>
        /// If true, the evals are tracked within this transformation
        /// </summary>
        protected bool mTrackEvals;

        #endregion


        #region "Properties"

        /// <summary>
        /// Methods provided by the transformation
        /// </summary>
        public static TransformationHelper Methods
        {
            get
            {
                return TransformationHelper.HelperObject;
            }
        }


        /// <summary>
        /// Custom data connected to the object, if not set, returns the Related data of the nearest IDataControl.
        /// </summary>
        public virtual object RelatedData
        {
            get
            {
                if ((mRelatedData == null) && !mRelatedDataLoaded)
                {
                    // Load the related data to the object
                    mRelatedDataLoaded = true;
                    IRelatedData dataControl = (IRelatedData)ControlsHelper.GetParentControl(this, typeof(IRelatedData));
                    if (dataControl != null)
                    {
                        mRelatedData = dataControl.RelatedData;
                    }
                }

                return mRelatedData;
            }
            set
            {
                mRelatedData = value;
            }
        }


        /// <summary>
        /// Editable items (document content).
        /// </summary>
        public EditableItems EditableItems
        {
            get
            {
                if (mEditableItems == null)
                {
                    mEditableItems = new EditableItems();
                    mEditableItems.LoadContentXml(ValidationHelper.GetString(Eval("DocumentContent"), ""));
                }
                return mEditableItems;
            }
        }


        /// <summary>
        /// Node custom data.
        /// </summary>
        public virtual CustomData NodeCustomData
        {
            get
            {
                if (mNodeCustomData == null)
                {
                    // Load the custom data
                    mNodeCustomData = new CustomData();
                    mNodeCustomData.LoadData(ValidationHelper.GetString(Eval("NodeCustomData"), ""));
                }
                return mNodeCustomData;
            }
        }


        /// <summary>
        /// Document custom data.
        /// </summary>
        public virtual CustomData DocumentCustomData
        {
            get
            {
                if (mDocumentCustomData == null)
                {
                    // Load the custom data
                    mDocumentCustomData = new CustomData();
                    mDocumentCustomData.LoadData(ValidationHelper.GetString(Eval("DocumentCustomData"), ""));
                }
                return mDocumentCustomData;
            }
        }


        /// <summary>
        /// Gets the data item object for current binding context.
        /// </summary>
        public object DataItem
        {
            get
            {
                // Get the data item from current transformation container
                IDataItemContainer container = Parent as IDataItemContainer;
                if (container != null)
                {
                    return container.DataItem;
                }
                return null;
            }
        }


        /// <summary>
        /// Gets the number of data items for the current binding context.
        /// </summary>
        public int DataItemCount
        {
            get
            {
                DataRowView drv = DataRowView;
                if (drv != null)
                {
                    return drv.DataView.Count;
                }
                return 0;
            }
        }


        /// <summary>
        /// Gets the data item index for current transformation container
        /// You have to use Container.DataItemIndex for index in current binding context
        /// </summary>
        public int DataItemIndex
        {
            get
            {
                IDataItemContainer container = Parent as IDataItemContainer;
                if (container != null)
                {
                    return container.DataItemIndex;
                }
                return 0;
            }
        }


        /// <summary>
        /// Gets the display index for current transformation container
        /// You have to use Container.DisplayIndex for display index in current binding context
        /// </summary>
        public int DisplayIndex
        {
            get
            {
                IDataItemContainer container = Parent as IDataItemContainer;
                if (container != null)
                {
                    return container.DisplayIndex;
                }
                return 0;
            }
        }


        /// <summary>
        /// Object with the data
        /// </summary>
        public BaseInfo Object
        {
            get
            {
                return EnsureObject();
            }
        }


        /// <summary>
        /// Returns current data row view.
        /// </summary>
        public DataRowView DataRowView
        {
            get
            {
                object item = DataItem;
                if (item is DataRowView)
                {
                    return (DataRowView)item;
                }

                return null;
            }
        }


        /// <summary>
        /// Parent data control.
        /// </summary>
        public IDataControl DataControl
        {
            get
            {
                if ((mDataControl == null) && !mDataControlSearched)
                {
                    // Search for the parent Data control
                    mDataControl = ControlsHelper.GetDataControl(this);
                    mDataControlSearched = true;
                }

                return mDataControl;
            }
            set
            {
                mDataControl = value;
            }
        }


        /// <summary>
        /// URL regular expression.
        /// </summary>
        public static Regex CDATARegExp
        {
            get
            {
                return mCDATARegExp ?? (mCDATARegExp = RegexHelper.GetRegex("]]>"));
            }
            set
            {
                mCDATARegExp = value;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Resolves the discussion macros.
        /// </summary>
        /// <param name="inputText">Text to resolve</param>
        public string ResolveDiscussionMacros(string inputText)
        {
            return TransformationHelper.HelperObject.ResolveDiscussionMacros(inputText);
        }


        /// <summary>
        /// Resolves the given alias path, applies the path segment to the given format string {0} for level 0.
        /// </summary>
        /// <param name="format">Alias path pattern</param>
        public string ResolveCurrentPath(string format)
        {
            return TransformationHelper.HelperObject.ResolveCurrentPath(format);
        }


        /// <summary>
        /// Resolves the macros in the given text
        /// </summary>
        /// <param name="inputText">Input text to resolve</param>
        public string ResolveMacros(string inputText)
        {
            return TransformationHelper.HelperObject.ResolveMacros(inputText);
        }


        /// <summary>
        /// Returns sitemap XML element for specified type (loc, lastmod, changefreq, priority). 
        /// </summary>
        /// <param name="type">Specify xml sitemap type (loc, lastmod, changefreq, priority)</param>
        public string GetSitemapItem(string type)
        {
            return TransformationHelper.HelperObject.GetSitemapItem(DataItem, type);
        }


        /// <summary>
        /// Limits length of the given plain text.
        /// </summary>
        /// <param name="textObj">Plain text to limit (Text containing HTML tags is not supported)</param>
        /// <param name="maxLength">Maximum text length</param>
        public string LimitLength(object textObj, int maxLength)
        {
            return TransformationHelper.HelperObject.LimitLength(textObj, maxLength, TextHelper.DEFAULT_ELLIPSIS);
        }


        /// <summary>
        /// Limits length of the given plain text.
        /// </summary>
        /// <param name="textObj">Plain text to limit (Text containing HTML tags is not supported)</param>
        /// <param name="maxLength">Maximum text length</param>
        /// <param name="padString">Trimming postfix</param>
        public string LimitLength(object textObj, int maxLength, string padString)
        {
            return TransformationHelper.HelperObject.LimitLength(textObj, maxLength, padString);
        }


        /// <summary>
        /// Trims the site prefix from user name(if any prefix found)
        /// </summary>
        /// <param name="userName">User name</param>
        public string TrimSitePrefix(object userName)
        {
            return TransformationHelper.HelperObject.TrimSitePrefix(userName);
        }


        /// <summary>
        /// Limits length of the given plain text.
        /// </summary>
        /// <param name="textObj">Plain text to limit (Text containing HTML tags is not supported)</param>
        /// <param name="maxLength">Maximum text length</param>
        /// <param name="padString">Trimming postfix</param>
        /// <param name="wholeWords">If true, the text won't be cut in the middle of the word</param>
        public string LimitLength(object textObj, int maxLength, string padString, bool wholeWords)
        {
            return TransformationHelper.HelperObject.LimitLength(textObj, maxLength, padString, wholeWords);
        }


        /// <summary>
        /// Limits line length of the given plain text.
        /// </summary>
        /// <param name="textObj">Plain text to limit (Text containing HTML tags is not supported)</param>
        /// <param name="maxLength">Maximum line length</param>
        public string EnsureMaximumLineLength(object textObj, int maxLength)
        {
            return TransformationHelper.HelperObject.EnsureMaximumLineLength(textObj, maxLength);
        }


        /// <summary>
        /// Returns URL of the attachment stored in the given column name.
        /// </summary>
        /// <param name="attachmentGuidColumn">Name of the column that contains the attachment.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="variant">Identifier of the attachment variant definition.</param>
        /// <remarks>If <paramref name="fileName"/> is not provided NodeAlias of a document is used.</remarks>
        [Obsolete("Use GetAttachmentUrlByGUID method instead where you need to provide value of the attachment GUID, not the column name containing the attachment GUID.")]
        public string GetFileUrl(object attachmentGuidColumn, string fileName = null, string variant = null)
        {
            var columnName = ValidationHelper.GetString(attachmentGuidColumn, "");
            var attachmentFileName = string.IsNullOrEmpty(fileName) ? Eval<string>("NodeAlias") : fileName;

            return GetAttachmentUrlByGUID(Eval(columnName), attachmentFileName , variant);
        }


        /// <summary>
        /// Returns URL of the attachment based on given GUID.
        /// </summary>
        /// <param name="attachmentGuid">Attachment GUID.</param>
        /// <param name="attachmentFileName">Name of the attachment file.</param>
        /// <param name="variant">Identifier of the attachment variant definition.</param>
        /// <remarks> 
        /// The <paramref name="attachmentFileName"/> is not mandatory and its value does not need to correspond with the actual 
        /// file name of the attachment. For the File page type the value of page NodeAlias can be used instead as a file name.
        /// </remarks>
        public string GetAttachmentUrlByGUID(object attachmentGuid, object attachmentFileName, string variant = null)
        {
            var attGuid = ValidationHelper.GetGuid(attachmentGuid, Guid.Empty);
            string fileName = ValidationHelper.GetString(attachmentFileName, "");

            return TransformationHelper.HelperObject.GetAttachmentUrlByGUID(attGuid, fileName, variant);
        }


        /// <summary>
        /// Returns URL of the attachment.
        /// </summary>
        /// <param name="attachmentFileName">Name of the attachment file.</param>
        /// <param name="attachmentDocumentId">Attachment document ID.</param>
        /// <param name="variant">Identifier of the attachment variant definition.</param>
        [Obsolete("Use GetAttachmentUrlByDocumentId method instead.")]
        public string GetFileUrl(object attachmentFileName, object attachmentDocumentId, string variant = null)
        {
            return GetAttachmentUrlByDocumentId(attachmentFileName, attachmentDocumentId, variant);
        }


        /// <summary>
        /// Returns friendly URL of the attachment.
        /// </summary>
        /// <param name="attachmentFileName">Name of the attachment file.</param>
        /// <param name="attachmentDocumentId">Attachment document ID.</param>
        /// <param name="variant">Identifier of the attachment variant definition.</param>
        public string GetAttachmentUrlByDocumentId(object attachmentFileName, object attachmentDocumentId, string variant = null)
        {
            var fileName = ValidationHelper.GetString(attachmentFileName, "");
            var documentId = ValidationHelper.GetInteger(attachmentDocumentId, 0);

            return TransformationHelper.HelperObject.GetAttachmentUrlByDocumentId(fileName, documentId, variant);
        }


        /// <summary>
        /// Returns friendly URL of the attachment.
        /// </summary>
        /// <param name="attachmentFileName">Name of the attachment file.</param>
        /// <param name="nodeAliasPath">Attachment document alias path.</param>
        /// <param name="variant">Identifier of the attachment variant definition.</param>
        public string GetAttachmentUrl(object attachmentFileName, object nodeAliasPath, string variant = null)
        {
            var aliasPath = ValidationHelper.GetString(nodeAliasPath, "");
            var fileName = ValidationHelper.GetString(attachmentFileName, "");

            return TransformationHelper.HelperObject.GetAttachmentUrl(fileName, aliasPath, variant);
        }


        /// <summary>
        /// Returns URL of the currently rendered document.
        /// </summary>
        public string GetDocumentUrl()
        {
            var siteName = GetSiteName();
            return TransformationHelper.HelperObject.GetDocumentUrl(siteName, Eval("NodeAliasPath"), Eval("DocumentUrlPath"), (URLHelper.UseLangPrefixForUrls(siteName) ? ValidationHelper.GetString(Eval("DocumentCulture"), null) : null));
        }


        /// <summary>
        /// Returns URL of the given document (for use with document selector).
        /// </summary>
        /// <param name="nodeGuidColumn">Node GUID column name</param>
        /// <param name="nodeAlias">Node alias</param>
        [Obsolete("Use GetDocumentUrlByGUID method instead where you need to provide value of the node GUID, not the column name containing the node GUID.")]
        public string GetDocumentUrl(object nodeGuidColumn, object nodeAlias)
        {
            var guid = Eval(ValidationHelper.GetString(nodeGuidColumn, ""));

            return GetDocumentUrlByGUID(guid, nodeAlias);
        }


        /// <summary>
        /// Returns URL of the given document.
        /// </summary>
        /// <param name="nodeGuid">Node GUID</param>
        /// <param name="nodeAlias">Node alias</param>
        public string GetDocumentUrlByGUID(object nodeGuid, object nodeAlias)
        {
            return TransformationHelper.HelperObject.GetDocumentUrl(nodeGuid, nodeAlias);
        }


        /// <summary>
        /// Returns URL of the given document.
        /// </summary>
        /// <param name="documentIdObj">Document ID</param>
        [Obsolete("Use GetDocumentUrlById method instead.")]
        public string GetDocumentUrl(object documentIdObj)
        {
            return TransformationHelper.HelperObject.GetDocumentUrl(documentIdObj);
        }


        /// <summary>
        /// Returns URL of the given document.
        /// </summary>
        /// <param name="documentId">Document ID</param>
        public string GetDocumentUrlById(object documentId)
        {
            return TransformationHelper.HelperObject.GetDocumentUrl(documentId);
        }


        /// <summary>
        /// Gets document CSS class comparing the current document node alias path.
        /// </summary>
        /// <param name="cssClass">CSS class</param>
        /// <param name="selectedCssClass">Selected CSS class</param>
        /// <returns>Returns selectedCssClass if given node alias path matches the current document node alias path. Otherwise returns cssClass.</returns>
        public string GetDocumentCssClass(string cssClass, string selectedCssClass)
        {
            return TransformationHelper.HelperObject.GetDocumentCssClass(Eval<string>("NodeAliasPath"), cssClass, selectedCssClass);
        }


        /// <summary>
        /// Indicates if the document is current document.
        /// </summary>
        public bool IsCurrentDocument()
        {
            return TransformationHelper.HelperObject.IsCurrentDocument(Eval<string>("NodeAliasPath"));
        }


        /// <summary>
        /// Indicates if the document is on selected path.
        /// </summary>
        public bool IsDocumentOnSelectedPath()
        {
            return TransformationHelper.HelperObject.IsDocumentOnSelectedPath(Eval<string>("NodeAliasPath"));
        }


        /// <summary>
        /// Returns URL of the specified forum post.
        /// </summary>
        /// <param name="postIdPath">Post id path</param>
        /// <param name="forumId">Forum id</param>
        public string GetForumPostUrl(object postIdPath, object forumId)
        {
            return TransformationHelper.HelperObject.GetForumPostUrl(postIdPath, forumId);
        }


        /// <summary>
        /// Returns URL of the specified media file.
        /// </summary>
        /// <param name="fileGUID">File GUID</param>
        /// <param name="fileName">File name</param>
        public string GetMediaFileUrl(object fileGUID, object fileName)
        {
            return TransformationHelper.HelperObject.GetMediaFileUrl(fileGUID, fileName);
        }


        /// <summary>
        /// Returns URL of the specified meta file.
        /// </summary>
        /// <param name="fileGUID">Meta file GUID</param>
        /// <param name="fileName">Meta file name</param>
        public string GetMetaFileUrl(object fileGUID, object fileName)
        {
            return TransformationHelper.HelperObject.GetMetaFileUrl(fileGUID, fileName);
        }


        /// <summary>
        /// Returns URL of the message board document.
        /// </summary>
        /// <param name="documentIdObj">Document ID</param>
        public string GetMessageBoardUrl(object documentIdObj)
        {
            return TransformationHelper.HelperObject.GetMessageBoardUrl(documentIdObj);
        }


        /// <summary>
        /// Returns URL of the blog comment document.
        /// </summary>
        /// <param name="documentIdObj">Document ID</param>
        public string GetBlogCommentUrl(object documentIdObj)
        {
            return TransformationHelper.HelperObject.GetBlogCommentUrl(documentIdObj);
        }


        /// <summary>
        /// Returns HTML markup representing file icon.
        /// </summary>
        /// <param name="fileExtension">File extension</param>
        public string GetFileIcon(object fileExtension)
        {
            return TransformationHelper.HelperObject.GetFileIcon(fileExtension, Page);
        }


        /// <summary>
        /// Returns font icon class for specified file extension.
        /// </summary>
        /// <param name="extension">File extension</param>
        public string GetFileIconClass(string extension)
        {
            return TransformationHelper.HelperObject.GetFileIconClass(extension);
        }


        /// <summary>
        /// Returns HTML markup representing attachment icon.
        /// </summary>
        /// <param name="attachmentGuidColumn">GUID column</param>
        public string GetAttachmentIcon(object attachmentGuidColumn)
        {
            return TransformationHelper.HelperObject.GetAttachmentIcon(Eval(ValidationHelper.GetString(attachmentGuidColumn, "")), Page);
        }


        /// <summary>
        /// Returns a complete HTML code of the image that is specified in the given column.
        /// </summary>
        /// <param name="attachmentGuidColumn">Name of the column that contains the attachment</param>        
        /// <param name="maxSideSize">Image max. side size</param>
        /// <param name="width">Image width</param>
        /// <param name="height">Image height</param>
        /// <param name="alt">Image alternation text</param>
        public string GetImage(object attachmentGuidColumn, object maxSideSize = null, object width = null, object height = null, object alt = null)
        {
            return TransformationHelper.HelperObject.GetImage(Eval("NodeAlias"), Eval(ValidationHelper.GetString(attachmentGuidColumn, "")), maxSideSize, width, height, alt);
        }


        /// <summary>
        /// Returns a complete HTML code of the image that is specified in the given column.
        /// </summary>
        /// <param name="attachmentGuidColumn">Name of the column that contains the attachment</param>
        /// <param name="maxSideSize">Image max. side size</param>
        public string GetImage(object attachmentGuidColumn, int maxSideSize)
        {
            return TransformationHelper.HelperObject.GetImage(Eval("NodeAlias"), Eval(ValidationHelper.GetString(attachmentGuidColumn, "")), maxSideSize);
        }


        /// <summary>
        /// Returns a complete HTML code of the image that is specified in the given column.
        /// </summary>
        /// <param name="attachmentGuidColumn">Name of the column that contains the attachment</param>
        /// <param name="width">Image width</param>
        /// <param name="height">Image height</param>
        public string GetImage(object attachmentGuidColumn, int width, int height)
        {
            return TransformationHelper.HelperObject.GetImage(Eval("NodeAlias"), Eval(ValidationHelper.GetString(attachmentGuidColumn, "")), width, height);
        }


        /// <summary>
        /// Returns a complete HTML code of the image that is specified by the given url.
        /// </summary>
        /// <param name="imageUrl">Image URL</param>       
        /// <param name="maxSideSize">Image max. side size</param>
        /// <param name="width">Image width</param>
        /// <param name="height">Image height</param>
        /// <param name="alt">Image alternation text</param>
        public string GetImageByUrl(object imageUrl, object maxSideSize = null, object width = null, object height = null, object alt = null)
        {
            return TransformationHelper.HelperObject.GetImageByUrl(imageUrl, maxSideSize, width, height, alt);
        }


        /// <summary>
        /// Returns a complete HTML code of the image that is specified by the given URL.
        /// </summary>
        /// <param name="imageUrl">Image URL</param>
        /// <param name="maxSideSize">Image max. side size</param>
        public string GetImageByUrl(object imageUrl, int maxSideSize)
        {
            return TransformationHelper.HelperObject.GetImageByUrl(imageUrl, maxSideSize);
        }


        /// <summary>
        /// Returns a complete HTML code of the image that is specified by the given URL.
        /// </summary>
        /// <param name="imageUrl">Image URL</param>
        /// <param name="width">Image width</param>
        /// <param name="height">Image height</param>
        public string GetImageByUrl(object imageUrl, int width, int height)
        {
            return TransformationHelper.HelperObject.GetImageByUrl(imageUrl, width, height);
        }


        /// <summary>
        /// Gets the editable image value.
        /// </summary>
        /// <param name="controlId">Editable image ID</param>
        public string GetEditableImageUrl(string controlId)
        {
            return TransformationHelper.HelperObject.GetEditableImageUrl(EditableItems[controlId]);
        }


        /// <summary>
        /// Returns a complete HTML code of the image that is specified by editable image ID.
        /// </summary>
        /// <param name="controlId">Editable image ID</param>
        /// <param name="maxSideSize">Image max. side size</param>
        /// <param name="width">Image width</param>
        /// <param name="height">Image height</param>
        /// <param name="alt">Image alternation text</param>
        public string GetEditableImage(string controlId, object maxSideSize, object width, object height, object alt)
        {
            return TransformationHelper.HelperObject.GetEditableImage(EditableItems[controlId], maxSideSize, width, height, alt);
        }


        /// <summary>
        /// Returns a complete HTML code of the link to the currently rendered document.
        /// </summary>
        /// <param name="encodeName">If true, the document name is encoded</param>
        public string GetDocumentLink(bool encodeName)
        {
            var siteName = GetSiteName();
            return TransformationHelper.HelperObject.GetDocumentLink(siteName, Eval("NodeAliasPath"), Eval("DocumentUrlPath"), Eval("DocumentName"), encodeName);
        }


        /// <summary>
        /// Returns a complete HTML code of the link to the currently rendered document.
        /// </summary>
        public string GetDocumentLink()
        {
            return GetDocumentLink(true);
        }


        /// <summary>
        /// Returns URL for the specified aliasPath and urlPath (preferable).
        /// </summary>
        /// <param name="aliasPath">Alias path</param>
        /// <param name="urlPath">URL Path</param>
        public string GetUrl(object aliasPath, object urlPath)
        {
            return TransformationHelper.HelperObject.GetUrl(aliasPath, urlPath);
        }


        /// <summary>
        /// Returns URL for the specified aliasPath and urlPath (preferable).
        /// </summary>
        /// <param name="aliasPath">Alias path</param>
        /// <param name="urlPath">URL Path</param>
        /// <param name="siteName">Site name</param>
        public string GetUrl(object aliasPath, object urlPath, object siteName)
        {
            return TransformationHelper.HelperObject.GetUrl(aliasPath, urlPath, siteName);
        }


        /// <summary>
        /// Returns resolved (i.e. absolute) URL of data item (page) that currently being processed. Method reflects page navigation settings.
        /// </summary>
        public string GetNavigationUrl()
        {
            // Create a resolver instance in order to resolve macros in document redirect URL
            var resolver = PortalUIHelper.GetControlResolver(Page);
            resolver.Settings.EncodeResolvedValues = true;
            resolver.SetAnonymousSourceData(DataItem);

            return TransformationHelper.HelperObject.GetNavigationUrl(DataItem, resolver);
        }


        /// <summary>
        /// Returns date and time in given format.
        /// </summary>
        /// <param name="dateTimeField">Date-time field name</param>
        /// <param name="formattingString">Standard .NET formatting string for date-time value</param>
        public string GetDateTime(string dateTimeField, string formattingString)
        {
            var evaluatedDateTimeFiled = Eval(dateTimeField);

            if (DataHelper.IsEmpty(evaluatedDateTimeFiled))
            {
                return "";
            }

            var dt = (DateTime)evaluatedDateTimeFiled;
            return (GetDateTime(dt)).ToString(formattingString);
        }


        /// <summary>
        /// Returns date and time string.
        /// </summary>
        /// <param name="dateTimeField">Date-time field name</param>
        public string GetDateTime(string dateTimeField)
        {
            var evaluatedDateTimeField = Eval(dateTimeField);
            if (DataHelper.IsEmpty(evaluatedDateTimeField))
            {
                return "";
            }

            DateTime dt = (DateTime)evaluatedDateTimeField;
            return (GetDateTime(dt)).ToString();
        }


        /// <summary>
        /// Returns date from the provided date-time value.
        /// </summary>
        /// <param name="dateTimeField">Date-time field name</param>
        public string GetDate(string dateTimeField)
        {
            return TransformationHelper.HelperObject.GetDate(Eval(dateTimeField));
        }


        /// <summary>
        /// Returns absolute URL from relative path.
        /// </summary>
        /// <param name="relativeUrl">Relative URL</param>
        public string GetAbsoluteUrl(string relativeUrl)
        {
            return TransformationHelper.HelperObject.GetAbsoluteUrl(relativeUrl);
        }


        /// <summary>
        /// Returns absolute URL from relative path.
        /// </summary>
        /// <param name="relativeUrl">Relative URL</param>
        /// <param name="site">Site identifier (name or ID)</param>
        public string GetAbsoluteUrl(string relativeUrl, SiteInfoIdentifier site)
        {
            return TransformationHelper.HelperObject.GetAbsoluteUrl(relativeUrl, site);
        }


        /// <summary>
        /// Returns first nonEmpty column.
        /// </summary>
        /// <param name="columnsObj">List of column names separated by semicolon</param>
        public string GetNotEmpty(object columnsObj)
        {
            string cols = ValidationHelper.GetString(columnsObj, "");
            string[] columns = cols.Split(';');

            foreach (string column in columns)
            {
                string value = ValidationHelper.GetString(Eval(column), "");
                if (value != "")
                {
                    return value;
                }
            }
            return "";
        }


        /// <summary>
        /// Returns nonEmptyResult if value is null or empty, else returns emptyResult.
        /// </summary>
        /// <param name="value">Conditional value</param>
        /// <param name="emptyResult">Empty value result</param>
        /// <param name="nonEmptyResult">Non empty value result</param>
        public object IfEmpty(object value, object emptyResult, object nonEmptyResult)
        {
            return TransformationHelper.HelperObject.IfEmpty(value, emptyResult, nonEmptyResult);
        }


        /// <summary>
        /// Returns isImage value if file is image.
        /// </summary>
        /// <param name="attachmentGuidColumn">Name of the column that contains the attachment</param>
        /// <param name="isImage">Is image value</param>
        /// <param name="notImage">Is not image value</param>
        public object IfImage(object attachmentGuidColumn, object isImage, object notImage)
        {
            return TransformationHelper.HelperObject.IfImage(Eval(ValidationHelper.GetString(attachmentGuidColumn, "")), isImage, notImage);
        }


        /// <summary>
        /// Indicates if the document is image.
        /// </summary>
        public bool IsImageDocument()
        {
            return TransformationHelper.HelperObject.IsImageDocument(Eval<string>("DocumentType"));
        }


        /// <summary>
        /// Format date time.
        /// </summary>
        /// <param name="datetime">Date time object</param>
        /// <param name="format">Format string (If not set, the date time is formated due to current culture settings.)</param>
        public object FormatDateTime(object datetime, string format)
        {
            return TransformationHelper.HelperObject.FormatDateTime(datetime, format);
        }


        /// <summary>
        /// Format date without time part.
        /// </summary>
        /// <param name="datetime">Date time object</param>
        public object FormatDate(object datetime)
        {
            return TransformationHelper.HelperObject.FormatDate(datetime);
        }


        /// <summary>
        /// Transformation "if" statement for guid, int, string, double, decimal, boolean, DateTime
        /// The type of compare depends on comparable value (second parameter)
        /// If both values are NULL, method returns false result.
        /// </summary>
        /// <param name="value">First value</param>
        /// <param name="comparableValue">Second value</param>
        /// <param name="falseResult">False result</param>
        /// <param name="trueResult">True result</param>
        public object IfCompare(object value, object comparableValue, object falseResult, object trueResult)
        {
            return TransformationHelper.HelperObject.IfCompare(value, comparableValue, falseResult, trueResult);
        }


        /// <summary>
        /// If input value is evaluated as True then 'true result' is returned, otherwise 'false result' is returned.
        /// </summary>
        /// <param name="value">Value to be evaluated</param>
        /// <param name="trueResult">True result</param>\
        /// <param name="falseResult">False result</param>
        public object If(object value, object trueResult, object falseResult)
        {
            return TransformationHelper.HelperObject.If(value, trueResult, falseResult);
        }


        /// <summary>
        /// If input value is evaluated as True then 'true result' is returned, otherwise empty string is returned.
        /// </summary>
        /// <param name="value">Value to be evaluated</param>
        /// <param name="trueResult">True result</param>\
        public object IfTrue(object value, object trueResult)
        {
            return If(value, trueResult, "");
        }


        /// <summary>
        /// Returns encoded text.
        /// </summary>
        /// <param name="text">Text to be encoded</param>        
        public string HTMLEncode(string text)
        {
            return TransformationHelper.HelperObject.HTMLEncode(text);
        }


        /// <summary>
        /// Returns localized text.
        /// </summary>
        /// <param name="text">Text to be localized</param>        
        public string Localize(object text)
        {
            return ResHelper.LocalizeString(ValidationHelper.GetString(text, ""));
        }


        /// <summary>
        /// Gets the editable control value.
        /// </summary>
        /// <param name="controlId">Control ID</param>
        public string GetEditableValue(string controlId)
        {
            return EditableItems[controlId];
        }


        /// <summary>
        /// Returns first column which is present.
        /// </summary>
        /// <param name="columns">Columns list, separated by semicolon</param>
        /// <param name="allowEmptyColumn">TRUE - it does not matter if a column is empty or with some value, FALSE - only column with some value can be returned</param>
        public string GetColumnName(string columns, bool allowEmptyColumn)
        {
            if ((columns != null) && (columns.Trim() != ""))
            {
                DataTable dt = DataHelper.GetDataTable(DataItem);
                if (dt != null)
                {
                    // Split column names
                    string[] columnList = columns.Split(';');

                    foreach (string column in columnList)
                    {
                        string columnName = column.Trim();
                        if (columnName != "")
                        {
                            // If found, get the column name
                            if (dt.Columns.Contains(columnName))
                            {
                                if (allowEmptyColumn || (DataRowView.Row[columnName] != DBNull.Value))
                                {
                                    return columnName;
                                }
                            }
                        }
                    }
                }
            }
            return "";
        }


        /// <summary>
        /// Returns first column which is present and not empty.
        /// </summary>
        /// <param name="columns">Columns list, separated by semicolon</param>        
        public string GetColumnName(string columns)
        {
            return GetColumnName(columns, false);
        }


        /// <summary>
        /// Data bind.
        /// </summary>
        public override void DataBind()
        {
            try
            {
                // Ensure the tracking
                if (DataControl != null)
                {
                    mTrackEvals = SqlDebug.DebugCurrentRequest;
                }

                base.DataBind();
            }
            catch (Exception ex)
            {
                // Log the exception
                Exception newex = new Exception("[CMSAbstractTransformation.DataBind]: " + ex.Message, ex);

                EventLogProvider.LogException("Controls", "DataBind", newex);

                // Add error report
                bool throwOriginal = false;

                try
                {
                    CMSErrorTransformation tr = new CMSErrorTransformation();
                    tr.InnerException = newex;
                    Controls.Add(tr);
                }
                catch
                {
                    throwOriginal = true;
                }

                if (throwOriginal)
                {
                    throw;
                }
            }
        }


        /// <summary>
        /// Remove all types of discussion macros from text.
        /// </summary>
        /// <param name="inputText">Text containing macros to be removed</param>
        public string RemoveDiscussionMacros(object inputText)
        {
            return TransformationHelper.HelperObject.RemoveDiscussionMacros(inputText);
        }


        /// <summary>
        /// Remove all dynamic controls macros from text.
        /// </summary>
        /// <param name="inputText">Text containing macros to be removed</param>
        public string RemoveDynamicControls(object inputText)
        {
            return TransformationHelper.HelperObject.RemoveDynamicControls(inputText);
        }


        /// <summary>
        /// Remove HTML tags from text.
        /// </summary>
        /// <param name="inputText">Text containing tags to be removed</param>
        public string StripTags(object inputText)
        {
            return TransformationHelper.HelperObject.StripTags(inputText);
        }


        /// <summary>
        /// Gets the Data control value.
        /// </summary>
        /// <param name="propertyName">Property name</param>
        public virtual object GetDataControlValue(string propertyName)
        {
            IDataControl control = DataControl;
            if (control != null)
            {
                return control.GetValue(propertyName);
            }

            return null;
        }


        /// <summary>
        /// Gets the Data control value.
        /// </summary>
        /// <param name="propertyName">Property name</param>
        public virtual ReturnType GetDataControlValue<ReturnType>(string propertyName)
        {
            object value = GetDataControlValue(propertyName);
            return ValidationHelper.GetValue<ReturnType>(value);
        }


        /// <summary>
        /// Indicates if current data item is the first.
        /// </summary>        
        public virtual bool IsFirst()
        {
            return (DataItemIndex <= 0);
        }


        /// <summary>
        /// Indicates it current data item is the last.
        /// </summary>        
        public virtual bool IsLast()
        {
            return (DataItemIndex >= (DataItemCount - 1));
        }

        #endregion


        #region "Eval methods"

        /// <summary>
        /// Ensures the internal transformation object which provides the nested data
        /// </summary>
        protected virtual BaseInfo EnsureObject()
        {
            if (!mObjectLoaded)
            {
                // Try if the bound object is base info
                var obj = DataItem as BaseInfo;
                if (obj != null)
                {
                    mObject = obj;
                }
                else
                {
                    var drv = DataRowView;
                    if (drv != null)
                    {
                        // Get the class name
                        object cName = null;
                        if (EvalColumnInternal(drv, "ClassName", ref cName))
                        {
                            // Get the class name
                            string className = ValidationHelper.GetString(cName, "");
                            if (!String.IsNullOrEmpty(className))
                            {
                                DataClassInfo dci = DataClassInfoProvider.GetDataClassInfo(className);
                                if (dci.ClassIsDocumentType)
                                {
                                    // Create document object
                                    mObject = TreeNode.New(className, drv.Row);
                                }
                                else if (dci.ClassIsCustomTable)
                                {
                                    // Create custom table item object
                                    mObject = CustomTableItem.New(className, drv.Row);
                                }
                                else
                                {
                                    // Create standard Info object
                                    mObject = ModuleManager.GetObject(drv.Row, className.ToLowerInvariant());
                                }
                            }
                        }

                        // Try to load by the parent DataSet type
                        if (mObject == null)
                        {
                            if ((drv.Row.Table != null) && (drv.Row.Table.DataSet != null))
                            {
                                // Try to cast to particular object type
                                var ds = drv.Row.Table.DataSet as IInfoDataSet;
                                if (ds != null)
                                {
                                    mObject = ds.GetNewObject(drv.Row);
                                }
                            }
                        }

                        // Check if the object was loaded properly
                        if (mObject == null)
                        {
                            throw new Exception("[CMSAbstractTransformation.EnsureObject]: Could not initialize the transformation object, missing the ClassName column or typed DataSet to properly determine the object type.");
                        }
                    }
                }

                mObjectLoaded = true;
            }

            return mObject;
        }


        /// <summary>
        /// Returns HTML encoded value if value is string and it should be encoded (it depends on value of CMSHTMLEncodeEval key in configuration file).
        /// </summary>
        /// <param name="value">Value to encode</param>  
        private string EnsureValueHTMLEncode(string value)
        {
            // We have some value, now determine if it's string
            if (!string.IsNullOrEmpty(value))
            {
                string valueStr = ValidationHelper.GetString(value, string.Empty);

                // Value is string and we should encode it 
                if (!string.IsNullOrEmpty(valueStr) && TransformationHelper.HTMLEncodeEval)
                {
                    return HTMLEncode(valueStr);
                }

                return value;
            }

            return null;
        }


        /// <summary>
        /// Templated Eval, returns the value converted to specific type.
        /// </summary>
        /// <typeparam name="ReturnType">Result type</typeparam>
        /// <param name="columnName">Column name</param>
        public override ReturnType Eval<ReturnType>(string columnName)
        {
            object value = Eval(columnName);
            return TransformationHelper.HelperObject.EnsureValueHTMLEncode<ReturnType>(value);
        }


        /// <summary>
        /// Evaluates the data column
        /// </summary>
        /// <param name="data">Source data</param>
        /// <param name="columnName">Column name or expression to get</param>
        /// <param name="value">Returning value of the evaluation</param>
        protected virtual bool EvalColumnInternal(object data, string columnName, ref object value)
        {
            // Try to get from the DataRow view source
            var drv = data as DataRowView;
            if (drv != null)
            {
                if (mTrackEvals)
                {
                    // Get the columns count
                    var colsCount = drv.DataView.Table.Columns.Count;

                    // Track evaluation within the parent data control
                    DataControl.LogEval(columnName, colsCount);
                }

                var result = DataHelper.TryGetDataRowViewValue(drv, columnName, out value);
                return result || RaiseEvaluateHandler(data, columnName, out value);
            }

            // Try to get from hierarchical object
            var obj = data as IHierarchicalObject;
            if (obj != null)
            {
                var result = obj.TryGetProperty(columnName, out value);
                return result || RaiseEvaluateHandler(data, columnName, out value);
            }

            // Try to get from Data container
            var container = data as IDataContainer;
            if (container != null)
            {
                var result = container.TryGetValue(columnName, out value);
                return result || RaiseEvaluateHandler(data, columnName, out value);
            }

            return false;
        }


        /// <summary>
        /// Evaluates the expression on the available data
        /// </summary>
        /// <param name="data">Source data</param>
        /// <param name="obj">Object to use as a base within further selection</param>
        /// <param name="columnName">Column name or expression to get</param>
        /// <param name="value">Returning value of the evaluation</param>
        protected virtual bool EvalInternal(object data, IHierarchicalObject obj, string columnName, ref object value)
        {
            // Examine for object selector
            int dotIndex = columnName.IndexOf('.');
            if (dotIndex < 0)
            {
                return EvalColumnInternal(data, columnName, ref value);
            }

            string selector = columnName.Substring(0, dotIndex);
            columnName = columnName.Substring(dotIndex + 1);

            if (obj == null)
            {
                // If not given, use the main object of the transformation
                obj = Object;
            }

            if (obj == null)
            {
                return false;
            }

            // Empty selector - Get column directly from object
            if (String.IsNullOrEmpty(selector))
            {
                return EvalInternal(obj, obj, columnName, ref value);
            }
                    
            // Get the object property by selector
            if (obj.TryGetProperty(selector, out value))
            {
                return EvalInternal(value, value as IHierarchicalObject, columnName, ref value);
            }
                    
            // Property not available
            string message = String.Format("[CMSAbstractTransformation]: Hosted transformation object does not provide property '{0}' to be able to evaluate expression '{0}.{1}'.", selector, columnName);

            throw new Exception(message);
        }


        /// <summary>
        /// Evaluates the item data (safe version).
        /// </summary>
        /// <param name="columnName">Column name</param>
        public override object Eval(string columnName)
        {
            object value = null;

            // Try to resolve internally
            if (!EvalInternal(DataItem, null, columnName, ref value))
            {
                try
                {
                    // Try to get regularly from base
                    value = base.Eval(columnName);
                }
                catch
                {
                }
            }

            // Try to get from the related data
            if (value == null)
            {
                object relatedData = RelatedData;
                if (relatedData is IDataContainer)
                {
                    return ((IDataContainer)relatedData).GetValue(columnName);
                }
            }

            var valueAsString = value as string;
            if (valueAsString != null)
            {
                return EnsureValueHTMLEncode(valueAsString);
            }

            return value;
        }


        /// <summary>
        /// Evaluates the item data and doesn't encode it. Method should be used for columns with html content.
        /// </summary>
        /// <param name="columnName">Column name</param>
        public override string EvalHTML(string columnName)
        {
            return ValidationHelper.GetString(Eval(columnName, false), string.Empty);
        }


        /// <summary>
        /// Evaluates the item data, encodes it to be used in javascript code and encapsulates it with "'".
        /// </summary>
        /// <param name="columnName">Column name</param>        
        public override string EvalJSString(string columnName)
        {
            try
            {
                object value = Eval(columnName);
                return TransformationHelper.HelperObject.JSEncode(ValidationHelper.GetString(value, ""));
            }
            catch
            {
                return null;
            }
        }


        /// <summary>
        /// Evaluates the item data and encodes it. Method should be used for columns with string non-html content.
        /// </summary>
        /// <param name="columnName">Column name</param>
        public override string EvalText(string columnName)
        {
            return EvalText(columnName, false);
        }


        /// <summary>
        /// Evaluates the item data, encodes it and optionally localizes it. Method should be used for columns with string non-html content.
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="localize">Indicates if text should be localized</param>
        public override string EvalText(string columnName, bool localize)
        {
            // Encode text
            string value = ValidationHelper.GetString(Eval(columnName, true), string.Empty);

            if (localize)
            {
                // Localize text
                return ResHelper.LocalizeString(value);
            }

            return value;
        }


        /// <summary>
        /// Evaluates the item data and converts it to the integer.
        /// </summary>
        /// <param name="columnName">Column name</param>
        public override int EvalInteger(string columnName)
        {
            return Eval<int>(columnName);
        }


        /// <summary>
        /// Evaluates the item data and converts it to the double.
        /// </summary>
        /// <param name="columnName">Column name</param>
        public override double EvalDouble(string columnName)
        {
            return Eval<double>(columnName);
        }


        /// <summary>
        /// Evaluates the item data and converts it to the decimal.
        /// </summary>
        /// <param name="columnName">Column name</param>
        public override decimal EvalDecimal(string columnName)
        {
            return Eval<decimal>(columnName);
        }


        /// <summary>
        /// Evaluates the item data and converts it to the date time.
        /// </summary>
        /// <param name="columnName">Column name</param>
        public override DateTime EvalDateTime(string columnName)
        {
            return Eval<DateTime>(columnName);
        }


        /// <summary>
        /// Evaluates the item data and converts it to the guid.
        /// </summary>
        /// <param name="columnName">Column name</param>
        public override Guid EvalGuid(string columnName)
        {
            return Eval<Guid>(columnName);
        }


        /// <summary>
        /// Evaluates the item data and converts it to the boolean.
        /// </summary>
        /// <param name="columnName">Column name</param>
        public override bool EvalBool(string columnName)
        {
            return Eval<bool>(columnName);
        }


        /// <summary>
        /// Raises handler to handle column evaluation
        /// </summary>
        /// <param name="data">Data</param>
        /// <param name="columnName">Column name to evaluate</param>
        /// <param name="value">Output value</param>
        private static bool RaiseEvaluateHandler(object data, string columnName, out object value)
        {
            if (DocumentEngineWebUIEvents.TransformationEval.IsBound)
            {
                var e = new TransformationEvalEventArgs
                {
                    Data = data,
                    ColumnName = columnName
                };

                DocumentEngineWebUIEvents.TransformationEval.StartEvent(e);

                if (e.HasValue)
                {
                    value = e.Value;
                    return true;
                }
            }

            value = null;
            return false;
        }


        /// <summary>
        /// Gets site name from data
        /// </summary>
        private string GetSiteName()
        {
            int siteId = Eval<int>("NodeSiteID");
            if (siteId <= 0)
            {
                siteId = SiteContext.CurrentSiteID;
            }

            return SiteInfoProvider.GetSiteName(siteId);
        }

        #endregion


        #region "UI image methods"

        /// <summary>
        /// Gets UI image resolved path.
        /// </summary>
        /// <param name="imagePath">Partial image path starting from ~/App_Themes/(Skin_Folder)/Images/ (e.g. '/CMSModules/CMS_MediaLibrary/module.png')</param>
        public virtual string GetUIImageUrl(string imagePath)
        {
            return TransformationHelper.HelperObject.GetUIImageUrl(imagePath, Page);
        }


        /// <summary>
        /// Gets UI image resolved path.
        /// </summary>
        /// <param name="imagePath">Partial image path starting from ~/App_Themes/(Skin_Folder)/Images/ (e.g. '/CMSModules/CMS_MediaLibrary/module.png')</param>
        /// <param name="isLiveSite">Indicates if URL should be returned for live site</param>
        public virtual string GetUIImageUrl(string imagePath, bool isLiveSite)
        {
            return TransformationHelper.HelperObject.GetUIImageUrl(imagePath, isLiveSite, Page);
        }


        /// <summary>
        /// Gets UI image path.
        /// </summary>
        /// <param name="imagePath">Partial image path starting from ~/App_Themes/(Skin_Folder)/Images/ (e.g. '/CMSModules/CMS_MediaLibrary/module.png')</param>
        /// <param name="isLiveSite">Indicates if URL should be returned for live site</param>
        /// <param name="ensureDefaultTheme">Indicates if default theme should be always used</param>
        public virtual string GetUIImageUrl(string imagePath, bool isLiveSite, bool ensureDefaultTheme)
        {
            return TransformationHelper.HelperObject.GetUIImageUrl(imagePath, isLiveSite, ensureDefaultTheme, Page);
        }

        #endregion


        #region "Time zones"

        /// <summary>
        /// Returns current user date time DateTime according to user time zone.
        /// </summary>
        /// <param name="dateTime">Date time</param>
        public DateTime GetUserDateTime(object dateTime)
        {
            return TransformationHelper.HelperObject.GetUserDateTime(dateTime);
        }


        /// <summary>
        /// Returns site date time according to site time zone.
        /// </summary>
        /// <param name="dateTime">Date time</param>
        public DateTime GetSiteDateTime(object dateTime)
        {
            return TransformationHelper.HelperObject.GetSiteDateTime(dateTime);
        }


        /// <summary>
        /// Returns date time with dependence on selected time zone.
        /// </summary>
        /// <param name="dateTime">DateTime to convert (server time zone)</param>
        /// <param name="timeZoneName">Time zone code name</param>
        public DateTime GetCustomDateTime(object dateTime, string timeZoneName)
        {
            return TransformationHelper.HelperObject.GetCustomDateTime(dateTime, timeZoneName);
        }


        /// <summary>
        /// Returns date time with dependence on current ITimeZone manager time zone settings.
        /// </summary>
        /// <param name="dateTime">DateTime to convert (server time zone)</param>
        public DateTime GetDateTime(object dateTime)
        {
            return TransformationHelper.HelperObject.GetDateTime(this, dateTime);
        }


        /// <summary>
        /// Returns date time with dependence on current ITimeZone manager time zone settings.
        /// </summary>
        /// <param name="dateTime">DateTime to convert (server time zone)</param>
        public DateTime GetDateTime(DateTime dateTime)
        {
            return TransformationHelper.HelperObject.GetDateTime(this, dateTime);
        }


        /// <summary>
        /// Returns string representation of date time with dependence on current ITimeZone manager
        /// time zone settings. 
        /// </summary>
        /// <param name="dateTime">DateTime to convert (server time zone)</param>
        /// <param name="showTooltip">Wraps date in span with tooltip</param>
        public string GetDateTimeString(object dateTime, bool showTooltip)
        {
            return TransformationHelper.HelperObject.GetDateTimeString(this, dateTime, showTooltip);
        }

        #endregion


        #region "Community"

        /// <summary>
        /// Returns age according to DOB. If DOB is not set, returns unknownAge string.
        /// </summary>
        /// <param name="dateOfBirth">Date of birth</param>
        /// <param name="unknownAge">Text which is returned when no DOB is given</param>
        public string GetAge(object dateOfBirth, string unknownAge)
        {
            return TransformationHelper.HelperObject.GetAge(dateOfBirth, unknownAge);
        }


        /// <summary>
        /// Returns gender of the user.
        /// </summary>
        /// <param name="genderObj">Gender of the user (0/1/2 = N/A / Male / Female)</param>
        public string GetGender(object genderObj)
        {
            return TransformationHelper.HelperObject.GetGender(genderObj);
        }


        /// <summary>
        /// Returns group profile URL.
        /// </summary>
        /// <param name="groupNameObj">Group name object</param>
        public string GetGroupProfileUrl(object groupNameObj)
        {
            return GetGroupProfileUrl(groupNameObj, SiteContext.CurrentSiteName);
        }


        /// <summary>
        /// Returns group profile URL.
        /// </summary>
        /// <param name="groupNameObj">Group name object</param>
        /// <param name="siteName">Name of the site</param>
        public string GetGroupProfileUrl(object groupNameObj, string siteName)
        {
            return TransformationHelper.HelperObject.GetGroupProfileUrl(groupNameObj, siteName);
        }


        /// <summary>
        /// Returns member profile URL.
        /// </summary>
        /// <param name="memberNameObj">Member name object</param>
        public string GetMemberProfileUrl(object memberNameObj)
        {
            return GetMemberProfileUrl(memberNameObj, SiteContext.CurrentSiteName);
        }


        /// <summary>
        /// Returns member profile URL.
        /// </summary>
        /// <param name="memberNameObj">Member name object</param>
        /// <param name="siteName">Name of the site</param>
        public string GetMemberProfileUrl(object memberNameObj, string siteName)
        {
            return TransformationHelper.HelperObject.GetMemberProfileUrl(memberNameObj, siteName);
        }


        /// <summary>
        /// Returns user profile URL.
        /// </summary>
        /// <param name="userNameObj">User name object</param>
        public string GetUserProfileURL(object userNameObj)
        {
            return GetUserProfileURL(userNameObj, SiteContext.CurrentSiteName);
        }


        /// <summary>
        /// Returns user profile URL.
        /// </summary>
        /// <param name="userNameObj">User name object</param>
        /// <param name="siteName">Name of the site</param>
        public string GetUserProfileURL(object userNameObj, string siteName)
        {
            return TransformationHelper.HelperObject.GetUserProfileURL(userNameObj, siteName);
        }


        /// <summary>
        /// Returns user avatar image. Datarow must contains 'AvatarGuid' and 'UserGender' columns.
        /// </summary>
        /// <param name="maxSideSize">Max side size</param>
        /// <param name="alt">Alternate text</param>
        public string GetUserAvatarImage(int maxSideSize, string alt)
        {
            return TransformationHelper.HelperObject.GetUserAvatarImageByGUID(Eval("AvatarGuid"), Eval("UserGender"), maxSideSize, 0, 0, alt);
        }


        /// <summary>
        /// Returns avatar image tag, if avatar is not defined returns gender depend avatar or user default avatar if is defined.
        /// </summary>
        /// <param name="avatarID">Avatar ID</param>
        /// <param name="userID">User ID, load gender avatar for specified user if avatar by avatar id doesn't exist</param>
        /// <param name="maxSideSize">Max side size</param>
        /// <param name="alt">Image alternate text</param>
        public string GetUserAvatarImage(object avatarID, object userID, int maxSideSize, object alt)
        {
            return TransformationHelper.HelperObject.GetUserAvatarImage(avatarID, userID, maxSideSize, 0, 0, alt);
        }


        /// <summary>
        /// Returns avatar image URL, if avatar is not defined returns gender depend avatar or user default avatar if is defined.
        /// </summary>
        /// <param name="avatarID">Avatar ID</param>
        /// <param name="userID">User ID, load gender avatar for specified user if avatar by avatar id doesn't exist</param>
        /// <param name="maxSideSize">Max side size</param>
        /// <param name="width">Image width</param>
        /// <param name="height">Image height</param>
        public string GetUserAvatarImageUrl(object avatarID, object userID, int maxSideSize, int width, int height)
        {
            return TransformationHelper.HelperObject.GetUserAvatarImageUrl(avatarID, userID, maxSideSize, width, height);
        }


        /// <summary>
        /// Returns avatar image URL, if avatar is not defined returns gender depend avatar or user default avatar if is defined.
        /// </summary>
        /// <param name="avatarID">Avatar ID</param>
        /// <param name="userID">User ID, load gender avatar for specified user if avatar by avatar id doesn't exist</param>
        /// <param name="userEmail">User e-mail</param>
        /// <param name="maxSideSize">Max side size</param>
        /// <param name="width">Image width</param>
        /// <param name="height">Image height</param>
        public string GetUserAvatarImageUrl(object avatarID, object userID, string userEmail, int maxSideSize, int width, int height)
        {
            return TransformationHelper.HelperObject.GetUserAvatarImageUrl(avatarID, userID, userEmail, maxSideSize, width, height);
        }


        /// <summary>
        /// Returns avatar image tag, if avatar is not defined returns gender depend avatar or user default avatar if is defined.
        /// </summary>
        /// <param name="userID">User ID, load gender avatar for specified user if avatar by avatar id doesn't exist</param>
        /// <param name="maxSideSize">Max side size</param>
        /// <param name="width">Image width</param>
        /// <param name="height">Image height</param>
        /// <param name="alt">Image alternate text</param>
        public string GetUserAvatarImage(object userID, int maxSideSize, int width, int height, object alt)
        {
            return TransformationHelper.HelperObject.GetUserAvatarImageForUser(userID, maxSideSize, width, height, alt);
        }


        /// <summary>
        /// Returns avatar image tag, if avatar is not defined returns gender depend avatar or user default avatar if is defined.
        /// </summary>
        /// <param name="avatarID">Avatar ID</param>
        /// <param name="userID">User ID, load gender avatar for specified user if avatar by avatar id doesn't exist</param>
        /// <param name="maxSideSize">Max side size</param>
        /// <param name="width">Image width</param>
        /// <param name="height">Image height</param>
        /// <param name="alt">Image alternate text</param>
        public string GetUserAvatarImage(object avatarID, object userID, int maxSideSize, int width, int height, object alt)
        {
            return TransformationHelper.HelperObject.GetUserAvatarImage(avatarID, userID, maxSideSize, width, height, alt);
        }


        /// <summary>
        /// Returns group avatar image tag, if avatar is not defined returns default group if is defined.
        /// </summary>
        /// <param name="maxSideSize">Max side size</param>
        /// <param name="alt">Image alternate text</param>
        public string GetGroupAvatarImage(int maxSideSize, object alt)
        {
            return TransformationHelper.HelperObject.GetGroupAvatarImage(Eval("AvatarGuid"), maxSideSize, alt);
        }


        /// <summary>
        /// Returns group avatar image tag, if avatar is not defined returns default group if is defined.
        /// </summary>
        /// <param name="avatarID">Avatar ID</param>
        /// <param name="maxSideSize">Max side size</param>
        /// <param name="alt">Image alternate text</param>
        public string GetGroupAvatarImage(object avatarID, int maxSideSize, object alt)
        {
            return TransformationHelper.HelperObject.GetGroupAvatarImage(avatarID, maxSideSize, 0, 0, alt);
        }


        /// <summary>
        /// Returns group avatar image tag, if avatar is not defined returns default group if is defined.
        /// </summary>
        /// <param name="avatarID">Avatar ID</param>
        /// <param name="maxSideSize">Max side size</param>
        /// <param name="width">Image width</param>
        /// <param name="height">Image height</param>
        /// <param name="alt">Image alternate text</param>
        public string GetGroupAvatarImage(object avatarID, int maxSideSize, int width, int height, object alt)
        {
            return TransformationHelper.HelperObject.GetGroupAvatarImage(avatarID, maxSideSize, width, height, alt);
        }


        /// <summary>
        /// Returns badge image tag.
        /// </summary>
        /// <param name="badgeId">Badge ID</param>
        public string GetBadgeImage(int badgeId)
        {
            return TransformationHelper.HelperObject.GetBadgeImage(badgeId);
        }


        /// <summary>
        /// Returns badge name.
        /// </summary>
        /// <param name="badgeId">Badge ID</param>
        public string GetBadgeName(int badgeId)
        {
            return TransformationHelper.HelperObject.GetBadgeName(badgeId);
        }


        /// <summary>
        /// Returns user full name.
        /// </summary>
        /// <param name="userId">User ID</param>
        public static string GetUserFullName(int userId)
        {
            return TransformationHelper.HelperObject.GetUserFullName(userId);
        }

        #endregion


        #region "SharePoint"

        /// <summary>
        /// Gets URL for accessing file on SharePoint server.
        /// </summary>
        /// <param name="fileRefColumnName">Name of column which contains SharePoint server relative path to the file. Null uses the default (FileRef).</param>
        /// <param name="cacheMinutes">How long should the file be cached (after accessing the URL). Blank uses the Settings value, 0 means no cache.</param>
        /// <param name="cacheFileSize">The maximum size of file the will be cached. Blank uses the Settings value.</param>
        /// <param name="width">Maximum width of the image the handler should return.</param>
        /// <param name="height">Maximum height of the image the handler should return.</param>
        /// <param name="maxSideSize">Maximum side size of the image the handler should return.</param>
        /// <returns>URL on which the file can be accessed.</returns>
        public string GetSharePointFileUrl(string fileRefColumnName = null, int? cacheMinutes = null, int? cacheFileSize = null, int? width = null, int? height = null, int? maxSideSize = null)
        {
            object connectionNameObject = null;
            object fileRefObject = null;
            string connectionName;
            string fileRef;

            if (!EvalColumnInternal(DataItem, "#connectionname", ref connectionNameObject) || !EvalColumnInternal(DataItem, fileRefColumnName ?? "FileRef", ref fileRefObject)
                || ((connectionName = connectionNameObject as string) == null) || ((fileRef = fileRefObject as string) == null))
            {
                return String.Empty;
            }

            return TransformationHelper.HelperObject.GetSharePointFileUrl(connectionName, fileRef, cacheMinutes, cacheFileSize, width, height, maxSideSize);
        }


        /// <summary>
        /// Gets URL for accessing an image on SharePoint server.
        /// </summary>
        /// <param name="fileRefColumnName">Name of column which contains SharePoint server relative path to the image. Null uses the default (FileRef).</param>
        /// <param name="width">Maximum width of the image the handler should return.</param>
        /// <param name="height">Maximum height of the image the handler should return.</param>
        /// <param name="maxSideSize">Maximum side size of the image the handler should return.</param>
        /// <returns></returns>
        public string GetSharePointImageUrl(string fileRefColumnName = null, int? width = null, int? height = null, int? maxSideSize = null)
        {
            return GetSharePointFileUrl(fileRefColumnName, null, null, width, height, maxSideSize);
        }

        #endregion


        #region "SmartSearch"

        /// <summary>
        /// Returns URL to current search result item.
        /// </summary>
        /// <param name="noImageUrl">URL to image which should be displayed if image is not defined</param>
        /// <param name="maxSideSize">Max. side size</param>
        public string GetSearchImageUrl(string noImageUrl, int maxSideSize)
        {
            return TransformationHelper.HelperObject.GetSearchImageUrl(ValidationHelper.GetString(Eval("id"), String.Empty),
                                                                       ValidationHelper.GetString(Eval("type"), String.Empty), ValidationHelper.GetString(Eval("image"), String.Empty), noImageUrl, maxSideSize);
        }


        /// <summary>
        /// Highlight input text with dependence on current search keywords.
        /// </summary>
        /// <param name="text">Input text</param>
        /// <param name="startTag">Start highlight tag</param>
        /// <param name="endTag">End tag</param>
        public string SearchHighlight(string text, string startTag, string endTag)
        {
            return TransformationHelper.HelperObject.SearchHighlight(text, startTag, endTag);
        }


        /// <summary>
        /// Returns content parsed as XML if required and removes dynamic controls.
        /// </summary>
        public string GetSearchedContent(string content)
        {
            return TransformationHelper.HelperObject.GetSearchedContent(ValidationHelper.GetString(Eval("id"), String.Empty),
                                                                        ValidationHelper.GetString(Eval("type"), ""), content);
        }


        /// <summary>
        /// Returns column value for current search result item.
        /// </summary>
        /// <param name="columnName">Column</param>
        public object GetSearchValue(string columnName)
        {
            return TransformationHelper.HelperObject.GetSearchValue(ValidationHelper.GetString(Eval("id"), String.Empty), columnName);
        }


        /// <summary>
        /// Returns URL for current search result.
        /// </summary>
        /// <param name="absolute">Indicates whether generated URL should be absolute. False by default.</param>
        /// <param name="addLangParameter">Adds culture specific query parameter to the URL if more than one culture version exists. True by default.</param>
        public string SearchResultUrl(bool absolute = false, bool addLangParameter = true)
        {
            return TransformationHelper.HelperObject.SearchResultUrl(ValidationHelper.GetString(Eval("id"), String.Empty), ValidationHelper.GetString(Eval("type"), String.Empty), absolute, addLangParameter);
        }

        #endregion


        #region "Syndication methods"

        /// <summary>
        /// Evaluates the item data and return escaped CDATA.
        /// </summary>
        /// <param name="columnName">Column name</param>
        public virtual object EvalCDATA(string columnName)
        {
            return TransformationHelper.HelperObject.EvalCDATA(Eval(columnName));
        }


        /// <summary>
        /// Evaluates the item data and return escaped CDATA.
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="encapsulate">Indicates if resulting string will be encapsulated in CDATA section</param>
        public virtual object EvalCDATA(string columnName, bool encapsulate)
        {
            return TransformationHelper.HelperObject.EvalCDATA(Eval(columnName), encapsulate);
        }


        /// <summary>
        /// Returns safe feed name. Checks "FeedName" and "InstanceGUID" columns in data controls.
        /// </summary>
        protected string GetFeedName()
        {
            var feedName = GetDataControlValue<string>("FeedName");
            
            return String.IsNullOrEmpty(feedName) ? GetDataControlValue<string>("InstanceGUID") : URLHelper.GetSafeUrlPart(feedName, GetSiteName());
        }


        /// <summary>
        /// Returns URL of the currently rendered document with feed parameter.
        /// </summary>
        public string GetDocumentUrlForFeed()
        {
            return TransformationHelper.HelperObject.GetDocumentUrlForFeed(GetFeedName(), GetDocumentUrl());
        }


        /// <summary>
        /// Returns URL of the specified forum post with feed parameter.
        /// </summary>
        /// <param name="forumId">Forum id</param>
        /// <param name="postIdPath">Post id path</param>
        public string GetForumPostUrlForFeed(object postIdPath, object forumId)
        {
            return TransformationHelper.HelperObject.GetForumPostUrlForFeed(GetFeedName(), postIdPath, forumId);
        }


        /// <summary>
        /// Returns URL of the specified media file with feed parameter.
        /// </summary>
        /// <param name="fileGUID">File GUID</param>
        /// <param name="fileName">File name</param>
        public string GetMediaFileUrlForFeed(object fileGUID, object fileName)
        {
            return TransformationHelper.HelperObject.GetMediaFileUrlForFeed(GetFeedName(), fileGUID, fileName);
        }


        /// <summary>
        /// Returns URL of the message board document with feed parameter.
        /// </summary>
        /// <param name="documentIdObj">Document ID</param>
        public string GetMessageBoardUrlForFeed(object documentIdObj)
        {
            return TransformationHelper.HelperObject.GetMessageBoardUrlForFeed(GetFeedName(), documentIdObj);
        }


        /// <summary>
        /// Returns URL of the blog comment document with feed parameter.
        /// </summary>
        /// <param name="documentIdObj">Document ID</param>
        public string GetBlogCommentUrlForFeed(object documentIdObj)
        {
            return TransformationHelper.HelperObject.GetBlogCommentUrlForFeed(GetFeedName(), documentIdObj);
        }


        /// <summary>
        /// Gets time according to RFC 3339 for Atom feeds.
        /// </summary>
        /// <param name="dateTime">DateTime object to format</param>
        /// <returns>Formatted date</returns>
        public string GetAtomDateTime(object dateTime)
        {
            return TransformationHelper.HelperObject.GetAtomDateTime(this, dateTime);
        }


        /// <summary>
        /// Gets time according to RFC 822 for RSS feeds.
        /// </summary>
        /// <param name="dateTime">DateTime object to format</param>
        /// <returns>Formatted date</returns>
        public string GetRSSDateTime(object dateTime)
        {
            return TransformationHelper.HelperObject.GetRSSDateTime(this, dateTime);
        }

        #endregion


        #region "Booking system"

        /// <summary>
        /// Returns string representation of event time.
        /// </summary>
        /// <param name="startTime">Event start time</param>
        /// <param name="endTime">Event end time</param>
        /// <param name="isAllDayEvent">Indicates if it is all day event - if yes, result does not contain times</param>
        public string GetEventDateString(object startTime, object endTime, bool isAllDayEvent)
        {
            return TransformationHelper.HelperObject.GetEventDateString(this, startTime, endTime, isAllDayEvent);
        }

        #endregion


        #region "Categories"

        /// <summary>
        /// Appends current category ID to given URL.
        /// </summary>
        /// <param name="url">URL to add parameter to</param>
        public string AddCurrentCategoryParameter(object url)
        {
            return TransformationHelper.HelperObject.AddCurrentCategoryParameter(url);
        }

        #endregion
    }
}