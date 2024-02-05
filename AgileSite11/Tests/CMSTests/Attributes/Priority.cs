using System;

using NUnit.Framework;

namespace CMS.Tests
{
    /// <summary>
    /// Class containing priority attributes.
    /// </summary>
    public class Priority
    {
        /// <summary>
        /// Critical pritority.
        /// </summary>
        public const string Critical = "PriorityCritical";


        /// <summary>
        /// Major pritority.
        /// </summary>
        public const string Major = "PriorityMajor";


        /// <summary>
        /// Normal pritority.
        /// </summary>
        public const string Normal = "PriorityNormal";


        /// <summary>
        /// Minor pritority.
        /// </summary>
        public const string Minor = "PriorityMinor";


        /// <summary>
        /// Critical priority test category.
        /// </summary>
        [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
        public class CriticalAttribute : CategoryAttribute
        {
            /// <summary>
            /// Construcor.
            /// </summary>
            public CriticalAttribute() : base(Critical)
            {
            }
        }


        /// <summary>
        /// Major priority test category.
        /// </summary>
        [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
        public class MajorAttribute : CategoryAttribute
        {
            /// <summary>
            /// Construcor.
            /// </summary>
            public MajorAttribute() : base(Major)
            {
            }
        }


        /// <summary>
        /// Normal priority test category.
        /// </summary>
        [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
        public class NormalAttribute : CategoryAttribute
        {
            /// <summary>
            /// Construcor.
            /// </summary>
            public NormalAttribute() : base(Normal)
            {
            }
        }


        /// <summary>
        /// Minor priority test category.
        /// </summary>
        [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
        public class MinorAttribute : CategoryAttribute
        {
            /// <summary>
            /// Construcor.
            /// </summary>
            public MinorAttribute() : base(Minor)
            {
            }
        }
    }
}
