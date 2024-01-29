using System;
using System.Web.UI;

using CMS.Base;
using CMS.Base.Web.UI;
using CMS.Helpers;
using CMS.MacroEngine;

namespace CMS.DocumentEngine.Web.UI
{
    /// <summary>
    /// Class containing wrapper methods for transformation methods which can be used in macro resolver.
    /// </summary>
    public class TransformationMethods : MacroMethodContainer
    {
        #region "Working methods"

        /// <summary>
        /// Returns sitemap XML element for specified type (loc, lastmod, changefreq, priority). 
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(string), "Returns sitemap XML element for specified type (loc, lastmod, changefreq, priority).", 2)]
        [MacroMethodParam(0, "dataitem", typeof(object), "Container data item")]
        [MacroMethodParam(1, "type", typeof(string), "Sitemap type")]
        public static string GetSitemapItem(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 2:
                    return TransformationHelper.HelperObject.GetSitemapItem(parameters[0], ValidationHelper.GetString(parameters[1], String.Empty));

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Limits line length of the given plain text.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(string), "Limits line length of the given plain text.", 2)]
        [MacroMethodParam(0, "textObj", typeof(object), "Plain text to limit (Text containing HTML tags is not supported).")]
        [MacroMethodParam(1, "maxLength", typeof(int), "Maximum line length.")]
        public static object EnsureMaximumLineLength(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 2:
                    return TransformationHelper.HelperObject.EnsureMaximumLineLength(parameters[0], ValidationHelper.GetInteger(parameters[1], 0));

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns URL of the attachment with given name.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(string), "Returns URL of the attachment.", 2, IsHidden = true)]
        [MacroMethodParam(0, "attachmentFileName", typeof(object), "File name of the attachment.")]
        [MacroMethodParam(1, "attachmentDocumentId", typeof(string), "ID of the document.")]
        [MacroMethodParam(2, "variant", typeof(string), "Identifier of the attachment variant definition.")]
        [Obsolete("Use GetAttachmentUrlByDocumentId method instead.")]
        public static object GetFileUrl(EvaluationContext context, params object[] parameters)
        {
            return GetAttachmentUrlByDocumentId(context, parameters);
        }


        /// <summary>
        /// Returns URL of the attachment.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(string), "Returns URL of the attachment.", 2)]
        [MacroMethodParam(0, "attachmentFileName", typeof(string), "File name of the attachment.")]
        [MacroMethodParam(1, "attachmentDocumentId", typeof(int), "ID of the document.")]
        [MacroMethodParam(2, "variant", typeof(string), "Identifier of the attachment variant definition.")]
        public static object GetAttachmentUrlByDocumentId(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 2:
                case 3:
                    var attachmentFileName = ValidationHelper.GetString(parameters[0], "");
                    var attachmentDocumentId = ValidationHelper.GetInteger(parameters[1], 0);
                    var variant = parameters.Length == 2 ? null : parameters[2] as String;

                    return TransformationHelper.HelperObject.GetAttachmentUrlByDocumentId(attachmentFileName, attachmentDocumentId, variant);

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns URL of the attachment with given attachment GUID.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(string), "Returns URL of the attachment with given attachment GUID.", 2, IsHidden = true)]
        [MacroMethodParam(0, "attachmentGuid", typeof(object), "GUID of the attachment.")]
        [MacroMethodParam(1, "attachmentFileName", typeof(object), "Name of the attachment file.")]
        [MacroMethodParam(2, "variant", typeof(string), "Identifier of the attachment variant definition.")]
        [Obsolete("Use GetAttachmentUrlByGUID method instead.")]
        public static object GetFileUrlByGUID(EvaluationContext context, params object[] parameters)
        {
            return GetAttachmentUrlByGUID(context, parameters);
        }


        /// <summary>
        /// Returns URL of the attachment with given attachment GUID.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(string), "Returns URL of the attachment.", 2)]
        [MacroMethodParam(0, "attachmentGuid", typeof(Guid), "GUID of the attachment.")]
        [MacroMethodParam(1, "attachmentFileName", typeof(string), "Name of the attachment file.")]
        [MacroMethodParam(2, "variant", typeof(string), "Identifier of the attachment variant definition.")]
        public static object GetAttachmentUrlByGUID(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 2:
                case 3:
                    var attachmentGuid = ValidationHelper.GetGuid(parameters[0], Guid.Empty);
                    var attachmentFileName = ValidationHelper.GetString(parameters[1], "");
                    var variant = parameters.Length == 2 ? null : parameters[2] as String;

                    return TransformationHelper.HelperObject.GetAttachmentUrlByGUID(attachmentGuid, attachmentFileName, variant);

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns URL of the attachment with given GUID.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(string), "Returns URL of the attachment with given GUID.", 2, IsHidden = true)]
        [MacroMethodParam(0, "fileGuid", typeof(Guid), "Guid of the attachment file.")]
        [MacroMethodParam(1, "nodeAlias", typeof(string), "Attachment page alias.")]
        [MacroMethodParam(2, "variant", typeof(string), "Identifier of the attachment variant definition.")]
        [Obsolete("Use GetAttachmentUrlByGUID method instead.")]
        public static object GetFileUrlFromAlias(EvaluationContext context, params object[] parameters)
        {
            return GetAttachmentUrlByGUID(context, parameters);
        }


        /// <summary>
        /// Returns URL of the attachment.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(string), "Returns URL of the attachment.", 2)]
        [MacroMethodParam(0, "attachmentFileName", typeof(string), "Name of the attachment file.")]
        [MacroMethodParam(1, "nodeAliasPath", typeof(string), "Attachment page alias path.")]
        [MacroMethodParam(2, "variant", typeof(string), "Identifier of the attachment variant definition.")]
        public static object GetAttachmentUrl(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 2:
                case 3:
                    var attachmentFileName = ValidationHelper.GetString(parameters[0], "");
                    var nodeAliasPath = ValidationHelper.GetString(parameters[1], "");
                    var variant = parameters.Length == 2 ? null : parameters[2] as String;

                    return TransformationHelper.HelperObject.GetAttachmentUrl(attachmentFileName, nodeAliasPath, variant);

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns country displayname based on its codename.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(string), "Returns country's displayname based on its codename.", 2)]
        [MacroMethodParam(0, "countryCode", typeof(string), "Code name of country.")]
        [MacroMethodParam(1, "appendState", typeof(bool), "If true, state code is appended to country name.")]
        [MacroMethodParam(2, "format", typeof(string), "Format for appending state to country. Default format is '{state}, {country}'.")]
        public static object GetCountryDisplayName(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 1:
                    return TransformationHelper.HelperObject.GetCountryDisplayName(parameters[0] as String);
                case 2:
                    return TransformationHelper.HelperObject.GetCountryDisplayName(parameters[0] as String, ValidationHelper.GetBoolean(parameters[1], true));
                case 3:
                    return TransformationHelper.HelperObject.GetCountryDisplayName(parameters[0] as String, ValidationHelper.GetBoolean(parameters[1], true), parameters[2] as String);

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns URL of specified document.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        /// <exclude />
        [MacroMethod(typeof(string), "Returns URL of the given document.", 1, IsHidden = true)]
        [MacroMethodParam(0, "documentIdObj", typeof(object), "Document ID.")]
        [Obsolete("Use GetDocumentUrlById method instead.")]
        public static object GetDocumentUrlByID(EvaluationContext context, params object[] parameters)
        {
            return GetDocumentUrlById(context, parameters);
        }


        /// <summary>
        /// Returns URL of specified document.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(string), "Returns URL of the given document.", 1)]
        [MacroMethodParam(0, "documentId", typeof(int), "Document ID.")]
        public static object GetDocumentUrlById(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 1:
                    return TransformationHelper.HelperObject.GetDocumentUrl(parameters[0]);

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns URL of specified page.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(string), "Returns URL of the given page.", 2)]
        [MacroMethodParam(0, "nodeGuid", typeof(Guid), "Node GUID.")]
        [MacroMethodParam(1, "nodeAlias", typeof(string), "Node alias.")]
        public static object GetDocumentUrlByGUID(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 2:
                    return TransformationHelper.HelperObject.GetDocumentUrl(parameters[0], parameters[1]);

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns URL of specified document.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(string), "Returns URL of the given document.", 0, SpecialParameters = new[] { "SiteName", "NodeAliasPath", "DocumentUrlPath" })]
        public static object GetDocumentUrl(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 3:
                    return TransformationHelper.HelperObject.GetDocumentUrl(parameters[0], parameters[1], parameters[2]);

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Gets page CSS class comparing the current page alias path. Returns selectedCssClass if given alias path matches the current page alias path. Otherwise returns cssClass.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(string), "Gets page CSS class comparing the current page alias path. Returns selectedCssClass if given alias path matches the current page alias path. Otherwise returns cssClass.", 2, SpecialParameters = new[] { "NodeAliasPath" })]
        [MacroMethodParam(0, "cssClass", typeof(string), "CSS class.")]
        [MacroMethodParam(1, "selectedCssClass", typeof(string), "Selected CSS class.")]
        public static object GetDocumentCssClass(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 3:
                    return TransformationHelper.HelperObject.GetDocumentCssClass(ValidationHelper.GetString(parameters[0], ""), ValidationHelper.GetString(parameters[1], ""), ValidationHelper.GetString(parameters[2], ""));

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Indicates if the given page is the current page.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(bool), "Indicates if the given page is the current page.", 0, SpecialParameters = new[] { "NodeAliasPath" })]
        public static object IsCurrentDocument(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 1:
                    return TransformationHelper.HelperObject.IsCurrentDocument(ValidationHelper.GetString(parameters[0], ""));

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Indicates if the page is on selected path.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(bool), "Indicates if the page is on selected path.", 0, SpecialParameters = new[] { "NodeAliasPath" })]
        public static object IsDocumentOnSelectedPath(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 1:
                    return TransformationHelper.HelperObject.IsDocumentOnSelectedPath(ValidationHelper.GetString(parameters[0], ""));

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Indicates if the page is an image.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(bool), "Indicates if the page is an image.", 0, SpecialParameters = new[] { "DocumentType" })]
        public static object IsImageDocument(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 1:
                    return TransformationHelper.HelperObject.IsImageDocument(ValidationHelper.GetString(parameters[0], ""));

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns URL of the specified forum post.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(string), "Returns URL of the specified forum post.", 2)]
        [MacroMethodParam(0, "postIdPath", typeof(object), "Post id path.")]
        [MacroMethodParam(1, "forumId", typeof(object), "Forum id.")]
        public static object GetForumPostUrl(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 2:
                    return TransformationHelper.HelperObject.GetForumPostUrl(parameters[0], parameters[1]);

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns URL of the message board document.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(string), "Returns URL of the message board document.", 1)]
        [MacroMethodParam(0, "documentIdObj", typeof(object), "Document ID.")]
        public static object GetMessageBoardUrl(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 1:
                    return TransformationHelper.HelperObject.GetMessageBoardUrl(parameters[0]);

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns URL of the blog comment document.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(string), "Returns URL of the blog comment document.", 1)]
        [MacroMethodParam(0, "documentIdObj", typeof(object), "Document ID.")]
        public static object GetBlogCommentUrl(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 1:
                    return TransformationHelper.HelperObject.GetBlogCommentUrl(parameters[0]);

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns HTML markup representing file icon.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(string), "Gets icon representing file extension.", 1, SpecialParameters = new[] { "Page" })]
        [MacroMethodParam(0, "fileExtension", typeof(object), "File extension.")]
        [MacroMethodParam(1, "iconSize", typeof(FontIconSizeEnum), "Icon size.")]
        [MacroMethodParam(2, "tooltip", typeof(string), "Tooltip.")]
        [MacroMethodParam(3, "additionalAttributes", typeof(string), "Additional HTML attributes.")]
        public static object GetFileIcon(EvaluationContext context, params object[] parameters)
        {
            if ((parameters.Length >= 2) && (parameters.Length <= 5))
            {
                return TransformationHelper.HelperObject.GetFileIcon(parameters[1], (Page)parameters[0], (FontIconSizeEnum)ValidationHelper.GetInteger(parameters[2], (int)FontIconSizeEnum.NotDefined), ValidationHelper.GetString(parameters[3], ""), ValidationHelper.GetString(parameters[4], ""));
            }

            throw new NotSupportedException();
        }


        /// <summary>
        /// Returns font icon class for specified file extension.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(string), "Returns font icon class for specified file extension.", 1)]
        [MacroMethodParam(0, "extension", typeof(string), "File extension.")]
        public static object GetFileIconClass(EvaluationContext context, params object[] parameters)
        {
            if (parameters.Length == 1)
            {
                return TransformationHelper.HelperObject.GetFileIconClass(ValidationHelper.GetString(parameters[0], ""));
            }

            throw new NotSupportedException();
        }


        /// <summary>
        /// Returns HTML markup representing attachment icon.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(string), "Gets icon for representing attachment extension.", 1, SpecialParameters = new[] { "Page" })]
        [MacroMethodParam(0, "attachmentGuid", typeof(object), "Attachment GUID.")]
        [MacroMethodParam(1, "iconSize", typeof(FontIconSizeEnum), "Icon size.")]
        [MacroMethodParam(2, "tooltip", typeof(string), "Tooltip.")]
        [MacroMethodParam(3, "additionalAttributes", typeof(string), "Additional HTML attributes.")]
        public static object GetAttachmentIcon(EvaluationContext context, params object[] parameters)
        {
            if ((parameters.Length >= 2) && (parameters.Length <= 5))
            {
                return TransformationHelper.HelperObject.GetAttachmentIcon(parameters[1], (Page)parameters[0], (FontIconSizeEnum)ValidationHelper.GetInteger(parameters[2], (int)FontIconSizeEnum.NotDefined), ValidationHelper.GetString(parameters[3], ""), ValidationHelper.GetString(parameters[4], ""));
            }

            throw new NotSupportedException();
        }


        /// <summary>
        /// Returns a complete HTML code of the image that is specified by node alias and attachment GUID.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">
        /// (Node alias; Attachment GUID) OR 
        /// (Node alias; Attachment GUID; Image alternation text)
        /// (Node alias; Attachment GUID; Image alternation text; Image max. side size)
        /// (Node alias; Attachment GUID; Image alternation text; Image max. side size; Image width)
        /// (Node alias; Attachment GUID; Image alternation text; Image max. side size; Image width; Image height)
        /// </param>
        [MacroMethod(typeof(string), "Returns a complete HTML code of the image.", 1, SpecialParameters = new[] { "NodeAlias" })]
        [MacroMethodParam(0, "attachmentGuid", typeof(object), "Attachment GUID.")]
        [MacroMethodParam(1, "alt", typeof(object), "Image alternation text.")]
        [MacroMethodParam(2, "maxSideSize", typeof(object), "Image max. side size.")]
        [MacroMethodParam(3, "width", typeof(object), "Image width.")]
        [MacroMethodParam(4, "height", typeof(object), "Image height.")]
        public static object GetImage(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 2:
                    return TransformationHelper.HelperObject.GetImage(parameters[0], parameters[1], 0, 0, 0, "");

                case 3:
                    return TransformationHelper.HelperObject.GetImage(parameters[0], parameters[1], 0, 0, 0, parameters[2]);

                case 4:
                    return TransformationHelper.HelperObject.GetImage(parameters[0], parameters[1], ValidationHelper.GetInteger(parameters[3], 0), 0, 0, parameters[2]);

                case 5:
                    return TransformationHelper.HelperObject.GetImage(parameters[0], parameters[1], ValidationHelper.GetInteger(parameters[3], 0), ValidationHelper.GetInteger(parameters[4], 0), 0, parameters[2]);

                case 6:
                    return TransformationHelper.HelperObject.GetImage(parameters[0], parameters[1], ValidationHelper.GetInteger(parameters[3], 0), ValidationHelper.GetInteger(parameters[4], 0), ValidationHelper.GetInteger(parameters[5], 0), parameters[2]);

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns a complete HTML code of the image that is specified by the given url.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">
        /// (Image url) OR
        /// (Image url; Image alternation text)
        /// (Image url; Image alternation text; Image max. side size)
        /// (Image url; Image alternation text; Image max. side size; Image width)
        /// (Image url; Image alternation text; Image max. side size; Image width; Image height)
        /// </param>
        [MacroMethod(typeof(string), "Returns a complete HTML code of the image that is specified by the given url.", 1)]
        [MacroMethodParam(0, "imageUrl", typeof(object), "Image url.")]
        [MacroMethodParam(1, "alt", typeof(object), "Image alternation text.")]
        [MacroMethodParam(2, "maxSideSize", typeof(object), "Image max. side size.")]
        [MacroMethodParam(3, "width", typeof(object), "Image width.")]
        [MacroMethodParam(4, "height", typeof(object), "Image height.")]
        public static object GetImageByUrl(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 1:
                    return TransformationHelper.HelperObject.GetImageByUrl(parameters[0], 0, 0, 0, "");

                case 2:
                    return TransformationHelper.HelperObject.GetImageByUrl(parameters[0], 0, 0, 0, parameters[1]);

                case 3:
                    return TransformationHelper.HelperObject.GetImageByUrl(parameters[0], ValidationHelper.GetInteger(parameters[2], 0), 0, 0, parameters[1]);

                case 4:
                    return TransformationHelper.HelperObject.GetImageByUrl(parameters[0], ValidationHelper.GetInteger(parameters[2], 0), ValidationHelper.GetInteger(parameters[3], 0), 0, parameters[1]);

                case 5:
                    return TransformationHelper.HelperObject.GetImageByUrl(parameters[0], ValidationHelper.GetInteger(parameters[2], 0), ValidationHelper.GetInteger(parameters[3], 0), ValidationHelper.GetInteger(parameters[4], 0), parameters[1]);

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Gets UI image resolved path.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(string), "Returns a complete HTML code of the image that is specified by the given url.", 1)]
        [MacroMethodParam(0, "imageUrl", typeof(object), "Image url.")]
        [MacroMethodParam(1, "alt", typeof(object), "Image alternation text.")]
        [MacroMethodParam(2, "maxSideSize", typeof(object), "Image max. side size.")]
        [MacroMethodParam(3, "width", typeof(object), "Image width.")]
        [MacroMethodParam(4, "height", typeof(object), "Image height.")]
        public static string GetImageUrl(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 1:
                    return UIHelper.GetImageUrl(null, ValidationHelper.GetString(parameters[0], ""));
                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Gets the editable image value.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(string), "Gets the editable image value.", 1)]
        [MacroMethodParam(0, "url", typeof(string), "Editable image URL.")]
        public static object GetEditableImageUrl(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 1:
                    return TransformationHelper.HelperObject.GetEditableImageUrl(ValidationHelper.GetString(parameters[0], ""));

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns a complete HTML code of the image that is specified by editable image content.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">
        /// (Editable image url) OR
        /// (Editable image url; Image alternation text)
        /// (Editable image url; Image alternation text; Image max. side size)
        /// (Editable image url; Image alternation text; Image max. side size; Image width)
        /// (Editable image url; Image alternation text; Image max. side size; Image width; Image height)
        /// </param>
        [MacroMethod(typeof(string), "Returns a complete HTML code of the image that is specified by editable image ID.", 1)]
        [MacroMethodParam(0, "url", typeof(string), "Editable image URL.")]
        [MacroMethodParam(1, "alt", typeof(object), "Image alternation text.")]
        [MacroMethodParam(2, "maxSideSize", typeof(object), "Image max. side size.")]
        [MacroMethodParam(3, "width", typeof(object), "Image width.")]
        [MacroMethodParam(4, "height", typeof(object), "Image height.")]
        public static object GetEditableImage(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 1:
                    return TransformationHelper.HelperObject.GetEditableImage(ValidationHelper.GetString(parameters[0], ""), null, null, null, null);

                case 2:
                    return TransformationHelper.HelperObject.GetEditableImage(ValidationHelper.GetString(parameters[0], ""), null, null, null, parameters[1]);

                case 3:
                    return TransformationHelper.HelperObject.GetEditableImage(ValidationHelper.GetString(parameters[0], ""), parameters[2], null, null, parameters[1]);

                case 4:
                    return TransformationHelper.HelperObject.GetEditableImage(ValidationHelper.GetString(parameters[0], ""), parameters[2], parameters[3], null, parameters[1]);

                case 5:
                    return TransformationHelper.HelperObject.GetEditableImage(ValidationHelper.GetString(parameters[0], ""), parameters[2], parameters[3], parameters[4], parameters[1]);

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns a complete HTML code of the link to the currently rendered page.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(string), "Returns a complete HTML code of the link to the currently rendered page.", 0, SpecialParameters = new[] { "SiteName", "NodeAliasPath", "DocumentUrlPath", "DocumentName" })]
        [MacroMethodParam(0, "encodeName", typeof(bool), "If true, the page name is encoded.")]
        public static object GetDocumentLink(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 4:
                    return TransformationHelper.HelperObject.GetDocumentLink(parameters[0], parameters[1], parameters[2], parameters[3], true);

                case 5:
                    return TransformationHelper.HelperObject.GetDocumentLink(parameters[0], parameters[1], parameters[2], parameters[3], ValidationHelper.GetBoolean(parameters[4], true));

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns URL for the specified aliasPath and urlPath (preferable).
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(string), "Returns URL for the specified aliasPath and urlPath (preferable).", 2)]
        [MacroMethodParam(0, "aliasPath", typeof(object), "Alias path.")]
        [MacroMethodParam(1, "urlPath", typeof(object), "URL Path.")]
        [MacroMethodParam(2, "siteName", typeof(object), "Site name.")]
        public static object GetUrl(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 2:
                    return TransformationHelper.HelperObject.GetUrl(parameters[0], parameters[1]);

                case 3:
                    return TransformationHelper.HelperObject.GetUrl(parameters[0], parameters[1], parameters[2]);

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns resolved (i.e. absolute) URL of data item (page) which is currently being processed. Method reflects page navigation settings.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(string), "Returns resolved (i.e. absolute) URL of data item (page) which is currently being processed. Method reflects page navigation settings.", 0, SpecialParameters = new[] { "NodeSiteID", "NodeAliasPath", "DocumentCulture", "DocumentMenuRedirectUrl", "DocumentMenuRedirectToFirstChild", "DocumentUrlPath", "NodeLinkedNodeID" })]
        public static object GetNavigationUrl(EvaluationContext context, params object[] parameters)
        {
            var dc = new DataContainer();

            if (parameters.Length == 7)
            {
                dc.SetValue("NodeSiteID", parameters[0]);
                dc.SetValue("NodeAliasPath", parameters[1]);
                dc.SetValue("DocumentCulture", parameters[2]);
                dc.SetValue("DocumentMenuRedirectUrl", parameters[3]);
                dc.SetValue("DocumentMenuRedirectToFirstChild", parameters[4]);
                dc.SetValue("DocumentUrlPath", parameters[5]);
                dc.SetValue("NodeLinkedNodeID", parameters[6]);

                return TransformationHelper.HelperObject.GetNavigationUrl(dc, context.Resolver);
            }

            throw new NotSupportedException();
        }


        /// <summary>
        /// Trims the site prefix from user name(if any prefix found)
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(string), "Trims site prefix from user name.", 1)]
        [MacroMethodParam(0, "userName", typeof(string), "User name to trim.")]
        public static object TrimSitePrefix(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 1:
                    return TransformationHelper.HelperObject.TrimSitePrefix(parameters[0]);

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns date from the provided date-time value.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">
        /// (Date-time field name or value) OR
        /// (Date-time field name or value; Default value for invalid input)
        /// (Date-time field name or value; Default value for invalid input; Culture code)
        /// </param>
        [MacroMethod(typeof(string), "Returns date from the provided date-time value.", 1)]
        [MacroMethodParam(0, "dateTimeField", typeof(string), "Date-time field name.")]
        [MacroMethodParam(1, "defaultValue", typeof(string), "Default value for invalid date-time input. ")]
        [MacroMethodParam(2, "cultureCode", typeof(string), "Culture code.")]
        public static object GetDate(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 1:
                    return TransformationHelper.HelperObject.GetDate(parameters[0]);

                case 2:
                    return TransformationHelper.HelperObject.GetDate(parameters[0], ValidationHelper.GetString(parameters[1], String.Empty));

                case 3:
                    return TransformationHelper.HelperObject.GetDate(parameters[0], ValidationHelper.GetString(parameters[1], String.Empty), ValidationHelper.GetString(parameters[2], String.Empty));

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns absolute URL from relative path.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(string), "Returns absolute URL from relative path.", 1)]
        [MacroMethodParam(0, "relativeUrl", typeof(string), "Relative URL.")]
        public static object GetAbsoluteUrl(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 1:
                    return TransformationHelper.HelperObject.GetAbsoluteUrl(ValidationHelper.GetString(parameters[0], ""));

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns absolute URL from relative path.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(string), "Returns absolute URL from relative path.", 2)]
        [MacroMethodParam(0, "relativeUrl", typeof(string), "Relative URL.")]
        [MacroMethodParam(1, "siteId", typeof(int), "Site ID.")]
        public static object GetAbsoluteUrlBySiteID(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 2:
                    return TransformationHelper.HelperObject.GetAbsoluteUrl(ValidationHelper.GetString(parameters[0], null), ValidationHelper.GetInteger(parameters[1], 0));

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns absolute URL from relative path.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(string), "Returns absolute URL from relative path.", 2)]
        [MacroMethodParam(0, "relativeUrl", typeof(string), "Relative URL.")]
        [MacroMethodParam(1, "siteNameObj", typeof(object), "Site name.")]
        public static object GetAbsoluteUrlBySiteName(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 2:
                    return TransformationHelper.HelperObject.GetAbsoluteUrl(ValidationHelper.GetString(parameters[0], null), ValidationHelper.GetString(parameters[1], null));

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns nonEmptyResult if value is null or empty, else returns emptyResult.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(object), "Returns nonEmptyResult if value is null or empty, else returns emptyResult.", 3)]
        [MacroMethodParam(0, "value", typeof(object), "Conditional value.")]
        [MacroMethodParam(1, "emptyResult", typeof(object), "Empty value result.")]
        [MacroMethodParam(2, "nonEmptyResult", typeof(object), "Non empty value result.")]
        public static object IfEmpty(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 3:
                    return TransformationHelper.HelperObject.IfEmpty(parameters[0], parameters[1], parameters[2]);

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns nonEmptyResult if specified data source is null or empty, else returns emptyResult.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(object), "Returns nonEmptyResult if specified data source is null or empty, else returns emptyResult.", 3)]
        [MacroMethodParam(0, "value", typeof(object), "Conditional value.")]
        [MacroMethodParam(1, "emptyResult", typeof(object), "Empty value result.")]
        [MacroMethodParam(2, "nonEmptyResult", typeof(object), "Non empty value result.")]
        public static object IfDataSourceIsEmpty(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 3:
                    return TransformationHelper.HelperObject.IfDataSourceIsEmpty(parameters[0], parameters[1], parameters[2]);

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns isImage value if file is image.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(object), "Returns isImage value if file is image.", 3)]
        [MacroMethodParam(0, "attachmentGuid", typeof(object), "Attachment GUID.")]
        [MacroMethodParam(1, "isImage", typeof(object), "Is image value.")]
        [MacroMethodParam(2, "notImage", typeof(object), "Is not image value.")]
        public static object IfImage(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 3:
                    return TransformationHelper.HelperObject.IfImage(parameters[0], parameters[1], parameters[2]);

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Format date time.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(object), "Format date time.", 2)]
        [MacroMethodParam(0, "datetime", typeof(object), "Date time object.")]
        [MacroMethodParam(1, "format", typeof(string), "Format string (If not set, the date time is formated due to current culture settings.).")]
        public static object FormatDateTime(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 2:
                    return TransformationHelper.HelperObject.FormatDateTime(parameters[0], ValidationHelper.GetString(parameters[1], ""));

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Format date without time part.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(object), "Format date without time part.", 1)]
        [MacroMethodParam(0, "datetime", typeof(object), "Date time object.")]
        public static object FormatDate(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 1:
                    return TransformationHelper.HelperObject.FormatDate(parameters[0]);

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Transformation "if" statement for guid, int, string, double, decimal, boolean, DateTime.
        /// The type of compare depends on comparable value (second parameter).
        /// If both values are NULL, method returns false result.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(object), "Transformation \"if\" statement for guid, int, string, double, decimal, boolean, DateTime. The type of compare depends on comparable value (second parameter). If both values are NULL, method returns false result.", 4)]
        [MacroMethodParam(0, "value", typeof(object), "First value.")]
        [MacroMethodParam(1, "comparableValue", typeof(object), "Second value.")]
        [MacroMethodParam(2, "falseResult", typeof(object), "False result.")]
        [MacroMethodParam(3, "trueResult", typeof(object), "True result.")]
        public static object IfCompare(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 4:
                    return TransformationHelper.HelperObject.IfCompare(parameters[0], parameters[1], parameters[2], parameters[3]);

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns encoded text.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>        
        [MacroMethod(typeof(string), "Returns encoded text.", 1)]
        [MacroMethodParam(0, "text", typeof(string), "Text to be encoded.")]
        public static object HTMLEncode(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 1:
                    return TransformationHelper.HelperObject.HTMLEncode(ValidationHelper.GetString(parameters[0], ""));

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Remove all oc of discussion macros from text.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(string), "Remove all types of discussion macros from text.", 1)]
        [MacroMethodParam(0, "inputText", typeof(object), "Text containing macros to be removed.")]
        public static object RemoveDiscussionMacros(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 1:
                    return TransformationHelper.HelperObject.RemoveDiscussionMacros(parameters[0]);

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Remove all dynamic controls macros from text.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(string), "Remove all dynamic controls macros from text.", 1)]
        [MacroMethodParam(0, "inputText", typeof(object), "Text containing macros to be removed.")]
        public static object RemoveDynamicControls(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 1:
                    return TransformationHelper.HelperObject.RemoveDynamicControls(parameters[0]);

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Evaluates the item data and returns it.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(object), "Evaluates the item data and returns it.", 1)]
        [MacroMethodParam(0, "value", typeof(string), "Value to be evaluated.")]
        public static object Eval(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 1:
                    return parameters[0];

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Evaluates the item data and returns it.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(string), "Returns javascript encoded text.", 1)]
        [MacroMethodParam(0, "text", typeof(string), "Text to be encoded.")]
        public static object JSEncode(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 1:
                    return TransformationHelper.HelperObject.JSEncode(ValidationHelper.GetString(parameters[0], string.Empty));

                default:
                    throw new NotSupportedException();
            }
        }

        #endregion


        #region "UI image methods"

        /// <summary>
        /// Gets UI image resolved path.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">
        /// (Page object; Partial image path starting from ~/App_Themes/(Skin_Folder)/Images/) OR 
        /// (Page object; Partial image path starting from ~/App_Themes/(Skin_Folder)/Images/; Indicates if URL should be returned for live site) OR
        /// (Page object; Partial image path starting from ~/App_Themes/(Skin_Folder)/Images/; Indicates if URL should be returned for live site; Indicates if URL should be resolved)
        /// </param>
        [MacroMethod(typeof(string), "Gets UI image path.", 1, SpecialParameters = new[] { "Page" })]
        [MacroMethodParam(0, "imagePath", typeof(string), "Partial image path starting from ~/App_Themes/(Skin_Folder)/Images/ (e.g. '/CMSModules/CMS_MediaLibrary/module.png').")]
        [MacroMethodParam(1, "isLiveSite", typeof(bool), "Indicates if URL should be returned for live site.")]
        [MacroMethodParam(2, "ensureDefaultTheme", typeof(bool), "Indicates if default theme should be always used.")]
        public static string GetUIImageUrl(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 2:
                    return TransformationHelper.HelperObject.GetUIImageUrl(ValidationHelper.GetString(parameters[1], ""), (Page)parameters[0]);

                case 3:
                    return TransformationHelper.HelperObject.GetUIImageUrl(ValidationHelper.GetString(parameters[1], ""), ValidationHelper.GetBoolean(parameters[2], false), (Page)parameters[0]);

                case 4:
                    return TransformationHelper.HelperObject.GetUIImageUrl(ValidationHelper.GetString(parameters[1], ""), ValidationHelper.GetBoolean(parameters[2], false), ValidationHelper.GetBoolean(parameters[3], false), (Page)parameters[0]);

                default:
                    throw new NotSupportedException();
            }
        }

        #endregion


        #region "Time zones"

        /// <summary>
        /// Returns date time string according to user or current site time zone.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(DateTime), "Returns date time string according to user or current site time zone.", 2)]
        [MacroMethodParam(0, "dateTime", typeof(object), "Date time.")]
        [MacroMethodParam(1, "userName", typeof(string), "User name.")]
        public static string GetCurrentDateTimeString(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 2:
                    return TransformationHelper.HelperObject.GetCurrentDateTimeString(parameters[0], ValidationHelper.GetString(parameters[1], ""));

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns current user date time DateTime according to user time zone.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(DateTime), "Returns current user date time DateTime according to user time zone.", 1)]
        [MacroMethodParam(0, "dateTime", typeof(object), "Date time.")]
        public static object GetUserDateTime(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 1:
                    return TransformationHelper.HelperObject.GetUserDateTime(parameters[0]);

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns site date time according to site time zone.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(DateTime), "Returns site date time DateTime according to user time zone.", 1)]
        [MacroMethodParam(0, "dateTime", typeof(object), "Date time.")]
        public static object GetSiteDateTime(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 1:
                    return TransformationHelper.HelperObject.GetSiteDateTime(parameters[0]);

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns date time with dependence on selected time zone.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(DateTime), "Returns date time with dependence on selected time zone.", 2)]
        [MacroMethodParam(0, "dateTime", typeof(object), "DateTim to convert (server time zone).")]
        [MacroMethodParam(1, "timeZoneName", typeof(string), "Time zone code name.")]
        public static object GetCustomDateTime(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 2:
                    return TransformationHelper.HelperObject.GetCustomDateTime(parameters[0], ValidationHelper.GetString(parameters[1], ""));

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns date time with dependence on current ITimeZone manager time zone settings.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(DateTime), "Returns date time with dependence on current ITimeZone manager time zone settings.", 1, SpecialParameters = new[] { "control" })]
        [MacroMethodParam(0, "dateTime", typeof(object), "DateTime to convert (server time zone).")]
        [MacroMethodParam(1, "format", typeof(string), "Format string (if not set, the date time is formatted due to current culture settings).")]
        public static object GetDateTime(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 2:
                    return TransformationHelper.HelperObject.GetDateTime((Control)parameters[0], parameters[1]);

                case 3:
                    return TransformationHelper.HelperObject.FormatDateTime(TransformationHelper.HelperObject.GetDateTime((Control)parameters[0], parameters[1]), (string)parameters[2]);

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns string representation of date time with dependence on current ITimeZone manager
        /// time zone settings. 
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(string), "Returns string representation of date time with dependence on current ITimeZone managertime zone settings.", 2, SpecialParameters = new[] { "control" })]
        [MacroMethodParam(0, "dateTime", typeof(object), "DateTime to convert (server time zone).")]
        [MacroMethodParam(1, "showTooltip", typeof(bool), "Wraps date in span with tooltip.")]
        public static object GetDateTimeString(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 3:
                    return TransformationHelper.HelperObject.GetDateTimeString((Control)parameters[0], parameters[1], ValidationHelper.GetBoolean(parameters[2], false));

                default:
                    throw new NotSupportedException();
            }
        }

        #endregion


        #region "Community"

        /// <summary>
        /// Returns age according to DOB. If DOB is not set, returns unknownAge string.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(string), "Returns age according to DOB. If DOB is not set, returns unknownAge string.", 2)]
        [MacroMethodParam(0, "dateOfBirth", typeof(object), "Date of birth.")]
        [MacroMethodParam(1, "unknownAge", typeof(string), "Text which is returned when no DOB is given.")]
        public static object GetAge(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 2:
                    return TransformationHelper.HelperObject.GetAge(parameters[0], ValidationHelper.GetString(parameters[1], ""));

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns gender of the user.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(string), "Returns gender of the user.", 1)]
        [MacroMethodParam(0, "genderObj", typeof(object), "Gender of the user (0/1/2 = N/A / Male / Female).")]
        public static object GetGender(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 1:
                    return TransformationHelper.HelperObject.GetGender(parameters[0]);

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns group profile URL.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(string), "Returns group profile URL.", 1)]
        [MacroMethodParam(0, "groupNameObj", typeof(object), "Group name.")]
        [MacroMethod(typeof(string), "Returns group profile URL.", 2)]
        [MacroMethodParam(0, "groupNameObj", typeof(object), "Group name.")]
        [MacroMethodParam(1, "siteName", typeof(string), "Name of the site.")]
        public static object GetGroupProfileUrl(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 1:
                    return TransformationHelper.HelperObject.GetGroupProfileUrl(parameters[0]);

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns member profile URL.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(string), "Returns member profile URL.", 1)]
        [MacroMethodParam(0, "memberNameObj", typeof(object), "Member name.")]
        [MacroMethod(typeof(string), "Returns member profile URL.", 2)]
        [MacroMethodParam(0, "memberNameObj", typeof(object), "Member name.")]
        [MacroMethodParam(1, "siteName", typeof(string), "Name of the site.")]
        public static object GetMemberProfileUrl(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 1:
                    return TransformationHelper.HelperObject.GetMemberProfileUrl(parameters[0]);

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns user profile URL.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(string), "Returns user profile URL.", 1)]
        [MacroMethodParam(0, "userNameObj", typeof(object), "User name.")]
        [MacroMethod(typeof(string), "Returns user profile URL.", 2)]
        [MacroMethodParam(0, "userNameObj", typeof(object), "User name.")]
        [MacroMethodParam(1, "siteName", typeof(string), "Name of the site.")]
        public static object GetUserProfileURL(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 1:
                    return TransformationHelper.HelperObject.GetUserProfileURL(parameters[0]);

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns user avatar image.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">
        /// (Avatar ID; User ID, load gender avatar for specified user if avatar by avatar id doesn't exist) OR
        /// (Avatar ID; User ID, load gender avatar for specified user if avatar by avatar id doesn't exist; Image alternation text)
        /// (Avatar ID; User ID, load gender avatar for specified user if avatar by avatar id doesn't exist; Image alternation text; Image max. side size)
        /// (Avatar ID; User ID, load gender avatar for specified user if avatar by avatar id doesn't exist; Image alternation text; Image max. side size; Image width)
        /// (Avatar ID; User ID, load gender avatar for specified user if avatar by avatar id doesn't exist; Image alternation text; Image max. side size; Image width; Image height)
        /// (Avatar ID; User ID, load gender avatar for specified user if avatar by avatar id doesn't exist; Image alternation text; Image max. side size; Image width; Image height; User e-mail)
        /// </param>
        [MacroMethod(typeof(string), "Returns avatar image tag, if avatar is not defined returns gender depend avatar or user default avatar if is defined.", 2)]
        [MacroMethodParam(0, "avatarID", typeof(object), "Avatar ID.")]
        [MacroMethodParam(1, "userID", typeof(object), "User ID, load gender avatar for specified user if avatar by avatar id doesn't exist.")]
        [MacroMethodParam(2, "alt", typeof(object), "Image alternation text.")]
        [MacroMethodParam(3, "maxSideSize", typeof(object), "Image max. side size.")]
        [MacroMethodParam(4, "width", typeof(object), "Image width.")]
        [MacroMethodParam(5, "height", typeof(object), "Image height.")]
        public static object GetUserAvatarImage(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 2:
                    return TransformationHelper.HelperObject.GetUserAvatarImage(parameters[0], parameters[1], 0, 0, 0, null);

                case 3:
                    return TransformationHelper.HelperObject.GetUserAvatarImage(parameters[0], parameters[1], 0, 0, 0, parameters[2]);

                case 4:
                    return TransformationHelper.HelperObject.GetUserAvatarImage(parameters[0], parameters[1], ValidationHelper.GetInteger(parameters[3], 0), 0, 0, parameters[2]);

                case 5:
                    return TransformationHelper.HelperObject.GetUserAvatarImage(parameters[0], parameters[1], ValidationHelper.GetInteger(parameters[3], 0), ValidationHelper.GetInteger(parameters[4], 0), 0, parameters[2]);

                case 6:
                    return TransformationHelper.HelperObject.GetUserAvatarImage(parameters[0], parameters[1], ValidationHelper.GetInteger(parameters[3], 0), ValidationHelper.GetInteger(parameters[4], 0), ValidationHelper.GetInteger(parameters[5], 0), parameters[2]);


                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns user avatar image.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">
        /// (Avatar GUID; Avatar gender) OR
        /// (Avatar GUID; Avatar gender; Image alternation text)
        /// (Avatar GUID; Avatar gender; Image alternation text; Image max. side size)
        /// (Avatar GUID; Avatar gender; Image alternation text; Image max. side size; Image width)
        /// (Avatar GUID; Avatar gender; Image alternation text; Image max. side size; Image width; Image height)
        /// </param>
        [MacroMethod(typeof(string), "Returns user avatar image.", 2)]
        [MacroMethodParam(0, "avatarGuid", typeof(Guid), "Avatar GUID")]
        [MacroMethodParam(1, "avatarGender", typeof(int), "Avatar gender (1 = male, 2 = female).")]
        [MacroMethodParam(2, "alt", typeof(object), "Image alternation text.")]
        [MacroMethodParam(3, "maxSideSize", typeof(object), "Image max. side size.")]
        [MacroMethodParam(4, "width", typeof(object), "Image width.")]
        [MacroMethodParam(5, "height", typeof(object), "Image height.")]
        public static object GetUserAvatarImageByGUID(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 2:
                    return TransformationHelper.HelperObject.GetUserAvatarImageByGUID(parameters[0], parameters[1], 0, 0, 0, null);

                case 3:
                    return TransformationHelper.HelperObject.GetUserAvatarImageByGUID(parameters[0], parameters[1], 0, 0, 0, parameters[2]);

                case 4:
                    return TransformationHelper.HelperObject.GetUserAvatarImageByGUID(parameters[0], parameters[1], ValidationHelper.GetInteger(parameters[3], 0), 0, 0, parameters[2]);

                case 5:
                    return TransformationHelper.HelperObject.GetUserAvatarImageByGUID(parameters[0], parameters[1], ValidationHelper.GetInteger(parameters[3], 0), ValidationHelper.GetInteger(parameters[4], 0), 0, parameters[2]);

                case 6:
                    return TransformationHelper.HelperObject.GetUserAvatarImageByGUID(parameters[0], parameters[1], ValidationHelper.GetInteger(parameters[3], 0), ValidationHelper.GetInteger(parameters[4], 0), ValidationHelper.GetInteger(parameters[5], 0), parameters[2]);

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns user avatar image.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">
        /// (User ID, load gender avatar for specified user if avatar by avatar id doesn't exist) OR
        /// (User ID, load gender avatar for specified user if avatar by avatar id doesn't exist; Image alternation text)
        /// (User ID, load gender avatar for specified user if avatar by avatar id doesn't exist; Image alternation text; Image max. side size)
        /// (User ID, load gender avatar for specified user if avatar by avatar id doesn't exist; Image alternation text; Image max. side size; Image width)
        /// (User ID, load gender avatar for specified user if avatar by avatar id doesn't exist; Image alternation text; Image max. side size; Image width; Image height)
        /// </param>
        [MacroMethod(typeof(string), "Returns avatar image tag, if avatar is not defined returns gender depend avatar or user default avatar if is defined.", 1)]
        [MacroMethodParam(0, "userID", typeof(object), "User ID, load gender avatar for specified user if avatar by avatar id doesn't exist.")]
        [MacroMethodParam(1, "alt", typeof(object), "Image alternation text.")]
        [MacroMethodParam(2, "maxSideSize", typeof(object), "Image max. side size.")]
        [MacroMethodParam(3, "width", typeof(object), "Image width.")]
        [MacroMethodParam(4, "height", typeof(object), "Image height.")]
        public static object GetUserAvatarImageForUser(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 1:
                    return TransformationHelper.HelperObject.GetUserAvatarImageForUser(parameters[0], 0, 0, 0, null);

                case 2:
                    return TransformationHelper.HelperObject.GetUserAvatarImageForUser(parameters[0], 0, 0, 0, parameters[1]);

                case 3:
                    return TransformationHelper.HelperObject.GetUserAvatarImageForUser(parameters[0], ValidationHelper.GetInteger(parameters[2], 0), 0, 0, parameters[1]);

                case 4:
                    return TransformationHelper.HelperObject.GetUserAvatarImageForUser(parameters[0], ValidationHelper.GetInteger(parameters[2], 0), ValidationHelper.GetInteger(parameters[3], 0), 0, parameters[1]);

                case 5:
                    return TransformationHelper.HelperObject.GetUserAvatarImageForUser(parameters[0], ValidationHelper.GetInteger(parameters[2], 0), ValidationHelper.GetInteger(parameters[3], 0), ValidationHelper.GetInteger(parameters[4], 0), parameters[1]);

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns user avatar image URL.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">
        /// (Avatar ID; User ID, load gender avatar for specified user if avatar by avatar id doesn't exist) OR
        /// (Avatar ID; User ID, load gender avatar for specified user if avatar by avatar id doesn't exist; Image max. side size) OR
        /// (Avatar ID; User ID, load gender avatar for specified user if avatar by avatar id doesn't exist; Image max. side size; Image width) OR
        /// (Avatar ID; User ID, load gender avatar for specified user if avatar by avatar id doesn't exist; Image max. side size; Image width; Image height) OR
        /// (Avatar ID; User ID, load gender avatar for specified user if avatar by avatar id doesn't exist; Image max. side size; Image width; Image height; User e-mail) 
        /// </param>
        [MacroMethod(typeof(string), "Returns avatar image url, if avatar is not defined returns gender dependent avatar or user default avatar.", 2)]
        [MacroMethodParam(0, "avatarID", typeof(object), "Avatar ID.")]
        [MacroMethodParam(1, "userID", typeof(object), "User ID, load gender avatar for specified user if avatar by avatar id doesn't exist.")]
        [MacroMethodParam(2, "maxSideSize", typeof(object), "Image max. side size.")]
        [MacroMethodParam(3, "width", typeof(object), "Image width.")]
        [MacroMethodParam(4, "height", typeof(object), "Image height.")]
        [MacroMethodParam(5, "userEmail", typeof(object), "User e-mail.")]
        public static object GetUserAvatarImageUrl(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 2:
                    return TransformationHelper.HelperObject.GetUserAvatarImageUrl(parameters[0], parameters[1], 0, 0, 0);

                case 3:
                    return TransformationHelper.HelperObject.GetUserAvatarImageUrl(parameters[0], parameters[1], ValidationHelper.GetInteger(parameters[2], 0), 0, 0);

                case 4:
                    return TransformationHelper.HelperObject.GetUserAvatarImageUrl(parameters[0], parameters[1], ValidationHelper.GetInteger(parameters[2], 0), ValidationHelper.GetInteger(parameters[3], 0), 0);

                case 5:
                    return TransformationHelper.HelperObject.GetUserAvatarImageUrl(parameters[0], parameters[1], ValidationHelper.GetInteger(parameters[2], 0), ValidationHelper.GetInteger(parameters[3], 0), ValidationHelper.GetInteger(parameters[4], 0));

                case 6:
                    return TransformationHelper.HelperObject.GetUserAvatarImageUrl(parameters[0], parameters[1], ValidationHelper.GetString(parameters[5], String.Empty), ValidationHelper.GetInteger(parameters[2], 0), ValidationHelper.GetInteger(parameters[3], 0), ValidationHelper.GetInteger(parameters[4], 0));

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns group avatar image tag, if avatar is not defined returns default group if is defined.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">
        /// (Avatar GUIID, load gender avatar for specified user if avatar by Avatar GUIID doesn't exist) OR
        /// (Avatar GUIID, load gender avatar for specified user if avatar by Avatar GUIID doesn't exist; Image max. side size) OR
        /// (Avatar GUIID, load gender avatar for specified user if avatar by Avatar GUIID doesn't exist; Image max. side size; Image width) OR
        /// (Avatar GUIID, load gender avatar for specified user if avatar by Avatar GUIID doesn't exist; Image max. side size; Image width; Image height) OR
        /// </param>
        [MacroMethod(typeof(string), "Returns group avatar image tag, if avatar is not defined returns default group if is defined.", 1)]
        [MacroMethodParam(0, "avatarGuid", typeof(Guid), "Avatar GUID.")]
        [MacroMethodParam(1, "alt", typeof(object), "Image alternation text.")]
        [MacroMethodParam(2, "maxSideSize", typeof(object), "Image max. side size.")]
        [MacroMethodParam(3, "width", typeof(object), "Image width.")]
        [MacroMethodParam(4, "height", typeof(object), "Image height.")]
        public static object GetGroupAvatarImageByGUID(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 1:
                    return TransformationHelper.HelperObject.GetGroupAvatarImageByGUID(parameters[0], 0, 0, 0, null);

                case 2:
                    return TransformationHelper.HelperObject.GetGroupAvatarImageByGUID(parameters[0], 0, 0, 0, parameters[1]);

                case 3:
                    return TransformationHelper.HelperObject.GetGroupAvatarImageByGUID(parameters[0], ValidationHelper.GetInteger(parameters[2], 0), 0, 0, parameters[1]);

                case 4:
                    return TransformationHelper.HelperObject.GetGroupAvatarImageByGUID(parameters[0], ValidationHelper.GetInteger(parameters[2], 0), ValidationHelper.GetInteger(parameters[3], 0), 0, parameters[1]);

                case 5:
                    return TransformationHelper.HelperObject.GetGroupAvatarImageByGUID(parameters[0], ValidationHelper.GetInteger(parameters[2], 0), ValidationHelper.GetInteger(parameters[3], 0), ValidationHelper.GetInteger(parameters[4], 0), parameters[1]);

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns group avatar image tag, if avatar is not defined returns default group if is defined.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">
        /// (Avatar ID, load gender avatar for specified user if avatar by avatar id doesn't exist) OR
        /// (Avatar ID, load gender avatar for specified user if avatar by avatar id doesn't exist; Image max. side size) OR
        /// (Avatar ID, load gender avatar for specified user if avatar by avatar id doesn't exist; Image max. side size; Image width) OR
        /// (Avatar ID, load gender avatar for specified user if avatar by avatar id doesn't exist; Image max. side size; Image width; Image height) OR
        /// </param>
        [MacroMethod(typeof(string), "Returns group avatar image tag, if avatar is not defined returns default group if is defined.", 1)]
        [MacroMethodParam(0, "avatarID", typeof(object), "Avatar ID.")]
        [MacroMethodParam(1, "alt", typeof(object), "Image alternation text.")]
        [MacroMethodParam(2, "maxSideSize", typeof(object), "Image max. side size.")]
        [MacroMethodParam(3, "width", typeof(object), "Image width.")]
        [MacroMethodParam(4, "height", typeof(object), "Image height.")]
        public static object GetGroupAvatarImage(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 1:
                    return TransformationHelper.HelperObject.GetGroupAvatarImage(parameters[0], 0, 0, 0, null);

                case 2:
                    return TransformationHelper.HelperObject.GetGroupAvatarImage(parameters[0], 0, 0, 0, parameters[1]);

                case 3:
                    return TransformationHelper.HelperObject.GetGroupAvatarImage(parameters[0], ValidationHelper.GetInteger(parameters[2], 0), 0, 0, parameters[1]);

                case 4:
                    return TransformationHelper.HelperObject.GetGroupAvatarImage(parameters[0], ValidationHelper.GetInteger(parameters[2], 0), ValidationHelper.GetInteger(parameters[3], 0), 0, parameters[1]);

                case 5:
                    return TransformationHelper.HelperObject.GetGroupAvatarImage(parameters[0], ValidationHelper.GetInteger(parameters[2], 0), ValidationHelper.GetInteger(parameters[3], 0), ValidationHelper.GetInteger(parameters[4], 0), parameters[1]);

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns badge image tag.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(string), "Returns badge image tag.", 1)]
        [MacroMethodParam(0, "badgeId", typeof(int), "Badge ID.")]
        public static object GetBadgeImage(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 1:
                    return TransformationHelper.HelperObject.GetBadgeImage(ValidationHelper.GetInteger(parameters[0], 0));

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns badge name.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(string), "Returns badge name.", 1)]
        [MacroMethodParam(0, "badgeId", typeof(int), "Badge ID.")]
        public static object GetBadgeName(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 1:
                    return TransformationHelper.HelperObject.GetBadgeName(ValidationHelper.GetInteger(parameters[0], 0));

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns user full name.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(string), "Returns user full name.", 1)]
        [MacroMethodParam(0, "userId", typeof(int), "User ID.")]
        public static object GetUserFullName(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 1:
                    return TransformationHelper.HelperObject.GetUserFullName(ValidationHelper.GetInteger(parameters[0], 0));

                default:
                    throw new NotSupportedException();
            }
        }

        #endregion


        #region "SmartSearch"

        /// <summary>
        /// Returns URL to current search result item.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(string), "Returns URL to current search result item.", 2, SpecialParameters = new[] { "id", "type", "image" })]
        [MacroMethodParam(0, "noImageUrl", typeof(string), "URL to image which should be displayed if image is not defined.")]
        [MacroMethodParam(1, "maxSideSize", typeof(int), "Max. side size.")]
        public static object GetSearchImageUrl(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 5:
                    return TransformationHelper.HelperObject.GetSearchImageUrl(ValidationHelper.GetString(parameters[0], ""), ValidationHelper.GetString(parameters[1], ""), ValidationHelper.GetString(parameters[2], ""), ValidationHelper.GetString(parameters[3], ""), ValidationHelper.GetInteger(parameters[4], 0));

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Highlight input text with dependence on current search keywords.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(string), "Highlight input text with dependence on current search keywords.", 3)]
        [MacroMethodParam(0, "text", typeof(string), "Input text.")]
        [MacroMethodParam(1, "startTag", typeof(string), "Start highlight tag.")]
        [MacroMethodParam(2, "endTag", typeof(string), "End tag.")]
        public static object SearchHighlight(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 3:
                    return TransformationHelper.HelperObject.SearchHighlight(ValidationHelper.GetString(parameters[0], ""), ValidationHelper.GetString(parameters[1], ""), ValidationHelper.GetString(parameters[2], ""));

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns content parsed as XML if required and removes dynamic controls.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(string), "Returns content parsed as XML if required and removes dynamic controls.", 1, SpecialParameters = new[] { "id", "type" })]
        [MacroMethodParam(0, "content", typeof(string), "Content.")]
        public static object GetSearchedContent(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 3:
                    return TransformationHelper.HelperObject.GetSearchedContent(ValidationHelper.GetString(parameters[0], ""), ValidationHelper.GetString(parameters[1], ""), ValidationHelper.GetString(parameters[2], ""));

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns column value for current search result item.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(object), "Returns column value for current search result item.", 1, SpecialParameters = new[] { "id" })]
        [MacroMethodParam(0, "columnName", typeof(string), "Column name.")]
        public static object GetSearchValue(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 2:
                    return TransformationHelper.HelperObject.GetSearchValue(ValidationHelper.GetString(parameters[0], ""), ValidationHelper.GetString(parameters[1], ""));

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns URL for current search result.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(string), "Returns URL for current search result.", 0, SpecialParameters = new[] { "id", "type" })]
        [MacroMethodParam(0, "absolute", typeof(bool), "Indicates whether generated url should be absolute. False by default.")]
        [MacroMethodParam(1, "addLangParameter", typeof(bool), "Adds culture specific query parameter to the URL if more than one culture version exists. True is default value.")]
        public static object SearchResultUrl(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 2:
                    return TransformationHelper.HelperObject.SearchResultUrl(ValidationHelper.GetString(parameters[0], ""), ValidationHelper.GetString(parameters[1], ""), false);
                case 3:
                    return TransformationHelper.HelperObject.SearchResultUrl(ValidationHelper.GetString(parameters[0], ""), ValidationHelper.GetString(parameters[1], ""), ValidationHelper.GetBoolean(parameters[2], false));
                case 4:
                    return TransformationHelper.HelperObject.SearchResultUrl(ValidationHelper.GetString(parameters[0], ""), ValidationHelper.GetString(parameters[1], ""), ValidationHelper.GetBoolean(parameters[2], false), ValidationHelper.GetBoolean(parameters[3], true));

                default:
                    throw new NotSupportedException();
            }
        }

        #endregion


        #region "Syndication methods"

        /// <summary>
        /// Evaluates the item data and return escaped CDATA.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(object), "Evaluates the item data and returns escaped CDATA.", 1)]
        [MacroMethodParam(0, "value", typeof(string), "Value to be encapsulated.")]
        [MacroMethodParam(1, "encapsulate", typeof(bool), "Indicates if resulting string will be encapsulated in CDATA section.")]
        public static object EvalCDATA(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 1:
                    return TransformationHelper.HelperObject.EvalCDATA(parameters[0], true);

                case 2:
                    return TransformationHelper.HelperObject.EvalCDATA(parameters[0], ValidationHelper.GetBoolean(parameters[1], true));

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns URL of the currently rendered document with feed parameter.
        /// </summary>
        /// <param name="context">Returns URL of the currently rendered document with feed parameter</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(object), "Evaluates the item data and returns escaped CDATA.", 1)]
        [MacroMethodParam(0, "feedName", typeof(string), "Name of the feed.")]
        [MacroMethodParam(1, "instanceGuid", typeof(bool), "Instance GUID.")]
        [MacroMethodParam(2, "url", typeof(bool), "Base url.")]
        public static object GetDocumentUrlForFeed(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 3:
                    return TransformationHelper.HelperObject.GetDocumentUrlForFeed(TransformationHelper.HelperObject.GetFeedName(parameters[0], parameters[1]), ValidationHelper.GetString(parameters[2], ""));

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns URL of the specified forum post with feed parameter.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(string), "Returns URL of the specified forum post with feed parameter.", 2, SpecialParameters = new[] { "FeedName", "InstanceGUID" })]
        [MacroMethodParam(0, "postIdPath", typeof(object), "Post id path.")]
        [MacroMethodParam(1, "forumId", typeof(object), "Forum id.")]
        public static object GetForumPostUrlForFeed(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 4:
                    return TransformationHelper.HelperObject.GetForumPostUrlForFeed(TransformationHelper.HelperObject.GetFeedName(parameters[0], parameters[1]), parameters[2], parameters[3]);

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns URL of the specified media file with feed parameter.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(string), "Returns URL of the specified media file with feed parameter.", 2, SpecialParameters = new[] { "FeedName", "InstanceGUID" })]
        [MacroMethodParam(0, "fileGUID", typeof(object), "File GUID.")]
        [MacroMethodParam(1, "fileName", typeof(object), "File name.")]
        public static object GetMediaFileUrlForFeed(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 4:
                    return TransformationHelper.HelperObject.GetMediaFileUrlForFeed(TransformationHelper.HelperObject.GetFeedName(parameters[0], parameters[1]), parameters[2], parameters[3]);

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns URL of the message board page with feed parameter.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(string), "Returns URL of the message board page with feed parameter.", 1, SpecialParameters = new[] { "FeedName", "InstanceGUID" })]
        [MacroMethodParam(0, "documentIdObj", typeof(object), "Document ID.")]
        public static object GetMessageBoardUrlForFeed(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 3:
                    return TransformationHelper.HelperObject.GetMessageBoardUrlForFeed(TransformationHelper.HelperObject.GetFeedName(parameters[0], parameters[1]), parameters[2]);

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Returns URL of the blog comment page with feed parameter.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(string), "Returns URL of the blog comment page with feed parameter.", 2, SpecialParameters = new[] { "FeedName", "InstanceGUID" })]
        [MacroMethodParam(0, "documentIdObj", typeof(object), "Document ID.")]
        public static object GetBlogCommentUrlForFeed(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 3:
                    return TransformationHelper.HelperObject.GetBlogCommentUrlForFeed(TransformationHelper.HelperObject.GetFeedName(parameters[0], parameters[1]), parameters[2]);

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Gets time according to RFC 3339 for Atom feeds.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        /// <returns>Formated date</returns>
        [MacroMethod(typeof(string), "Gets time according to RFC 3339 for Atom feeds.", 1, SpecialParameters = new[] { "control" })]
        [MacroMethodParam(0, "dateTime", typeof(object), "DateTime object to format.")]
        public static object GetAtomDateTime(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 2:
                    return TransformationHelper.HelperObject.GetAtomDateTime((Control)parameters[0], parameters[1]);

                default:
                    throw new NotSupportedException();
            }
        }


        /// <summary>
        /// Gets time according to RFC 822 for RSS feeds.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        /// <returns>Formated date</returns>
        [MacroMethod(typeof(string), "Gets time according to RFC 822 for RSS feeds.", 1, SpecialParameters = new[] { "control" })]
        [MacroMethodParam(0, "dateTime", typeof(object), "DateTime object to format.")]
        public static object GetRSSDateTime(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 2:
                    return TransformationHelper.HelperObject.GetAtomDateTime((Control)parameters[0], parameters[1]);

                default:
                    throw new NotSupportedException();
            }
        }

        #endregion


        #region "Categories"

        /// <summary>
        /// Appends current category ID to given url.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(string), "Appends categoryId parameter to given URL.", 1)]
        [MacroMethodParam(0, "url", typeof(object), "URL.")]
        public static object AddCurrentCategoryParameter(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 1:
                    return TransformationHelper.HelperObject.AddCurrentCategoryParameter(parameters[0]);

                default:
                    throw new NotSupportedException();
            }
        }

        #endregion
    }
}