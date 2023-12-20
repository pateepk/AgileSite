using System.Collections.Generic;
using System.Linq;

using CMS.DataEngine;

namespace CMS.ContinuousIntegration
{
    /// <summary>
    /// Default dynamic type provider
    /// </summary>
    internal sealed class CustomTableTypeProvider : ICustomTableTypeProvider
    {
        public IEnumerable<string> GetTypes()
        {
            return DataClassInfoProvider.GetClasses()
                                        .Columns("ClassName").WhereEquals("ClassIsCustomTable", 1)
                                        .ToList().Select(i => PredefinedObjectType.CUSTOM_TABLE_ITEM_PREFIX + i.ClassName)
                                        .ToArray();
        }
    }
}