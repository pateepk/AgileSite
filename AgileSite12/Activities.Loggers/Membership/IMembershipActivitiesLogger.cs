using CMS.Base;

namespace CMS.Activities.Loggers
{
    /// <summary>
    /// Provides methods for membership activities logging.
    /// </summary>
    public interface IMembershipActivityLogger
    {
        /// <summary>
        /// Logs login activity for given user.
        /// </summary>
        /// <remarks>
        /// This method should be called whenever the user is authenticated to ensure logging of correct <see cref="ActivityInfo"/>.
        /// </remarks>
        /// <param name="userName">User name of the authenticated user.</param>
        void LogLogin(string userName);


        /// <summary>
        /// Logs login activity for given user.
        /// </summary>
        /// <remarks>
        /// This method should be called whenever the user is authenticated to ensure logging of correct <see cref="ActivityInfo"/>.
        /// </remarks>
        /// <param name="userName">User name of the authenticated user.</param>
        /// <param name="currentDocument">Current document tree representation, for MVC should be null.</param>
        void LogLogin(string userName, ITreeNode currentDocument);


        /// <summary>
        /// Logs user registration activity for given user.
        /// </summary>
        /// <remarks>
        /// This method should be called whenever the user is registered to ensure logging of correct <see cref="ActivityInfo"/>.
        /// </remarks>
        /// <param name="userName">User name of the registered user.</param>
        void LogRegistration(string userName);


        /// <summary>
        /// Logs user registration activity for given user.
        /// </summary>
        /// <remarks>
        /// This method should be called whenever the user is registered to ensure logging of correct <see cref="ActivityInfo"/>.
        /// </remarks>
        /// <param name="userName">User name of the registered user.</param>
        /// <param name="currentDocument">Current document tree representation, for MVC should be null</param>
        void LogRegistration(string userName, ITreeNode currentDocument);


        /// <summary>
        /// Logs user registration activity for given user.
        /// </summary>
        /// <remarks>
        /// This method should be called whenever the user is registered to ensure logging of correct <see cref="ActivityInfo"/>.
        /// </remarks>
        /// <param name="userName">User name of the registered user.</param>
        /// <param name="currentDocument">Current document tree representation, for MVC should be null</param>
        /// <param name="checkViewModel"><c>True</c> if activities should not be logged in administration</param>
        void LogRegistration(string userName, ITreeNode currentDocument, bool checkViewModel);
    }
}