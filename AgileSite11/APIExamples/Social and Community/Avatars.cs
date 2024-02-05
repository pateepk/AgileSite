using System;

using CMS.Membership;

namespace APIExamples
{
    /// <summary>
    /// Holds avatars API examples.
    /// </summary>
    /// <pageTitle>Avatars</pageTitle>
    internal class Avatars
    {
        /// <heading>Creating an avatar</heading>
        private void CreateAvatar()
        {
            // Creates a new avatar object based on an image file
            AvatarInfo newAvatar = new AvatarInfo(System.Web.HttpContext.Current.Server.MapPath("~\\Avatars\\Files\\avatar_man.jpg"));

            // Sets the avatar properties
            newAvatar.AvatarName = "NewAvatar";
            newAvatar.AvatarType = AvatarInfoProvider.GetAvatarTypeString(AvatarTypeEnum.All);
            newAvatar.AvatarIsCustom = false;

            // Saves the avatar to the database
            AvatarInfoProvider.SetAvatarInfo(newAvatar);
        }


        /// <heading>Updating an avatar</heading>
        private void GetAndUpdateAvatar()
        {
            // Gets the avatar
            AvatarInfo updateAvatar = AvatarInfoProvider.GetAvatarInfo("NewAvatar");
            if (updateAvatar != null)
            {
                // Updates the avatar properties
                updateAvatar.AvatarName = updateAvatar.AvatarName.ToLower();

                // Saves the changes to the database
                AvatarInfoProvider.SetAvatarInfo(updateAvatar);
            }
        }


        /// <heading>Updating multiple avatars</heading>
        private void GetAndBulkUpdateAvatars()
        {
            // Gets all avatars whose name starts with 'New'
            var avatars = AvatarInfoProvider.GetAvatars().WhereStartsWith("AvatarName", "New");

            // Loops through individual avatars
            foreach (AvatarInfo modifyAvatar in avatars)
            {
                // Updates the avatar properties
                modifyAvatar.AvatarName = modifyAvatar.AvatarName.ToUpper();

                // Saves the changes to the database
                AvatarInfoProvider.SetAvatarInfo(modifyAvatar);
            }
        }


        /// <heading>Assigning an avatar to a user</heading>
        private void AddAvatarToUser()
        {
            // Gets the avatar and the user
            AvatarInfo avatar = AvatarInfoProvider.GetAvatarInfo("NewAvatar");
            UserInfo user = UserInfoProvider.GetUserInfo("Andy");

            if ((avatar != null) && (user != null))
            {
                // Assigns the avatar to the user
                user.UserAvatarID = avatar.AvatarID;

                // Saves the updated user to the database
                UserInfoProvider.SetUserInfo(user);
            }
        }


        /// <heading>Removing an avatar from a user</heading>
        private void RemoveAvatarFromUser()
        {
            // Gets the user
            UserInfo user = UserInfoProvider.GetUserInfo("Andy");

            if (user != null)
            {
                // Removes the user's current avatar
                user.UserAvatarID = 0;

                // Saves the updated user to the database
                UserInfoProvider.SetUserInfo(user);
            }
        }


        /// <heading>Deleting avatars</heading>
        private void DeleteAvatar()
        {
            // Gets the avatar
            AvatarInfo deleteAvatar = AvatarInfoProvider.GetAvatarInfo("NewAvatar");

            if (deleteAvatar != null)
            {
                // Deletes the avatar
                AvatarInfoProvider.DeleteAvatarInfo(deleteAvatar);
            }
        }
    }
}
