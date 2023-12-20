using System.Collections.Specialized;
using System.Web;
using System.Web.SessionState;

using CMS.Base;

namespace CMS.AspNet.Platform
{
    /// <summary>
    /// Wrapper over <see cref="HttpSessionStateBase"/> object implementing <see cref="ISession"/> interface.
    /// </summary>
    internal class SessionImpl : ISession
    {
#pragma warning disable BH1000 // 'Session.SessionId' should not be used. Use 'SessionHelper.GetSessionID()' instead.
#pragma warning disable BH1001 // 'Session[]' should not be used. Use 'SessionHelper.GetValue()' instead.
#pragma warning disable BH1002 // 'Session[]' should not be used. Use 'SessionHelper.SetValue()' instead.

        private readonly HttpContextBase mContext;


        public SessionImpl(HttpContextBase context)
        {
            mContext = context;
        }


        public int Timeout => mContext.Session.Timeout;


        public NameObjectCollectionBase.KeysCollection Keys => mContext.Session.Keys;


        public string SessionID => mContext.Session.SessionID;


        public bool IsReadOnly => mContext.Handler is IReadOnlySessionState;


        public object this[string name]
        {
            get => mContext.Session[name];
            set => mContext.Session[name] = value;
        }


        public void Abandon()
        {
            mContext.Session.Abandon();
        }


        public void Clear()
        {
            mContext.Session.Clear();
        }


        public void Remove(string name)
        {
            mContext.Session.Remove(name);
        }
    }

#pragma warning restore BH1000 // 'Session.SessionId' should not be used. Use 'SessionHelper.GetSessionID()' instead.
#pragma warning restore BH1001 // 'Session[]' should not be used. Use 'SessionHelper.GetValue()' instead.
#pragma warning restore BH1002 // 'Session[]' should not be used. Use 'SessionHelper.SetValue()' instead.
}
