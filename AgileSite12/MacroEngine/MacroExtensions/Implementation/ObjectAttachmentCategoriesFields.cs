using System;
using System.Linq;
using System.Text;

using CMS.DataEngine;

namespace CMS.MacroEngine
{
    /// <summary>
    /// Extends "ObjectAttachment" macro namespace, so object attachment (metafiles) category names are accessible in macros.
    /// </summary>
    internal class ObjectAttachmentCategoriesFields : MacroFieldContainer
    {
        /// <summary>
        /// Registers fields.
        /// </summary>
        protected override void RegisterFields()
        {
            base.RegisterFields();

            Action<string, string> reg = (name, value) => RegisterField(new MacroField(name, () => value));

            reg("Layout", ObjectAttachmentsCategories.ATTACHMENT);
            reg("EProduct", ObjectAttachmentsCategories.EPRODUCT);
            reg("FormLayout", ObjectAttachmentsCategories.FORMLAYOUT);
            reg("Icon", ObjectAttachmentsCategories.ICON);
            reg("Image", ObjectAttachmentsCategories.IMAGE);
            reg("Issue", ObjectAttachmentsCategories.ISSUE);
            reg("Layout", ObjectAttachmentsCategories.LAYOUT);
            reg("Template", ObjectAttachmentsCategories.TEMPLATE);
            reg("Thumbnail", ObjectAttachmentsCategories.THUMBNAIL);
        }
    }
}
