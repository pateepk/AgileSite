namespace Kentico.Web.Mvc
{
	/// <summary>
	/// Represents a set of features available for the current request.
	/// </summary>
	public interface IFeatureSet
    {
        /// <summary>
        /// Adds or replaces a feature of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the feature.</typeparam>
        /// <param name="feature">The feature to add or replace.</param>
        void SetFeature<T>(T feature) where T : class;


        /// <summary>
        /// Returns a feature of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the feature.</typeparam>
        /// <returns>A feature of the specified type, if available; otherwise, null.</returns>
        T GetFeature<T>() where T : class;
        

        /// <summary>
        /// Removes a feature of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the feature.</typeparam>
        void RemoveFeature<T>() where T : class;
    }
}
