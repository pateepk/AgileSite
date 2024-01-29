using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace CMS.Base
{
    /// <summary>
    /// Provides methods to access types attributes. Supports extra attributes provided by the <see cref="Extension"/>.
    /// </summary>
    public sealed class AttributeHelper
    {
        /// <summary>
        /// Gets the custom attributes of the given type for the given member.
        /// </summary>
        /// <param name="method">Method for which extract the custom attributes.</param>
        /// <param name="attributeType">The type of attribute to search for. Only attributes that are assignable to this type are returned.</param>
        public static object[] GetCustomAttributes(MethodInfo method, Type attributeType)
        {
            var result = new ArrayList();

            // Add default attributes
            result.AddRange(method.GetCustomAttributes(attributeType, true));

            // Get extra metadata
            var extra = Extension<MetadataContainer>.GetExtensionsForType(method.DeclaringType);
            if (extra != null)
            {
                foreach (var ext in extra)
                {
                    var metadataContainer = ext.Value;
                    var mdType = metadataContainer.MetadataType;

                    // Find members by their signature
                    var parameters = method.GetParameters();

                    Type[] parameterTypes = Type.EmptyTypes;
                    if (parameters.Length > 0)
                    {
                        parameterTypes = (from p in parameters select p.ParameterType).ToArray();
                    }

                    var found = mdType.GetMethod(method.Name, parameterTypes);
                    if (found != null)
                    {
                        result.AddRange(found.GetCustomAttributes(attributeType, true));
                    }
                }
            }

            return result.ToArray();
        }
    }
}
