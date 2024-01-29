using CMS;
using CMS.DataEngine;
using CMS.DocumentEngine;

[assembly: RegisterModule(typeof(SampleHandlerModule))]

/// <summary>
/// Sample module with custom handler for document events
/// </summary>
internal class SampleHandlerModule : Module
{
    public SampleHandlerModule()
        : base("SampleHandlerModule")
    {
    }


    protected override void OnInit()
    {
        // Custom event handling
        DocumentEvents.Insert.Before += UpperCaseDocumentNameBeforeInsert;
    }


    private void UpperCaseDocumentNameBeforeInsert(object sender, DocumentEventArgs e)
    {
        var doc = e.Node;
        doc.DocumentName = doc.DocumentName.ToUpperInvariant();
    }
}