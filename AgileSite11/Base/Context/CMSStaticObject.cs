using System;

namespace CMS.Base
{
    /// <summary>
    /// Static object wrapper which provides static variables based on the current context name - Variant that provides a new object instance for each context
    /// </summary>
    [Obsolete("Use CMSStatic with initializer method.")]
    public class CMSStaticObject<TValue> : CMSStatic<TValue>
        where TValue : new()
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public CMSStaticObject()
            : base(() => new TValue())
        {
        }
    }
}