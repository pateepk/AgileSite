using System;

namespace CMS.DataEngine
{
    /// <summary>
    /// Provider returns query obtained via <see cref="BaseInfo"/>
    /// </summary>
    internal sealed class InfoObjectExecutingQueryProvider : IExecutingQueryProvider
    {
        private BaseInfo mInfo;


        /// <summary>
        /// Creates a new instance of <see cref="InfoObjectExecutingQueryProvider"/>
        /// </summary>
        internal InfoObjectExecutingQueryProvider(ObjectTypeInfo typeInfo)
        {
            Refresh(typeInfo);
        }


        /// <summary>
        /// Returns executing query as <see cref="IDataQuery"/>
        /// </summary>
        public IDataQuery GetExecutionQuery()
        {
            return mInfo.GetDataQuery(false, null, false);
        }


        /// <summary>
        /// Refreshes the underlying data affecting the query retrieval
        /// </summary>
        /// <param name="typeInfo">Type info holding all necessary data</param>
        public void Refresh(ObjectTypeInfo typeInfo)
        {
            if (typeInfo == null)
            {
                throw new ArgumentNullException("typeInfo");
            }

            mInfo = ModuleManager.GetReadOnlyObject(typeInfo.ObjectType, true);
        }
    }
}