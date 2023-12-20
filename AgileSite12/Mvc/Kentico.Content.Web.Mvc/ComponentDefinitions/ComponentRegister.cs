﻿using System;
using System.Collections.Generic;
using System.Threading;

namespace Kentico.Content.Web.Mvc
{
    /// <summary>
    /// A thread-safe register of components.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys in the register.</typeparam>
    /// <typeparam name="TValue">The type of the components in the register.</typeparam>
    /// <remarks>
    /// The register is optimized for read operations while modifications to the register are less efficient.
    /// </remarks>
    internal class ComponentRegister<TKey, TValue>
    {
        private volatile Dictionary<TKey, TValue> register;
        private readonly IEqualityComparer<TKey> comparer;


        /// <summary>
        /// Initializes a new instance of the <see cref="ComponentRegister{TKey, TValue}"/> class that uses the specified <paramref name="comparer"/>.
        /// </summary>
        /// <param name="comparer">Comparer to be used for keys.</param>
        public ComponentRegister(IEqualityComparer<TKey> comparer)
        {
            this.comparer = comparer;
            register = new Dictionary<TKey, TValue>(comparer);
        }


        /// <summary>
        /// Tries to add a new key and value to the register, if given <paramref name="key"/> is not already present.
        /// </summary>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="value">The element to be added.</param>
        /// <returns>Returns true if register did not contain <paramref name="key"/> and the new element was added. Otherwise returns false.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="key"/> is null.</exception>
        public bool TryAdd(TKey key, TValue value)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            Dictionary<TKey, TValue> initialRegister;
            Dictionary<TKey, TValue> newRegister;

            do
            {
                initialRegister = register;
                if (initialRegister.ContainsKey(key))
                {
                    return false;
                }

                newRegister = new Dictionary<TKey, TValue>(initialRegister, comparer);
                newRegister.Add(key, value);
            } while (Interlocked.CompareExchange(ref register, newRegister, initialRegister) != initialRegister);

            return true;
        }


        /// <summary>
        /// Tries to add a new key and value to the register, if given <paramref name="key"/> is not already present. This method is not thread-safe.
        /// </summary>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="value">The element to be added.</param>
        /// <returns>Returns true if register did not contain <paramref name="key"/> and the new element was added. Otherwise returns false.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="key"/> is null.</exception>
        /// <remarks>
        /// This member is not thread-safe while providing optimal performance of the modification operation.
        /// </remarks>
        public bool TryAddUnsafe(TKey key, TValue value)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (register.ContainsKey(key))
            {
                return false;
            }

            register.Add(key, value);

            return true;
        }


        /// <summary>
        /// Gets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key of the value to get.</param>
        /// <param name="value">Value associated with the key, if found, or the default value for the <typeparamref name="TValue"/> type.</param>
        /// <returns>Returns true if <paramref name="key"/> is found. Otherwise returns false.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="key"/> is null.</exception>
        public bool TryGetValue(TKey key, out TValue value)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            return register.TryGetValue(key, out value);
        }


        /// <summary>
        /// Gets all values registered within the register.
        /// </summary>
        /// <returns>Returns enumeration of all registered values.</returns>
        public IEnumerable<TValue> GetAll()
        {
            return register.Values;
        }
    }
}
