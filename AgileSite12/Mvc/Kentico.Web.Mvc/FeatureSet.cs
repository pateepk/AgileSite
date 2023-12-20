using System;
using System.Collections.Generic;

namespace Kentico.Web.Mvc
{
    /// <summary>
    /// Represents a set of features available for the current request.
    /// </summary>
    internal sealed class FeatureSet : IFeatureSet
    {
        private readonly Dictionary<Type, object> mFeaturesByType = new Dictionary<Type, object>();

        /// <summary>
        /// Adds or replaces a feature of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the feature.</typeparam>
        /// <param name="feature">The feature to add or replace.</param>
        public void SetFeature<T>(T feature) where T : class
        {
            mFeaturesByType[typeof(T)] = feature;
        }


        /// <summary>
        /// Returns a feature of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the feature.</typeparam>
        /// <returns>A feature of the specified type, if available; otherwise, null.</returns>
        public T GetFeature<T>() where T : class
        {
            mFeaturesByType.TryGetValue(typeof(T), out object feature);

            return (T)feature;
        }


        /// <summary>
        /// Removes a feature of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the feature.</typeparam>
        public void RemoveFeature<T>() where T : class
        {
            mFeaturesByType.Remove(typeof(T));
        }
    }
}