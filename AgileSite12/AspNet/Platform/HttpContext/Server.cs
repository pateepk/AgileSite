using System.Web;

using CMS.Base;

namespace CMS.AspNet.Platform
{
    /// <summary>
    /// Wrapper over <see cref="HttpServerUtilityBase"/> object implementing <see cref="IServer"/> interface.
    /// </summary>
    internal class ServerImpl : IServer
    {
        private readonly HttpContextBase mContext;


        public ServerImpl(HttpContextBase context)
        {
            mContext = context;
        }


        public int ScriptTimeout
        {
            get => mContext.Server.ScriptTimeout;
            set => mContext.Server.ScriptTimeout = value;
        }


        public string MapPath(string path)
        {
            return mContext.Server.MapPath(path);
        }


        public void Transfer(string path)
        {
            mContext.Server.Transfer(path);
        }


        public string UrlPathEncode(string requestUrl)
        {
            return mContext.Server.UrlPathEncode(requestUrl);
        }
    }
}
