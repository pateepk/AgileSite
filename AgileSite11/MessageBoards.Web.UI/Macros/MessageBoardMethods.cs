using System;

using CMS;
using CMS.DocumentEngine.Web.UI;
using CMS.Helpers;
using CMS.MacroEngine;
using CMS.MessageBoards.Web.UI;

[assembly: RegisterExtension(typeof(MessageBoardMethods), typeof(TransformationNamespace))]

namespace CMS.MessageBoards.Web.UI
{
    /// <summary>
    /// Message board methods - wrapping methods for macro resolver.
    /// </summary>
    public class MessageBoardMethods : MacroMethodContainer
    {
        /// <summary>
        /// Returns user name.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof (int), "Returns count of messages in messageboard.", 2)]
        [MacroMethodParam(0, "documentId", typeof (int), "ID of the document.")]
        [MacroMethodParam(1, "boardWebpartName", typeof (object), "Name of the webpart used for creating messageboard.")]
        [MacroMethodParam(2, "type", typeof (object), "String constant representing the type of board 'user', 'group' or 'document' (default) are accepted.")]
        public static object GetBoardMessagesCount(EvaluationContext context, params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 2:
                    return MessageBoardTransformationFunctions.GetBoardMessagesCount(ValidationHelper.GetInteger(parameters[0], 0), ValidationHelper.GetString(parameters[1], ""), "document");

                case 3:
                    return MessageBoardTransformationFunctions.GetBoardMessagesCount(ValidationHelper.GetInteger(parameters[0], 0), ValidationHelper.GetString(parameters[1], ""), ValidationHelper.GetString(parameters[2], ""));

                default:
                    throw new NotSupportedException();
            }
        }
    }
}
