using CMS.Base;

namespace CMS.ContactManagement
{
    /// <summary>
    /// Event fires when <see cref="ContactInfoProvider.DeleteContactInfos"/> deletes some contacts. Deleted contacts IDs are passed in event arguments.
    /// </summary>
    public class ContactInfosDeletedHandler : SimpleHandler<ContactInfosDeletedHandler, ContactInfosDeletedHandlerEventArgs>
    {
        
    }
}