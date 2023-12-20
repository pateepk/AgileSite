using System;
using System.Collections.Specialized;
using System.Globalization;
using System.Text;

using CMS.DocumentEngine;

namespace Kentico.Content.Web.Mvc
{
    /// <summary>
    /// Provides extension methods for the <see cref="AttachmentExtensions"/> class.
    /// </summary>
    [Obsolete("Use CMS.DocumentEngine.DocumentAttachment instead.")]
    public static class AttachmentExtensions
    {
        /// <summary>
        /// Returns relative path for the attachment.
        /// </summary>
        /// <param name="attachment">The attachment.</param>
        /// <param name="variant">Identifier of the attachmet variant.</param>
        public static string GetPath(this Attachment attachment, string variant = "")
        {
            if (attachment == null)
            {
                throw new ArgumentNullException(nameof(attachment));
            }

            var variantQueryParameter = GetVariantQueryParameter(variant);

            var fileName = GetFileName(attachment);
            var url = $"~/getattachment/{attachment.GUID:D}/{GetFileNameForUrl(fileName)}{variantQueryParameter?.ToQueryString()}";

            return url;
        }


        private static NameValueCollection GetVariantQueryParameter(string variant)
        {
            if (string.IsNullOrEmpty(variant))
            {
                return null;
            }
           
            return new NameValueCollection
            {
                { "variant", variant }
            };
        }


        private static string GetFileName(Attachment attachment)
        {
            if (!String.IsNullOrEmpty(attachment.Name))
            {
                return attachment.Name;
            }

            return "attachment";
        }


        private static string GetFileNameForUrl(string fileName)
        {
            if (fileName == null)
            {
                throw new ArgumentNullException(nameof(fileName));
            }

            var builder = new StringBuilder();

            // Remove characters that prevent normalization and use compatibility normalization to decompose certain code points, e.g. ligatures into the constituent letters.
            fileName = RemoveCharacters(fileName, UnicodeCategory.OtherNotAssigned, builder).Normalize(NormalizationForm.FormKD);

            // Remove characters that modify base characters, such as accents, and recompose.
            fileName = RemoveCharacters(fileName, UnicodeCategory.NonSpacingMark, builder).Normalize(NormalizationForm.FormKC);

            builder.Clear();
            var separatorFlag = false;

            foreach (var character in fileName)
            {
                if (((character >= 'a') && (character <= 'z')) || ((character >= '0') && (character <= '9')) || (character == '.') || (character == '_'))
                {
                    builder.Append(character);
                    separatorFlag = false;
                }
                else if ((character >= 'A') && (character <= 'Z'))
                {
                    builder.Append(Char.ToLowerInvariant(character));
                    separatorFlag = false;
                }
                else
                {
                    if (!separatorFlag && (builder.Length > 0))
                    {
                        builder.Append('-');
                        separatorFlag = true;
                    }
                }
            }

            // Remove the ending hyphen, if present.
            if (separatorFlag)
            {
                builder.Length = builder.Length - 1;
            }

            return builder.ToString();
        }


        private static string RemoveCharacters(string text, UnicodeCategory category, StringBuilder builder)
        {
            builder.Clear();
            foreach (var character in text)
            {
                if (Char.GetUnicodeCategory(character) != category)
                {
                    builder.Append(character);
                }
            }

            return builder.ToString();
        }
    }
}
