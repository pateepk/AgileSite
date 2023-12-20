using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;

namespace CMS.Base
{
    /// <summary>
    /// Class encapsulating the request logs.
    /// </summary>
    public class RequestLogs : INotCopyThreadItem, IEnumerable<RequestLog>
    {
        #region "Variables"

        /// <summary>
        /// Flags for enabled status of particular debugs
        /// </summary>
        private readonly RequestLog[] logs = new RequestLog[DebugHelper.RegisteredDebugsCount];

        #endregion


        #region "Properties"

        /// <summary>
        /// If true, new logs are automatically registered
        /// </summary>
        public bool RegisterNewLogs
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the enabled status for particular debug
        /// </summary>
        /// <param name="key">Debug key</param>
        public RequestLog this[int key]
        {
            get
            {
                if (key < 0)
                {
                    throw new NotSupportedException("[RequestLogs]: Debug is not registered, you need to call DebugHelper.RegisterDebug to be able to use it!");
                }

                return logs[key];
            }
            set
            {
                logs[key] = value;

                // Register the log if not registered yet
                if ((value != null) && RegisterNewLogs)
                {
                    value.Register();
                }
            }
        }


        /// <summary>
        /// Gets or sets the enabled status for particular debug
        /// </summary>
        /// <param name="debug">Debug</param>
        public RequestLog this[DebugSettings debug]
        {
            get
            {
                return this[debug.DebugKey];
            }
            set
            {
                this[debug.DebugKey] = value;
            }
        }


        /// <summary>
        /// Request GUID.
        /// </summary>
        public Guid RequestGUID
        {
            get;
            set;
        }

        
        /// <summary>
        /// Request time.
        /// </summary>
        public DateTime RequestTime
        {
            get;
            set;
        }


        /// <summary>
        /// Request URL.
        /// </summary>
        public string RequestURL
        {
            get;
            set;
        }
        

        /// <summary>
        /// Current thread stack
        /// </summary>
        public string ThreadStack
        {
            get;
            private set;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Constructor.
        /// </summary>
        public RequestLogs(Guid requestGuid = default(Guid))
        {
            RequestGUID = (requestGuid != default(Guid)) ? requestGuid : Guid.NewGuid();
            RequestTime = DateTime.Now;
            RequestURL = DebugHelper.GetRequestUrl();
            ThreadStack = DebugHelper.GetThreadStack();
        }


        /// <summary>
        /// Creates new request log.
        /// </summary>
        /// <param name="dt">Log table</param>
        /// <param name="settings">Debug settings</param>
        public RequestLog CreateLog(DataTable dt, DebugSettings settings)
        {
            return new RequestLog(this, dt, settings);
        }


        /// <summary>
        /// Ensures the request log for the given settings
        /// </summary>
        /// <param name="settings">Debug settings</param>
        /// <param name="newTable">New log table function</param>
        public RequestLog EnsureLog(DebugSettings settings, Func<DataTable> newTable)
        {
            var log = this[settings];
            if (log == null)
            {
                log = CreateLog(newTable(), settings);

                this[settings] = log;
            }

            return log;
        }
        
        #endregion


        #region "IEnumerable members"

        /// <summary>
        /// Gets the logs enumerator
        /// </summary>
        public IEnumerator<RequestLog> GetEnumerator()
        {
            // Get all logs
            foreach (var log in logs)
            {
                if (log != null)
                {
                    yield return log;
                }
            }
        }


        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}