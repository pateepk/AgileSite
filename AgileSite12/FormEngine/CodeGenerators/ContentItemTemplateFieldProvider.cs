using System;
using System.Collections.Generic;
using System.Linq;

using CMS.DataEngine;
using CMS.DataEngine.CollectionExtensions;

namespace CMS.FormEngine
{
    /// <summary>
    /// Field provider for code generation template.
    /// </summary>
    internal class ContentItemTemplateFieldProvider : IContentItemTemplateFieldProvider
    {
        private readonly FormInfo mForm;
        private readonly Type mBaseType;


        /// <summary>
        /// Creates a new instance of <see cref="ContentItemTemplateFieldProvider"/>.
        /// </summary>
        /// <param name="form">Form definition containing all fields.</param>
        /// <param name="baseType">Type from which the generated class is derived.</param>
        public ContentItemTemplateFieldProvider(FormInfo form, Type baseType)
        {
            mForm = form;
            mBaseType = baseType;
        }


        /// <summary>
        /// Returns all fields which will be generated in code template.
        /// </summary>
        IEnumerable<FormFieldInfo> IContentItemTemplateFieldProvider.GetFields()
        {
            var fields = mForm.GetFields<FormFieldInfo>();

            // For custom tables or forms do not create primary key field, as this field is already included in base class.
            if (mBaseType.Name.Equals("CustomTableItem", StringComparison.InvariantCultureIgnoreCase) || mBaseType.Name.Equals("BizFormItem", StringComparison.InvariantCultureIgnoreCase))
            {
                fields = fields.Where(x => !x.PrimaryKey);
            }

            // System field inheritance has to be considered
            fields = fields.Where(i => !IsSystemField(i));

            return fields;
        }


        private bool IsSystemField(FormFieldInfo formField)
        {
            if (formField.System)
            {
                return true;
            }

            var types = ObjectTypeManager.GetTypeInfos(mBaseType);

            return types.Any(i => IsSystemField(formField, DataClassInfoProvider.GetDataClassInfo(i.ObjectClassName)));
        }


        private bool IsSystemField(FormFieldInfo formField, DataClassInfo dataClass)
        {
            if (dataClass == null)
            {
                return false;
            }

            var form = new FormInfo(dataClass.ClassFormDefinition);
            var systemFields = form.GetFields<FormFieldInfo>()
                                   .Where(x => x.System)
                                   .Select(i => i.Name)
                                   .ToHashSetCollection(StringComparer.OrdinalIgnoreCase);

            if (systemFields.Contains(formField.Name))
            {
                return true;
            }

            return IsSystemField(formField, DataClassInfoProvider.GetDataClassInfo(dataClass.ClassInheritsFromClassID));
        }
    }
}
