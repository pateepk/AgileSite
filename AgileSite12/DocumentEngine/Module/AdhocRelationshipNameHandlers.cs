using System;

using CMS.DataEngine;
using CMS.FormEngine;
using CMS.Relationships;
using CMS.Base;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Provides handlers for managing ad-hoc relationship names of documents
    /// </summary>
    internal static class AdhocRelationshipNameHandlers
    {
        /// <summary>
        /// Initializes the handlers
        /// </summary>
        public static void Init()
        {
            var docTypeEvents = DocumentTypeInfo.TYPEINFODOCUMENTTYPE.Events;
            docTypeEvents.Delete.Before += DeleteRelationshipNames;
            docTypeEvents.Update.Before += UpdateRelationshipNames;

            DataDefinitionItemEvents.RemoveItem.Before += DeleteRelationshipName;
            DataDefinitionItemEvents.ChangeItem.After += ChangeRelationshipName;
            DataDefinitionItemEvents.AddItem.After += EnsureRelationshipName;
        }


        private static void EnsureRelationshipName(object sender, DataDefinitionItemEventArgs e)
        {
            var classInfo = e.ClassInfo;
            if ((classInfo == null) || !classInfo.ClassIsDocumentType)
            {
                return;
            }

            var field = (FormFieldInfo)e.Item;
            if (field == null)
            {
                return;
            }

            if (field.DataType.EqualsCSafe(DocumentFieldDataType.DocRelationships))
            {
                RelationshipNameInfoProvider.EnsureAdHocRelationshipNameInfo(classInfo, field);
            }
        }


        private static void DeleteRelationshipName(object sender, DataDefinitionItemEventArgs e)
        {
            var classInfo = e.ClassInfo;
            if ((classInfo == null) || !classInfo.ClassIsDocumentType)
            {
                return;
            }

            var field = (FormFieldInfo)e.Item;
            if (field == null)
            {
                return;
            }

            DeleteFieldRelationshipName(classInfo, field);
        }


        private static void ChangeRelationshipName(object sender, DataDefinitionItemChangeEventArgs e)
        {
            var classInfo = e.ClassInfo;
            if ((classInfo == null) || !classInfo.ClassIsDocumentType)
            {
                return;
            }

            var originalField = (FormFieldInfo)e.OriginalItem;
            var field = (FormFieldInfo)e.Item;
            if ((originalField == null) || (field == null))
            {
                return;
            }

            HandleChangedFieldRelationshipName(classInfo, originalField, field);
        }


        private static void HandleChangedFieldRelationshipName(DataClassInfo classInfo, FormFieldInfo originalField, FormFieldInfo field)
        {
            var originalIsRelationship = originalField.DataType.EqualsCSafe(DocumentFieldDataType.DocRelationships);
            var fieldIsRelationship = field.DataType.EqualsCSafe(DocumentFieldDataType.DocRelationships);
            var fieldName = !String.IsNullOrEmpty(field.Caption) ? field.Caption : field.Name;
            var originalFieldName = !String.IsNullOrEmpty(originalField.Caption) ? originalField.Caption : originalField.Name;

            if (originalIsRelationship && !fieldIsRelationship)
            {
                DeleteFieldRelationshipNameInternal(classInfo, field);
            }
            else if (!originalIsRelationship && fieldIsRelationship)
            {
                RelationshipNameInfoProvider.EnsureAdHocRelationshipNameInfo(classInfo, field);
            }
            else if (!originalFieldName.EqualsCSafe(fieldName, true))
            {
                var relationshipName = RelationshipNameInfoProvider.GetRelationshipNameInfo(RelationshipNameInfoProvider.GetAdHocRelationshipNameCodeName(classInfo.ClassName, originalField));
                if (relationshipName == null)
                {
                    return;
                }

                relationshipName.RelationshipDisplayName = RelationshipNameInfoProvider.GetAdHocRelationshipNameDisplayName(classInfo.ClassDisplayName, field);
                relationshipName.Update();
            }
        }


        private static void DeleteFieldRelationshipName(DataClassInfo classInfo, FormFieldInfo field)
        {
            if (!field.DataType.EqualsCSafe(DocumentFieldDataType.DocRelationships))
            {
                return;
            }

            DeleteFieldRelationshipNameInternal(classInfo, field);
        }


        private static void DeleteFieldRelationshipNameInternal(DataClassInfo classInfo, FormFieldInfo field)
        {
            var codeName = RelationshipNameInfoProvider.GetAdHocRelationshipNameCodeName(classInfo.ClassName, field);
            var relationshipName = RelationshipNameInfoProvider.GetRelationshipNameInfo(codeName);
            if ((relationshipName == null) || !relationshipName.RelationshipNameIsAdHoc)
            {
                return;
            }

            RelationshipNameInfoProvider.DeleteRelationshipName(relationshipName);
        }


        private static void UpdateFieldRelationshipNameInternal(string originalClassName, DataClassInfo classInfo, FormFieldInfo field)
        {
            var relationshipName = RelationshipNameInfoProvider.GetRelationshipNameInfo(RelationshipNameInfoProvider.GetAdHocRelationshipNameCodeName(originalClassName, field));
            if (relationshipName == null)
            {
                return;
            }

            relationshipName.RelationshipName = RelationshipNameInfoProvider.GetAdHocRelationshipNameCodeName(classInfo.ClassName, field);
            relationshipName.RelationshipDisplayName = RelationshipNameInfoProvider.GetAdHocRelationshipNameDisplayName(classInfo.ClassDisplayName, field);
            relationshipName.Update();
        }


        private static void DeleteRelationshipNames(object sender, ObjectEventArgs e)
        {
            var classInfo = (DataClassInfo)e.Object;
            var formInfo = FormHelper.GetFormInfo(classInfo.ClassName, false);
            var fields = formInfo.GetFields(DocumentFieldDataType.DocRelationships);
            fields.ForEach(f => DeleteFieldRelationshipNameInternal(classInfo, f));
        }


        private static void UpdateRelationshipNames(object sender, ObjectEventArgs e)
        {
            var originalClassInfo = (DataClassInfo)e.Object;
            var originalClassName = originalClassInfo.GetOriginalValue("ClassName").ToString("");
            var originalClassDisplayName = originalClassInfo.GetOriginalValue("ClassDisplayName").ToString("");
            e.CallWhenFinished(
                () =>
                {
                    var classInfo = (DataClassInfo)e.Object;
                    if (originalClassName.EqualsCSafe(classInfo.ClassName, true) && 
                        originalClassDisplayName.EqualsCSafe(classInfo.ClassDisplayName, true))
                    {
                        return;
                    }

                    var formInfo = FormHelper.GetFormInfo(classInfo.ClassName, false);
                    var fields = formInfo.GetFields(DocumentFieldDataType.DocRelationships);
                    fields.ForEach(f => UpdateFieldRelationshipNameInternal(originalClassName, classInfo, f));
                });
        }
    }
}