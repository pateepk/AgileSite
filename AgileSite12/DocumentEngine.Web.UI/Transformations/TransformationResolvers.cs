using System;

using CMS.MacroEngine;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.CustomTables;


namespace CMS.DocumentEngine.Web.UI
{
    /// <summary>
    /// Transformation resolver used for macro transformations
    /// </summary>
    internal class TransformationResolvers : ResolverDefinition
    {
        /// <summary>
        /// Returns a resolver based on given name.
        /// </summary>
        /// <param name="name">Name of the resolver</param>
        public static MacroResolver GetResolver(string name)
        {
            MacroResolver resolver = null;
            if ((name != null) && name.StartsWith("transformation.", StringComparison.OrdinalIgnoreCase))
            {
                // transformation.{classid}.{isascx}
                string[] items = name.Split('.');
                int classId = ValidationHelper.GetInteger(items[1], 0);
                bool isAscx = ValidationHelper.GetBoolean(items[2], false);

                resolver = GetTransformationResolver(classId, isAscx);
            }

            return resolver;
        }


        /// <summary>
        /// Returns transformation resolver for specific class
        /// </summary>
        /// <param name="classId">Class ID</param>
        /// <param name="isAscx">Indicates whether ASCX transformation is used</param>
        public static MacroResolver GetTransformationResolver(int classId, bool isAscx)
        {
            var resolver = MacroResolver.GetInstance();

            resolver.SetNamedSourceData("DataItemCount", "");
            resolver.SetNamedSourceData("DataItemIndex", "");
            resolver.SetNamedSourceData("DisplayIndex", "");
            resolver.SetNamedSourceData("DataItem", "");

            var resolverClassInfo = DataClassInfoProvider.GetDataClassInfo(classId);
            if (resolverClassInfo == null)
            {
                return resolver;
            }

            var baseInfo = GetEmptyObject(resolverClassInfo);

            resolver.SetNamedSourceData("Object", baseInfo);
            resolver.AddAnonymousSourceData(baseInfo);

            if (baseInfo.Generalized.PrioritizedProperties != null)
            {
                foreach (string prop in baseInfo.Generalized.PrioritizedProperties)
                {
                    resolver.PrioritizeProperty(prop);
                }
            }

            return resolver;
        }


        private static BaseInfo GetEmptyObject(DataClassInfo resolverClassInfo)
        {
            BaseInfo baseInfo;

            if (resolverClassInfo.ClassIsCustomTable)
            {
                baseInfo = CustomTableItem.New(resolverClassInfo.ClassName);
            }
            else if (resolverClassInfo.ClassIsDocumentType)
            {
                baseInfo = TreeNode.New(resolverClassInfo.ClassName);
            }
            else
            {
                baseInfo = ModuleManager.GetReadOnlyObjectByClassName(resolverClassInfo.ClassName);
            }

            return baseInfo;
        }
    }
}
