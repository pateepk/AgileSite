using CMS;
using CMS.DataEngine;
using CMS.OutputFilter;
using CMS.Base;


[assembly: RegisterModule(typeof(SampleMacroModule))]

/// <summary>
/// Sample module for macros.
/// </summary>
internal class SampleMacroModule : Module
{
    public SampleMacroModule()
        : base("SampleMacroModule")
    {
    }


    protected override void OnInit()
    {
        // Custom string macro methods
        Extend<string>.With<CustomMacroMethods>();

        // Custom output substitution resolving
        ResponseOutputFilter.OnResolveSubstitution += OutputFilter_OnResolveSubstitution;
    }


    /// <summary>
    /// Resolves the output substitution
    /// </summary>
    private void OutputFilter_OnResolveSubstitution(object sender, SubstitutionEventArgs e)
    {
        if (!e.Match)
        {
            // Add your custom macro evaluation
            switch (e.Expression.ToLowerCSafe())
            {
                case "somesubstitution":
                    e.Match = true;
                    e.Result = "Resolved substitution";
                    break;
            }
        }
    }
}