namespace WTE.Communication
{
    /// <summary>
    /// AsynchronousResponseHandler, interface for callback handler
    /// </summary>
    public interface AsynchronousResponseHandler
    {
        void HandleResponse(CommunicationResponse p_response);
    }

    /// <summary>
    /// Default noop callback AsynchronousResponseHandler
    /// </summary>
    public class DefaultAsynchronousResponseHandler : AsynchronousResponseHandler
    {
        public void HandleResponse(CommunicationResponse p_response)
        {
            //noop
        }
    }
}