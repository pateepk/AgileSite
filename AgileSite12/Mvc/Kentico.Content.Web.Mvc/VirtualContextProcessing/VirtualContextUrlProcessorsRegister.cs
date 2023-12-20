using System;
using System.Collections.Generic;

using CMS.Helpers;

namespace Kentico.Content.Web.Mvc
{
    /// <summary>
    /// Contains instances of <see cref="IVirtualContextUrlProcessor"/>s used for handling <see cref="VirtualContext"/> URLs.
    /// </summary>
    internal class VirtualContextUrlProcessorsRegister
    {
        /// <summary>
        /// List of processors added via <see cref="Add(IVirtualContextUrlProcessor)"/>.
        /// </summary>
        private readonly List<IVirtualContextUrlProcessor> processors = new List<IVirtualContextUrlProcessor>();
        private static Lazy<VirtualContextUrlProcessorsRegister> instance = new Lazy<VirtualContextUrlProcessorsRegister>(() => new VirtualContextUrlProcessorsRegister());

        /// <summary>
        /// Returns instance of the <see cref="VirtualContextUrlProcessorsRegister"/>.
        /// </summary>
        public static VirtualContextUrlProcessorsRegister Instance => instance.Value;


        /// <summary>
        /// Initializes a new instance of the <see cref="VirtualContextUrlProcessorsRegister"/>. 
        /// </summary>
        protected VirtualContextUrlProcessorsRegister()
        {
        }


        /// <summary>
        /// Gets the collection of registered processors.
        /// </summary>
        public ICollection<IVirtualContextUrlProcessor> Processors => processors;


        /// <summary>
        /// Adds a processor for processing <see cref="VirtualContext"/> URL.
        /// </summary>
        /// <param name="virtualContextProcessor">Processor to process data parsed from the <see cref="VirtualContext"/> URL.</param>
        public void Add(IVirtualContextUrlProcessor virtualContextProcessor)
        {
            if (virtualContextProcessor == null)
            {
                throw new ArgumentNullException(nameof(virtualContextProcessor));
            }

            processors.Add(virtualContextProcessor);
        }
    }
}
