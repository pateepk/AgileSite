using System;

using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.DocumentEngine.Internal;
using CMS.Tests;

namespace Tests.DocumentEngine
{
    /// <summary>
    /// Provides support for faking documents within automated tests
    /// </summary>
    public static class IFakeMethodsExtensions
    {
        /// <summary>
        /// Class ID used for faking document type classes
        /// </summary>
        private static int mDocTypeClassId = 10000;


        /// <summary>
        /// Fakes the given document type
        /// </summary>
        /// <param name="fake">Fake methods</param>
        /// <param name="className">Class name</param>
        /// <param name="setup">Setup action for further fields of the document type</param>
        public static void DocumentType<T>(this IFakeMethods fake, string className, Action<DataClassInfo> setup = null)
            where T : TreeNode, new()
        {
            fake.Info<TreeNode>();

            // Fake data class info
            fake.InfoProvider<DataClassInfo, DataClassInfoProvider>().IncludeData(
                CreateFakeDocumentTypeInfo<T>(fake, className, setup)
            );

            // Fake coupled data
            var objectType = DocumentFieldsInfoProvider.GetObjectType(className);
            var coupledSettings = new InfoFakeSettings(typeof(T))
            {
                ObjectType = objectType,
                IncludeInheritedFields = false
            };

            var coupledFake = fake.Info<T>(coupledSettings);

            // Replicate the coupled structure to document fields
            var coupledTypeInfo = DocumentFieldsInfoProvider.GetTypeInfo(className);

            // Update ID column information as ID column was already cached in the instance before faking
            coupledTypeInfo.IDColumn = coupledFake.ClassStructureInfo.IDColumn;
        }


        /// <summary>
        /// Creates a fake document type info for faking of the documents
        /// </summary>
        /// <param name="fake">Fake methods</param>
        /// <param name="className">Class name</param>
        /// <param name="setup">Setup action for further fields of the document type</param>
        private static DataClassInfo CreateFakeDocumentTypeInfo<T>(this IFakeMethods fake, string className, Action<DataClassInfo> setup = null)
        {
            var dc = DataClassInfo.New(c =>
            {
                var classId = ++mDocTypeClassId;

                c.ClassID = classId;
                c.ClassGUID = new Guid(String.Format("00000000-0000-0000-0000-{0}", classId.ToString().PadLeft(12, '0')));
                c.ClassName = className;
                c.ClassIsDocumentType = true;
                c.ClassIsCoupledClass = true;
                c.ClassXmlSchema = fake.GetClassXmlSchema<T>();
            });

            setup?.Invoke(dc);

            return dc;
        }
    }
}
