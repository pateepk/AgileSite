using System.Collections.Generic;

namespace CMS.SalesForce
{

    /// <summary>
    /// Represents the interface for classes that represents a single entity command result.
    /// </summary>
    public interface IEntityCommandResult
    {

        #region "Properties"


        /// <summary>
        /// Gets the identifier of the entity, if the operation was successful.
        /// </summary>
        string EntityId
        {
            get;
        }

        /// <summary>
        /// Gets the value indicating whether the operation was successful.
        /// </summary>
        bool IsSuccess
        {
            get;
        }

        /// <summary>
        /// Gets a list of errors, if the operation was not successful.
        /// </summary>
        IEnumerable<Error> Errors
        {
            get;
        }

        #endregion

    }

}