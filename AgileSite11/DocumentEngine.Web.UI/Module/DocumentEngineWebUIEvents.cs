namespace CMS.DocumentEngine.Web.UI
{
    /// <summary>
    /// Controls events
    /// </summary>
    public class DocumentEngineWebUIEvents
    {
        /// <summary>
        /// Fires when the data is evaluated for a column within the transformation
        /// </summary>
        public static TransformationEvalHandler TransformationEval = new TransformationEvalHandler { Name = "DocumentEngineWebUIEvents.TransformationEval" };
    }
}
