using CMS;
using CMS.DataEngine;
using CMS.MacroEngine;
using CMS.Base;
using CMS.MessageBoards;

[assembly: RegisterModule(typeof(MessageBoardModule))]

namespace CMS.MessageBoards
{
    /// <summary>
    /// Represents the Message Board module.
    /// </summary>
    public class MessageBoardModule : Module
    {
        #region "Constants"

        /// <summary>
        /// Name of email template type for boards.
        /// </summary>
        public const string BOARDS_EMAIL_TEMPLATE_TYPE_NAME = "boards";


        /// <summary>
        /// Name of email template type for board subscription.
        /// </summary>
        public const string BOARD_SUBSCRIPTION_EMAIL_TEMPLATE_TYPE_NAME = "boardssubscription";

        #endregion


        /// <summary>
        /// Default constructor
        /// </summary>
        public MessageBoardModule()
            : base(new MessageBoardModuleMetadata())
        {
        }


        /// <summary>
        /// Initializes the module.
        /// </summary>
        protected override void OnInit()
        {
            base.OnInit();

            InitImportExport();
            InitMacros();

            MessageBoardHandlers.Init();
        }


        /// <summary>
        /// Initializes blog import/export actions
        /// </summary>
        private static void InitImportExport()
        {
            BoardExport.Init();
            BoardImport.Init();

            ImportSpecialActions.Init();
        }


        /// <summary>
        /// Initializes the message board macros
        /// </summary>
        private static void InitMacros()
        {
            ExtendList<MacroResolverStorage, MacroResolver>.With("BoardsResolver").WithLazyInitialization(() => MessageBoardResolvers.BoardsResolver);
            ExtendList<MacroResolverStorage, MacroResolver>.With("BoardsSubscriptionResolver").WithLazyInitialization(() => MessageBoardResolvers.BoardsSubscriptionResolver);
        }


        /// <summary>
        /// Registers the object type of this module
        /// </summary>
        protected override void RegisterCommands()
        {
            base.RegisterCommands();

            RegisterCommand("GetMessageBoardInfo", GetMessageBoardInfo);
            RegisterCommand("GetBoardMessageInfo", GetBoardMessageInfo);
            RegisterCommand("GetDocumentBoardsCount", GetDocumentBoardsCount);
            RegisterCommand("AddRoleToBoard", AddRoleToBoard);
            RegisterCommand("RemoveRoleFromBoard", RemoveRoleFromBoard);
            RegisterCommand("AddModeratorToBoard", AddModeratorToBoard);
            RegisterCommand("RemoveModeratorFromBoard", RemoveModeratorFromBoard);
        }


        /// <summary>
        /// Removes the user from the board
        /// </summary>
        /// <param name="parameters">Parameters array</param>
        private static object RemoveModeratorFromBoard(object[] parameters)
        {
            int userId = (int)parameters[0];
            int boardId = (int)parameters[1];

            BoardModeratorInfoProvider.RemoveModeratorFromBoard(userId, boardId);

            return null;
        }


        /// <summary>
        /// Adds the moderator to the board
        /// </summary>
        /// <param name="parameters">Parameters array</param>
        private static object AddModeratorToBoard(object[] parameters)
        {
            int userId = (int)parameters[0];
            int boardId = (int)parameters[1];

            BoardModeratorInfoProvider.AddModeratorToBoard(userId, boardId);

            return null;
        }


        /// <summary>
        /// Removes the role from the board
        /// </summary>
        /// <param name="parameters">Parameters array</param>
        private static object RemoveRoleFromBoard(object[] parameters)
        {
            int roleId = (int)parameters[0];
            int boardId = (int)parameters[1];

            BoardRoleInfoProvider.RemoveRoleFromBoard(roleId, boardId);

            return null;
        }


        /// <summary>
        /// Adds the role to the board
        /// </summary>
        /// <param name="parameters">Parameters array</param>
        private static object AddRoleToBoard(object[] parameters)
        {
            int roleId = (int)parameters[0];
            int boardId = (int)parameters[1];

            BoardRoleInfoProvider.AddRoleToBoard(roleId, boardId);

            return null;
        }


        /// <summary>
        /// Get number of boards for selected document
        /// </summary>
        /// <param name="parameters">Parameters array</param>
        private static object GetDocumentBoardsCount(object[] parameters)
        {
            int documentId = (int)parameters[0];

            return BoardInfoProvider.GetBoardsCount(documentId);
        }


        /// <summary>
        /// Gets the message info
        /// </summary>
        /// <param name="parameters">Parameters array</param>
        private static BoardMessageInfo GetBoardMessageInfo(object[] parameters)
        {
            int boardMessageId = (int)parameters[0];
            return BoardMessageInfoProvider.GetBoardMessageInfo(boardMessageId);
        }


        /// <summary>
        /// Get message board info with dependence on specified board id
        /// </summary>
        /// <param name="parameters">Parameters array</param>
        private static BoardInfo GetMessageBoardInfo(object[] parameters)
        {
            int boardid = (int)parameters[0];
            return BoardInfoProvider.GetBoardInfo(boardid);
        }
    }
}