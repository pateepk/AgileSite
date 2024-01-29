using CMS;
using CMS.DataEngine;
using CMS.Polls;

[assembly: RegisterModule(typeof(PollsModule))]

namespace CMS.Polls
{
    /// <summary>
    /// Represents the Polls module.
    /// </summary>
    public class PollsModule : Module
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public PollsModule()
            : base(new PollsModuleMetadata())
        {
        }


        /// <summary>
        /// Registers the object type of this module
        /// </summary>
        protected override void RegisterCommands()
        {
            base.RegisterCommands();

            RegisterCommand("AddRoleToPoll", AddRoleToPoll);
            RegisterCommand("RemoveRoleFromPoll", RemoveRoleFromPoll);
            RegisterCommand("BelongsToGroup", BelongsToGroup);
            RegisterCommand("GetPollInfo", GetPollInfo);
            RegisterCommand("GetPollAnswerInfo", GetPollAnswerInfo);
        }


        /// <summary>
        /// Returns answer info for specified ID
        /// </summary>
        /// <param name="parameters">Parameters array</param>
        private static object GetPollAnswerInfo(object[] parameters)
        {
            return PollAnswerInfoProvider.GetPollAnswerInfo((int)parameters[0]);
        }


        /// <summary>
        /// Returns poll info for specified ID
        /// </summary>
        /// <param name="parameters">Parameters array</param>
        private static object GetPollInfo(object[] parameters)
        {
            return PollInfoProvider.GetPollInfo((int)parameters[0]);
        }


        /// <summary>
        /// Returns true if poll belongs to specified group
        /// </summary>
        /// <param name="parameters">Parameters array</param>
        private static object BelongsToGroup(object[] parameters)
        {
            int pollId = (int)parameters[0];
            int groupId = (int)parameters[1];

            PollInfo poll = PollInfoProvider.GetPollInfo(pollId);

            // Poll found and belongs to specified group
            if ((poll != null) && (poll.PollGroupID == groupId))
            {
                return true;
            }

            return false;
        }


        /// <summary>
        /// Remove role from poll
        /// </summary>
        /// <param name="parameters">Parameters array</param>
        private static object RemoveRoleFromPoll(object[] parameters)
        {
            int roleId = (int)parameters[0];
            int pollId = (int)parameters[1];

            PollRoleInfoProvider.RemoveRoleFromPoll(roleId, pollId);

            return null;
        }


        /// <summary>
        /// Add role to poll
        /// </summary>
        /// <param name="parameters">Parameters array</param>
        private static object AddRoleToPoll(object[] parameters)
        {
            int roleId = (int)parameters[0];
            int pollId = (int)parameters[1];

            PollRoleInfoProvider.AddRoleToPoll(roleId, pollId);

            return null;
        }
    }
}