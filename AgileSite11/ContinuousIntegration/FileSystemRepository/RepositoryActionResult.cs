using System;
using System.Collections.Generic;
using System.Linq;

namespace CMS.ContinuousIntegration
{
    /// <summary>
    /// Result object for Store/Restore operation.
    /// </summary>
    [Serializable]
    public sealed class RepositoryActionResult
    {
        readonly ICollection<LogItem> errors;
        readonly ICollection<LogItem> warnings;

        /// <summary>
        /// Creates new object.
        /// </summary>
        internal RepositoryActionResult()
        {
            Success = true;
            errors = new List<LogItem>();
            warnings = new List<LogItem>();
        }


        /// <summary>
        /// Indicates whether action was successful.
        /// </summary>
        /// <remarks>Details for unsuccessful action are accessible in <see cref="Errors"/>.</remarks>
        public bool Success 
        { 
            get;
            private set;
        }


        /// <summary>
        /// List of errors for unsuccessful action.
        /// </summary>
        public IEnumerable<string> Errors 
        { 
            get
            {
                return errors.Select(x => x.Message);
            }
        }


        /// <summary>
        /// List of warnings for action.
        /// </summary>
        public IEnumerable<string> Warnings
        {
            get
            {
                return warnings.Select(x => x.Message);
            }
        }


        /// <summary>
        /// Logs the error to the list of errors and causes that action will be unsuccessful.
        /// </summary>
        /// <param name="errorText">Error text.</param>
        /// <param name="actionType">Action type.</param>
        /// <returns>Returns current instance of <see cref="RepositoryActionResult"/>.</returns>
        internal RepositoryActionResult LogError(string errorText, LogItemActionTypeEnum actionType = LogItemActionTypeEnum.Unknown)
        {
            return Log(new LogItem(errorText, LogItemTypeEnum.Error, actionType));
        }


        /// <summary>
        /// Logs all warnings and errors to the action result. Logged error causes that action will be unsuccessful. 
        /// </summary>
        /// <param name="logItem">Item to log.</param>
        /// <returns>Returns current instance of <see cref="RepositoryActionResult"/>.</returns>
        internal RepositoryActionResult Log(LogItem logItem)
        {
            switch (logItem.Type)
            {
                case LogItemTypeEnum.Error:
                    Success = false;
                    errors.Add(logItem);

                    break;

                case LogItemTypeEnum.Warning:
                    warnings.Add(logItem);

                    break;
            }

            return this;
        }
    }
}
