using System;
using System.Collections;
using System.Collections.Generic;

namespace Kentico.Content.Web.Mvc
{
    /// <summary>
    /// Collection of component filters.
    /// </summary>
    public sealed class ComponentFilterCollection<TFilter> : IEnumerable<TFilter>
        where TFilter : IComponentFilter
    {
        private readonly List<TFilter> filters = new List<TFilter>();


        /// <summary>
        /// Adds a component filter.
        /// </summary>
        /// <param name="filter">Component filter to add.</param>
        /// <exception cref="ArgumentNullException">Throws when <paramref name="filter"/> is <c>null</c>.</exception>
        public void Add(TFilter filter)
        {
            if (filter == null)
            {
                throw new ArgumentNullException(nameof(filter));
            }

            filters.Add(filter);
        }


        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// An enumerator that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<TFilter> GetEnumerator()
        {
            return filters.GetEnumerator();
        }


        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return filters.GetEnumerator();
        }


        /// <summary>
        /// Clears the component filter collection.
        /// </summary>
        internal void Clear()
        {
            filters.Clear();
        }
    }
}
