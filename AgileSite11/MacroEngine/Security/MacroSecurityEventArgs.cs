using CMS.Base;

namespace CMS.MacroEngine
{
    /// <summary>
    /// OnCheckObjectPermissions event arguments.
    /// </summary>
    public class MacroSecurityEventArgs : CMSEventArgs
    {
        /// <summary>
        /// Result of the security check. If true, the security check is allowed.
        /// </summary>
        public bool Result
        {
            get;
            set;
        }


        /// <summary>
        /// Evaluation context under which the macro is being resolved.
        /// </summary>
        public EvaluationContext Context
        {
            get;
            set;
        }


        /// <summary>
        /// Object which should be checked.
        /// </summary>
        public object ObjectToCheck
        {
            get;
            set;
        }


        /// <summary>
        /// If true, only collections should be checked within this permission check request.
        /// </summary>
        public bool OnlyCollections
        {
            get;
            set;
        }


        /// <summary>
        /// Creates new instance of event args with given context and default result.
        /// </summary>
        /// <param name="context">Evaluation context under which the macro is being resolved</param>
        /// <param name="defaultResult">Result of the security check. If true, the security check is allowed</param>
        public MacroSecurityEventArgs(EvaluationContext context, bool defaultResult)
        {
            Context = context;
            Result = defaultResult;
        }
    }
}