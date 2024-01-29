using System;
using System.Collections.Generic;
using System.Linq;

using CMS.DataEngine;

namespace CMS.ContinuousIntegration
{
    /// <summary>
    /// Default implementation od <see cref="IMainObjectTypeProvider"/>.
    /// </summary>
    internal sealed class MainObjectTypeProvider : IMainObjectTypeProvider
    {
        private readonly IEnumerable<ObjectTypeInfo> mObjectTypeInfos;


        /// <summary>
        /// Creates a new instance of <see cref="MainObjectTypeProvider"/>.
        /// </summary>
        /// <param name="objectTypeInfos">Available object type infos, these are filtered in GetObjectTypes() method.</param>
        public MainObjectTypeProvider(IEnumerable<ObjectTypeInfo> objectTypeInfos)
        {
            if (objectTypeInfos == null)
            {
                throw new ArgumentNullException("objectTypeInfos");
            }

            mObjectTypeInfos = objectTypeInfos;
        }


        /// <summary>
        /// Returns set of supported main object types.
        /// </summary>
        public ISet<string> GetObjectTypes()
        {
            return mObjectTypeInfos.Where(t => t.ContinuousIntegrationSettings.Enabled && t.IsMainObject)
                                   .Select(t => t.ObjectType)
                                   .ToHashSet();
        }
    }
}
