namespace CMS.Membership
{
    /// <summary>
    /// Authentication result enumeration.
    /// </summary>
    public enum AuthenticationResultEnum
    {
        ///<summary>Provided credentials are valid. The user can log on.</summary>
        OK = 0,

        ///<summary>Provided credentials are invalid. The user cannot log on.</summary>
        WrongUserNameOrPassword = 1
    }
}