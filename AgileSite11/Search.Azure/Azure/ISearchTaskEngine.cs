namespace CMS.Search.Azure
{
    internal interface ISearchTaskEngine
    {
        void ProcessAzureSearchTask(SearchTaskAzureInfo task);
    }
}