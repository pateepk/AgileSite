using System;

using CMS.Community;
using CMS.Membership;
using CMS.Messaging;
using System.Data;
using CMS.Helpers;
using CMS.Base;

namespace APIExamples
{
    /// <summary>
    /// Holds messaging API examples.
    /// </summary>
    /// <pageTitle>Messages</pageTitle>
    internal class MessagingMain
    {
        /// <summary>
        /// Holds message API examples.
        /// </summary>
        /// <groupHeading>Messages</groupHeading>
        private class Messages
        {
            /// <heading>Creating a message</heading>
            private void CreateMessage()
            {
                // Creates a new message object
                MessageInfo newMessage = new MessageInfo();

                // Sets the message content
                newMessage.MessageSubject = "API example message";
                newMessage.MessageBody = "This is a sample message created by the Kentico API.";

                // Gets the sender and the recipient of the message
                UserInfo sender = MembershipContext.AuthenticatedUser;
                UserInfo recipient = UserInfoProvider.AdministratorUser;

                // Checks if both sender and recipient exist and sets the message properties
                if ((sender != null) && (recipient != null))
                {
                    newMessage.MessageSenderUserID = sender.UserID;
                    newMessage.MessageSenderNickName = sender.UserNickName;
                    newMessage.MessageRecipientUserID = recipient.UserID;
                    newMessage.MessageRecipientNickName = recipient.UserNickName;
                    newMessage.MessageSent = DateTime.Now;

                    // Saves the message to the database
                    MessageInfoProvider.SetMessageInfo(newMessage);
                }
            }


            /// <heading>Updating a message</heading>
            private void GetAndUpdateMessage()
            {
                // Prepares a where condition for loading a message with the subject 'API example message'
                string where = "[MessageSubject] = N'API example message'";

                // Gets the message based on the condition
                DataSet messages = MessageInfoProvider.GetMessages(where, null);

                if (!DataHelper.DataSourceIsEmpty(messages))
                {
                    // Gets the message from the DataSet
                    MessageInfo modifyMessage = new MessageInfo(messages.Tables[0].Rows[0]);

                    // Updates the properties of the message
                    modifyMessage.MessageBody = modifyMessage.MessageBody.ToUpper();

                    // Saves the changes to the database
                    MessageInfoProvider.SetMessageInfo(modifyMessage);
                }
            }


            /// <heading>Updating multiple messages</heading>
            private void GetAndBulkUpdateMessages()
            {
                // Prepares a where condition for loading all messages whose subject contains the word 'API'
                string where = "[MessageSubject] LIKE N'API'";

                // Gets the messages that fulfill the condition
                DataSet messages = MessageInfoProvider.GetMessages(where, null);

                if (!DataHelper.DataSourceIsEmpty(messages))
                {
                    // Loops through individual messages
                    foreach (DataRow messageDr in messages.Tables[0].Rows)
                    {
                        // Converts the DataRow to a message object
                        MessageInfo modifyMessage = new MessageInfo(messageDr);

                        // Updates the properties of the message
                        modifyMessage.MessageBody = modifyMessage.MessageBody.ToLowerCSafe();

                        // Saves the changes to the database
                        MessageInfoProvider.SetMessageInfo(modifyMessage);
                    }
                }
            }


            /// <heading>Deleting messages</heading>
            private void DeleteMessage()
            {
                // Prepares a where condition for loading all messages whose subject contains the word 'API'
                string where = "[MessageSubject] LIKE N'API'";

                // Gets the messages that fulfill the condition
                DataSet messages = MessageInfoProvider.GetMessages(where, null);

                if (!DataHelper.DataSourceIsEmpty(messages))
                {
                    // Loops through individual messages
                    foreach (DataRow messageDr in messages.Tables[0].Rows)
                    {
                        // Converts the DataRow to a message object
                        MessageInfo deleteMessage = new MessageInfo(messageDr);

                        if (deleteMessage != null)
                        {
                            // Deletes the message
                            MessageInfoProvider.DeleteMessageInfo(deleteMessage);
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Holds contact list API examples.
        /// </summary>
        /// <groupHeading>Contact lists</groupHeading>
        private class ContactList
        {
            /// <heading>Adding users to a contact list</heading>
            private void AddUserToContactList()
            {
                // Creates a new sample user which will be added to the contact list
                UserInfo user = new UserInfo();
                user.UserName = "John";
                user.FullName = "John Doe";

                UserInfoProvider.SetUserInfo(user);

                // Checks that new user is not already in the current user's contact list
                if (!ContactListInfoProvider.IsInContactList(MembershipContext.AuthenticatedUser.UserID, user.UserID))
                {
                    // Adds the new user to the current user's contact list
                    ContactListInfoProvider.AddToContactList(MembershipContext.AuthenticatedUser.UserID, user.UserID);
                }
            }


            /// <heading>Removing users from a contact list</heading>
            private void RemoveUserFromContactList()
            {
                // Gets the user
                UserInfo user = UserInfoProvider.GetUserInfo("John");

                // Checks that the user is in the current user's contact list
                if (ContactListInfoProvider.IsInContactList(MembershipContext.AuthenticatedUser.UserID, user.UserID))
                {
                    // Removes the user from the current user's contact list
                    ContactListInfoProvider.RemoveFromContactList(MembershipContext.AuthenticatedUser.UserID, user.UserID);
                }
            }
        }


        /// <summary>
        /// Holds ignore list API examples.
        /// </summary>
        /// <groupHeading>Ignore lists</groupHeading>
        private class IgnoreList
        {
            /// <heading>Adding users to an ignore list</heading>
            private void AddUserToIgnoreList()
            {
                // Creates a new sample user which will be added to the ignore list
                UserInfo user = new UserInfo();
                user.UserName = "Jane";
                user.FullName = "Jane Doe";

                UserInfoProvider.SetUserInfo(user);

                // Checks that new user is not already in the current user's ignore list
                if (!IgnoreListInfoProvider.IsInIgnoreList(MembershipContext.AuthenticatedUser.UserID, user.UserID))
                {
                    // Adds the new user to the current user's ignore list
                    IgnoreListInfoProvider.AddToIgnoreList(MembershipContext.AuthenticatedUser.UserID, user.UserID);
                }
            }

            
            /// <heading>Removing users from an ignore list</heading>
            private void RemoveUserFromIgnoreList()
            {
                // Gets the user
                UserInfo user = UserInfoProvider.GetUserInfo("Jane");

                // Checks that the user is in the current user's ignore list
                if (IgnoreListInfoProvider.IsInIgnoreList(MembershipContext.AuthenticatedUser.UserID, user.UserID))
                {
                    // Removes the user from the current user's ignore list
                    IgnoreListInfoProvider.RemoveFromIgnoreList(MembershipContext.AuthenticatedUser.UserID, user.UserID);
                }
            }
        }
    }
}