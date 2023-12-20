using Castle.MicroKernel;

namespace CMS.Core
{
    /// <summary>
    /// Castle Windsor container release policy which provides similar level of tracking as <see cref="ObjectFactory{T}"/> does.
    /// </summary>
    internal sealed class ObjectFactoryLikeReleasePolicy : IReleasePolicy
    {
        public IReleasePolicy CreateSubPolicy()
        {
            return this;
        }


        public void Dispose()
        {
            // Nothing to dispose
        }


        public bool HasTrack(object instance)
        {
            return false;
        }


        public void Release(object instance)
        {
            // Nothing to release
        }


        public void Track(object instance, Burden burden)
        {
            // No objects are tracked
        }
    }
}
