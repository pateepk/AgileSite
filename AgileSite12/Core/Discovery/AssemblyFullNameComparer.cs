using System;
using System.Collections.Generic;
using System.Reflection;

namespace CMS.Core
{
    /// <summary>
    /// Encapsulates logic for comparing assemblies via <see cref="Assembly.FullName"/>.
    /// </summary>
    internal class AssemblyFullNameComparer : IEqualityComparer<Assembly>
    {
        /// <summary>
        /// Determines whether the specified assembly names are equal.
        /// </summary>
        public bool Equals(Assembly x, Assembly y)
        {
            if (x == y)
            {
                return true;
            }

            if (x == null || y == null)
            {
                return false;
            }

            if (x.FullName.Equals(y.FullName, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return false;
        }


        /// <summary>
        /// Returns a hash code for the specified <see cref="Assembly.FullName"/>.
        /// </summary>
        public int GetHashCode(Assembly obj)
        {
            return obj.FullName.GetHashCode();
        }
    }
}
