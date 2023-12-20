namespace CMS.Base
{
    /// <summary>
    /// Interface defining non-generic methods for <see cref="CMSStatic{TValue}"/>.
    /// </summary>
    public interface ICMSStatic
    {
        /// <summary>
        /// Resets the static field to its original state.
        /// </summary>
        void Reset();
    }
}
