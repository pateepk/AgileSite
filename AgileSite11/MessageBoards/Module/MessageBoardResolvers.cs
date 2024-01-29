using CMS.DataEngine;
using CMS.MacroEngine;
using CMS.Membership;

namespace CMS.MessageBoards
{
    /// <summary>
    /// Resolvers used in e-mail templates and other macro visual components.
    /// </summary>
    public class MessageBoardResolvers : ResolverDefinition
    {
        #region "Variables"

        private static MacroResolver mBoardsResolver;
        private static MacroResolver mBoardsSubscribeResolver;

        #endregion

        /// <summary>
        /// Boards e-mail template macro resolver.
        /// </summary>
        public static MacroResolver BoardsResolver
        {
            get
            {
                if (mBoardsResolver == null)
                {
                    MacroResolver resolver = MacroResolver.GetInstance();

                    resolver.SetNamedSourceData("Board", ModuleManager.GetReadOnlyObject("board.board"));
                    resolver.SetNamedSourceData("Message", ModuleManager.GetReadOnlyObject("board.message"));
                    resolver.SetNamedSourceData("MessageUser", ModuleManager.GetReadOnlyObject(PredefinedObjectType.USER));
                    resolver.SetNamedSourceData("MessageUserSettings", ModuleManager.GetReadOnlyObject(UserSettingsInfo.OBJECT_TYPE));

                    // Register flat values
                    RegisterStringValues(resolver, new[] { "UnsubscriptionLink", "DocumentLink" });

                    mBoardsResolver = resolver;
                }

                return mBoardsResolver;
            }
        }


        /// <summary>
        /// Boards subscription e-mail template macro resolver.
        /// </summary>
        public static MacroResolver BoardsSubscriptionResolver
        {
            get
            {
                if (mBoardsSubscribeResolver == null)
                {
                    MacroResolver resolver = MacroResolver.GetInstance();

                    resolver.SetNamedSourceData("Board", ModuleManager.GetReadOnlyObject("board.board"));

                    // Register flat values
                    RegisterStringValues(resolver, new[] { "UnsubscriptionLink", "SubscriptionLink", "OptInInterval", "DocumentLink" });

                    mBoardsSubscribeResolver = resolver;
                }

                return mBoardsSubscribeResolver;
            }
        }
    }
}