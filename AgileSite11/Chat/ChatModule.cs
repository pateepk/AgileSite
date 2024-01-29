using CMS;
using CMS.Chat;
using CMS.Core;
using CMS.DataEngine;

[assembly: RegisterModule(typeof(ChatModule))]

namespace CMS.Chat
{
    /// <summary>
    /// Represents the Chat module.
    /// </summary>
    public class ChatModule : Module
    {
        internal const string CHAT = "##CHAT##";
        

        /// <summary>
        /// Default constructor
        /// </summary>
        public ChatModule()
            : base(new ChatModuleMetadata())
        {
        }


        #region "Protected methods"

        /// <summary>
        /// Initializes chat module.
        /// </summary>
        protected override void OnInit()
        {
            base.OnInit();

            ChatHandlers.Init();
        }

        #endregion
    }
}
