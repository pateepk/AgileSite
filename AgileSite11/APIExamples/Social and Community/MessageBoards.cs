using System;
using System.Data;

using CMS.Community;
using CMS.Membership;
using CMS.MessageBoards;
using CMS.SiteProvider;
using CMS.DocumentEngine;
using CMS.Base;
using CMS.Helpers;
using CMS.DataEngine;

namespace APIExamples
{
    /// <summary>
    /// Holds API examples related to message boards.
    /// </summary>
    /// <pageTitle>Message boards</pageTitle>
    internal class MessageBoardsMain
    {
        /// <summary>
        /// Holds message board API examples.
        /// </summary>
        /// <groupHeading>Message boards</groupHeading>
        private class MessageBoards
        {
            /// <heading>Updating a message board</heading>
            private void GetAndUpdateMessageBoard()
            {
                /* NOTE: New message board can only be created in the Pages application using the Message board web part.
                * The system automatically creates a message board after the first message is submitted on the page via the web part. */

                // Gets a TreeProvider instance
                TreeProvider tree = new TreeProvider(MembershipContext.AuthenticatedUser);

                // Gets the page where the message board is located
                TreeNode page = tree.SelectNodes()
                    .Path("/BoardPage")
                    .OnCurrentSite()
                    .FirstObject;

                if (page != null)
                {
                    // Gets the message board based on its name and the related page
                    BoardInfo updateBoard = BoardInfoProvider.GetBoardInfo("BoardName", page.DocumentID);

                    if (updateBoard != null)
                    {
                        // Updates the board properties
                        updateBoard.BoardDisplayName = updateBoard.BoardDisplayName.ToLowerCSafe();

                        // Saves the updated message board to the database
                        BoardInfoProvider.SetBoardInfo(updateBoard);
                    }
                }
            }


            /// <heading>Updating multiple message boards</heading>
            private void GetAndBulkUpdateMessageBoards()
            {
                // Prepares a where condition for loading all message boards whose name starts with 'New'
                string where = "BoardName LIKE N'New%'";

                // Gets the message boards that fulfill the condition
                InfoDataSet<BoardInfo> boards = BoardInfoProvider.GetMessageBoards(where, null);

                // Loops through individual message boards
                foreach (BoardInfo board in boards)
                {
                    // Updates the board properties
                    board.BoardDisplayName = board.BoardDisplayName.ToUpper();

                    // Saves the updated message board to the database
                    BoardInfoProvider.SetBoardInfo(board);
                }
            }


            /// <heading>Adding users as message board moderators</heading>
            private void AddModeratorToMessageBoard()
            {
                // Gets a TreeProvider instance
                TreeProvider tree = new TreeProvider(MembershipContext.AuthenticatedUser);

                // Gets the page where the message board is located
                TreeNode page = tree.SelectNodes()
                    .Path("/BoardPage")
                    .OnCurrentSite()
                    .FirstObject;

                if (page != null)
                {
                    // Gets the message board based on its name and the related page
                    BoardInfo board = BoardInfoProvider.GetBoardInfo("BoardName", page.DocumentID);
                    
                    if (board != null)
                    {
                        // Add the current users as a moderator of the message board
                        BoardModeratorInfoProvider.AddModeratorToBoard(MembershipContext.AuthenticatedUser.UserID, board.BoardID);
                    }
                }
            }

            
            /// <heading>Removing moderators from message boards</heading>
            private void RemoveModeratorFromMessageBoard()
            {
                // Gets a TreeProvider instance
                TreeProvider tree = new TreeProvider(MembershipContext.AuthenticatedUser);

                // Gets the page where the message board is located
                TreeNode page = tree.SelectNodes()
                    .Path("/BoardPage")
                    .OnCurrentSite()
                    .FirstObject;

                if (page != null)
                {
                    // Gets the message board based on its name and the related page
                    BoardInfo board = BoardInfoProvider.GetBoardInfo("MyNewBoard", page.DocumentID);
                    
                    if (board != null)
                    {
                        // Gets the moderator relationship between the message board and the current user
                        BoardModeratorInfo boardModerator = BoardModeratorInfoProvider.GetBoardModeratorInfo(MembershipContext.AuthenticatedUser.UserID, board.BoardID);

                        if (boardModerator != null)
                        {
                            // Removes the moderator from the message board
                            BoardModeratorInfoProvider.DeleteBoardModeratorInfo(boardModerator);
                        }
                    }
                }
            }


            /// <heading>Deleting a message board</heading>
            private void DeleteMessageBoard()
            {
                // Gets a TreeProvider instance
                TreeProvider tree = new TreeProvider(MembershipContext.AuthenticatedUser);

                // Gets the page where the message board is located
                TreeNode page = tree.SelectNodes()
                    .Path("/BoardPage")
                    .OnCurrentSite()
                    .FirstObject;

                if (page != null)
                {
                    // Gets the message board based on its name and the related page
                    BoardInfo deleteBoard = BoardInfoProvider.GetBoardInfo("BoardName", page.DocumentID);

                    if (deleteBoard != null)
                    {
                        // Deletes the message board
                        BoardInfoProvider.DeleteBoardInfo(deleteBoard);
                    }
                }
            }
        }


        /// <summary>
        /// Holds message board message API examples.
        /// </summary>
        /// <groupHeading>Message board messages</groupHeading>
        private class MessageBoardMessages
        {
            /// <heading>Creating a message on a message board</heading>
            private void CreateMessage()
            {
                // Gets a TreeProvider instance
                TreeProvider tree = new TreeProvider(MembershipContext.AuthenticatedUser);

                // Gets the page where the message board is located
                TreeNode page = tree.SelectNodes()
                    .Path("/BoardPage")
                    .OnCurrentSite()
                    .FirstObject;

                if (page != null)
                {
                    // Gets the message board
                    BoardInfo board = BoardInfoProvider.GetBoardInfo("BoardName", page.DocumentID);

                    if (board != null)
                    {
                        // Creates a new message object
                        BoardMessageInfo newMessage = new BoardMessageInfo();

                        // Sets the message properties
                        newMessage.MessageUserName = MembershipContext.AuthenticatedUser.UserName;
                        newMessage.MessageText = "New message";
                        newMessage.MessageEmail = "user@localhost.local";
                        newMessage.MessageURL = "";
                        newMessage.MessageIsSpam = false;
                        newMessage.MessageApproved = true;
                        newMessage.MessageInserted = DateTime.Now;
                        newMessage.MessageBoardID = board.BoardID;

                        // Saves the message to the database
                        BoardMessageInfoProvider.SetBoardMessageInfo(newMessage);
                    }
                }
            }


            /// <heading>Updating a message</heading>
            private void GetAndUpdateMessage()
            {
                // Gets a TreeProvider instance
                TreeProvider tree = new TreeProvider(MembershipContext.AuthenticatedUser);

                // Gets the page where the message board is located
                TreeNode page = tree.SelectNodes()
                    .Path("/BoardPage")
                    .OnCurrentSite()
                    .FirstObject;

                if (page != null)
                {
                    // Gets the message board
                    BoardInfo board = BoardInfoProvider.GetBoardInfo("BoardName", page.DocumentID);

                    if (board != null)
                    {
                        // Gets the first message from the selected board whose text starts with 'New message'
                        BoardMessageInfo updateMessage = BoardMessageInfoProvider.GetMessages()
                                                                    .WhereStartsWith("MessageText", "New message")
                                                                    .WhereEquals("MessageBoardID", board.BoardID)
                                                                    .FirstObject;
                                                                    
                        // Updates the message text
                        updateMessage.MessageText = updateMessage.MessageText.ToUpperCSafe();

                        // Saves the message to the database
                        BoardMessageInfoProvider.SetBoardMessageInfo(updateMessage);
                    }
                }
            }


            /// <heading>Updating multiple messages</heading>
            private void GetAndBulkUpdateMessages()
            {
                // Gets a TreeProvider instance
                TreeProvider tree = new TreeProvider(MembershipContext.AuthenticatedUser);

                // Gets the page where the message board is located
                TreeNode page = tree.SelectNodes()
                    .Path("/BoardPage")
                    .OnCurrentSite()
                    .FirstObject;

                if (page != null)
                {
                    // Get the message board
                    BoardInfo board = BoardInfoProvider.GetBoardInfo("BoardName", page.DocumentID);

                    if (board != null)
                    {
                        // Gets all messages from the selected board whose text starts with 'New message'
                        var messages = BoardMessageInfoProvider.GetMessages()
                                                                    .WhereStartsWith("MessageText", "New message")
                                                                    .WhereEquals("MessageBoardID", board.BoardID);
                        
                        // Loops through individual messages
                        foreach (BoardMessageInfo modifyMessage in messages)
                        {
                            // Updates the message text
                            modifyMessage.MessageText = modifyMessage.MessageText.ToUpper();

                            // Saves the updated message to the database
                            BoardMessageInfoProvider.SetBoardMessageInfo(modifyMessage);
                        }
                    }
                }
            }


            /// <heading>Deleting a message</heading>
            private void DeleteMessage()
            {
                // Gets a TreeProvider instance
                TreeProvider tree = new TreeProvider(MembershipContext.AuthenticatedUser);

                // Gets the page where the message board is located
                TreeNode page = tree.SelectNodes()
                    .Path("/BoardPage")
                    .OnCurrentSite()
                    .FirstObject;

                if (page != null)
                {
                    // Gets the message board
                    BoardInfo board = BoardInfoProvider.GetBoardInfo("BoardName", page.DocumentID);

                    if (board != null)
                    {
                        // Gets the first message from the selected board whose text starts with 'New message'
                        BoardMessageInfo deleteMessage = BoardMessageInfoProvider.GetMessages()
                                                                    .WhereStartsWith("MessageText", "New message")
                                                                    .WhereEquals("MessageBoardID", board.BoardID)
                                                                    .FirstObject;

                        if (deleteMessage != null)
                        {
                            // Deletes the message
                            BoardMessageInfoProvider.DeleteBoardMessageInfo(deleteMessage);
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Holds message board subscription API examples.
        /// </summary>
        /// <groupHeading>Message board subscriptions</groupHeading>
        private class MessageBoardSubscriptions
        {
            /// <heading>Creating a message board subscription</heading>
            private void CreateMessageBoardSubscription()
            {
                // Creates a new message board subscription object
                BoardSubscriptionInfo newSubscription = new BoardSubscriptionInfo();

                // Gets a TreeProvider instance
                TreeProvider tree = new TreeProvider(MembershipContext.AuthenticatedUser);

                // Gets the page where the message board is located
                TreeNode page = tree.SelectNodes()
                    .Path("/BoardPage")
                    .OnCurrentSite()
                    .FirstObject;

                if (page != null)
                {
                    // Gets the message board
                    BoardInfo board = BoardInfoProvider.GetBoardInfo("BoardName", page.DocumentID);

                    if (board != null)
                    {
                        // Sets the subsription properties (subscribes the current user)
                        newSubscription.SubscriptionBoardID = board.BoardID;
                        newSubscription.SubscriptionUserID = MembershipContext.AuthenticatedUser.UserID;
                        newSubscription.SubscriptionEmail = MembershipContext.AuthenticatedUser.Email;

                        // Creates the message board subscription
                        BoardSubscriptionInfoProvider.SetBoardSubscriptionInfo(newSubscription);
                    }
                }
            }


            /// <heading>Updating a message board subscription</heading>
            private void GetAndUpdateMessageBoardSubscription()
            {
                // Gets a TreeProvider instance
                TreeProvider tree = new TreeProvider(MembershipContext.AuthenticatedUser);

                // Gets the page where the message board is located
                TreeNode page = tree.SelectNodes()
                    .Path("/BoardPage")
                    .OnCurrentSite()
                    .FirstObject;

                if (page != null)
                {
                    // Gets the message board
                    BoardInfo board = BoardInfoProvider.GetBoardInfo("BoardName", page.DocumentID);

                    if (board != null)
                    {
                        // Gets the message board subscription for the current user
                        BoardSubscriptionInfo updateSubscription = BoardSubscriptionInfoProvider.GetBoardSubscriptionInfo(board.BoardID, MembershipContext.AuthenticatedUser.UserID);
                        
                        if (updateSubscription != null)
                        {
                            // Updates the subscription properties
                            updateSubscription.SubscriptionEmail = updateSubscription.SubscriptionEmail.ToLowerCSafe();

                            // Saves the updated subscription to the database
                            BoardSubscriptionInfoProvider.SetBoardSubscriptionInfo(updateSubscription);
                        }
                    }
                }
            }


            /// <heading>Updating multiple message board subscriptions</heading>
            private void GetAndBulkUpdateMessageBoardSubscriptions()
            {
                // Gets a TreeProvider instance
                TreeProvider tree = new TreeProvider(MembershipContext.AuthenticatedUser);

                // Gets the page where the message board is located
                TreeNode page = tree.SelectNodes()
                    .Path("/BoardPage")
                    .OnCurrentSite()
                    .FirstObject;

                if (page != null)
                {
                    // Gets the message board
                    BoardInfo board = BoardInfoProvider.GetBoardInfo("BoardName", page.DocumentID);

                    if (board != null)
                    {
                        // Prepares a where condition for loading all subscriptions of the message board
                        string where = "SubscriptionBoardID = " + board.BoardID;

                        // Gets a DataSet containing all subscriptions of the selected message board
                        DataSet subscriptions = BoardSubscriptionInfoProvider.GetSubscriptions(where, null);

                        if (!DataHelper.DataSourceIsEmpty(subscriptions))
                        {
                            // Loops through individual subscriptions
                            foreach (DataRow subscriptionDr in subscriptions.Tables[0].Rows)
                            {
                                // Converts the DataRow to a subscription object
                                BoardSubscriptionInfo modifySubscription = new BoardSubscriptionInfo(subscriptionDr);

                                // Updates the subscription properties
                                modifySubscription.SubscriptionEmail = modifySubscription.SubscriptionEmail.ToUpper();

                                // Saves the modified subscription to the database
                                BoardSubscriptionInfoProvider.SetBoardSubscriptionInfo(modifySubscription);
                            }
                        }
                    }
                }
            }


            /// <heading>Deleting a message board subscription</heading>
            private void DeleteMessageBoardSubscription()
            {
                // Gets a TreeProvider instance
                TreeProvider tree = new TreeProvider(MembershipContext.AuthenticatedUser);

                // Gets the page where the message board is located
                TreeNode page = tree.SelectNodes()
                    .Path("/BoardPage")
                    .OnCurrentSite()
                    .FirstObject;

                if (page != null)
                {
                    // Gets the message board
                    BoardInfo board = BoardInfoProvider.GetBoardInfo("BoardName", page.DocumentID);

                    if (board != null)
                    {
                        // Gets the message board subscription for the current user
                        BoardSubscriptionInfo deleteSubscription = BoardSubscriptionInfoProvider.GetBoardSubscriptionInfo(board.BoardID, MembershipContext.AuthenticatedUser.UserID);

                        if (deleteSubscription != null)
                        {
                            // Deletes the message board subscription
                            BoardSubscriptionInfoProvider.DeleteBoardSubscriptionInfo(deleteSubscription);
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Holds message board security API examples.
        /// </summary>
        /// <groupHeading>Message board security</groupHeading>
        private class RolesOnMessageBoards
        {
            /// <heading>Allowing a message board only for members of a role</heading>
            private void AddRoleToMessageBoard()
            {
                // Gets a TreeProvider instance
                TreeProvider tree = new TreeProvider(MembershipContext.AuthenticatedUser);

                // Gets the page where the message board is located
                TreeNode page = tree.SelectNodes()
                    .Path("/BoardPage")
                    .OnCurrentSite()
                    .FirstObject;

                if (page != null)
                {
                    // Gets the message board
                    BoardInfo board = BoardInfoProvider.GetBoardInfo("BoardName", page.DocumentID);

                    // Gets the role
                    RoleInfo role = RoleInfoProvider.GetRoleInfo("RoleName", SiteContext.CurrentSite.SiteID);

                    if ((board != null) && (role != null))
                    {
                        // Configures the message board to be usable only by assigned roles
                        board.BoardAccess = SecurityAccessEnum.AuthorizedRoles;
                        BoardInfoProvider.SetBoardInfo(board);

                        // Assigns the role to the message board (allows members of the role to use the message board)
                        BoardRoleInfoProvider.AddRoleToBoard(role.RoleID, board.BoardID);
                    }
                }
            }


            /// <heading>Removing a role from a message board</heading>
            private void RemoveRoleFromMessageBoard()
            {
                // Gets a TreeProvider instance
                TreeProvider tree = new TreeProvider(MembershipContext.AuthenticatedUser);

                // Gets the page where the message board is located
                TreeNode page = tree.SelectNodes()
                    .Path("/BoardPage")
                    .OnCurrentSite()
                    .FirstObject;

                if (page != null)
                {
                    // Gets the message board
                    BoardInfo board = BoardInfoProvider.GetBoardInfo("NewBoard", page.DocumentID);

                    // Gets the role
                    RoleInfo role = RoleInfoProvider.GetRoleInfo("RoleName", SiteContext.CurrentSite.SiteID);

                    if ((board != null) && (role != null))
                    {
                        // Gets the relationship between the role and the message board
                        BoardRoleInfo boardRole = BoardRoleInfoProvider.GetBoardRoleInfo(role.RoleID, board.BoardID);

                        if (boardRole != null)
                        {
                            // Removes the role from the message board
                            BoardRoleInfoProvider.DeleteBoardRoleInfo(boardRole);
                        }
                    }
                }
            }
        }
    }
}
