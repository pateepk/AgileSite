using CMS.Base;

namespace CMS.DocumentEngine.Taxonomy
{
    /// <summary>
    /// Attaches handlers for tags processing.
    /// </summary>
    internal class DocumentTagHandlers
    {
        /// <summary>
        /// Initializes the events handlers.
        /// </summary>
        public static void Init()            
        {
            DocumentEvents.Insert.After += CreateTags;
            DocumentEvents.InsertNewCulture.After += CreateTags;
            DocumentEvents.Update.Before += UpdateTags;
        }


        private static void UpdateTags(object sender, DocumentEventArgs e)
        {
            var document = e.Node;
            var originalParentId = document.GetOriginalValue("NodeParentID").ToInteger(0);
            var parentChanged = originalParentId != document.NodeParentID;

            var originalState = new OriginalStateOfTagsInDocument(document);
           
            e.CallWhenFinished(() =>
            {
                var tagsManager = new DocumentTagUpdater(document, originalState);
                tagsManager.UpdateTags(parentChanged);
            });
        }


        private static void CreateTags(object sender, DocumentEventArgs e)
        {
            var document = e.Node;

            var tagsManager = new DocumentTagCreator(document);
            tagsManager.CreateTags();
        }
    }
}
